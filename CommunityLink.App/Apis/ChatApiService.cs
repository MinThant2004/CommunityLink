using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using Microsoft.AspNetCore.Http;

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
}