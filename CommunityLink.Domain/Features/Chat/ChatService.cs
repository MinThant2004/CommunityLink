using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;

namespace CommunityLink.Domain.Features.Chat;

public interface IChatService
{
    Task<Result<IReadOnlyList<ConversationModel>>> GetConversationsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatMessageModel>>> GetMessagesAsync(int conversationId, CancellationToken cancellationToken = default);
    Task<Result<ChatMessageModel>> SendMessageAsync(SendMessageRequestModel request, CancellationToken cancellationToken = default);
    Task<int> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default);
}

public sealed class ChatService(AppDbContext dbContext, ICurrentUserContext currentUser) : IChatService
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

            if (conversation is null)
            {
                conversation = new TblConversation
                {
                    UserOneId = currentUserId,
                    UserTwoId = targetId,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.TblConversations.Add(conversation);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
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

        return Result<ChatMessageModel>.Success(model);
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
}