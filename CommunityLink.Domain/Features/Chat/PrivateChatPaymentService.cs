using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.Admin;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;

namespace CommunityLink.Domain.Features.Chat;

public interface IPrivateChatPaymentService
{
    Task<Result<int>> UnlockPrivateChatAsync(int buyerUserId, int creatorUserId, CancellationToken cancellationToken = default);
    Task<bool> IsConversationUnlockedAsync(int conversationId, int userId, CancellationToken cancellationToken = default);
    Task<bool> IsPrivateChatPaidRequiredAsync(int creatorUserId, CancellationToken cancellationToken = default);
}

public sealed class PrivateChatPaymentService(
    AppDbContext dbContext,
    IPlatformSettingService platformSettingService) : IPrivateChatPaymentService
{
    public async Task<Result<int>> UnlockPrivateChatAsync(int buyerUserId, int creatorUserId, CancellationToken cancellationToken = default)
    {
        if (buyerUserId == creatorUserId)
        {
            return Result<int>.Failure("Cannot unlock private chat with yourself.", ResultStatus.ValidationError);
        }

        // 1. Load creator chat setting
        var setting = await dbContext.TblCreatorChatSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreatorUserId == creatorUserId, cancellationToken);

        if (setting is null || !setting.IsPrivateChatEnabled)
        {
            return Result<int>.Failure("Private chat is not enabled for this creator.", ResultStatus.ValidationError);
        }

        long fee = setting.PrivateChatFeeLinkDrops;

        // 2. Find or create conversation
        var conversation = await dbContext.TblConversations.FirstOrDefaultAsync(c =>
            ((c.UserOneId == buyerUserId && c.UserTwoId == creatorUserId) ||
             (c.UserOneId == creatorUserId && c.UserTwoId == buyerUserId)) && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            conversation = new TblConversation
            {
                UserOneId = buyerUserId,
                UserTwoId = creatorUserId,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TblConversations.Add(conversation);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // Check if already unlocked (duplicate unlock prevention)
        var existingTx = await dbContext.TblPrivateChatPaymentTransactions.FirstOrDefaultAsync(t =>
            t.ConversationId == conversation.ConversationId &&
            t.BuyerUserId == buyerUserId &&
            t.Status == "COMPLETED", cancellationToken);

        if (existingTx != null)
        {
            return Result<int>.Success(conversation.ConversationId);
        }

        // 3. Check buyer wallet balance
        var buyerWallet = await dbContext.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == buyerUserId, cancellationToken);

        if (buyerWallet is null || buyerWallet.Balance < fee)
        {
            return Result<int>.Failure("Insufficient LinkDrop balance.", ResultStatus.ValidationError);
        }

        // 4. Deduct buyer wallet (PurchasedBalance first, then EarnedBalance)
        long feeRemaining = fee;
        long purchasedDeducted = Math.Min(buyerWallet.PurchasedBalance, feeRemaining);
        buyerWallet.PurchasedBalance -= purchasedDeducted;
        feeRemaining -= purchasedDeducted;

        long earnedDeducted = Math.Min(buyerWallet.EarnedBalance, feeRemaining);
        buyerWallet.EarnedBalance -= earnedDeducted;

        long buyerBalanceBefore = buyerWallet.Balance;
        buyerWallet.Balance = buyerWallet.PurchasedBalance + buyerWallet.EarnedBalance;
        buyerWallet.UpdatedAt = DateTime.UtcNow;

        // 5. Calculate platform commission
        var commissionRes = await platformSettingService.GetPlatformCommissionAsync(cancellationToken);
        decimal commissionRate = commissionRes.IsSuccess && commissionRes.Data != null
            ? commissionRes.Data.CommissionPercentage
            : 10.00m;

        long commissionAmount = (long)Math.Round(fee * (commissionRate / 100m), MidpointRounding.AwayFromZero);
        long creatorAmount = fee - commissionAmount;

        // 6. Credit creator EarnedBalance
        var creatorWallet = await dbContext.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == creatorUserId, cancellationToken);

        if (creatorWallet is null)
        {
            creatorWallet = new TblLinkDropWallet
            {
                UserId = creatorUserId,
                Balance = 0,
                PurchasedBalance = 0,
                EarnedBalance = 0,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TblLinkDropWallets.Add(creatorWallet);
        }

        long creatorBalanceBefore = creatorWallet.Balance;
        creatorWallet.EarnedBalance += creatorAmount;
        creatorWallet.Balance = creatorWallet.PurchasedBalance + creatorWallet.EarnedBalance;
        creatorWallet.UpdatedAt = DateTime.UtcNow;

        // 7. Record TblPrivateChatPaymentTransaction
        var paymentTx = new TblPrivateChatPaymentTransaction
        {
            ConversationId = conversation.ConversationId,
            BuyerUserId = buyerUserId,
            CreatorUserId = creatorUserId,
            GrossAmountLinkDrops = fee,
            CommissionAmount = commissionAmount,
            CreatorAmount = creatorAmount,
            Status = "COMPLETED",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.TblPrivateChatPaymentTransactions.Add(paymentTx);

        // 8. Record LinkDrop Transaction Ledgers
        var buyerLedger = new TblLinkDropTransaction
        {
            WalletId = buyerWallet.WalletId,
            UserId = buyerUserId,
            TransactionType = "PRIVATE_CHAT_UNLOCK",
            Amount = fee,
            BalanceBefore = buyerBalanceBefore,
            BalanceAfter = buyerWallet.Balance,
            PurchasedAmountDeducted = purchasedDeducted,
            EarnedAmountDeducted = earnedDeducted,
            ReferenceType = "CONVERSATION",
            ReferenceId = conversation.ConversationId,
            Notes = $"Unlocked private chat with Creator #{creatorUserId}",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.TblLinkDropTransactions.Add(buyerLedger);

        var creatorLedger = new TblLinkDropTransaction
        {
            WalletId = creatorWallet.WalletId,
            UserId = creatorUserId,
            TransactionType = "PRIVATE_CHAT_EARNING",
            Amount = creatorAmount,
            BalanceBefore = creatorBalanceBefore,
            BalanceAfter = creatorWallet.Balance,
            PurchasedAmountDeducted = 0,
            EarnedAmountDeducted = 0,
            ReferenceType = "CONVERSATION",
            ReferenceId = conversation.ConversationId,
            Notes = $"Earned {creatorAmount} LinkDrops from private chat unlock by User #{buyerUserId}",
            CreatedAt = DateTime.UtcNow
        };
        dbContext.TblLinkDropTransactions.Add(creatorLedger);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(conversation.ConversationId);
    }

    public async Task<bool> IsConversationUnlockedAsync(int conversationId, int userId, CancellationToken cancellationToken = default)
    {
        var conversation = await dbContext.TblConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null) return false;

        int creatorUserId = conversation.UserOneId == userId ? conversation.UserTwoId : conversation.UserOneId;

        var setting = await dbContext.TblCreatorChatSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreatorUserId == creatorUserId, cancellationToken);

        if (setting is null || !setting.IsPrivateChatEnabled)
        {
            return true;
        }

        if (userId == creatorUserId) return true;

        return await dbContext.TblPrivateChatPaymentTransactions
            .AsNoTracking()
            .AnyAsync(t => t.ConversationId == conversationId && t.BuyerUserId == userId && t.Status == "COMPLETED", cancellationToken);
    }

    public async Task<bool> IsPrivateChatPaidRequiredAsync(int creatorUserId, CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.TblCreatorChatSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreatorUserId == creatorUserId, cancellationToken);

        return setting != null && setting.IsPrivateChatEnabled;
    }
}
