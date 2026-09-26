namespace CommunityLink.Domain.Features.Payout;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Payout;
using Microsoft.EntityFrameworkCore;

public sealed class CreatorPayoutService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser) : ICreatorPayoutService
{
    private const decimal MmkConversionRatePerDrop = 100.0m;

    public async Task<Result<CreatorPayoutModel>> CreatePayoutRequestAsync(
        CreatePayoutRequestModel request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<CreatorPayoutModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (!currentUser.IsPremiumCreator)
        {
            return Result<CreatorPayoutModel>.Failure("Only Premium Creators can request payouts.", ResultStatus.Forbidden);
        }

        if (request.AmountLinkDrops <= 0)
        {
            return Result<CreatorPayoutModel>.Failure("Payout amount must be greater than zero.", ResultStatus.ValidationError);
        }

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            return Result<CreatorPayoutModel>.Failure("Payment method is required.", ResultStatus.ValidationError);
        }

        if (string.IsNullOrWhiteSpace(request.PaymentAccountName))
        {
            return Result<CreatorPayoutModel>.Failure("Payment account name is required.", ResultStatus.ValidationError);
        }

        if (string.IsNullOrWhiteSpace(request.PaymentAccountNumber))
        {
            return Result<CreatorPayoutModel>.Failure("Payment account number is required.", ResultStatus.ValidationError);
        }

        var creatorUserId = currentUser.UserId.Value;

        // Check Creator Wallet Earned Balance
        var creatorWallet = await dbContext.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == creatorUserId, cancellationToken);

        long availableEarned = creatorWallet?.EarnedBalance ?? 0;

        // Calculate pending payouts
        long pendingPayoutTotal = await dbContext.TblCreatorPayoutRequests
            .AsNoTracking()
            .Where(p => p.CreatorUserId == creatorUserId && p.Status == "PENDING" && !p.IsDeleted)
            .SumAsync(p => p.AmountLinkDrops, cancellationToken);

        long netAvailableForPayout = availableEarned - pendingPayoutTotal;

        if (netAvailableForPayout < request.AmountLinkDrops)
        {
            return Result<CreatorPayoutModel>.Failure(
                $"Insufficient earned Link Drops. Available: {availableEarned} LD, Pending Payouts: {pendingPayoutTotal} LD. Net available: {netAvailableForPayout} LD.",
                ResultStatus.ValidationError);
        }

        decimal amountMmk = request.AmountLinkDrops * MmkConversionRatePerDrop;

        var payoutRequest = new TblCreatorPayoutRequest
        {
            CreatorUserId = creatorUserId,
            AmountLinkDrops = request.AmountLinkDrops,
            AmountMMK = amountMmk,
            PaymentMethod = request.PaymentMethod.Trim(),
            PaymentAccountName = request.PaymentAccountName.Trim(),
            PaymentAccountNumber = request.PaymentAccountNumber.Trim(),
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorUserId
        };

        dbContext.TblCreatorPayoutRequests.Add(payoutRequest);
        await dbContext.SaveChangesAsync(cancellationToken);

        var model = MapToCreatorPayoutModel(payoutRequest);
        return Result<CreatorPayoutModel>.Success(model, "Payout request submitted successfully. Awaiting admin review.");
    }

    public async Task<Result<IReadOnlyList<CreatorPayoutModel>>> GetMyPayoutsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<CreatorPayoutModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var creatorUserId = currentUser.UserId.Value;

        var payouts = await dbContext.TblCreatorPayoutRequests
            .AsNoTracking()
            .Where(p => p.CreatorUserId == creatorUserId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new CreatorPayoutModel(
                p.CreatorPayoutRequestId,
                p.CreatorUserId,
                p.AmountLinkDrops,
                p.AmountMMK,
                p.PaymentMethod,
                p.PaymentAccountName,
                p.PaymentAccountNumber,
                p.Status,
                p.AdminNote,
                p.CreatedAt,
                p.ReviewedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CreatorPayoutModel>>.Success(payouts);
    }

    public async Task<Result<CreatorPayoutSummaryModel>> GetMyPayoutSummaryAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<CreatorPayoutSummaryModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var creatorUserId = currentUser.UserId.Value;

        var creatorWallet = await dbContext.TblLinkDropWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == creatorUserId, cancellationToken);

        long availableEarned = creatorWallet?.EarnedBalance ?? 0;

        var payouts = await dbContext.TblCreatorPayoutRequests
            .AsNoTracking()
            .Where(p => p.CreatorUserId == creatorUserId && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        long pendingAmount = payouts.Where(p => p.Status == "PENDING").Sum(p => p.AmountLinkDrops);
        long completedAmount = payouts.Where(p => p.Status == "COMPLETED" || p.Status == "APPROVED").Sum(p => p.AmountLinkDrops);
        int totalCount = payouts.Count;

        var summary = new CreatorPayoutSummaryModel(
            AvailableEarnedBalance: availableEarned,
            PendingPayoutAmount: pendingAmount,
            CompletedPayoutAmount: completedAmount,
            TotalPayoutCount: totalCount
        );

        return Result<CreatorPayoutSummaryModel>.Success(summary);
    }

    public async Task<Result<IReadOnlyList<AdminPayoutModel>>> GetAdminPayoutsAsync(
        string? statusFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblCreatorPayoutRequests
            .AsNoTracking()
            .Include(p => p.CreatorUser)
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(p => p.Status.ToUpper() == statusFilter.Trim().ToUpper());
        }

        var payouts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminPayoutModel(
                p.CreatorPayoutRequestId,
                p.CreatorUserId,
                p.CreatorUser.DisplayName ?? p.CreatorUser.UserName,
                p.CreatorUser.Email,
                p.AmountLinkDrops,
                p.AmountMMK,
                p.PaymentMethod,
                p.PaymentAccountName,
                p.PaymentAccountNumber,
                p.Status,
                p.AdminNote,
                p.CreatedAt,
                p.ReviewedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AdminPayoutModel>>.Success(payouts);
    }

    public async Task<Result<AdminPayoutModel>> ApprovePayoutAsync(
        long payoutRequestId,
        string? adminNote = null,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = currentUser.UserId ?? 0;

        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? dbTx = null;
        if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            dbTx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        try
        {
            var payout = await dbContext.TblCreatorPayoutRequests
                .Include(p => p.CreatorUser)
                .FirstOrDefaultAsync(p => p.CreatorPayoutRequestId == payoutRequestId && !p.IsDeleted, cancellationToken);

            if (payout == null)
            {
                if (dbTx != null) await dbTx.RollbackAsync(cancellationToken);
                return Result<AdminPayoutModel>.Failure("Payout request not found.", ResultStatus.NotFound);
            }

            if (!string.Equals(payout.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
            {
                if (dbTx != null) await dbTx.RollbackAsync(cancellationToken);
                return Result<AdminPayoutModel>.Failure($"Cannot approve payout request with status '{payout.Status}'.", ResultStatus.ValidationError);
            }

            // Lock Creator Wallet
            var creatorWallet = await dbContext.TblLinkDropWallets
                .FirstOrDefaultAsync(w => w.UserId == payout.CreatorUserId, cancellationToken);

            if (creatorWallet == null || creatorWallet.EarnedBalance < payout.AmountLinkDrops)
            {
                if (dbTx != null) await dbTx.RollbackAsync(cancellationToken);
                return Result<AdminPayoutModel>.Failure(
                    $"Creator has insufficient EarnedBalance for this payout. Available EarnedBalance: {creatorWallet?.EarnedBalance ?? 0} LD.",
                    ResultStatus.ValidationError);
            }

            // 1. Deduct EarnedBalance from Creator Wallet
            long balanceBefore = creatorWallet.Balance;
            creatorWallet.EarnedBalance -= payout.AmountLinkDrops;
            creatorWallet.Balance = creatorWallet.PurchasedBalance + creatorWallet.EarnedBalance;
            creatorWallet.UpdatedAt = DateTime.UtcNow;
            creatorWallet.UpdatedBy = adminUserId;

            // 2. Add Audit Ledger Transaction
            var auditTransaction = new TblLinkDropTransaction
            {
                WalletId = creatorWallet.WalletId,
                UserId = payout.CreatorUserId,
                TransactionType = "CREATOR_PAYOUT",
                Amount = payout.AmountLinkDrops,
                BalanceBefore = balanceBefore,
                BalanceAfter = creatorWallet.Balance,
                ReferenceType = "TblCreatorPayoutRequest",
                ReferenceId = (int)payout.CreatorPayoutRequestId,
                Notes = $"Creator payout #{payout.CreatorPayoutRequestId} completed. Paid via {payout.PaymentMethod}.",
                CreatedAt = DateTime.UtcNow,
                PurchasedAmountDeducted = 0,
                EarnedAmountDeducted = payout.AmountLinkDrops
            };

            dbContext.TblLinkDropTransactions.Add(auditTransaction);

            // 3. Update Payout Status to COMPLETED
            payout.Status = "COMPLETED";
            payout.AdminNote = adminNote?.Trim();
            payout.ReviewedAt = DateTime.UtcNow;
            payout.ReviewedBy = adminUserId;
            payout.UpdatedAt = DateTime.UtcNow;
            payout.UpdatedBy = adminUserId;

            await dbContext.SaveChangesAsync(cancellationToken);

            if (dbTx != null)
            {
                await dbTx.CommitAsync(cancellationToken);
            }

            var adminModel = MapToAdminPayoutModel(payout);
            return Result<AdminPayoutModel>.Success(adminModel, "Payout request approved and completed successfully.");
        }
        catch (Exception ex)
        {
            if (dbTx != null)
            {
                await dbTx.RollbackAsync(cancellationToken);
            }
            return Result<AdminPayoutModel>.Failure($"Error approving payout: {ex.Message}");
        }
    }

    public async Task<Result<AdminPayoutModel>> RejectPayoutAsync(
        long payoutRequestId,
        string? adminNote = null,
        CancellationToken cancellationToken = default)
    {
        var adminUserId = currentUser.UserId ?? 0;

        var payout = await dbContext.TblCreatorPayoutRequests
            .Include(p => p.CreatorUser)
            .FirstOrDefaultAsync(p => p.CreatorPayoutRequestId == payoutRequestId && !p.IsDeleted, cancellationToken);

        if (payout == null)
        {
            return Result<AdminPayoutModel>.Failure("Payout request not found.", ResultStatus.NotFound);
        }

        if (!string.Equals(payout.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AdminPayoutModel>.Failure($"Cannot reject payout request with status '{payout.Status}'.", ResultStatus.ValidationError);
        }

        // Rejection updates status to REJECTED. Wallet balance is NOT touched.
        payout.Status = "REJECTED";
        payout.AdminNote = adminNote?.Trim();
        payout.ReviewedAt = DateTime.UtcNow;
        payout.ReviewedBy = adminUserId;
        payout.UpdatedAt = DateTime.UtcNow;
        payout.UpdatedBy = adminUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var adminModel = MapToAdminPayoutModel(payout);
        return Result<AdminPayoutModel>.Success(adminModel, "Payout request rejected.");
    }

    private static CreatorPayoutModel MapToCreatorPayoutModel(TblCreatorPayoutRequest p) =>
        new(
            p.CreatorPayoutRequestId,
            p.CreatorUserId,
            p.AmountLinkDrops,
            p.AmountMMK,
            p.PaymentMethod,
            p.PaymentAccountName,
            p.PaymentAccountNumber,
            p.Status,
            p.AdminNote,
            p.CreatedAt,
            p.ReviewedAt
        );

    private static AdminPayoutModel MapToAdminPayoutModel(TblCreatorPayoutRequest p) =>
        new(
            p.CreatorPayoutRequestId,
            p.CreatorUserId,
            p.CreatorUser?.DisplayName ?? p.CreatorUser?.UserName ?? $"User #{p.CreatorUserId}",
            p.CreatorUser?.Email ?? "N/A",
            p.AmountLinkDrops,
            p.AmountMMK,
            p.PaymentMethod,
            p.PaymentAccountName,
            p.PaymentAccountNumber,
            p.Status,
            p.AdminNote,
            p.CreatedAt,
            p.ReviewedAt
        );
}
