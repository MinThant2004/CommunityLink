namespace CommunityLink.Domain.Features.Creator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.LinkDrop;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Creator;
using Microsoft.EntityFrameworkCore;

public sealed class CreatorEarningsService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser) : ICreatorEarningsService
{
    private const decimal MmkConversionRatePerDrop = 100.0m;

    public async Task<Result<CreatorEarningsDashboardModel>> GetCreatorEarningsAsync(
        string? filterType = null,
        int? chatGroupId = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<CreatorEarningsDashboardModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var creatorUserId = currentUser.UserId.Value;

        // 1. Fetch Creator Wallet - MUST count ONLY EarnedBalance (PurchasedBalance is excluded from Creator Earnings)
        var creatorWallet = await dbContext.TblLinkDropWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == creatorUserId, cancellationToken);

        long availableEarnings = creatorWallet?.EarnedBalance ?? 0;
        decimal estimatedValueMmk = availableEarnings * MmkConversionRatePerDrop;

        // 2. Fetch Chat Group payment transactions for this creator
        var groupTransactions = await dbContext.TblChatGroupPaymentTransactions
            .AsNoTracking()
            .Include(t => t.ChatGroup)
            .Include(t => t.User)
            .Where(t => t.CreatorUserId == creatorUserId && t.Status.ToUpper() == "COMPLETED")
            .ToListAsync(cancellationToken);

        // 3. Fetch Private Chat payment transactions for this creator
        var privateTransactions = await dbContext.TblPrivateChatPaymentTransactions
            .AsNoTracking()
            .Include(t => t.BuyerUser)
            .Where(t => t.CreatorUserId == creatorUserId && t.Status.ToUpper() == "COMPLETED")
            .ToListAsync(cancellationToken);

        long groupGross = groupTransactions.Sum(t => t.GrossAmount);
        long groupCommission = groupTransactions.Sum(t => t.CommissionAmount);
        long groupNet = groupTransactions.Sum(t => t.NetAmount);

        long privateGross = privateTransactions.Sum(t => t.GrossAmountLinkDrops);
        long privateCommission = privateTransactions.Sum(t => t.CommissionAmount);
        long privateNet = privateTransactions.Sum(t => t.CreatorAmount);

        long grossEarnings = groupGross + privateGross;
        long platformCommission = groupCommission + privateCommission;
        long netEarnings = groupNet + privateNet;
        int totalTransactions = groupTransactions.Count + privateTransactions.Count;

        var summary = new CreatorEarningsSummaryModel(
            AvailableEarnings: availableEarnings,
            TotalEarned: grossEarnings,
            TotalCommission: platformCommission,
            TotalTransactions: totalTransactions,
            EstimatedValueMmk: estimatedValueMmk,
            GrossEarnings: grossEarnings,
            PlatformCommission: platformCommission,
            NetEarnings: netEarnings
        );

        // 4. Calculate Group Breakdown
        var creatorGroups = await dbContext.TblChatGroups
            .AsNoTracking()
            .Where(cg => cg.CreatorId == creatorUserId && !cg.IsDeleted)
            .ToListAsync(cancellationToken);

        var groupBreakdownDict = creatorGroups.ToDictionary(
            g => g.ChatGroupId,
            g => new CreatorGroupEarningsModel(g.ChatGroupId, g.Name, 0, 0)
        );

        foreach (var tx in groupTransactions)
        {
            if (groupBreakdownDict.TryGetValue(tx.ChatGroupId, out var existing))
            {
                groupBreakdownDict[tx.ChatGroupId] = new CreatorGroupEarningsModel(
                    existing.ChatGroupId,
                    existing.ChatGroupName,
                    existing.TotalEarned + tx.NetAmount,
                    existing.TransactionCount + 1
                );
            }
            else
            {
                string groupName = tx.ChatGroup?.Name ?? $"Group #{tx.ChatGroupId}";
                groupBreakdownDict[tx.ChatGroupId] = new CreatorGroupEarningsModel(
                    tx.ChatGroupId,
                    groupName,
                    tx.NetAmount,
                    1
                );
            }
        }

        var groupBreakdownList = groupBreakdownDict.Values
            .OrderByDescending(g => g.TotalEarned)
            .ThenBy(g => g.ChatGroupName)
            .ToList();

        // 5. Combine and Filter Transactions List for History Table
        var combinedTxModels = new List<CreatorEarningsTransactionModel>();

        foreach (var t in groupTransactions)
        {
            combinedTxModels.Add(new CreatorEarningsTransactionModel(
                PaymentTransactionId: t.PaymentTransactionId,
                Date: t.CreatedAt,
                ChatGroupId: t.ChatGroupId,
                ChatGroupName: t.ChatGroup?.Name ?? "Chat Group",
                BuyerUserId: t.UserId,
                BuyerName: t.User?.DisplayName ?? t.User?.UserName ?? "User",
                GrossAmount: t.GrossAmount,
                CommissionAmount: t.CommissionAmount,
                NetAmount: t.NetAmount,
                Status: t.Status
            ));
        }

        foreach (var pt in privateTransactions)
        {
            combinedTxModels.Add(new CreatorEarningsTransactionModel(
                PaymentTransactionId: pt.PrivateChatPaymentTransactionId,
                Date: pt.CreatedAt,
                ChatGroupId: 0,
                ChatGroupName: "Private Chat Unlock",
                BuyerUserId: pt.BuyerUserId,
                BuyerName: pt.BuyerUser?.DisplayName ?? pt.BuyerUser?.UserName ?? "User",
                GrossAmount: pt.GrossAmountLinkDrops,
                CommissionAmount: pt.CommissionAmount,
                NetAmount: pt.CreatorAmount,
                Status: pt.Status
            ));
        }

        IEnumerable<CreatorEarningsTransactionModel> filteredTxs = combinedTxModels;

        if (chatGroupId.HasValue && chatGroupId.Value > 0)
        {
            filteredTxs = filteredTxs.Where(t => t.ChatGroupId == chatGroupId.Value);
        }

        if (string.Equals(filterType, "GROUP", StringComparison.OrdinalIgnoreCase))
        {
            filteredTxs = filteredTxs.Where(t => t.ChatGroupId > 0);
        }
        else if (string.Equals(filterType, "PRIVATE_CHAT", StringComparison.OrdinalIgnoreCase))
        {
            filteredTxs = filteredTxs.Where(t => t.ChatGroupId == 0);
        }

        var transactionModels = filteredTxs
            .OrderByDescending(t => t.Date)
            .ToList();

        var dashboard = new CreatorEarningsDashboardModel(
            Summary: summary,
            GroupBreakdown: groupBreakdownList,
            Transactions: transactionModels
        );

        return Result<CreatorEarningsDashboardModel>.Success(dashboard);
    }
}
