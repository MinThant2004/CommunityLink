using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.Premium;

public sealed record SubmitIdentityVerificationRequestDto(
    int PlanId,
    string FullLegalName,
    string? WorkEmail,
    string? ProfessionalUrl,
    string IdCardFrontUrl,
    string? IdCardBackUrl,
    string PaymentMethod = "LinkDropPoints"
);

public sealed record IdentityVerificationDetailDto(
    int VerificationId,
    int UserId,
    string UserName,
    string DisplayName,
    string UserEmail,
    string? UserAvatarUrl,
    int PlanId,
    string PlanName,
    string TargetRoleCode,
    decimal PriceAmount,
    long LinkDropCost,
    string FullLegalName,
    string? WorkEmail,
    string? ProfessionalUrl,
    string IdCardFrontUrl,
    string? IdCardBackUrl,
    string PaymentMethod,
    long LinkDropPointsDeducted,
    string Status,
    string? ReviewNotes,
    string? ReviewedByAdminEmail,
    DateTime? ReviewedAtUtc,
    DateTime CreatedAtUtc
);

public sealed record UserSubscriptionStatusDto(
    bool HasActiveSubscription,
    string? TierName,
    string? TargetRoleCode,
    string? Status,
    DateTime? StartDateUtc,
    DateTime? ExpiresAtUtc,
    int? DaysRemaining,
    IdentityVerificationDetailDto? PendingVerification
);
