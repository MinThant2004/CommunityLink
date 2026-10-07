using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace CommunityLink.App.Apis;

public sealed class ChatApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<ConversationModel>>> GetConversationsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ConversationModel>>("api/chat/conversations", cancellationToken);

    public Task<Result<IReadOnlyList<ChatMessageModel>>> GetMessagesAsync(int conversationId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<ChatMessageModel>>($"api/chat/conversations/{conversationId}/messages", cancellationToken);

    public Task<Result<ChatMessageModel>> SendMessageAsync(SendMessageRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<ChatMessageModel, SendMessageRequestModel>("api/chat/messages", request, cancellationToken);

    /// <summary>
    /// Uploads an attachment and returns the public URL to send along with the message.
    /// The file is streamed from the browser file handle, so nothing is buffered whole in
    /// memory on the way out.
    /// </summary>
    public async Task<Result<ChatAttachmentUploadResponse>> UploadAttachmentAsync(
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Size > ChatAttachmentPolicy.MaxBytes)
        {
            return Result<ChatAttachmentUploadResponse>.Failure(
                $"File is too large. The maximum is 25 MB.",
                ResultStatus.ValidationError);
        }

        if (!ChatAttachmentPolicy.IsAllowed(file.Name))
        {
            return Result<ChatAttachmentUploadResponse>.Failure(
                $"Unsupported file type. Allowed: {ChatAttachmentPolicy.AllowedExtensionsDisplay}.",
                ResultStatus.ValidationError);
        }

        try
        {
            using var stream = file.OpenReadStream(ChatAttachmentPolicy.MaxBytes, cancellationToken);
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var client = CreateUploadClient();
            var response = await client.PostAsync("api/chat/attachments", content, cancellationToken);
            return await ReadResultAsync<ChatAttachmentUploadResponse>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<ChatAttachmentUploadResponse>.Failure(
                $"The file could not be uploaded: {ex.Message}",
                ResultStatus.SystemError);
        }
    }

    public Task<Result<int>> MarkConversationReadAsync(int conversationId, CancellationToken cancellationToken = default) =>
        PostAsync<int, object>($"api/chat/conversations/{conversationId}/read", new { }, cancellationToken);

    public Task<Result<PrivateChatStatusModel>> GetPrivateChatStatusAsync(int creatorUserId, CancellationToken cancellationToken = default) =>
        GetAsync<PrivateChatStatusModel>($"api/chat/status/{creatorUserId}", cancellationToken);

    public Task<Result<int>> UnlockPrivateChatAsync(int creatorUserId, CancellationToken cancellationToken = default) =>
        PostAsync<int, object>($"api/chat/unlock/{creatorUserId}", new { }, cancellationToken);

    public Task<Result> DeleteMessageForEveryoneAsync(int conversationId, int messageId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat/conversations/{conversationId}/messages/{messageId}", cancellationToken);

    public Task<Result> DeleteMessageForSelfAsync(int conversationId, int messageId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat/conversations/{conversationId}/messages/{messageId}/hide", new { }, cancellationToken);

    public Task<Result<IReadOnlyList<MessageReactionModel>>> SetMessageReactionAsync(int conversationId, int messageId, string? emoji, CancellationToken cancellationToken = default) =>
        PostAsync<IReadOnlyList<MessageReactionModel>, SetMessageReactionRequestModel>(
            $"api/chat/conversations/{conversationId}/messages/{messageId}/reaction",
            new SetMessageReactionRequestModel(emoji),
            cancellationToken);

    public Task<Result<CreatorChatSettingModel>> GetCreatorChatSettingsAsync(int? creatorUserId = null, CancellationToken cancellationToken = default) =>
        creatorUserId.HasValue
            ? GetAsync<CreatorChatSettingModel>($"api/creator/chat/settings/{creatorUserId.Value}", cancellationToken)
            : GetAsync<CreatorChatSettingModel>("api/creator/chat/settings", cancellationToken);

    public Task<Result<CreatorChatSettingModel>> SaveCreatorChatSettingsAsync(SaveCreatorChatSettingRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<CreatorChatSettingModel, SaveCreatorChatSettingRequestModel>("api/creator/chat/settings", request, cancellationToken);

    public Task<Result> DeleteConversationForSelfAsync(int conversationId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat/conversations/{conversationId}", cancellationToken);

    public Task<Result> BlockUserAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/chat/block/{targetUserId}", new { }, cancellationToken);

    public Task<Result> UnblockUserAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/chat/block/{targetUserId}", cancellationToken);

    public Task<Result<UserBlockStatusModel>> GetUserBlockStatusAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        GetAsync<UserBlockStatusModel>($"api/chat/block/status/{targetUserId}", cancellationToken);

    public Task<Result<IReadOnlyList<int>>> GetBlockedUserIdsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<int>>("api/chat/block/list", cancellationToken);

    public Task<Result<SharedMediaCountsModel>> GetSharedMediaCountsAsync(int conversationId, CancellationToken cancellationToken = default) =>
        GetAsync<SharedMediaCountsModel>($"api/chat/conversations/{conversationId}/media/counts", cancellationToken);

    public Task<Result<SharedMediaPagedResultModel>> GetSharedMediaAsync(int conversationId, string category, int page = 1, int pageSize = 30, CancellationToken cancellationToken = default) =>
        GetAsync<SharedMediaPagedResultModel>($"api/chat/conversations/{conversationId}/media/{category}?page={page}&pageSize={pageSize}", cancellationToken);
}