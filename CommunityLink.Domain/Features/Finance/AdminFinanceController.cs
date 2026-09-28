namespace CommunityLink.Domain.Features.Finance;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/admin/finance")]
[Authorize]
public sealed class AdminFinanceController(
    IAdminFinanceService financeService,
    ICurrentUserContext currentUser) : BaseController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetFinanceSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<AdminFinanceSummaryModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await financeService.GetFinanceSummaryAsync(fromDate, toDate, cancellationToken));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetFinanceTransactions(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? transactionType,
        [FromQuery] string? status,
        [FromQuery] int? chatGroupId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<AdminFinancePagedTransactionModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await financeService.GetFinanceTransactionsAsync(fromDate, toDate, transactionType, status, chatGroupId, page, pageSize, cancellationToken));
    }

    [HttpGet("groups")]
    public async Task<IActionResult> GetFinanceGroupBreakdown(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await financeService.GetFinanceGroupBreakdownAsync(fromDate, toDate, cancellationToken));
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportFinanceReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? transactionType,
        [FromQuery] string? status,
        [FromQuery] int? chatGroupId,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<byte[]>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        var result = await financeService.ExportFinanceReportCsvAsync(fromDate, toDate, transactionType, status, chatGroupId, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToActionResult(result);
        }

        var fileName = $"communitylink-finance-report-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return File(result.Data!, "text/csv", fileName);
    }

    [HttpGet("reconciliation")]
    public async Task<IActionResult> GetFinanceReconciliation(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<AdminFinanceReconciliationModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await financeService.GetFinanceReconciliationAsync(fromDate, toDate, cancellationToken));
    }
}
