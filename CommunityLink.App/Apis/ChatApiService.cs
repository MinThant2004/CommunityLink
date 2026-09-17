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
}