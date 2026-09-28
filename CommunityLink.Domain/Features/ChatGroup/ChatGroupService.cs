namespace CommunityLink.Domain.Features.ChatGroup;

using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.Admin;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

public sealed class ChatGroupService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    IPlatformSettingService platformSettingService
) : IChatGroupService
{
    public async Task<Result<ChatGroupModel>> CreateChatGroupAsync(CreateChatGroupRequestModel request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (!currentUser.IsPremiumCreator)
        {
            return Result<ChatGroupModel>.Failure("Only Premium Creators can create Chat Groups.", ResultStatus.Forbidden);
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<ChatGroupModel>.Failure("Chat Group name is required.");
        }

        if (name.Length > 200)
        {
            return Result<ChatGroupModel>.Failure("Chat Group name cannot exceed 200 characters.");
        }

        var chatType = string.Equals(request.ChatType, "PAID", StringComparison.OrdinalIgnoreCase) ? "PAID" : "FREE";
        var joinFee = chatType == "PAID" ? Math.Max(0, request.JoinFeeLinkDrops) : 0;

        var commissionRes = await platformSettingService.GetPlatformCommissionAsync(cancellationToken);
        var commissionSnapshot = commissionRes.IsSuccess && commissionRes.Data != null ? commissionRes.Data.CommissionPercentage : 10.00m;

        var chatGroup = new TblChatGroup
        {
            Name = name,
            Description = request.Description?.Trim(),
            AvatarUrl = request.AvatarUrl,
            CreatorId = currentUser.UserId.Value,
            ChatType = chatType,
            JoinFeeLinkDrops = joinFee,
            CommissionPercentageSnapshot = commissionSnapshot,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value
        };

        dbContext.TblChatGroups.Add(chatGroup);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Creator is automatically added as Owner
        var ownerMember = new TblChatGroupMember
        {
            ChatGroupId = chatGroup.ChatGroupId,
            UserId = currentUser.UserId.Value,
            Role = "OWNER",
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value
        };

        dbContext.TblChatGroupMembers.Add(ownerMember);
        await dbContext.SaveChangesAsync(cancellationToken);

        var creator = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == currentUser.UserId.Value, cancellationToken);

        var model = new ChatGroupModel(
            chatGroup.ChatGroupId,
            chatGroup.Name,
            chatGroup.Description,
            chatGroup.AvatarUrl,
            chatGroup.CreatorId,
            creator?.DisplayName ?? creator?.UserName ?? "Unknown",
            chatGroup.ChatType,
            chatGroup.JoinFeeLinkDrops,
            chatGroup.CommissionPercentageSnapshot,
            1,
            chatGroup.CreatedAt,
            IsJoined: true,
            UserRole: "OWNER"
        );

        return Result<ChatGroupModel>.Success(model);
    }

    public async Task<Result<IReadOnlyList<ChatGroupModel>>> GetChatGroupsAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblChatGroups
            .AsNoTracking()
            .Where(cg => !cg.IsDeleted && cg.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(cg => cg.Name.Contains(s) || (cg.Description != null && cg.Description.Contains(s)));
        }

        var userId = currentUser.UserId;

        var groups = await query
            .OrderByDescending(cg => cg.CreatedAt)
            .Select(cg => new
            {
                Group = cg,
                CreatorName = cg.Creator.DisplayName ?? cg.Creator.UserName,
                MemberCount = cg.TblChatGroupMembers.Count(m => !m.IsDeleted),
                UserMembership = userId.HasValue
                    ? cg.TblChatGroupMembers.FirstOrDefault(m => m.UserId == userId.Value && !m.IsDeleted)
                    : null
            })
            .ToListAsync(cancellationToken);

        var result = groups.Select(x => new ChatGroupModel(
            x.Group.ChatGroupId,
            x.Group.Name,
            x.Group.Description,
            x.Group.AvatarUrl,
            x.Group.CreatorId,
            x.CreatorName,
            x.Group.ChatType,
            x.Group.JoinFeeLinkDrops,
            x.Group.CommissionPercentageSnapshot,
            x.MemberCount,
            x.Group.CreatedAt,
            IsJoined: x.UserMembership != null,
            UserRole: x.UserMembership?.Role ?? "NONE"
        )).ToList();

        return Result<IReadOnlyList<ChatGroupModel>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyChatGroupsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupModel>>.Failure("User is not authenticated.");
        }

        var userId = currentUser.UserId.Value;

        var groups = await dbContext.TblChatGroups
            .AsNoTracking()
            .Where(cg => !cg.IsDeleted && cg.IsActive &&
                (cg.CreatorId == userId || cg.TblChatGroupMembers.Any(m => m.UserId == userId && !m.IsDeleted)))
            .Select(cg => new
            {
                Group = cg,
                CreatorName = cg.Creator.DisplayName ?? cg.Creator.UserName,
                MemberCount = cg.TblChatGroupMembers.Count(m => !m.IsDeleted),
                UserMembership = cg.TblChatGroupMembers.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var result = groups.Select(x => new ChatGroupModel(
            x.Group.ChatGroupId,
            x.Group.Name,
            x.Group.Description,
            x.Group.AvatarUrl,
            x.Group.CreatorId,
            x.CreatorName,
            x.Group.ChatType,
            x.Group.JoinFeeLinkDrops,
            x.Group.CommissionPercentageSnapshot,
            x.MemberCount,
            x.Group.CreatedAt,
            IsJoined: x.UserMembership != null,
            UserRole: x.UserMembership?.Role ?? "NONE"
        )).ToList();

        return Result<IReadOnlyList<ChatGroupModel>>.Success(result);
    }

    public async Task<Result<ChatGroupModel>> GetChatGroupByIdAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        var groupData = await dbContext.TblChatGroups
            .AsNoTracking()
            .Where(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted)
            .Select(cg => new
            {
                Group = cg,
                CreatorName = cg.Creator.DisplayName ?? cg.Creator.UserName,
                MemberCount = cg.TblChatGroupMembers.Count(m => !m.IsDeleted),
                UserMembership = currentUser.UserId.HasValue
                    ? cg.TblChatGroupMembers.FirstOrDefault(m => m.UserId == currentUser.UserId.Value && !m.IsDeleted)
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (groupData == null)
        {
            return Result<ChatGroupModel>.Failure("Chat Group not found.");
        }

        var model = new ChatGroupModel(
            groupData.Group.ChatGroupId,
            groupData.Group.Name,
            groupData.Group.Description,
            groupData.Group.AvatarUrl,
            groupData.Group.CreatorId,
            groupData.CreatorName,
            groupData.Group.ChatType,
            groupData.Group.JoinFeeLinkDrops,
            groupData.Group.CommissionPercentageSnapshot,
            groupData.MemberCount,
            groupData.Group.CreatedAt,
            IsJoined: groupData.UserMembership != null,
            UserRole: groupData.UserMembership?.Role ?? "NONE"
        );

        return Result<ChatGroupModel>.Success(model);
    }

    public async Task<Result> JoinChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!chatGroup.IsActive)
        {
            return Result.Failure("Chat Group is not active.", ResultStatus.ValidationError);
        }

        var existingMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId, cancellationToken);

        if (existingMember != null && !existingMember.IsDeleted)
        {
            return Result.Failure("Already joined this Chat Group.", ResultStatus.Conflict);
        }

        if (string.Equals(chatGroup.ChatType, "PAID", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Paid Chat Group requires Link Drop payment.", ResultStatus.ValidationError);
        }

        if (existingMember != null)
        {
            existingMember.IsDeleted = false;
            existingMember.JoinedAt = DateTime.UtcNow;
            existingMember.Role = "MEMBER";
            existingMember.UpdatedAt = DateTime.UtcNow;
            existingMember.UpdatedBy = userId;
        }
        else
        {
            var newMember = new TblChatGroupMember
            {
                ChatGroupId = chatGroupId,
                UserId = userId,
                Role = "MEMBER",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            dbContext.TblChatGroupMembers.Add(newMember);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Joined Chat Group successfully.");
    }

    public async Task<Result> LeaveChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        var member = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (member == null)
        {
            return Result.Failure("You are not a member of this Chat Group.", ResultStatus.NotFound);
        }

        if (string.Equals(member.Role, "OWNER", StringComparison.OrdinalIgnoreCase) || chatGroup.CreatorId == userId)
        {
            return Result.Failure("Owner cannot leave their own Chat Group.", ResultStatus.ValidationError);
        }

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
        member.DeletedBy = userId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Left Chat Group successfully.");
    }

    public async Task<Result<IReadOnlyList<ChatGroupMemberModel>>> GetMembersAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        var chatGroupExists = await dbContext.TblChatGroups
            .AnyAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (!chatGroupExists)
        {
            return Result<IReadOnlyList<ChatGroupMemberModel>>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        var members = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && !m.IsDeleted)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new ChatGroupMemberModel(
                m.ChatGroupMemberId,
                m.ChatGroupId,
                m.UserId,
                m.User.UserName,
                m.User.DisplayName ?? m.User.UserName,
                m.User.AvatarUrl,
                m.Role,
                m.JoinedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ChatGroupMemberModel>>.Success(members);
    }

    public async Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyMembershipsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var groups = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId && !m.IsDeleted && !m.ChatGroup.IsDeleted && m.ChatGroup.IsActive)
            .Select(m => new
            {
                Group = m.ChatGroup,
                CreatorName = m.ChatGroup.Creator.DisplayName ?? m.ChatGroup.Creator.UserName,
                MemberCount = m.ChatGroup.TblChatGroupMembers.Count(x => !x.IsDeleted),
                UserRole = m.Role
            })
            .ToListAsync(cancellationToken);

        var result = groups.Select(x => new ChatGroupModel(
            x.Group.ChatGroupId,
            x.Group.Name,
            x.Group.Description,
            x.Group.AvatarUrl,
            x.Group.CreatorId,
            x.CreatorName,
            x.Group.ChatType,
            x.Group.JoinFeeLinkDrops,
            x.Group.CommissionPercentageSnapshot,
            x.MemberCount,
            x.Group.CreatedAt,
            IsJoined: true,
            UserRole: x.UserRole
        )).ToList();

        return Result<IReadOnlyList<ChatGroupModel>>.Success(result);
    }

    public async Task<Result> JoinPaidChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!chatGroup.IsActive)
        {
            return Result.Failure("Chat Group is not active.", ResultStatus.ValidationError);
        }

        if (!string.Equals(chatGroup.ChatType, "PAID", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("This Chat Group is free. Use standard join.", ResultStatus.ValidationError);
        }

        var existingMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId, cancellationToken);

        if (existingMember != null && !existingMember.IsDeleted)
        {
            return Result.Failure("Already joined this Chat Group.", ResultStatus.Conflict);
        }

        var userWallet = await dbContext.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        long fee = chatGroup.JoinFeeLinkDrops;

        if (userWallet == null || userWallet.Balance < fee)
        {
            return Result.Failure("Insufficient Link Drops balance.", ResultStatus.ValidationError);
        }

        // Safety check for legacy wallets
        if (userWallet.PurchasedBalance == 0 && userWallet.EarnedBalance == 0 && userWallet.Balance > 0)
        {
            userWallet.PurchasedBalance = userWallet.Balance;
        }

        long userBalanceBefore = userWallet.Balance;
        long purchasedDeducted = Math.Min(userWallet.PurchasedBalance, fee);
        long remainingToDeduct = fee - purchasedDeducted;
        long earnedDeducted = remainingToDeduct;

        userWallet.PurchasedBalance -= purchasedDeducted;
        userWallet.EarnedBalance -= earnedDeducted;
        userWallet.Balance = userWallet.PurchasedBalance + userWallet.EarnedBalance;
        userWallet.UpdatedAt = DateTime.UtcNow;
        userWallet.UpdatedBy = userId;

        // Platform Commission Calculation
        var commissionRes = await platformSettingService.GetPlatformCommissionAsync(cancellationToken);
        decimal commissionRate = commissionRes.IsSuccess && commissionRes.Data != null
            ? commissionRes.Data.CommissionPercentage
            : chatGroup.CommissionPercentageSnapshot;

        long commissionAmount = (long)Math.Round(fee * (commissionRate / 100m), MidpointRounding.AwayFromZero);
        long netAmount = fee - commissionAmount;

        // Creator Wallet Credit
        var creatorWallet = await dbContext.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == chatGroup.CreatorId, cancellationToken);

        if (creatorWallet == null)
        {
            creatorWallet = new TblLinkDropWallet
            {
                UserId = chatGroup.CreatorId,
                Balance = 0,
                PurchasedBalance = 0,
                EarnedBalance = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            dbContext.TblLinkDropWallets.Add(creatorWallet);
        }

        long creatorBalanceBefore = creatorWallet.Balance;
        creatorWallet.EarnedBalance += netAmount;
        creatorWallet.Balance = creatorWallet.PurchasedBalance + creatorWallet.EarnedBalance;
        creatorWallet.UpdatedAt = DateTime.UtcNow;
        creatorWallet.UpdatedBy = userId;

        // Create ChatGroupPaymentTransaction
        var paymentTransaction = new TblChatGroupPaymentTransaction
        {
            ChatGroupId = chatGroupId,
            UserId = userId,
            CreatorUserId = chatGroup.CreatorId,
            GrossAmount = fee,
            CommissionPercentage = commissionRate,
            CommissionAmount = commissionAmount,
            NetAmount = netAmount,
            PurchasedAmountDeducted = purchasedDeducted,
            EarnedAmountDeducted = earnedDeducted,
            Status = "COMPLETED",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblChatGroupPaymentTransactions.Add(paymentTransaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Audit Ledgers
        var userLedger = new TblLinkDropTransaction
        {
            WalletId = userWallet.WalletId,
            UserId = userId,
            TransactionType = "CHAT_GROUP_JOIN",
            Amount = fee,
            BalanceBefore = userBalanceBefore,
            BalanceAfter = userWallet.Balance,
            PurchasedAmountDeducted = purchasedDeducted,
            EarnedAmountDeducted = earnedDeducted,
            RelatedUserId = chatGroup.CreatorId,
            ReferenceType = "TblChatGroupPaymentTransaction",
            ReferenceId = (int)paymentTransaction.PaymentTransactionId,
            Notes = $"Paid {fee} Link Drops to join Chat Group '{chatGroup.Name}'",
            CreatedAt = DateTime.UtcNow
        };

        var creatorLedger = new TblLinkDropTransaction
        {
            WalletId = creatorWallet.WalletId,
            UserId = chatGroup.CreatorId,
            TransactionType = "CHAT_GROUP_EARNING",
            Amount = netAmount,
            BalanceBefore = creatorBalanceBefore,
            BalanceAfter = creatorWallet.Balance,
            PurchasedAmountDeducted = 0,
            EarnedAmountDeducted = 0,
            RelatedUserId = userId,
            ReferenceType = "TblChatGroupPaymentTransaction",
            ReferenceId = (int)paymentTransaction.PaymentTransactionId,
            Notes = $"Earned {netAmount} Link Drops from user join in Chat Group '{chatGroup.Name}' (Fee: {fee}, Commission: {commissionAmount})",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblLinkDropTransactions.Add(userLedger);
        dbContext.TblLinkDropTransactions.Add(creatorLedger);

        // Add or Restore Member
        if (existingMember != null)
        {
            existingMember.IsDeleted = false;
            existingMember.JoinedAt = DateTime.UtcNow;
            existingMember.Role = "MEMBER";
            existingMember.UpdatedAt = DateTime.UtcNow;
            existingMember.UpdatedBy = userId;
        }
        else
        {
            var newMember = new TblChatGroupMember
            {
                ChatGroupId = chatGroupId,
                UserId = userId,
                Role = "MEMBER",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            dbContext.TblChatGroupMembers.Add(newMember);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Successfully joined Paid Chat Group.");
    }

    public async Task<Result<IReadOnlyList<ChatGroupMessageModel>>> GetMessagesAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupMessageModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var isMember = await dbContext.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (!isMember)
        {
            return Result<IReadOnlyList<ChatGroupMessageModel>>.Failure("You are not a member of this Chat Group.", ResultStatus.Forbidden);
        }

        // Resolved once here so the client is never offered a moderation action the service
        // would reject.
        var canModerate = await IsModeratorAsync(chatGroupId, userId, cancellationToken);

        // Messages hidden for this member are filtered out, which is what makes
        // "Delete for myself" a per-viewer view rather than a client-side trick.
        var rawMessages = await dbContext.TblChatGroupMessages
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId &&
                        !m.IsDeleted &&
                        !dbContext.TblChatGroupMessageUserStates.Any(s =>
                            s.ChatGroupMessageId == m.ChatGroupMessageId && s.UserId == userId && s.IsHidden))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                m.ChatGroupMessageId,
                m.ChatGroupId,
                m.SenderId,
                SenderName = m.Sender.UserName,
                SenderDisplayName = m.Sender.DisplayName ?? m.Sender.UserName,
                SenderAvatar = m.Sender.AvatarUrl,
                m.Content,
                m.CreatedAt,
                m.ReplyToChatGroupMessageId
            })
            .ToListAsync(cancellationToken);

        var replies = await GetReplyStubsAsync(
            rawMessages.Where(m => m.ReplyToChatGroupMessageId.HasValue)
                .Select(m => m.ReplyToChatGroupMessageId!.Value)
                .Distinct()
                .ToList(),
            cancellationToken);

        var reactions = await GetReactionsAsync(
            rawMessages.Select(m => m.ChatGroupMessageId).ToList(),
            cancellationToken);

        var result = rawMessages.Select(m =>
        {
            var hasReply = replies.TryGetValue(m.ReplyToChatGroupMessageId ?? -1, out var reply);

            return new ChatGroupMessageModel(
                m.ChatGroupMessageId,
                m.ChatGroupId,
                m.SenderId,
                m.SenderName,
                m.SenderDisplayName,
                m.SenderAvatar,
                m.Content,
                m.CreatedAt,
                IsMine: m.SenderId == userId,
                m.ReplyToChatGroupMessageId,
                hasReply ? reply.SenderName : null,
                hasReply ? reply.Preview : null,
                hasReply && reply.IsDeleted,
                reactions.TryGetValue(m.ChatGroupMessageId, out var list) ? list : Array.Empty<MessageReactionModel>(),
                m.SenderId == userId || canModerate
            );
        }).ToList();

        return Result<IReadOnlyList<ChatGroupMessageModel>>.Success(result);
    }

    /// <summary>OWNER and ADMIN may moderate other members' messages.</summary>
    private async Task<bool> IsModeratorAsync(int chatGroupId, int userId, CancellationToken cancellationToken)
    {
        var role = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(role, "OWNER", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves the quoted sender and a truncated preview for replied-to group message ids.
    /// Soft-deleted originals come back as stubs so the client can render the tombstone.
    /// </summary>
    private async Task<Dictionary<int, (string SenderName, string Preview, bool IsDeleted)>> GetReplyStubsAsync(
        List<int> messageIds,
        CancellationToken cancellationToken)
    {
        var stubs = new Dictionary<int, (string, string, bool)>();

        if (messageIds.Count == 0)
        {
            return stubs;
        }

        var rows = await dbContext.TblChatGroupMessages
            .AsNoTracking()
            .Where(m => messageIds.Contains(m.ChatGroupMessageId))
            .Select(m => new
            {
                m.ChatGroupMessageId,
                SenderName = m.Sender.DisplayName ?? m.Sender.UserName,
                m.Content,
                m.IsDeleted
            })
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            stubs[row.ChatGroupMessageId] = (row.SenderName, TruncatePreview(row.Content), row.IsDeleted);
        }

        return stubs;
    }

    /// <summary>Loads the flat reaction rows for a page of group messages, keyed by message id.</summary>
    private async Task<Dictionary<int, IReadOnlyList<MessageReactionModel>>> GetReactionsAsync(
        List<int> messageIds,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, IReadOnlyList<MessageReactionModel>>();

        if (messageIds.Count == 0)
        {
            return result;
        }

        var rows = await dbContext.TblChatGroupMessageReactions
            .AsNoTracking()
            .Where(r => messageIds.Contains(r.ChatGroupMessageId))
            .OrderBy(r => r.CreatedAt)
            .Select(r => new
            {
                r.ChatGroupMessageId,
                r.UserId,
                UserName = r.User.DisplayName,
                r.Emoji
            })
            .ToListAsync(cancellationToken);

        foreach (var group in rows.GroupBy(r => r.ChatGroupMessageId))
        {
            result[group.Key] = group
                .Select(r => new MessageReactionModel(r.UserId, r.UserName, r.Emoji))
                .ToList();
        }

        return result;
    }

    private static string TruncatePreview(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var single = text.ReplaceLineEndings(" ").Trim();
        return single.Length <= 120 ? single : single[..120] + "…";
    }

    public async Task<Result<ChatGroupMessageModel>> SendMessageAsync(int chatGroupId, SendChatGroupMessageRequestModel request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupMessageModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var text = request?.Content?.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result<ChatGroupMessageModel>.Failure("Message content cannot be empty.");
        }

        var userId = currentUser.UserId.Value;

        var isMember = await dbContext.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (!isMember)
        {
            return Result<ChatGroupMessageModel>.Failure("You must join this Chat Group to send messages.", ResultStatus.Forbidden);
        }

        // A reply must point at a live message in the same group, so a member cannot quote a
        // message from a group they never joined.
        var replyToId = request?.ReplyToChatGroupMessageId;
        if (replyToId.HasValue)
        {
            var isValidReply = await dbContext.TblChatGroupMessages
                .AsNoTracking()
                .AnyAsync(m => m.ChatGroupMessageId == replyToId.Value &&
                               m.ChatGroupId == chatGroupId &&
                               !m.IsDeleted,
                    cancellationToken);

            if (!isValidReply)
            {
                return Result<ChatGroupMessageModel>.Failure("The message you replied to is no longer available.");
            }
        }

        var msg = new TblChatGroupMessage
        {
            ChatGroupId = chatGroupId,
            SenderId = userId,
            Content = text,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            ReplyToChatGroupMessageId = replyToId
        };

        dbContext.TblChatGroupMessages.Add(msg);
        await dbContext.SaveChangesAsync(cancellationToken);

        var sender = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        var replies = await GetReplyStubsAsync(
            replyToId.HasValue ? [replyToId.Value] : [],
            cancellationToken);
        var hasReply = replies.TryGetValue(replyToId ?? -1, out var replyStub);

        var model = new ChatGroupMessageModel(
            msg.ChatGroupMessageId,
            msg.ChatGroupId,
            msg.SenderId,
            sender?.UserName ?? "User",
            sender?.DisplayName ?? sender?.UserName ?? "User",
            sender?.AvatarUrl,
            msg.Content,
            msg.CreatedAt,
            IsMine: true,
            msg.ReplyToChatGroupMessageId,
            hasReply ? replyStub.SenderName : null,
            hasReply ? replyStub.Preview : null,
            hasReply && replyStub.IsDeleted,
            Array.Empty<MessageReactionModel>(),
            CanDeleteForEveryone: true
        );

        return Result<ChatGroupMessageModel>.Success(model);
    }

    public async Task<Result> DeleteMessageForSelfAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var message = await FindVisibleMessageAsync(chatGroupId, messageId, userId, cancellationToken);
        if (message is null)
        {
            return Result.Failure("Message not found.", ResultStatus.NotFound);
        }

        var state = await dbContext.TblChatGroupMessageUserStates
            .FirstOrDefaultAsync(s => s.ChatGroupMessageId == messageId && s.UserId == userId, cancellationToken);

        if (state is null)
        {
            dbContext.TblChatGroupMessageUserStates.Add(new TblChatGroupMessageUserState
            {
                ChatGroupMessageId = messageId,
                UserId = userId,
                IsHidden = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (state.IsHidden)
        {
            return Result.Success("Message deleted for you.");
        }
        else
        {
            state.IsHidden = true;
            state.UpdatedAt = DateTime.UtcNow;
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A second "delete for me" from another tab can win the race to insert the unique
            // (message, user) row. The end state is the one we wanted, so this is not an error.
            dbContext.ChangeTracker.Clear();
        }

        // No broadcast: hiding is per-viewer and the rest of the group must still see it.
        return Result.Success("Message deleted for you.");
    }

    public async Task<Result<IReadOnlyList<MessageReactionModel>>> SetReactionAsync(
        int chatGroupId, int messageId, string? emoji, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var isMember = await dbContext.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (!isMember)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure(
                "You must join this Chat Group to react.", ResultStatus.Forbidden);
        }

        var message = await FindVisibleMessageAsync(chatGroupId, messageId, userId, cancellationToken);
        if (message is null)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("Message not found.", ResultStatus.NotFound);
        }

        var canonical = MessageEmoji.Normalize(emoji);
        if (canonical is null)
        {
            return Result<IReadOnlyList<MessageReactionModel>>.Failure("That reaction is not supported.");
        }

        var existing = await dbContext.TblChatGroupMessageReactions
            .FirstOrDefaultAsync(r => r.ChatGroupMessageId == messageId && r.UserId == userId, cancellationToken);

        if (existing is null)
        {
            dbContext.TblChatGroupMessageReactions.Add(new TblChatGroupMessageReaction
            {
                ChatGroupMessageId = messageId,
                UserId = userId,
                Emoji = canonical,
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (string.Equals(existing.Emoji, canonical, StringComparison.Ordinal))
        {
            // Same emoji again clears the reaction, matching the Telegram toggle.
            dbContext.TblChatGroupMessageReactions.Remove(existing);
        }
        else
        {
            // One reaction per person per message: swap in place instead of adding a row.
            existing.Emoji = canonical;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ChangeTracker.Clear();
        var reactions = (await GetReactionsAsync([messageId], cancellationToken))
            .TryGetValue(messageId, out var list)
            ? list
            : Array.Empty<MessageReactionModel>();

        return Result<IReadOnlyList<MessageReactionModel>>.Success(reactions);
    }

    /// <summary>
    /// Loads a message the caller may act on: in their group, not deleted for everyone, and not
    /// hidden for themselves.
    /// </summary>
    private async Task<TblChatGroupMessage?> FindVisibleMessageAsync(
        int chatGroupId, int messageId, int userId, CancellationToken cancellationToken)
    {
        return await dbContext.TblChatGroupMessages
            .FirstOrDefaultAsync(m => m.ChatGroupMessageId == messageId &&
                                       m.ChatGroupId == chatGroupId &&
                                       !m.IsDeleted &&
                                       !dbContext.TblChatGroupMessageUserStates.Any(s =>
                                           s.ChatGroupMessageId == messageId && s.UserId == userId && s.IsHidden),
                cancellationToken);
    }

    public async Task<Result> DeleteMessageAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var isMember = await dbContext.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (!isMember)
        {
            return Result.Failure("You are not a member of this Chat Group.", ResultStatus.Forbidden);
        }

        var msg = await dbContext.TblChatGroupMessages
            .FirstOrDefaultAsync(m => m.ChatGroupMessageId == messageId && m.ChatGroupId == chatGroupId && !m.IsDeleted, cancellationToken);

        if (msg == null)
        {
            return Result.Failure("Message not found.", ResultStatus.NotFound);
        }

        // The author may always retract; OWNER/ADMIN may moderate anyone.
        if (msg.SenderId != userId && !await IsModeratorAsync(chatGroupId, userId, cancellationToken))
        {
            return Result.Failure("You can only delete your own messages.", ResultStatus.Forbidden);
        }

        var deleted = false;

        // TblChatGroupMessage is rowversion-stamped, so a delete racing another writer throws.
        // The end state is identical either way, so re-read and retry once.
        for (var attempt = 0; attempt < 2 && !deleted; attempt++)
        {
            var row = await dbContext.TblChatGroupMessages
                .FirstOrDefaultAsync(m => m.ChatGroupMessageId == messageId && m.ChatGroupId == chatGroupId && !m.IsDeleted, cancellationToken);

            if (row == null)
            {
                deleted = true;
                break;
            }

            row.IsDeleted = true;
            row.DeletedAt = DateTime.UtcNow;
            row.DeletedBy = userId;
            row.UpdatedAt = DateTime.UtcNow;

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                deleted = true;
            }
            catch (DbUpdateConcurrencyException) when (attempt == 0)
            {
                dbContext.ChangeTracker.Clear();
            }
        }

        return deleted
            ? Result.Success("Message deleted.")
            : Result.Failure("Message could not be deleted.", ResultStatus.SystemError);
    }

    /// <summary>
    /// Newest message per group, in one grouped query. TblChatGroup has no denormalized
    /// last-message columns, so the unified Chat list needs this to order and preview
    /// group threads alongside 1:1 conversations.
    /// </summary>
    public async Task<Result<IReadOnlyList<ChatGroupPreviewModel>>> GetPreviewsAsync(
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupPreviewModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var joinedGroupIds = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.ChatGroupId)
            .ToListAsync(cancellationToken);

        if (joinedGroupIds.Count == 0)
        {
            return Result<IReadOnlyList<ChatGroupPreviewModel>>.Success(Array.Empty<ChatGroupPreviewModel>());
        }

        var previews = await dbContext.TblChatGroupMessages
            .AsNoTracking()
            .Where(m => joinedGroupIds.Contains(m.ChatGroupId) && !m.IsDeleted)
            .GroupBy(m => m.ChatGroupId)
            .Select(g => g
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.ChatGroupMessageId)
                .Select(m => new ChatGroupPreviewModel(
                    m.ChatGroupId,
                    m.Content != null && m.Content.Length > 120 ? m.Content.Substring(0, 120) : m.Content,
                    m.CreatedAt,
                    m.Sender.DisplayName ?? m.Sender.UserName))
                .First())
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ChatGroupPreviewModel>>.Success(previews);
    }
}
