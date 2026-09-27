namespace CommunityLink.Domain.Features.Finance;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Finance;

public interface IAdminFinanceService
{
    Task<Result<AdminFinanceSummaryModel>> GetFinanceSummaryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<Result<AdminFinancePagedTransactionModel>> GetFinanceTransactionsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionType = null,
        string? status = null,
        int? chatGroupId = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>> GetFinanceGroupBreakdownAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<Result<byte[]>> ExportFinanceReportCsvAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionType = null,
        string? status = null,
        int? chatGroupId = null,
        CancellationToken cancellationToken = default);

    Task<Result<AdminFinanceReconciliationModel>> GetFinanceReconciliationAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
