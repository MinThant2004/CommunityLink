using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Activity;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class UserActivityApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<UserActivityModel>>> GetMyActivitiesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<UserActivityModel>>("api/activity", cancellationToken);

    public Task<Result> DeleteActivityAsync(long activityId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/activity/{activityId}", cancellationToken);
}
