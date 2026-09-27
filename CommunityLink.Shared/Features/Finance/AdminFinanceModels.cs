namespace CommunityLink.Shared.Features.Finance;

using System;

public sealed record AdminFinanceSummaryModel(
    long GrossChatGroupRevenueLinkDrops,
    long PlatformCommissionLinkDrops,
    long CreatorNetEarningsLinkDrops,
    long CompletedCreatorPayoutsLinkDrops,
    long PendingCreatorPayoutsLinkDrops,
    int GrossTransactionCount,
    int CompletedPayoutCount,
    int PendingPayoutCount
);

public sealed record AdminFinanceTransactionModel(
    long TransactionId,
    DateTime Date,
    string TransactionType,
    string CreatorOrUserName,
    string? ChatGroupName,
    long GrossAmountLinkDrops,
    long CommissionAmountLinkDrops,
    long NetAmountLinkDrops,
    string Status
);

public sealed record AdminFinancePagedTransactionModel(
    IReadOnlyList<AdminFinanceTransactionModel> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);


public sealed record AdminFinanceGroupBreakdownModel(
    int ChatGroupId,
    string ChatGroupName,
    long GrossRevenueLinkDrops,
    long PlatformCommissionLinkDrops,
    long CreatorNetEarningsLinkDrops,
    int CompletedTransactionCount
);

public sealed record AdminFinanceReconciliationModel(
    long GrossRevenueLinkDrops,
    long PlatformCommissionLinkDrops,
    long CreatorNetEarningsLinkDrops,
    long CompletedPayoutsLinkDrops,
    long PendingPayoutsLinkDrops,
    long ExpectedEarnedBalanceLinkDrops,
    long WalletEarnedBalanceTotalLinkDrops,
    long NetDifferenceLinkDrops,
    string ReconciliationStatus,
    string SummaryDetails
);
