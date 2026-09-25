using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Report;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class ReportApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<CommunityReportItemModel>>> GetCommunityReportAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CommunityReportItemModel>>("api/reports/communities", cancellationToken);

    public Task<Result<IReadOnlyList<UserReportItemModel>>> GetUserReportAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<UserReportItemModel>>("api/reports/users", cancellationToken);

    public Task<Result<UserTopContentModel>> GetUserTopContentAsync(int userId, CancellationToken cancellationToken = default) =>
        GetAsync<UserTopContentModel>($"api/reports/users/{userId}/top-content", cancellationToken);
}
