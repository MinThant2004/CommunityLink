using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

    public Task<Result<AdminDashboardModel>> GetDashboardAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AdminDashboardModel>("api/admin/dashboard", cancellationToken);

    public Task<Result<IReadOnlyList<UserInfoModel>>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<UserInfoModel>>("api/admin/users", cancellationToken);

    public Task<Result<PagedResult<UserInfoModel>>> GetUsersPageAsync(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/admin/users/page?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={WebUtility.UrlEncode(search)}";
        return GetAsync<PagedResult<UserInfoModel>>(url, cancellationToken);
    }

    public Task<Result<IReadOnlyList<AuditLogModel>>> GetAuditLogsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<AuditLogModel>>("api/admin/audits", cancellationToken);

    public Task<Result<PagedResult<AuditLogModel>>> GetAuditLogsPageAsync(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/admin/audits/page?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={WebUtility.UrlEncode(search)}";
        return GetAsync<PagedResult<AuditLogModel>>(url, cancellationToken);
    }

    public Task<Result<IReadOnlyList<JoinRequestModel>>> GetPendingJoinRequestsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<JoinRequestModel>>("api/admin/join-requests", cancellationToken);

    public Task<Result> ApproveJoinRequestAsync(int id, CancellationToken cancellationToken = default) =>
        PostAsync($"api/admin/join-requests/{id}/approve", new { }, cancellationToken);

    public Task<Result> RejectJoinRequestAsync(int id, CancellationToken cancellationToken = default) =>
        PostAsync($"api/admin/join-requests/{id}/reject", new { }, cancellationToken);
}