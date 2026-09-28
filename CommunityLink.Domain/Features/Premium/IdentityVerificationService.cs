using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;

namespace CommunityLink.Domain.Features.Premium;

public interface IIdentityVerificationService
{
    Task<Result<IdentityVerificationDetailDto>> SubmitVerificationAsync(int userId, SubmitIdentityVerificationRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<UserSubscriptionStatusDto>> GetUserSubscriptionStatusAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<IdentityVerificationDetailDto>>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<IdentityVerificationDetailDto>>> GetAllVerificationsAdminAsync(CancellationToken cancellationToken = default);
    Task<Result<IdentityVerificationDetailDto>> ApproveVerificationAsync(int verificationId, int adminId, string? reviewNotes = null, CancellationToken cancellationToken = default);
    Task<Result<IdentityVerificationDetailDto>> RejectVerificationAsync(int verificationId, int adminId, string rejectionReason, CancellationToken cancellationToken = default);
    Task<Result<int>> ProcessExpiredSubscriptionsAsync(CancellationToken cancellationToken = default);
}

public sealed class IdentityVerificationService(AppDbContext dbContext) : IIdentityVerificationService
{
    public async Task<Result<int>> ProcessExpiredSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiredSubscriptions = await dbContext.TblUserSubscriptions
            .Include(s => s.User)
            .Include(s => s.Role)
            .Where(s => s.Status == "Active" && s.ExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        if (!expiredSubscriptions.Any())
        {
            return Result<int>.Success(0, "No expired subscriptions found.");
        }

        var memberRole = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "MEMBER" || r.RoleCode == "USER", cancellationToken);
        var memberRoleId = memberRole?.RoleId ?? 1;

        int processedCount = 0;
        foreach (var sub in expiredSubscriptions)
        {
            sub.Status = "Expired";

            // Check if user has any other active premium subscription
            var hasOtherActive = await dbContext.TblUserSubscriptions
                .AnyAsync(s => s.UserId == sub.UserId && s.SubscriptionId != sub.SubscriptionId && s.Status == "Active" && s.ExpiresAtUtc > now, cancellationToken);

            if (!hasOtherActive)
            {
                // Revert user role to MEMBER
                var userRoles = await dbContext.TblUserRoles
                    .Where(ur => ur.UserId == sub.UserId)
                    .ToListAsync(cancellationToken);

                // Preserve admin role if user is an admin
                var isAdmin = userRoles.Any(ur => ur.RoleId == 100);
                if (!isAdmin)
                {
                    dbContext.TblUserRoles.RemoveRange(userRoles);
                    dbContext.TblUserRoles.Add(new TblUserRole
                    {
                        UserId = sub.UserId,
                        RoleId = memberRoleId,
                        CreatedAt = DateTime.UtcNow
                    });

                    if (sub.User != null)
                    {
                        sub.User.IsVerified = false;
                        sub.User.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // Notify member of expiration
                dbContext.TblNotifications.Add(new TblNotification
                {
                    RecipientUserId = sub.UserId,
                    NotificationType = "TIER_SUBSCRIPTION_EXPIRED",
                    Title = "Premium Subscription Expired",
                    Message = "Your premium subscription period has ended and your tier has returned to standard Member. Visit the Upgrade page to renew!",
                    TargetEntityName = "TblUserSubscription",
                    TargetEntityId = sub.SubscriptionId,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            processedCount++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(processedCount, $"Successfully processed {processedCount} expired subscriptions.");
    }
    public async Task<Result<IdentityVerificationDetailDto>> SubmitVerificationAsync(int userId, SubmitIdentityVerificationRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive && !u.IsDeleted, cancellationToken);
        if (user is null)
            return Result<IdentityVerificationDetailDto>.Failure("User not found or account is inactive.", ResultStatus.NotFound);

        var plan = await dbContext.TblSubscriptionPlans.FirstOrDefaultAsync(p => p.PlanId == request.PlanId && p.IsActive, cancellationToken);
        if (plan is null)
            return Result<IdentityVerificationDetailDto>.Failure("Selected subscription plan was not found or is currently inactive.", ResultStatus.NotFound);

        if (string.IsNullOrWhiteSpace(request.FullLegalName))
            return Result<IdentityVerificationDetailDto>.Failure("Legal Full Name matching your government-issued ID is required.", ResultStatus.ValidationError);

        if (string.IsNullOrWhiteSpace(request.IdCardFrontUrl))
            return Result<IdentityVerificationDetailDto>.Failure("Front of Identity Card image is required.", ResultStatus.ValidationError);

        // Check if there is already an active pending request
        var hasPending = await dbContext.TblIdentityVerifications
            .AnyAsync(v => v.UserId == userId && v.Status == "PendingReview", cancellationToken);
        if (hasPending)
        {
            return Result<IdentityVerificationDetailDto>.Failure("You already have an upgrade & verification audit pending review.", ResultStatus.Conflict);
        }

        // Handle LinkDrop payment deduction if selected
        long pointsDeducted = 0;
        if (request.PaymentMethod.Equals("LinkDropPoints", StringComparison.OrdinalIgnoreCase) && plan.LinkDropCost > 0)
        {
            var wallet = await dbContext.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
            if (wallet == null || wallet.Balance < plan.LinkDropCost)
            {
                var currentBalance = wallet?.Balance ?? 0;
                return Result<IdentityVerificationDetailDto>.Failure($"Insufficient LinkDrop points balance. Plan requires {plan.LinkDropCost:N0} Drops, but your balance is {currentBalance:N0} Drops.", ResultStatus.ValidationError);
            }

            // Deduct from wallet (first from purchased, then from earned)
            var amountToDeduct = plan.LinkDropCost;
            var fromPurchased = Math.Min(wallet.PurchasedBalance, amountToDeduct);
            var fromEarned = amountToDeduct - fromPurchased;

            var bBefore = wallet.Balance;
            var pBefore = wallet.PurchasedBalance;
            var eBefore = wallet.EarnedBalance;

            wallet.PurchasedBalance -= fromPurchased;
            wallet.EarnedBalance -= fromEarned;
            wallet.Balance = wallet.PurchasedBalance + wallet.EarnedBalance;
            wallet.UpdatedAt = DateTime.UtcNow;

            pointsDeducted = plan.LinkDropCost;

            // Log Transaction
            dbContext.TblLinkDropTransactions.Add(new TblLinkDropTransaction
            {
                WalletId = wallet.WalletId,
                UserId = userId,
                TransactionType = "TIER_UPGRADE",
                Amount = -plan.LinkDropCost,
                BalanceBefore = bBefore,
                BalanceAfter = wallet.Balance,
                PurchasedBalanceBefore = pBefore,
                PurchasedBalanceAfter = wallet.PurchasedBalance,
                EarnedBalanceBefore = eBefore,
                EarnedBalanceAfter = wallet.EarnedBalance,
                PurchasedAmountDeducted = fromPurchased,
                EarnedAmountDeducted = fromEarned,
                ReferenceType = "TblSubscriptionPlan",
                ReferenceId = plan.PlanId,
                Notes = $"Verification fee for {plan.PlanName}",
                CreatedAt = DateTime.UtcNow
            });
        }

        var verification = new TblIdentityVerification
        {
            UserId = userId,
            PlanId = plan.PlanId,
            TargetRoleCode = plan.TargetRoleCode,
            FullLegalName = request.FullLegalName.Trim(),
            WorkEmail = string.IsNullOrWhiteSpace(request.WorkEmail) ? null : request.WorkEmail.Trim(),
            ProfessionalUrl = string.IsNullOrWhiteSpace(request.ProfessionalUrl) ? null : request.ProfessionalUrl.Trim(),
            IdCardFrontUrl = request.IdCardFrontUrl.Trim(),
            IdCardBackUrl = string.IsNullOrWhiteSpace(request.IdCardBackUrl) ? null : request.IdCardBackUrl.Trim(),
            PaymentMethod = request.PaymentMethod,
            LinkDropPointsDeducted = pointsDeducted,
            Status = "PendingReview",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.TblIdentityVerifications.Add(verification);

        // Also add pending UserSubscription
        var targetRole = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == plan.TargetRoleCode, cancellationToken);
        var roleId = targetRole?.RoleId ?? 1;

        var userSub = new TblUserSubscription
        {
            UserId = userId,
            PlanId = plan.PlanId,
            RoleId = roleId,
            Status = "PendingReview",
            PaymentMethod = request.PaymentMethod,
            StartDateUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(plan.DurationDays),
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.TblUserSubscriptions.Add(userSub);

        // Notify user of submission
        dbContext.TblNotifications.Add(new TblNotification
        {
            RecipientUserId = userId,
            NotificationType = "TIER_VERIFICATION_SUBMITTED",
            Title = "Identity Verification Audit Submitted",
            Message = $"Your application for {plan.PlanName} has been submitted for SecOps Review. We will verify your credentials shortly.",
            TargetEntityName = "TblIdentityVerification",
            TargetEntityId = 0,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<IdentityVerificationDetailDto>.Success(
            MapToDetailDto(verification, user, plan, null),
            "Identity verification and tier upgrade request submitted successfully."
        );
    }

    public async Task<Result<UserSubscriptionStatusDto>> GetUserSubscriptionStatusAsync(int userId, CancellationToken cancellationToken = default)
    {
        var activeSub = await dbContext.TblUserSubscriptions
            .Include(s => s.Plan)
            .Include(s => s.Role)
            .Where(s => s.UserId == userId && s.Status == "Active" && s.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(s => s.ExpiresAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var pendingVerif = await dbContext.TblIdentityVerifications
            .Include(v => v.User)
            .Include(v => v.Plan)
            .Include(v => v.ReviewedByAdmin)
            .Where(v => v.UserId == userId && v.Status == "PendingReview")
            .OrderByDescending(v => v.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        IdentityVerificationDetailDto? pendingDto = null;
        if (pendingVerif != null)
        {
            pendingDto = MapToDetailDto(pendingVerif, pendingVerif.User, pendingVerif.Plan, pendingVerif.ReviewedByAdmin);
        }

        if (activeSub != null)
        {
            var daysRemaining = (int)Math.Max(0, (activeSub.ExpiresAtUtc - DateTime.UtcNow).TotalDays);
            return Result<UserSubscriptionStatusDto>.Success(new UserSubscriptionStatusDto(
                HasActiveSubscription: true,
                TierName: activeSub.Plan.PlanName,
                TargetRoleCode: activeSub.Role.RoleCode,
                Status: activeSub.Status,
                StartDateUtc: activeSub.StartDateUtc,
                ExpiresAtUtc: activeSub.ExpiresAtUtc,
                DaysRemaining: daysRemaining,
                PendingVerification: pendingDto
            ));
        }

        return Result<UserSubscriptionStatusDto>.Success(new UserSubscriptionStatusDto(
            HasActiveSubscription: false,
            TierName: "Normal User",
            TargetRoleCode: "USER",
            Status: "Inactive",
            StartDateUtc: null,
            ExpiresAtUtc: null,
            DaysRemaining: 0,
            PendingVerification: pendingDto
        ));
    }

    public async Task<Result<IReadOnlyList<IdentityVerificationDetailDto>>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default)
    {
        var items = await dbContext.TblIdentityVerifications
            .AsNoTracking()
            .Include(v => v.User)
            .Include(v => v.Plan)
            .Include(v => v.ReviewedByAdmin)
            .Where(v => v.Status == "PendingReview")
            .OrderByDescending(v => v.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(v => MapToDetailDto(v, v.User, v.Plan, v.ReviewedByAdmin)).ToList();
        return Result<IReadOnlyList<IdentityVerificationDetailDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<IdentityVerificationDetailDto>>> GetAllVerificationsAdminAsync(CancellationToken cancellationToken = default)
    {
        var items = await dbContext.TblIdentityVerifications
            .AsNoTracking()
            .Include(v => v.User)
            .Include(v => v.Plan)
            .Include(v => v.ReviewedByAdmin)
            .OrderByDescending(v => v.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(v => MapToDetailDto(v, v.User, v.Plan, v.ReviewedByAdmin)).ToList();
        return Result<IReadOnlyList<IdentityVerificationDetailDto>>.Success(dtos);
    }

    public async Task<Result<IdentityVerificationDetailDto>> ApproveVerificationAsync(int verificationId, int adminId, string? reviewNotes = null, CancellationToken cancellationToken = default)
    {
        var verification = await dbContext.TblIdentityVerifications
            .Include(v => v.User)
            .Include(v => v.Plan)
            .FirstOrDefaultAsync(v => v.VerificationId == verificationId, cancellationToken);

        if (verification is null)
            return Result<IdentityVerificationDetailDto>.Failure("Verification record not found.", ResultStatus.NotFound);

        if (verification.Status != "PendingReview")
            return Result<IdentityVerificationDetailDto>.Failure($"Cannot approve verification in '{verification.Status}' state.", ResultStatus.Conflict);

        var targetRole = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == verification.TargetRoleCode, cancellationToken);
        if (targetRole is null)
            return Result<IdentityVerificationDetailDto>.Failure($"Target role '{verification.TargetRoleCode}' was not found in system roles.", ResultStatus.NotFound);

        // 1. Switch User Role in TblUserRole
        var existingUserRoles = await dbContext.TblUserRoles.Where(ur => ur.UserId == verification.UserId).ToListAsync(cancellationToken);
        dbContext.TblUserRoles.RemoveRange(existingUserRoles);
        dbContext.TblUserRoles.Add(new TblUserRole
        {
            UserId = verification.UserId,
            RoleId = targetRole.RoleId,
            CreatedAt = DateTime.UtcNow
        });

        // 2. Mark Verification as Approved & Update User verified flag
        verification.Status = "Approved";
        verification.ReviewNotes = reviewNotes ?? "Verified & credentials approved by Administrator SecOps.";
        verification.ReviewedByAdminId = adminId;
        verification.ReviewedAtUtc = DateTime.UtcNow;

        verification.User.IsVerified = true;
        verification.User.UpdatedAt = DateTime.UtcNow;

        // Record UserVerificationAudit record if not already recorded
        var existingAudit = await dbContext.TblUserVerificationAudits
            .FirstOrDefaultAsync(a => a.UserId == verification.UserId && a.AuditCode == "GOV_ID_CARD", cancellationToken);
        if (existingAudit == null)
        {
            dbContext.TblUserVerificationAudits.Add(new TblUserVerificationAudit
            {
                UserId = verification.UserId,
                AuditCode = "GOV_ID_CARD",
                AuditTitle = $"{targetRole.RoleName} Verified",
                AuditDescription = $"SecOps verified identity card for legal name '{verification.FullLegalName}'.",
                Authority = "CommunityLink Verification SecOps",
                Status = "ACTIVE",
                AuditedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 3. Activate UserSubscription
        var userSub = await dbContext.TblUserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == verification.UserId && s.PlanId == verification.PlanId && s.Status == "PendingReview", cancellationToken);

        var durationDays = verification.Plan.DurationDays > 0 ? verification.Plan.DurationDays : 30;
        if (userSub != null)
        {
            userSub.Status = "Active";
            userSub.RoleId = targetRole.RoleId;
            userSub.StartDateUtc = DateTime.UtcNow;
            userSub.ExpiresAtUtc = DateTime.UtcNow.AddDays(durationDays);
        }
        else
        {
            dbContext.TblUserSubscriptions.Add(new TblUserSubscription
            {
                UserId = verification.UserId,
                PlanId = verification.PlanId,
                RoleId = targetRole.RoleId,
                Status = "Active",
                PaymentMethod = verification.PaymentMethod,
                StartDateUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(durationDays),
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        // 4. Send Approval Notification
        dbContext.TblNotifications.Add(new TblNotification
        {
            RecipientUserId = verification.UserId,
            ActorUserId = adminId,
            NotificationType = "TIER_VERIFICATION_APPROVED",
            Title = "Congratulations! Tier Upgrade Approved 🎉",
            Message = $"Your credentials have been verified. You have been promoted to {targetRole.RoleName}! Enjoy your verified badge and pro tools.",
            TargetEntityName = "TblIdentityVerification",
            TargetEntityId = verification.VerificationId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var admin = await dbContext.TblAdmins.FindAsync([adminId], cancellationToken);
        return Result<IdentityVerificationDetailDto>.Success(
            MapToDetailDto(verification, verification.User, verification.Plan, admin),
            $"Verification approved. User '{verification.User.UserName}' promoted to {targetRole.RoleName}."
        );
    }

    public async Task<Result<IdentityVerificationDetailDto>> RejectVerificationAsync(int verificationId, int adminId, string rejectionReason, CancellationToken cancellationToken = default)
    {
        var verification = await dbContext.TblIdentityVerifications
            .Include(v => v.User)
            .Include(v => v.Plan)
            .FirstOrDefaultAsync(v => v.VerificationId == verificationId, cancellationToken);

        if (verification is null)
            return Result<IdentityVerificationDetailDto>.Failure("Verification record not found.", ResultStatus.NotFound);

        if (verification.Status != "PendingReview")
            return Result<IdentityVerificationDetailDto>.Failure($"Cannot reject verification in '{verification.Status}' state.", ResultStatus.Conflict);

        if (string.IsNullOrWhiteSpace(rejectionReason))
            return Result<IdentityVerificationDetailDto>.Failure("A clear rejection reason must be provided to the applicant.", ResultStatus.ValidationError);

        // 1. Mark Verification as Rejected
        verification.Status = "Rejected";
        verification.ReviewNotes = rejectionReason.Trim();
        verification.ReviewedByAdminId = adminId;
        verification.ReviewedAtUtc = DateTime.UtcNow;

        // 2. Mark pending subscription as Rejected
        var userSub = await dbContext.TblUserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == verification.UserId && s.PlanId == verification.PlanId && s.Status == "PendingReview", cancellationToken);
        if (userSub != null)
        {
            userSub.Status = "Rejected";
        }

        // 3. Refund LinkDrop points if they were deducted
        if (verification.LinkDropPointsDeducted > 0)
        {
            var wallet = await dbContext.TblLinkDropWallets.FirstOrDefaultAsync(w => w.UserId == verification.UserId, cancellationToken);
            if (wallet != null)
            {
                var bBefore = wallet.Balance;
                var pBefore = wallet.PurchasedBalance;
                var eBefore = wallet.EarnedBalance;

                // Refund as earned/purchased balance
                wallet.PurchasedBalance += verification.LinkDropPointsDeducted;
                wallet.Balance = wallet.PurchasedBalance + wallet.EarnedBalance;
                wallet.UpdatedAt = DateTime.UtcNow;

                dbContext.TblLinkDropTransactions.Add(new TblLinkDropTransaction
                {
                    WalletId = wallet.WalletId,
                    UserId = verification.UserId,
                    TransactionType = "TIER_REFUND",
                    Amount = verification.LinkDropPointsDeducted,
                    BalanceBefore = bBefore,
                    BalanceAfter = wallet.Balance,
                    PurchasedBalanceBefore = pBefore,
                    PurchasedBalanceAfter = wallet.PurchasedBalance,
                    EarnedBalanceBefore = eBefore,
                    EarnedBalanceAfter = wallet.EarnedBalance,
                    PurchasedAmountDeducted = 0,
                    EarnedAmountDeducted = 0,
                    ReferenceType = "TblIdentityVerification",
                    ReferenceId = verification.VerificationId,
                    Notes = $"Verification rejected refund: {rejectionReason.Trim()}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // 4. Send Rejection Notification
        dbContext.TblNotifications.Add(new TblNotification
        {
            RecipientUserId = verification.UserId,
            ActorUserId = adminId,
            NotificationType = "TIER_VERIFICATION_REJECTED",
            Title = "Verification Audit Notice",
            Message = $"Your application for {verification.Plan.PlanName} was not approved: {rejectionReason.Trim()}. {(verification.LinkDropPointsDeducted > 0 ? "Your LinkDrop points have been refunded." : "")}",
            TargetEntityName = "TblIdentityVerification",
            TargetEntityId = verification.VerificationId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var admin = await dbContext.TblAdmins.FindAsync([adminId], cancellationToken);
        return Result<IdentityVerificationDetailDto>.Success(
            MapToDetailDto(verification, verification.User, verification.Plan, admin),
            "Verification rejected and applicant notified."
        );
    }

    private static IdentityVerificationDetailDto MapToDetailDto(
        TblIdentityVerification v,
        TblUser user,
        TblSubscriptionPlan plan,
        TblAdmin? admin)
    {
        return new IdentityVerificationDetailDto(
            v.VerificationId,
            v.UserId,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.AvatarUrl,
            v.PlanId,
            plan.PlanName,
            v.TargetRoleCode,
            plan.PriceAmount,
            plan.LinkDropCost,
            v.FullLegalName,
            v.WorkEmail,
            v.ProfessionalUrl,
            v.IdCardFrontUrl,
            v.IdCardBackUrl,
            v.PaymentMethod,
            v.LinkDropPointsDeducted,
            v.Status,
            v.ReviewNotes,
            admin?.Email,
            v.ReviewedAtUtc,
            v.CreatedAtUtc
        );
    }
}
