using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class ChatGroupApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<ChatGroupModel>>> GetChatGroupsAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/chat-groups"
            : $"api/chat-groups?search={Uri.EscapeDataString(search.Trim())}";
        return GetAsync<IReadOnlyList<ChatGroupModel>>(url, cancellationToken);
    }

    public Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyChatGroupsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupModel>>("api/chat-groups/my", cancellationToken);

    public Task<Result<ChatGroupModel>> GetChatGroupByIdAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<ChatGroupModel>($"api/chat-groups/{chatGroupId}", cancellationToken);

    public Task<Result<ChatGroupModel>> CreateChatGroupAsync(CreateChatGroupRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<ChatGroupModel, CreateChatGroupRequestModel>("api/chat-groups", request, cancellationToken);

    public Task<Result> JoinChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/join", new { }, cancellationToken);

    public Task<Result> JoinPaidChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/join-paid", new { }, cancellationToken);

    public Task<Result> LeaveChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/leave", new { }, cancellationToken);

    public Task<Result<IReadOnlyList<ChatGroupMemberModel>>> GetMembersAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupMemberModel>>($"api/chat-groups/{chatGroupId}/members", cancellationToken);

    public Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyMembershipsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupModel>>("api/chat-groups/my-memberships", cancellationToken);

    public Task<Result<IReadOnlyList<ChatGroupPreviewModel>>> GetPreviewsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupPreviewModel>>("api/chat-groups/preview", cancellationToken);

    public Task<Result<IReadOnlyList<ChatGroupMessageModel>>> GetMessagesAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupMessageModel>>($"api/chat-groups/{chatGroupId}/messages", cancellationToken);

    public Task<Result<ChatGroupMessageModel>> SendMessageAsync(int chatGroupId, SendChatGroupMessageRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<ChatGroupMessageModel, SendChatGroupMessageRequestModel>($"api/chat-groups/{chatGroupId}/messages", request, cancellationToken);

    public Task<Result> DeleteMessageAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}/messages/{messageId}", cancellationToken);

    public Task<Result> HideMessageForSelfAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/messages/{messageId}/hide", new { }, cancellationToken);

    public Task<Result<IReadOnlyList<MessageReactionModel>>> SetMessageReactionAsync(int chatGroupId, int messageId, string? emoji, CancellationToken cancellationToken = default) =>
        PostAsync<IReadOnlyList<MessageReactionModel>, SetMessageReactionRequestModel>(
            $"api/chat-groups/{chatGroupId}/messages/{messageId}/reaction",
            new SetMessageReactionRequestModel(emoji),
            cancellationToken);

    public Task<Result> ToggleMuteAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/mute", new { }, cancellationToken);

    public Task<Result> PromoteMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/members/{userId}/promote", new { }, cancellationToken);

    public Task<Result> DemoteMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/members/{userId}/demote", new { }, cancellationToken);

    public Task<Result> RemoveMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}/members/{userId}", cancellationToken);
}
