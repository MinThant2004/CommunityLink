namespace CommunityLink.Domain.Features.ChatGroup;

using CommunityLink.Shared;
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
    Task<Result> DeleteMessageAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default);
}
