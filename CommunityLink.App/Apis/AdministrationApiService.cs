using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class AdministrationApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<AdminDashboardStatsModel>> GetStatsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AdminDashboardStatsModel>("api/admin/stats", cancellationToken);

    public Task<Result<IReadOnlyList<UserInfoModel>>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<UserInfoModel>>("api/admin/users", cancellationToken);

    public Task<Result<IReadOnlyList<AuditLogModel>>> GetAuditLogsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<AuditLogModel>>("api/admin/audit-logs", cancellationToken);
}