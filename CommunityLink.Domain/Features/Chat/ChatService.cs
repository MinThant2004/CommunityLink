using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using Microsoft.AspNetCore.SignalR;

namespace CommunityLink.Domain.Features.Chat;

public interface IChatService
{
    Task<Result<IReadOnlyList<ConversationModel>>> GetConversationsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatMessageModel>>> GetMessagesAsync(int conversationId, CancellationToken cancellationToken = default);
    Task<Result<ChatMessageModel>> SendMessageAsync(SendMessageRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<int>> MarkConversationReadAsync(int conversationId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default);
}

public sealed class ChatService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    IHubContext<ChatHub> hubContext) : IChatService
{
    public async Task<Result<IReadOnlyList<ConversationModel>>> GetConversationsAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<IReadOnlyList<ConversationModel>>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var currentUserId = currentUser.UserId.Value;
        var list = await dbContext.TblConversations
            .Include(c => c.UserOne)
            .Include(c => c.UserTwo)
            .Where(c => (c.UserOneId == currentUserId || c.UserTwoId == currentUserId) && !c.IsDeleted)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Select(c => new ConversationModel(
                c.ConversationId,
                c.UserOneId == currentUserId ? c.UserTwoId : c.UserOneId,
                c.UserOneId == currentUserId ? c.UserTwo.UserName : c.UserOne.UserName,
                c.UserOneId == currentUserId ? c.UserTwo.DisplayName : c.UserOne.DisplayName,
                c.UserOneId == currentUserId ? c.UserTwo.AvatarUrl : c.UserOne.AvatarUrl,
                c.LastMessagePreview,
                c.LastMessageAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ConversationModel>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<ChatMessageModel>>> GetMessagesAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<IReadOnlyList<ChatMessageModel>>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        // A conversation id is guessable, so confirm the caller is actually a participant.
        var isParticipant = await dbContext.TblConversations
            .AsNoTracking()
            .AnyAsync(c => c.ConversationId == conversationId &&
                            !c.IsDeleted &&
                            (c.UserOneId == currentUser.UserId.Value || c.UserTwoId == currentUser.UserId.Value),
                cancellationToken);

        if (!isParticipant)
        {
            return Result<IReadOnlyList<ChatMessageModel>>.Failure("Conversation not found.", ResultStatus.NotFound);
        }

        var messages = await dbContext.TblChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageModel(
                m.ChatMessageId,
                m.ConversationId,
                m.SenderId,
                m.Sender.DisplayName,
                m.Sender.AvatarUrl,
                m.MessageText,
                m.IsRead,
                m.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ChatMessageModel>>.Success(messages);
    }

    public async Task<Result<ChatMessageModel>> SendMessageAsync(SendMessageRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<ChatMessageModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var currentUserId = currentUser.UserId.Value;
        TblConversation? conversation = null;

        if (request.ConversationId.HasValue)
        {
            conversation = await dbContext.TblConversations.FindAsync([request.ConversationId.Value], cancellationToken);
        }
        else if (request.TargetUserId.HasValue)
        {
            var targetId = request.TargetUserId.Value;
            conversation = await dbContext.TblConversations
                .FirstOrDefaultAsync(c => ((c.UserOneId == currentUserId && c.UserTwoId == targetId) ||
                                          (c.UserOneId == targetId && c.UserTwoId == currentUserId)) && !c.IsDeleted, cancellationToken);
        }

        int recipientId = 0;
        if (conversation != null)
        {
            recipientId = conversation.UserOneId == currentUserId ? conversation.UserTwoId : conversation.UserOneId;
        }
        else if (request.TargetUserId.HasValue)
        {
            recipientId = request.TargetUserId.Value;
        }

        // STEP 10D: Private Chat Authorization check
        if (recipientId != 0 && recipientId != currentUserId)
        {
            var accessCheck = await ValidatePrivateChatAccessAsync(currentUserId, recipientId, conversation?.ConversationId, cancellationToken);
            if (accessCheck != null && !accessCheck.IsSuccess)
            {
                return Result<ChatMessageModel>.Failure(accessCheck.Message, accessCheck.Status);
            }
        }

        if (conversation is null && request.TargetUserId.HasValue)
        {
            var targetId = request.TargetUserId.Value;
            conversation = new TblConversation
            {
                UserOneId = currentUserId,
                UserTwoId = targetId,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TblConversations.Add(conversation);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (conversation is null) return Result<ChatMessageModel>.Failure("Conversation not found.", ResultStatus.NotFound);

        var message = new TblChatMessage
        {
            ConversationId = conversation.ConversationId,
            SenderId = currentUserId,
            MessageText = request.MessageText.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblChatMessages.Add(message);

        conversation.LastMessagePreview = message.MessageText;
        conversation.LastMessageAt = message.CreatedAt;
        conversation.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.TblUsers.FindAsync([currentUserId], cancellationToken);
        var model = new ChatMessageModel(
            message.ChatMessageId,
            message.ConversationId,
            message.SenderId,
            user?.DisplayName ?? "Unknown",
            user?.AvatarUrl,
            message.MessageText,
            message.IsRead,
            message.CreatedAt);

        // Push to the recipient's private user scope. The client already has the row from the
        // REST response and de-duplicates on message id.
        if (recipientId != 0 && recipientId != currentUserId)
        {
            try
            {
                await hubContext.Clients.User(recipientId.ToString()).SendAsync(
                    "ReceivePrivateMessage", model, cancellationToken);
            }
            catch
            {
                // A failed push must not lose the message; the client refreshes on next load.
            }
        }

        return Result<ChatMessageModel>.Success(model);
    }

    public async Task<Result<int>> MarkConversationReadAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<int>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var isParticipant = await dbContext.TblConversations
            .AsNoTracking()
            .AnyAsync(c => c.ConversationId == conversationId &&
                            !c.IsDeleted &&
                            (c.UserOneId == currentUserId || c.UserTwoId == currentUserId),
                cancellationToken);

        if (!isParticipant)
        {
            return Result<int>.Failure("Conversation not found.", ResultStatus.NotFound);
        }

        var readAt = DateTime.UtcNow;

        // TblChatMessage carries a rowversion concurrency token, so marking read with tracked
        // entities throws DbUpdateConcurrencyException whenever two clients mark the same
        // conversation read at once (the REST endpoint and the hub both reach this method,
        // and a user may have the thread open in two tabs).
        //
        // A read receipt is idempotent, so losing that race is not an error: it just means
        // there is no unread state left to mark. Re-read and re-apply once, and treat an
        // empty re-read as "nothing to report" instead of surfacing a 500.
        var marked = 0;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var unread = await dbContext.TblChatMessages
                .Where(m => m.ConversationId == conversationId &&
                            m.SenderId != currentUserId &&
                            !m.IsRead &&
                            !m.IsDeleted)
                .ToListAsync(cancellationToken);

            if (unread.Count == 0)
            {
                break;
            }

            foreach (var message in unread)
            {
                message.IsRead = true;
                message.ReadAt = readAt;
                message.UpdatedAt = readAt;
            }

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                marked = unread.Count;
                break;
            }
            catch (DbUpdateConcurrencyException) when (attempt == 0)
            {
                // Drop the stale snapshots so the retry re-reads current rowversions.
                dbContext.ChangeTracker.Clear();
            }
        }

        if (marked == 0)
        {
            return Result<int>.Success(0);
        }

        var conversation = await dbContext.TblConversations
            .AsNoTracking()
            .FirstAsync(c => c.ConversationId == conversationId, cancellationToken);
        var peerId = conversation.UserOneId == currentUserId ? conversation.UserTwoId : conversation.UserOneId;

        try
        {
            await hubContext.Clients.User(peerId.ToString()).SendAsync(
                "MessageRead", conversationId, currentUserId, cancellationToken);
        }
        catch
        {
            // Receipt delivery is best-effort.
        }

        return Result<int>.Success(marked);
    }

    public async Task<int> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return 0;

        var currentUserId = currentUser.UserId.Value;

        var myConversationIds = await dbContext.TblConversations
            .Where(c => (c.UserOneId == currentUserId || c.UserTwoId == currentUserId) && !c.IsDeleted)
            .Select(c => c.ConversationId)
            .ToListAsync(cancellationToken);

        if (!myConversationIds.Any()) return 0;

        return await dbContext.TblChatMessages
            .CountAsync(m => myConversationIds.Contains(m.ConversationId) &&
                             m.SenderId != currentUserId &&
                             !m.IsRead &&
                             !m.IsDeleted, cancellationToken);
    }

    private async Task<Result?> ValidatePrivateChatAccessAsync(int senderId, int recipientId, int? conversationId, CancellationToken cancellationToken)
    {
        var setting = await dbContext.TblCreatorChatSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreatorUserId == recipientId, cancellationToken);

        if (setting is null || !setting.IsPrivateChatEnabled)
        {
            // Free chat allowed
            return null;
        }

        // Check if COMPLETED payment transaction exists
        bool isUnlocked = await dbContext.TblPrivateChatPaymentTransactions
            .AsNoTracking()
            .AnyAsync(t => (t.BuyerUserId == senderId && t.CreatorUserId == recipientId && t.Status == "COMPLETED") ||
                           (conversationId.HasValue && t.ConversationId == conversationId.Value && t.BuyerUserId == senderId && t.Status == "COMPLETED"), cancellationToken);

        if (!isUnlocked)
        {
            return Result.Failure("PRIVATE_CHAT_PAYMENT_REQUIRED", ResultStatus.Forbidden);
        }

        return null;
    }
}