using CommunityLink.Shared;
using CommunityLink.Shared.Features.Group;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class GroupApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<GroupModel>>> GetGroupsAsync(int? subCommunityId = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (subCommunityId.HasValue && subCommunityId.Value > 0) queryParams.Add($"subCommunityId={subCommunityId.Value}");
        if (!string.IsNullOrWhiteSpace(search)) queryParams.Add($"search={Uri.EscapeDataString(search.Trim())}");

        var url = queryParams.Count > 0 ? $"api/groups?{string.Join("&", queryParams)}" : "api/groups";
        return GetAsync<IReadOnlyList<GroupModel>>(url, cancellationToken);
    }

    public Task<Result<GroupModel>> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default) =>
        GetAsync<GroupModel>($"api/groups/{groupId}", cancellationToken);

    public Task<Result<GroupModel>> CreateGroupAsync(CreateGroupRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<GroupModel, CreateGroupRequestModel>("api/groups", request, cancellationToken);

    public Task<Result> JoinGroupAsync(int groupId, string? requestNote = null, CancellationToken cancellationToken = default) =>
        PostAsync($"api/groups/{groupId}/join", requestNote ?? "", cancellationToken);

    public Task<Result> LeaveGroupAsync(int groupId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/groups/{groupId}/leave", new { }, cancellationToken);

    public Task<Result<IReadOnlyList<GroupMemberModel>>> GetGroupMembersAsync(int groupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<GroupMemberModel>>($"api/groups/{groupId}/members", cancellationToken);

    public Task<Result<IReadOnlyList<GroupJoinRequestModel>>> GetGroupJoinRequestsAsync(int groupId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<GroupJoinRequestModel>>($"api/groups/{groupId}/join-requests", cancellationToken);

    public Task<Result> ApproveJoinRequestAsync(int requestId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/groups/join-requests/{requestId}/approve", new { }, cancellationToken);

    public Task<Result> RejectJoinRequestAsync(int requestId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/groups/join-requests/{requestId}/reject", new { }, cancellationToken);
}
