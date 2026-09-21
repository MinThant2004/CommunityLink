using CommunityLink.Shared;
using CommunityLink.Shared.Features.Poll;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class PollApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId = null, int? groupId = null, CancellationToken cancellationToken = default)
    {
        var url = groupId.HasValue ? $"api/polls?groupId={groupId.Value}" : (communityId.HasValue ? $"api/polls?communityId={communityId.Value}" : "api/polls");
        return GetAsync<IReadOnlyList<PollModel>>(url, cancellationToken);
    }

    public Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<PollModel, CreatePollRequestModel>("api/polls", request, cancellationToken);

    public Task<Result<PollModel>> VoteAsync(VoteRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<PollModel, VoteRequestModel>("api/polls/vote", request, cancellationToken);
}