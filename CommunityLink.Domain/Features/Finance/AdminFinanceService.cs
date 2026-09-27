namespace CommunityLink.Domain.Features.Finance;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Finance;
using Microsoft.EntityFrameworkCore;

public sealed class AdminFinanceService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser) : IAdminFinanceService
{
    public async Task<Result<AdminFinanceSummaryModel>> GetFinanceSummaryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<AdminFinanceSummaryModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (!currentUser.IsAdmin)
        {
            return Result<AdminFinanceSummaryModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden);
        }

        var (effectiveFrom, effectiveTo) = NormalizeDateRange(fromDate, toDate);

        // 1. Query Chat Group Revenue Transactions (Status = COMPLETED)
        var groupTxQuery = dbContext.TblChatGroupPaymentTransactions
            .AsNoTracking()
            .Where(t => t.Status.ToUpper() == "COMPLETED");

        if (effectiveFrom.HasValue) groupTxQuery = groupTxQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) groupTxQuery = groupTxQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

        long groupGross = await groupTxQuery.SumAsync(t => (long?)t.GrossAmount, cancellationToken) ?? 0;
        long groupCommission = await groupTxQuery.SumAsync(t => (long?)t.CommissionAmount, cancellationToken) ?? 0;
        long groupNet = await groupTxQuery.SumAsync(t => (long?)t.NetAmount, cancellationToken) ?? 0;
        int groupCount = await groupTxQuery.CountAsync(cancellationToken);

        // 2. Query Private Chat Revenue Transactions (Status = COMPLETED)
        var privateTxQuery = dbContext.TblPrivateChatPaymentTransactions
            .AsNoTracking()
            .Where(t => t.Status.ToUpper() == "COMPLETED");

        if (effectiveFrom.HasValue) privateTxQuery = privateTxQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) privateTxQuery = privateTxQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

        long privateGross = await privateTxQuery.SumAsync(t => (long?)t.GrossAmountLinkDrops, cancellationToken) ?? 0;
        long privateCommission = await privateTxQuery.SumAsync(t => (long?)t.CommissionAmount, cancellationToken) ?? 0;
        long privateNet = await privateTxQuery.SumAsync(t => (long?)t.CreatorAmount, cancellationToken) ?? 0;
        int privateCount = await privateTxQuery.CountAsync(cancellationToken);

        long grossRevenue = groupGross + privateGross;
        long commission = groupCommission + privateCommission;
        long netEarnings = groupNet + privateNet;
        int grossCount = groupCount + privateCount;

        // 3. Query Creator Payout Requests
        var payoutQuery = dbContext.TblCreatorPayoutRequests
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (effectiveFrom.HasValue) payoutQuery = payoutQuery.Where(p => p.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) payoutQuery = payoutQuery.Where(p => p.CreatedAt <= effectiveTo.Value);

        var completedPayoutsQuery = payoutQuery.Where(p => p.Status.ToUpper() == "COMPLETED" || p.Status.ToUpper() == "APPROVED");
        long completedPayoutsAmount = await completedPayoutsQuery.SumAsync(p => (long?)p.AmountLinkDrops, cancellationToken) ?? 0;
        int completedPayoutsCount = await completedPayoutsQuery.CountAsync(cancellationToken);

        var pendingPayoutsQuery = payoutQuery.Where(p => p.Status.ToUpper() == "PENDING");
        long pendingPayoutsAmount = await pendingPayoutsQuery.SumAsync(p => (long?)p.AmountLinkDrops, cancellationToken) ?? 0;
        int pendingPayoutsCount = await pendingPayoutsQuery.CountAsync(cancellationToken);

        var summary = new AdminFinanceSummaryModel(
            GrossChatGroupRevenueLinkDrops: grossRevenue,
            PlatformCommissionLinkDrops: commission,
            CreatorNetEarningsLinkDrops: netEarnings,
            CompletedCreatorPayoutsLinkDrops: completedPayoutsAmount,
            PendingCreatorPayoutsLinkDrops: pendingPayoutsAmount,
            GrossTransactionCount: grossCount,
            CompletedPayoutCount: completedPayoutsCount,
            PendingPayoutCount: pendingPayoutsCount
        );

        return Result<AdminFinanceSummaryModel>.Success(summary);
    }

    public async Task<Result<AdminFinancePagedTransactionModel>> GetFinanceTransactionsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionType = null,
        string? status = null,
        int? chatGroupId = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<AdminFinancePagedTransactionModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (!currentUser.IsAdmin)
        {
            return Result<AdminFinancePagedTransactionModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden);
        }

        int effectivePage = page < 1 ? 1 : page;
        int effectivePageSize = pageSize < 1 ? 25 : (pageSize > 100 ? 100 : pageSize);

        var (effectiveFrom, effectiveTo) = NormalizeDateRange(fromDate, toDate);
        var resultList = new List<AdminFinanceTransactionModel>();

        bool includeChatGroupTxs = string.IsNullOrWhiteSpace(transactionType)
            || string.Equals(transactionType.Trim(), "CHAT_GROUP_JOIN", StringComparison.OrdinalIgnoreCase);

        bool includePrivateChatTxs = string.IsNullOrWhiteSpace(transactionType)
            || string.Equals(transactionType.Trim(), "PRIVATE_CHAT_UNLOCK", StringComparison.OrdinalIgnoreCase);

        bool includePayoutTxs = string.IsNullOrWhiteSpace(transactionType)
            || string.Equals(transactionType.Trim(), "CREATOR_PAYOUT", StringComparison.OrdinalIgnoreCase);

        // 1. Chat Group Txs
        if (includeChatGroupTxs)
        {
            var txQuery = dbContext.TblChatGroupPaymentTransactions
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.ChatGroup)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                txQuery = txQuery.Where(t => t.Status.ToUpper() == status.Trim().ToUpper());
            }

            if (chatGroupId.HasValue && chatGroupId.Value > 0)
            {
                txQuery = txQuery.Where(t => t.ChatGroupId == chatGroupId.Value);
            }

            if (effectiveFrom.HasValue) txQuery = txQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
            if (effectiveTo.HasValue) txQuery = txQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

            var groupTxs = await txQuery.ToListAsync(cancellationToken);

            resultList.AddRange(groupTxs.Select(t => new AdminFinanceTransactionModel(
                TransactionId: t.PaymentTransactionId,
                Date: t.CreatedAt,
                TransactionType: "CHAT_GROUP_JOIN",
                CreatorOrUserName: t.User?.DisplayName ?? t.User?.UserName ?? $"User #{t.UserId}",
                ChatGroupName: t.ChatGroup?.Name ?? $"Group #{t.ChatGroupId}",
                GrossAmountLinkDrops: t.GrossAmount,
                CommissionAmountLinkDrops: t.CommissionAmount,
                NetAmountLinkDrops: t.NetAmount,
                Status: t.Status
            )));
        }

        // 2. Private Chat Txs
        if (includePrivateChatTxs && (!chatGroupId.HasValue || chatGroupId.Value <= 0))
        {
            var privateQuery = dbContext.TblPrivateChatPaymentTransactions
                .AsNoTracking()
                .Include(pt => pt.BuyerUser)
                .Include(pt => pt.CreatorUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                privateQuery = privateQuery.Where(t => t.Status.ToUpper() == status.Trim().ToUpper());
            }

            if (effectiveFrom.HasValue) privateQuery = privateQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
            if (effectiveTo.HasValue) privateQuery = privateQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

            var privateTxs = await privateQuery.ToListAsync(cancellationToken);

            resultList.AddRange(privateTxs.Select(pt => new AdminFinanceTransactionModel(
                TransactionId: pt.PrivateChatPaymentTransactionId,
                Date: pt.CreatedAt,
                TransactionType: "PRIVATE_CHAT_UNLOCK",
                CreatorOrUserName: pt.BuyerUser?.DisplayName ?? pt.BuyerUser?.UserName ?? $"Buyer #{pt.BuyerUserId}",
                ChatGroupName: "Private Chat Unlock",
                GrossAmountLinkDrops: pt.GrossAmountLinkDrops,
                CommissionAmountLinkDrops: pt.CommissionAmount,
                NetAmountLinkDrops: pt.CreatorAmount,
                Status: pt.Status
            )));
        }

        // 3. Creator Payout Transactions
        if (includePayoutTxs && (!chatGroupId.HasValue || chatGroupId.Value <= 0))
        {
            var payoutQuery = dbContext.TblCreatorPayoutRequests
                .AsNoTracking()
                .Include(p => p.CreatorUser)
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(status))
            {
                payoutQuery = payoutQuery.Where(p => p.Status.ToUpper() == status.Trim().ToUpper());
            }

            if (effectiveFrom.HasValue) payoutQuery = payoutQuery.Where(p => p.CreatedAt >= effectiveFrom.Value);
            if (effectiveTo.HasValue) payoutQuery = payoutQuery.Where(p => p.CreatedAt <= effectiveTo.Value);

            var payouts = await payoutQuery.ToListAsync(cancellationToken);

            resultList.AddRange(payouts.Select(p => new AdminFinanceTransactionModel(
                TransactionId: p.CreatorPayoutRequestId,
                Date: p.CreatedAt,
                TransactionType: "CREATOR_PAYOUT",
                CreatorOrUserName: p.CreatorUser?.DisplayName ?? p.CreatorUser?.UserName ?? $"Creator #{p.CreatorUserId}",
                ChatGroupName: null,
                GrossAmountLinkDrops: p.AmountLinkDrops,
                CommissionAmountLinkDrops: 0,
                NetAmountLinkDrops: p.AmountLinkDrops,
                Status: p.Status
            )));
        }

        var sorted = resultList
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.TransactionId)
            .ToList();

        int totalCount = sorted.Count;
        int totalPages = effectivePageSize > 0 ? (int)Math.Ceiling(totalCount / (double)effectivePageSize) : 0;

        var pagedItems = sorted
            .Skip((effectivePage - 1) * effectivePageSize)
            .Take(effectivePageSize)
            .ToList();

        var pagedResult = new AdminFinancePagedTransactionModel(
            Items: pagedItems,
            Page: effectivePage,
            PageSize: effectivePageSize,
            TotalCount: totalCount,
            TotalPages: totalPages
        );

        return Result<AdminFinancePagedTransactionModel>.Success(pagedResult);
    }

    public async Task<Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>> GetFinanceGroupBreakdownAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (!currentUser.IsAdmin)
        {
            return Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>.Failure("Administrator privileges required.", ResultStatus.Forbidden);
        }

        var (effectiveFrom, effectiveTo) = NormalizeDateRange(fromDate, toDate);

        var txQuery = dbContext.TblChatGroupPaymentTransactions
            .AsNoTracking()
            .Include(t => t.ChatGroup)
            .Where(t => t.Status.ToUpper() == "COMPLETED");

        if (effectiveFrom.HasValue) txQuery = txQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) txQuery = txQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

        var groupList = await txQuery.ToListAsync(cancellationToken);

        var breakdownList = groupList
            .GroupBy(t => t.ChatGroupId)
            .Select(g => new AdminFinanceGroupBreakdownModel(
                ChatGroupId: g.Key,
                ChatGroupName: g.First().ChatGroup?.Name ?? $"Group #{g.Key}",
                GrossRevenueLinkDrops: g.Sum(t => t.GrossAmount),
                PlatformCommissionLinkDrops: g.Sum(t => t.CommissionAmount),
                CreatorNetEarningsLinkDrops: g.Sum(t => t.NetAmount),
                CompletedTransactionCount: g.Count()
            ))
            .OrderBy(g => g.ChatGroupId)
            .ToList();

        return Result<IReadOnlyList<AdminFinanceGroupBreakdownModel>>.Success(breakdownList);
    }

    public async Task<Result<byte[]>> ExportFinanceReportCsvAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? transactionType = null,
        string? status = null,
        int? chatGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var pagedRes = await GetFinanceTransactionsAsync(
            fromDate, toDate, transactionType, status, chatGroupId, page: 1, pageSize: 100000, cancellationToken);

        if (!pagedRes.IsSuccess || pagedRes.Data == null)
        {
            return Result<byte[]>.Failure(pagedRes.Message ?? "Failed to export report.", pagedRes.Status);
        }

        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Date,Transaction Type,User / Creator,Chat Group,Gross Amount,Commission Amount,Net Amount,Status");

        foreach (var tx in pagedRes.Data.Items)
        {
            builder.AppendLine(string.Join(",",
                EscapeCsvField(tx.Date.ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsvField(tx.TransactionType),
                EscapeCsvField(tx.CreatorOrUserName),
                EscapeCsvField(tx.ChatGroupName ?? "-"),
                tx.GrossAmountLinkDrops,
                tx.CommissionAmountLinkDrops,
                tx.NetAmountLinkDrops,
                EscapeCsvField(tx.Status)
            ));
        }

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
        return Result<byte[]>.Success(bytes);
    }

    public async Task<Result<AdminFinanceReconciliationModel>> GetFinanceReconciliationAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<AdminFinanceReconciliationModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (!currentUser.IsAdmin)
        {
            return Result<AdminFinanceReconciliationModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden);
        }

        var (effectiveFrom, effectiveTo) = NormalizeDateRange(fromDate, toDate);

        var groupTxQuery = dbContext.TblChatGroupPaymentTransactions
            .AsNoTracking()
            .Where(t => t.Status.ToUpper() == "COMPLETED");

        if (effectiveFrom.HasValue) groupTxQuery = groupTxQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) groupTxQuery = groupTxQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

        long groupGross = await groupTxQuery.SumAsync(t => (long?)t.GrossAmount, cancellationToken) ?? 0;
        long groupCommission = await groupTxQuery.SumAsync(t => (long?)t.CommissionAmount, cancellationToken) ?? 0;
        long groupNet = await groupTxQuery.SumAsync(t => (long?)t.NetAmount, cancellationToken) ?? 0;

        var privateTxQuery = dbContext.TblPrivateChatPaymentTransactions
            .AsNoTracking()
            .Where(t => t.Status.ToUpper() == "COMPLETED");

        if (effectiveFrom.HasValue) privateTxQuery = privateTxQuery.Where(t => t.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) privateTxQuery = privateTxQuery.Where(t => t.CreatedAt <= effectiveTo.Value);

        long privateGross = await privateTxQuery.SumAsync(t => (long?)t.GrossAmountLinkDrops, cancellationToken) ?? 0;
        long privateCommission = await privateTxQuery.SumAsync(t => (long?)t.CommissionAmount, cancellationToken) ?? 0;
        long privateNet = await privateTxQuery.SumAsync(t => (long?)t.CreatorAmount, cancellationToken) ?? 0;

        long grossRevenue = groupGross + privateGross;
        long commission = groupCommission + privateCommission;
        long creatorNet = groupNet + privateNet;

        var payoutQuery = dbContext.TblCreatorPayoutRequests
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (effectiveFrom.HasValue) payoutQuery = payoutQuery.Where(p => p.CreatedAt >= effectiveFrom.Value);
        if (effectiveTo.HasValue) payoutQuery = payoutQuery.Where(p => p.CreatedAt <= effectiveTo.Value);

        var completedPayoutsQuery = payoutQuery.Where(p => p.Status.ToUpper() == "COMPLETED" || p.Status.ToUpper() == "APPROVED");
        long completedPayouts = await completedPayoutsQuery.SumAsync(p => (long?)p.AmountLinkDrops, cancellationToken) ?? 0;

        var pendingPayoutsQuery = payoutQuery.Where(p => p.Status.ToUpper() == "PENDING");
        long pendingPayouts = await pendingPayoutsQuery.SumAsync(p => (long?)p.AmountLinkDrops, cancellationToken) ?? 0;

        long walletEarnedTotal = await dbContext.TblLinkDropWallets
            .AsNoTracking()
            .SumAsync(w => (long?)w.EarnedBalance, cancellationToken) ?? 0;

        long expectedEarnedBalance = creatorNet - completedPayouts;
        long netDifference = walletEarnedTotal - expectedEarnedBalance;

        string status = "OK";
        if (netDifference != 0)
        {
            status = "MISMATCH";
        }
        else if (pendingPayouts > walletEarnedTotal)
        {
            status = "WARNING";
        }

        string details = $"Historical Net: {creatorNet} LD, Completed Payouts: {completedPayouts} LD, Expected Remaining Balance: {expectedEarnedBalance} LD, Actual Wallet Total: {walletEarnedTotal} LD, Net Difference: {netDifference} LD. Reconciliation Status: {status}.";

        var reconciliation = new AdminFinanceReconciliationModel(
            GrossRevenueLinkDrops: grossRevenue,
            PlatformCommissionLinkDrops: commission,
            CreatorNetEarningsLinkDrops: creatorNet,
            CompletedPayoutsLinkDrops: completedPayouts,
            PendingPayoutsLinkDrops: pendingPayouts,
            ExpectedEarnedBalanceLinkDrops: expectedEarnedBalance,
            WalletEarnedBalanceTotalLinkDrops: walletEarnedTotal,
            NetDifferenceLinkDrops: netDifference,
            ReconciliationStatus: status,
            SummaryDetails: details
        );

        return Result<AdminFinanceReconciliationModel>.Success(reconciliation);
    }

    public static string EscapeCsvField(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        bool needsQuotes = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');
        if (needsQuotes)
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    private static (DateTime? From, DateTime? To) NormalizeDateRange(DateTime? fromDate, DateTime? toDate)
    {
        DateTime? effectiveFrom = fromDate;
        DateTime? effectiveTo = toDate;

        if (effectiveTo.HasValue && effectiveTo.Value.TimeOfDay == TimeSpan.Zero)
        {
            effectiveTo = effectiveTo.Value.Date.AddDays(1).AddTicks(-1);
        }

        return (effectiveFrom, effectiveTo);
    }
}
