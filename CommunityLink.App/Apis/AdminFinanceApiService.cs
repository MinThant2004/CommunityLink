namespace CommunityLink.App.Apis;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Finance;
using Microsoft.AspNetCore.Http;

public sealed class AdminFinanceApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<AdminFinanceSummaryModel>> GetSummaryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
        if (toDate.HasValue) queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");

        var url = "api/admin/finance/summary";
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        return GetAsync<AdminFinanceSummaryModel>(url, cancellationToken);
    }

    public Task<Result<AdminFinancePagedTransactionModel>> GetTransactionsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionType = null,
        string? status = null,
        int? chatGroupId = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };
        if (fromDate.HasValue) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
        if (toDate.HasValue) queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");
        if (!string.IsNullOrWhiteSpace(transactionType)) queryParams.Add($"transactionType={Uri.EscapeDataString(transactionType.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (chatGroupId.HasValue && chatGroupId.Value > 0) queryParams.Add($"chatGroupId={chatGroupId.Value}");

        var url = "api/admin/finance/transactions?" + string.Join("&", queryParams);

        return GetAsync<AdminFinancePagedTransactionModel>(url, cancellationToken);
    }

    public Task<Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>> GetGroupsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
        if (toDate.HasValue) queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");

        var url = "api/admin/finance/groups";
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        return GetAsync<IReadOnlyList<AdminFinanceGroupBreakdownModel>>(url, cancellationToken);
    }

    public async Task<Result<byte[]>> GetExportCsvBytesAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionType = null,
        string? status = null,
        int? chatGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
        if (toDate.HasValue) queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");
        if (!string.IsNullOrWhiteSpace(transactionType)) queryParams.Add($"transactionType={Uri.EscapeDataString(transactionType.Trim())}");
        if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status.Trim())}");
        if (chatGroupId.HasValue && chatGroupId.Value > 0) queryParams.Add($"chatGroupId={chatGroupId.Value}");

        var url = "api/admin/finance/export";
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        var client = CreateClient();
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.Forbidden)
                    return Result<byte[]>.Failure("Administrator privileges required.", ResultStatus.Forbidden);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return Result<byte[]>.Failure("User is not authenticated.", ResultStatus.Unauthorized);

                return Result<byte[]>.Failure($"Export failed with status {response.StatusCode}.", ResultStatus.SystemError);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Failure($"Export failed: {ex.Message}", ResultStatus.SystemError);
        }
    }

    public Task<Result<AdminFinanceReconciliationModel>> GetReconciliationAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (fromDate.HasValue) queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
        if (toDate.HasValue) queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");

        var url = "api/admin/finance/reconciliation";
        if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

        return GetAsync<AdminFinanceReconciliationModel>(url, cancellationToken);
    }
}
