namespace CommunityLink.Shared.Features.Payout;

public sealed record CreatePayoutRequestModel(
    long AmountLinkDrops,
    string PaymentMethod,
    string PaymentAccountName,
    string PaymentAccountNumber
);

public sealed record ReviewPayoutRequestModel(
    string? AdminNote = null
);

public sealed record CreatorPayoutModel(
    long CreatorPayoutRequestId,
    int CreatorUserId,
    long AmountLinkDrops,
    decimal AmountMMK,
    string PaymentMethod,
    string PaymentAccountName,
    string PaymentAccountNumber,
    string Status,
    string? AdminNote,
    DateTime CreatedAt,
    DateTime? ReviewedAt
);

public sealed record CreatorPayoutSummaryModel(
    long AvailableEarnedBalance,
    long PendingPayoutAmount,
    long CompletedPayoutAmount,
    int TotalPayoutCount
);

public sealed record AdminPayoutModel(
    long CreatorPayoutRequestId,
    int CreatorUserId,
    string CreatorName,
    string CreatorEmail,
    long AmountLinkDrops,
    decimal AmountMMK,
    string PaymentMethod,
    string PaymentAccountName,
    string PaymentAccountNumber,
    string Status,
    string? AdminNote,
    DateTime CreatedAt,
    DateTime? ReviewedAt
);
