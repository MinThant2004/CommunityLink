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

    public Task<Result<PagedResult<DirectoryPersonModel>>> GetPeopleDirectoryAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? roleFilter = null,
        string? sortOption = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }
        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            query.Add($"roleFilter={Uri.EscapeDataString(roleFilter.Trim())}");
        }
        if (!string.IsNullOrWhiteSpace(sortOption))
        {
            query.Add($"sortOption={Uri.EscapeDataString(sortOption.Trim())}");
        }
        var url = $"api/user/people?{string.Join("&", query)}";
        return GetAsync<PagedResult<DirectoryPersonModel>>(url, cancellationToken);
    }
}
