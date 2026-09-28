namespace CommunityLink.App.Apis;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Creator;
using Microsoft.AspNetCore.Http;

public sealed class CreatorEarningsApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<CreatorEarningsDashboardModel>> GetEarningsAsync(
        string? filterType = null,
        int? chatGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(filterType))
        {
            queryParams.Add($"filterType={Uri.EscapeDataString(filterType.Trim())}");
        }
        if (chatGroupId.HasValue && chatGroupId.Value > 0)
        {
            queryParams.Add($"chatGroupId={chatGroupId.Value}");
        }

        var url = queryParams.Count > 0
            ? $"api/creator/earnings?{string.Join("&", queryParams)}"
            : "api/creator/earnings";

        return GetAsync<CreatorEarningsDashboardModel>(url, cancellationToken);
    }
}
