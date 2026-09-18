using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Dashboard;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class UserDashboardApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<UserDashboardModel>> GetUserDashboardAsync(CancellationToken cancellationToken = default) =>
        GetAsync<UserDashboardModel>("api/user/dashboard", cancellationToken);
}
