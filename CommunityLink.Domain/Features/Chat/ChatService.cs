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

    /// <summary>Hides the message for the caller only; the other participant still sees it.</summary>
    Task<Result> DeleteMessageForSelfAsync(int conversationId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes the message for both participants. Sender only.</summary>
    Task<Result> DeleteMessageForEveryoneAsync(int conversationId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or clears the caller's single reaction. Sending the emoji already on the message
    /// clears it; sending a different one replaces it. Returns the full reaction list for the
    /// message afterwards so the caller can render authoritative counts.
    /// </summary>
    Task<Result<IReadOnlyList<MessageReactionModel>>> SetReactionAsync(
        int conversationId, int messageId, string? emoji, CancellationToken cancellationToken = default);
}

public sealed class ChatService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    IPrivateChatPaymentService privateChatPaymentService,
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

        var currentUserId = currentUser.UserId.Value;

        // Messages the caller hid for themselves are filtered out here, which is what makes
        // "Delete for myself" a per-viewer view rather than a client-side trick.
        var rows = await dbContext.TblChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId &&
                        !m.IsDeleted &&
                        !dbContext.TblChatMessageUserStates.Any(s =>
                            s.ChatMessageId == m.ChatMessageId && s.UserId == currentUserId && s.IsHidden))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                m.ChatMessageId,
                m.ConversationId,
                m.SenderId,
                SenderName = m.Sender.DisplayName,
                SenderAvatar = m.Sender.AvatarUrl,
                m.MessageText,
                m.IsRead,
                m.CreatedAt,
                m.ReplyToMessageId
            })
            .ToListAsync(cancellationToken);

        var replies = await GetReplyStubsAsync(
            rows.Where(r => r.ReplyToMessageId.HasValue)
                .Select(r => r.ReplyToMessageId!.Value)
                .Distinct()
                .ToList(),
            cancellationToken);

        var reactions = await GetReactionsAsync(
            rows.Select(r => r.ChatMessageId).ToList(),
            cancellationToken);

        var messages = rows.Select(m =>
        {
            var hasReply = replies.TryGetValue(m.ReplyToMessageId ?? -1, out var reply);

            return new ChatMessageModel(
                m.ChatMessageId,
                m.ConversationId,
                m.SenderId,
                m.SenderName,
                m.SenderAvatar,
                m.MessageText,
                m.IsRead,
                m.CreatedAt,
                m.ReplyToMessageId,
                hasReply ? reply.SenderName : null,
                hasReply ? reply.Preview : null,
                hasReply && reply.IsDeleted,
                reactions.TryGetValue(m.ChatMessageId, out var list) ? list : Array.Empty<MessageReactionModel>(),
                m.SenderId == currentUserId);
        }).ToList();

        return Result<IReadOnlyList<ChatMessageModel>>.Success(messages);
    }

    /// <summary>
    /// Resolves the quoted sender and a truncated preview for a set of replied-to message ids.
    /// Soft-deleted originals are returned as stubs so the client can render the tombstone.
    /// </summary>
    private async Task<Dictionary<int, (string SenderName, string Preview, bool IsDeleted)>> GetReplyStubsAsync(
        List<int> messageIds,
        CancellationToken cancellationToken)
    {
        var stubs = new Dictionary<int, (string, string, bool)>();

        if (messageIds.Count == 0)
        {
            return stubs;
        }

        var rows = await dbContext.TblChatMessages
            .AsNoTracking()
            .Where(m => messageIds.Contains(m.ChatMessageId))
            .Select(m => new
            {
                m.ChatMessageId,
                SenderName = m.Sender.DisplayName,
                m.MessageText,
                m.IsDeleted
            })
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            stubs[row.ChatMessageId] = (row.SenderName, TruncatePreview(row.MessageText), row.IsDeleted);
        }

        return stubs;
    }

    /// <summary>Loads the flat reaction rows for a page of messages, keyed by message id.</summary>
    private async Task<Dictionary<int, IReadOnlyList<MessageReactionModel>>> GetReactionsAsync(
        List<int> messageIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, IReadOnlyList<MessageReactionModel>>();

        if (messageIds.Count == 0)
        {
            return result;
        }

        var rows = await dbContext.TblChatMessageReactions
            .AsNoTracking()
            .Where(r => messageIds.Contains(r.ChatMessageId))
            .OrderBy(r => r.CreatedAt)
            .Select(r => new
            {
                r.ChatMessageId,
                r.UserId,
                UserName = r.User.DisplayName,
                r.Emoji
            })
            .ToListAsync(cancellationToken);

        foreach (var group in rows.GroupBy(r => r.ChatMessageId))
        {
            result[group.Key] = group
                .Select(r => new MessageReactionModel(r.UserId, r.UserName, r.Emoji))
                .ToList();
        }

        return result;
    }

    private static string TruncatePreview(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var single = text.ReplaceLineEndings(" ").Trim();
        return single.Length <= 120 ? single : single[..120] + "…";
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

        // A reply must point at a live message in the same conversation. Validating here stops
        // a caller from quoting a message from a different thread they merely know the id of.
        if (request.ReplyToMessageId.HasValue)
        {
            var replyId = request.ReplyToMessageId.Value;
            var isValidReply = await dbContext.TblChatMessages
                .AsNoTracking()
                .AnyAsync(m => m.ChatMessageId == replyId &&
                               m.ConversationId == conversation.ConversationId &&
                               !m.IsDeleted,
                    cancellationToken);

            if (!isValidReply)
            {
                return Result<ChatMessageModel>.Failure("The message you replied to is no longer available.");
            }
        }

        var message = new TblChatMessage
        {
            ConversationId = conversation.ConversationId,
            SenderId = currentUserId,
            MessageText = request.MessageText.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            ReplyToMessageId = request.ReplyToMessageId
        };

        dbContext.TblChatMessages.Add(message);

        conversation.LastMessagePreview = message.MessageText;
        conversation.LastMessageAt = message.CreatedAt;
        conversation.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.TblUsers.FindAsync([currentUserId], cancellationToken);
        var replies = await GetReplyStubsAsync(
            request.ReplyToMessageId.HasValue ? [request.ReplyToMessageId.Value] : [],
            cancellationToken);
        var hasReply = replies.TryGetValue(request.ReplyToMessageId ?? -1, out var replyStub);

        var model = new ChatMessageModel(
            message.ChatMessageId,
            message.ConversationId,
            message.SenderId,
            user?.DisplayName ?? "Unknown",
            user?.AvatarUrl,
            message.MessageText,
            message.IsRead,
            message.CreatedAt,
            message.ReplyToMessageId,
            hasReply ? replyStub.SenderName : null,
            hasReply ? replyStub.Preview : null,
            hasReply && replyStub.IsDeleted,
            Array.Empty<MessageReactionModel>(),
            true);

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

    public async Task<Result> DeleteMessageForSelfAsync(int conversationId, int messageId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var message = await FindVisibleMessageAsync(conversationId, messageId, currentUserId, cancellationToken);
        if (message == null)
        {
            return Result.Failure("Message not found.", ResultStatus.NotFound);
        }

        var state = await dbContext.TblChatMessageUserStates
            .FirstOrDefaultAsync(s => s.ChatMessageId == messageId && s.UserId == currentUserId, cancellationToken);

        if (state is null)
        {
            state = new TblChatMessageUserState
            {
                ChatMessageId = messageId,
                UserId = currentUserId,
                IsHidden = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TblChatMessageUserStates.Add(state);
        }
        else if (state.IsHidden)
        {
            return Result.Success("Message deleted for you.");
        }
        else
        {
            state.IsHidden = true;
            state.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A second "delete for me" from another tab can win the race to insert the unique
            // (message, user) row. The end state is the one we wanted, so this is not an error.
            dbContext.ChangeTracker.Clear();
        }

        // No broadcast: hiding a message is a per-viewer concern and the other participant
        // must keep seeing it.
        return Result.Success("Message deleted for you.");
    }

    public async Task<Result> DeleteMessageForEveryoneAsync(int conversationId, int messageId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var peerId = await GetPeerIdAsync(conversationId, currentUserId, cancellationToken);
        if (peerId is null)
        {
            return Result.Failure("Conversation not found.", ResultStatus.NotFound);
        }

        var message = await FindVisibleMessageAsync(conversationId, messageId, currentUserId, cancellationToken);
        if (message == null)
        {
            return Result.Failure("Message not found.", ResultStatus.NotFound);
        }

        // 1:1 has no moderator, so only the author can retract a message for both sides.
        if (message.SenderId != currentUserId)
        {
            return Result.Failure("You can only delete your own messages.", ResultStatus.Forbidden);
        }

        var deletedAt = DateTime.UtcNow;
        var deleted = false;

        // TblChatMessage is rowversion-stamped, so a delete racing another writer on the same
        // rows throws. The outcome is the same either way, so re-read and retry once rather
        // than surfacing a 500.
        for (var attempt = 0; attempt < 2 && !deleted; attempt++)
        {
            var row = await FindVisibleMessageAsync(conversationId, messageId, currentUserId, cancellationToken);
            if (row is null)
            {
                // Someone else already retracted it, which is the state we wanted.
                deleted = true;
                break;
            }

            row.IsDeleted = true;
            row.DeletedAt = deletedAt;
            row.DeletedBy = currentUserId;
            row.UpdatedAt = deletedAt;

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                deleted = true;
            }
            catch (DbUpdateConcurrencyException) when (attempt == 0)
            {
                dbContext.ChangeTracker.Clear();
            }
        }

        if (!deleted)
        {
            return Result.Failure("Message could not be deleted.", ResultStatus.SystemError);
        }

        // The denormalized preview would otherwise keep advertising deleted content in the
        // thread list until the next message arrives.
        await RefreshConversationPreviewAsync(conversationId, cancellationToken);

        try
        {
            await hubContext.Clients.User(peerId.Value.ToString()).SendAsync(
                "MessageDeleted", conversationId, messageId, cancellationToken);
        }
        catch
        {
            // Peer notification is best-effort; they refresh on next load.
        }

        return Result.Success("Message deleted for everyone.");
    }

    public async Task<Result<IReadOnlyList<MessageReactionModel>>> SetReactionAsync(
        int conversationId, int messageId, string? emoji, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var peerId = await GetPeerIdAsync(conversationId, currentUserId, cancellationToken);
        if (peerId is null)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("Conversation not found.", ResultStatus.NotFound);
        }

        var message = await FindVisibleMessageAsync(conversationId, messageId, currentUserId, cancellationToken);
        if (message == null)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("Message not found.", ResultStatus.NotFound);
        }

        var canonical = MessageEmoji.Normalize(emoji);
        if (canonical is null)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("That reaction is not supported.");
        }

        var existing = await dbContext.TblChatMessageReactions
            .FirstOrDefaultAsync(r => r.ChatMessageId == messageId && r.UserId == currentUserId, cancellationToken);

        if (existing is null)
        {
            dbContext.TblChatMessageReactions.Add(new TblChatMessageReaction
            {
                ChatMessageId = messageId,
                UserId = currentUserId,
                Emoji = canonical,
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (string.Equals(existing.Emoji, canonical, StringComparison.Ordinal))
        {
            // Same emoji again clears the reaction, matching the Telegram toggle.
            dbContext.TblChatMessageReactions.Remove(existing);
        }
        else
        {
            // One reaction per person per message: swap in place instead of adding a row.
            existing.Emoji = canonical;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ChangeTracker.Clear();
        var reactions = (await GetReactionsAsync([messageId], cancellationToken))
            .TryGetValue(messageId, out var list)
            ? list
            : Array.Empty<MessageReactionModel>();

        // Send the resulting state rather than the toggle, so both sides converge on the same
        // counts without refetching the thread.
        try
        {
            await hubContext.Clients.User(peerId.Value.ToString()).SendAsync(
                "MessageReactionUpdated", conversationId, messageId, reactions, cancellationToken);
        }
        catch
        {
            // Best-effort; the next thread load shows the same state.
        }

        return Result<IReadOnlyList<MessageReactionModel>>.Success(reactions);
    }

    /// <summary>Returns the other participant, or null when the caller is not a participant.</summary>
    private async Task<int?> GetPeerIdAsync(int conversationId, int userId, CancellationToken cancellationToken)
    {
        var conversation = await dbContext.TblConversations
            .AsNoTracking()
            .Where(c => c.ConversationId == conversationId && !c.IsDeleted)
            .Select(c => new { c.UserOneId, c.UserTwoId })
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
        {
            return null;
        }

        if (conversation.UserOneId == userId)
        {
            return conversation.UserTwoId;
        }

        return conversation.UserTwoId == userId ? conversation.UserOneId : null;
    }

    /// <summary>
    /// Loads a message the caller is allowed to act on: in their conversation, not deleted for
    /// everyone, and not hidden for themselves.
    /// </summary>
    private async Task<TblChatMessage?> FindVisibleMessageAsync(
        int conversationId, int messageId, int userId, CancellationToken cancellationToken)
    {
        var peerId = await GetPeerIdAsync(conversationId, userId, cancellationToken);
        if (peerId is null)
        {
            return null;
        }

        return await dbContext.TblChatMessages
            .FirstOrDefaultAsync(m => m.ChatMessageId == messageId &&
                                       m.ConversationId == conversationId &&
                                       !m.IsDeleted &&
                                       !dbContext.TblChatMessageUserStates.Any(s =>
                                           s.ChatMessageId == messageId && s.UserId == userId && s.IsHidden),
                cancellationToken);
    }

    /// <summary>
    /// Recomputes the thread-list preview from the newest surviving message, so a retracted
    /// last message stops showing its text in the conversation list.
    /// </summary>
    private async Task RefreshConversationPreviewAsync(int conversationId, CancellationToken cancellationToken)
    {
        var conversation = await dbContext.TblConversations
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellationToken);

        if (conversation is null)
        {
            return;
        }

        var newest = await dbContext.TblChatMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.ChatMessageId)
            .Select(m => new { m.MessageText, m.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        conversation.LastMessagePreview = newest?.MessageText;
        conversation.LastMessageAt = newest?.CreatedAt;
        conversation.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
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