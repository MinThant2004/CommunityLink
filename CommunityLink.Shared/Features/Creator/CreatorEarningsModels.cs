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

public sealed record CreatorEarningsDashboardModel(
    CreatorEarningsSummaryModel Summary,
    IReadOnlyList<CreatorGroupEarningsModel> GroupBreakdown,
    IReadOnlyList<CreatorEarningsTransactionModel> Transactions
);
