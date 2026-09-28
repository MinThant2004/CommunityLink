namespace CommunityLink.Domain.Features.ChatGroup;

using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

public interface IChatGroupService
{
    Task<Result<ChatGroupModel>> CreateChatGroupAsync(CreateChatGroupRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetChatGroupsAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyChatGroupsAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatGroupModel>> GetChatGroupByIdAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result> JoinChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result> JoinPaidChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result> LeaveChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupMemberModel>>> GetMembersAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyMembershipsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupMessageModel>>> GetMessagesAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result<ChatGroupMessageModel>> SendMessageAsync(int chatGroupId, SendChatGroupMessageRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes for all members. Sender, or a group OWNER/ADMIN as moderator.</summary>
    Task<Result> DeleteMessageAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>Hides the message for the caller only; the rest of the group still sees it.</summary>
    Task<Result> DeleteMessageForSelfAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>Sets or clears the caller's single reaction; returns the resulting list.</summary>
    Task<Result<IReadOnlyList<MessageReactionModel>>> SetReactionAsync(
        int chatGroupId, int messageId, string? emoji, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ChatGroupPreviewModel>>> GetPreviewsAsync(CancellationToken cancellationToken = default);
}
