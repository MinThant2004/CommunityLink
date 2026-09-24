using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.GroupChat;

namespace CommunityLink.App.Apis;

public sealed class GroupChatApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<GroupChatRoomModel>> CreateGroupChatRoomAsync(int groupId, CreateGroupChatRoomRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<GroupChatRoomModel, CreateGroupChatRoomRequestModel>($"api/groups/{groupId}/chatroom", request, cancellationToken);

    public Task<Result<GroupChatRoomModel>> GetGroupChatRoomAsync(int groupId, CancellationToken cancellationToken = default) =>
        GetAsync<GroupChatRoomModel>($"api/groups/{groupId}/chatroom", cancellationToken);

    public Task<Result<IReadOnlyList<GroupChatMessageModel>>> GetMessagesAsync(int groupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<GroupChatMessageModel>>($"api/groups/{groupId}/chatroom/messages", cancellationToken);

    public Task<Result<GroupChatMessageModel>> SendMessageAsync(int groupId, SendGroupChatMessageRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<GroupChatMessageModel, SendGroupChatMessageRequestModel>($"api/groups/{groupId}/chatroom/messages", request, cancellationToken);
}
