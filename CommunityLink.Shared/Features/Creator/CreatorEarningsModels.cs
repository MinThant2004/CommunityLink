namespace CommunityLink.Shared.Features.Creator;

public sealed record CreatorEarningsSummaryModel(
    long AvailableEarnings,
    long TotalEarned,
    long TotalCommission,
    int TotalTransactions,
    decimal EstimatedValueMmk,
    long GrossEarnings,
    long PlatformCommission,
    long NetEarnings
);

public sealed record CreatorGroupEarningsModel(
    int ChatGroupId,
    string ChatGroupName,
    long TotalEarned,
    int TransactionCount
);

public sealed record CreatorUserEarningsModel(
    int BuyerUserId,
    string BuyerName,
    string? BuyerAvatarUrl,
    long TotalLinkDrops,
    long NetLinkDrops,
    int TransactionCount,
    DateTime LastPaidAt
);

public sealed record CreatorEarningsTransactionModel(
    long PaymentTransactionId,
    DateTime Date,
    int ChatGroupId,
    string ChatGroupName,
    int BuyerUserId,
    string BuyerName,
    long GrossAmount,
    long CommissionAmount,
    long NetAmount,
    string Status
);

public sealed record CreatorSourceEarningsModel(
    string Source,
    string Label,
    long Gross,
    long Commission,
    long Net,
    int TransactionCount
);

public sealed record CreatorEarningsDashboardModel(
    CreatorEarningsSummaryModel Summary,
    IReadOnlyList<CreatorGroupEarningsModel> GroupBreakdown,
    IReadOnlyList<CreatorUserEarningsModel> UserBreakdown,
    IReadOnlyList<CreatorSourceEarningsModel> SourceBreakdown,
    IReadOnlyList<CreatorEarningsTransactionModel> Transactions
);
