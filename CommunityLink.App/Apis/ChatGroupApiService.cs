using System.Net.Http.Headers;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;
using Microsoft.AspNetCore.Components.Forms;
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

    /// <summary>Pending invites addressed to the current user, priced from the invite snapshot.</summary>
    public Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyInvitationsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupModel>>("api/chat-groups/my-invitations", cancellationToken);

    public Task<Result<IReadOnlyList<ChatGroupInviteModel>>> GetPendingInvitationsAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupInviteModel>>($"api/chat-groups/{chatGroupId}/invitations", cancellationToken);

    public Task<Result> RevokeInvitationAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}/invitations/{targetUserId}", cancellationToken);

    public Task<Result> DeclineInvitationAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/invitations/decline", new { }, cancellationToken);

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

    /// <summary>Null when the group has nothing pinned; a plain "no pin" is not an error.</summary>
    public Task<Result<ChatGroupPinnedMessageModel?>> GetPinnedMessageAsync(
        int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<ChatGroupPinnedMessageModel?>($"api/chat-groups/{chatGroupId}/pinned-message", cancellationToken);

    public Task<Result<ChatGroupPinnedMessageModel>> PinMessageAsync(
        int chatGroupId, int messageId, CancellationToken cancellationToken = default) =>
        PostAsync<ChatGroupPinnedMessageModel, object>(
            $"api/chat-groups/{chatGroupId}/messages/{messageId}/pin", new { }, cancellationToken);

    public Task<Result> UnpinMessageAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}/pinned-message", cancellationToken);

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

    /// <summary>OWNER. Sends the whole permission set so a partial edit cannot be persisted.</summary>
    public Task<Result<ChatGroupPermissionSet>> UpdateMemberPermissionsAsync(
        int chatGroupId, int userId, ChatGroupPermissionSet permissions, CancellationToken cancellationToken = default) =>
        PutAsync<ChatGroupPermissionSet, ChatGroupPermissionSet>(
            $"api/chat-groups/{chatGroupId}/members/{userId}/permissions", permissions, cancellationToken);

    // ---- Creator/Admin group management ----

    public Task<Result<ChatGroupModel>> UpdateInfoAsync(int chatGroupId, UpdateChatGroupRequestModel request, CancellationToken cancellationToken = default) =>
        PutAsync<ChatGroupModel, UpdateChatGroupRequestModel>($"api/chat-groups/{chatGroupId}", request, cancellationToken);

    public Task<Result<ChatGroupModel>> SetJoinFeeAsync(int chatGroupId, long joinFeeLinkDrops, CancellationToken cancellationToken = default) =>
        PutAsync<ChatGroupModel, SetChatGroupJoinFeeRequestModel>(
            $"api/chat-groups/{chatGroupId}/join-fee",
            new SetChatGroupJoinFeeRequestModel(joinFeeLinkDrops),
            cancellationToken);

    public Task<Result> AddMembersAsync(int chatGroupId, IReadOnlyList<int> userIds, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/members",
            new AddChatGroupMembersRequestModel(userIds),
            cancellationToken);

    public Task<Result> BanMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/{chatGroupId}/members/{userId}/ban", new { }, cancellationToken);

    public Task<Result> UnbanMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}/members/{userId}/ban", cancellationToken);

    public Task<Result> DeleteChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}", cancellationToken);

    public Task<Result<IReadOnlyList<ChatGroupBannedMemberModel>>> GetBannedMembersAsync(
        int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupBannedMemberModel>>($"api/chat-groups/{chatGroupId}/bans", cancellationToken);

    /// <summary>
    /// Uploads a group avatar. The policy check is repeated client-side to fail fast, but the
    /// API is the authority: it re-validates the extension, the size and the file's real bytes.
    /// </summary>
    public async Task<Result<ChatGroupImageUploadResponse>> UpdateImageAsync(
        int chatGroupId, IBrowserFile file, CancellationToken cancellationToken = default)
    {
        if (!ChatGroupImagePolicy.IsAllowed(file.Name))
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"Unsupported image type. Allowed: {ChatGroupImagePolicy.AllowedExtensionsDisplay}.",
                ResultStatus.ValidationError);
        }

        if (file.Size > ChatGroupImagePolicy.MaxBytes)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                "Image is too large. The maximum is 5 MB.", ResultStatus.ValidationError);
        }

        try
        {
            using var stream = file.OpenReadStream(ChatGroupImagePolicy.MaxBytes, cancellationToken);
            return await UpdateImageAsync(chatGroupId, stream, file.Name, file.ContentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"The image could not be uploaded: {ex.Message}",
                ResultStatus.SystemError);
        }
    }

    public async Task<Result<ChatGroupImageUploadResponse>> UpdateImageAsync(
        int chatGroupId, byte[] imageBytes, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!ChatGroupImagePolicy.IsAllowed(fileName))
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"Unsupported image type. Allowed: {ChatGroupImagePolicy.AllowedExtensionsDisplay}.",
                ResultStatus.ValidationError);
        }

        if (imageBytes.Length > ChatGroupImagePolicy.MaxBytes)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                "Image is too large. The maximum is 5 MB.", ResultStatus.ValidationError);
        }

        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
            content.Add(fileContent, "file", fileName);

            var client = CreateUploadClient();
            var response = await client.PostAsync($"api/chat-groups/{chatGroupId}/image", content, cancellationToken);
            return await ReadResultAsync<ChatGroupImageUploadResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"The image could not be uploaded: {ex.Message}",
                ResultStatus.SystemError);
        }
    }

    public async Task<Result<ChatGroupImageUploadResponse>> UpdateImageAsync(
        int chatGroupId, Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        if (!ChatGroupImagePolicy.IsAllowed(fileName))
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"Unsupported image type. Allowed: {ChatGroupImagePolicy.AllowedExtensionsDisplay}.",
                ResultStatus.ValidationError);
        }

        try
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(imageStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
            content.Add(fileContent, "file", fileName);

            var client = CreateUploadClient();
            var response = await client.PostAsync($"api/chat-groups/{chatGroupId}/image", content, cancellationToken);
            return await ReadResultAsync<ChatGroupImageUploadResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"The image could not be uploaded: {ex.Message}",
                ResultStatus.SystemError);
        }
    }

    // ---- Shareable invite links ----

    public Task<Result<IReadOnlyList<ChatGroupInviteLinkModel>>> GetInviteLinksAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatGroupInviteLinkModel>>($"api/chat-groups/{chatGroupId}/invite-links", cancellationToken);

    public Task<Result<ChatGroupInviteLinkModel>> GetOrCreatePrimaryInviteLinkAsync(int chatGroupId, CancellationToken cancellationToken = default) =>
        GetAsync<ChatGroupInviteLinkModel>($"api/chat-groups/{chatGroupId}/invite-links/primary", cancellationToken);

    public Task<Result<ChatGroupInviteLinkModel>> CreateInviteLinkAsync(int chatGroupId, CreateInviteLinkRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<ChatGroupInviteLinkModel, CreateInviteLinkRequestModel>($"api/chat-groups/{chatGroupId}/invite-links", request, cancellationToken);

    public Task<Result> RevokeInviteLinkAsync(int chatGroupId, int linkId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat-groups/{chatGroupId}/invite-links/{linkId}", cancellationToken);

    public Task<Result<InviteLinkPreviewModel>> GetInviteLinkPreviewAsync(string token, CancellationToken cancellationToken = default) =>
        GetAsync<InviteLinkPreviewModel>($"api/chat-groups/invite/{Uri.EscapeDataString(token)}/preview", cancellationToken);

    public Task<Result> JoinViaInviteLinkAsync(string token, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat-groups/invite/{Uri.EscapeDataString(token)}/join", new { }, cancellationToken);
}
