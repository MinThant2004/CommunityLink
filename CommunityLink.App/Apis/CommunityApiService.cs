using CommunityLink.Shared;
using CommunityLink.Shared.Features.Community;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class CommunityApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<CommunityModel>>> GetCommunitiesAsync(string? search = null, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CommunityModel>>(string.IsNullOrWhiteSpace(search) ? "api/communities" : $"api/communities?search={Uri.EscapeDataString(search)}", cancellationToken);

    public Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default) =>
        GetAsync<CommunityModel>($"api/communities/{communityId}", cancellationToken);

    public Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<CommunityModel, CreateCommunityRequestModel>("api/communities", request, cancellationToken);

    public Task<Result> JoinCommunityAsync(int communityId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/communities/{communityId}/join", new { }, cancellationToken);
}