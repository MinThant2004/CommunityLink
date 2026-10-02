namespace CommunityLink.Domain.Features.ChatGroup;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.Admin;
using CommunityLink.Domain.Features.Notification;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;
using CommunityLink.Shared.Utils;

public sealed class ChatGroupService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    IPlatformSettingService platformSettingService,
    IChatGroupImageStorage imageStorage,
    IHubContext<ChatGroupHub> hubContext,
    INotificationService notificationService,
    CommunityLink.Domain.Features.Chat.PresenceTracker presenceTracker
) : IChatGroupService
{
    /// <summary>
    /// A group that charges a real fee. The add/invite path keys off this rather than ChatType
    /// alone, because CreateChatGroupAsync accepts a PAID group with a zero fee, and gating a
    /// free group behind an invite would add friction for nothing.
    /// </summary>
    private static bool RequiresPaidInvite(TblChatGroup group) =>
        string.Equals(group.ChatType, "PAID", StringComparison.OrdinalIgnoreCase)
        && group.JoinFeeLinkDrops > 0;

    // Invite lifecycle. Only PENDING is actionable; the terminal states are retained for audit.
    // The values match the CHECK constraint on CK_TblChatGroupInvite_Status.
    private const string InviteStatusPending = "PENDING";
    private const string InviteStatusAccepted = "ACCEPTED";
    private const string InviteStatusRevoked = "REVOKED";
    private const string InviteStatusDeclined = "DECLINED";

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
                (cg.CreatorId == userId ||
                 cg.TblChatGroupMembers.Any(m => m.UserId == userId && !m.IsDeleted) ||
                 dbContext.TblChatGroupBans.Any(b => b.ChatGroupId == cg.ChatGroupId && b.UserId == userId && !b.IsDeleted)))
            .Select(cg => new
            {
                Group = cg,
                CreatorName = cg.Creator.DisplayName ?? cg.Creator.UserName,
                MemberCount = cg.TblChatGroupMembers.Count(m => !m.IsDeleted),
                UserMembership = cg.TblChatGroupMembers.FirstOrDefault(m => m.UserId == userId && !m.IsDeleted),
                IsBanned = dbContext.TblChatGroupBans.Any(b => b.ChatGroupId == cg.ChatGroupId && b.UserId == userId && !b.IsDeleted)
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
            UserRole: x.UserMembership?.Role ?? "NONE",
            IsBanned: x.IsBanned
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
                    : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (groupData == null)
        {
            // Explicit NotFound: a bare Failure() maps to 400, which misreports a missing or
            // soft-deleted group as a client error rather than a missing resource.
            return Result<ChatGroupModel>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (currentUser.UserId.HasValue && await IsBannedAsync(chatGroupId, currentUser.UserId.Value, cancellationToken))
        {
            return Result<ChatGroupModel>.Failure("You are banned from this Chat Group.", ResultStatus.Forbidden);
        }

        // Resolved separately from the group projection: an invite is a thin row and the group
        // query above is the hot path, so this only runs for a viewer who might have one.
        // A PENDING invite makes the group visible with the paywall still applied, which is what
        // the invite notification's deep link lands on.
        var pendingInvite = currentUser.UserId.HasValue
            ? await dbContext.TblChatGroupInvites
                .AsNoTracking()
                .Where(i => i.ChatGroupId == chatGroupId
                    && i.UserId == currentUser.UserId.Value
                    && i.Status == InviteStatusPending)
                .Select(i => new
                {
                    i.FeeAtInviteLinkDrops,
                    InviterName = i.InvitedByUser.DisplayName ?? i.InvitedByUser.UserName
                })
                .FirstOrDefaultAsync(cancellationToken)
            : null;

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
            UserRole: groupData.UserMembership?.Role ?? "NONE",
            IsInvited: pendingInvite != null,
            // The quoted price wins over the group's current fee so an open invite cannot be
            // repriced if the owner changes the fee after inviting.
            InviteFeeLinkDrops: pendingInvite?.FeeAtInviteLinkDrops,
            InvitedByName: pendingInvite?.InviterName
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

        // A ban blocks self-service join as well as the owner's direct add. Checked before
        // the fee branch so a banned user gets the ban message rather than a payment prompt.
        if (await IsBannedAsync(chatGroupId, userId, cancellationToken))
        {
            return Result.Failure("You are banned from this Chat Group.", ResultStatus.Forbidden);
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

        var user = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        var userName = !string.IsNullOrWhiteSpace(user?.DisplayName) ? user.DisplayName : (user?.UserName ?? "User");

        var systemMsg = new TblChatGroupMessage
        {
            ChatGroupId = chatGroupId,
            SenderId = userId,
            Content = $"{userName} joined the group",
            MessageType = "SYSTEM",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        dbContext.TblChatGroupMessages.Add(systemMsg);

        await dbContext.SaveChangesAsync(cancellationToken);

        var msgModel = new ChatGroupMessageModel(
            ChatGroupMessageId: systemMsg.ChatGroupMessageId,
            ChatGroupId: chatGroupId,
            SenderId: userId,
            SenderName: userName,
            SenderDisplayName: userName,
            SenderAvatar: user?.AvatarUrl,
            Content: systemMsg.Content,
            CreatedAt: systemMsg.CreatedAt,
            MessageType: "SYSTEM");

        await BroadcastAsync(chatGroupId, "ReceiveChatGroupMessage", new object[] { msgModel }, cancellationToken);

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

        // The owner is the group's only revenue recipient, so ownership is deliberately not
        // transferable: there is no co-owner tier and no hand-off. Rather than orphan a group
        // whose owner account is lost, an owner leaving takes the whole group with them. The
        // cascade and the audit-retention rules are identical to an explicit delete.
        if (string.Equals(member.Role, "OWNER", StringComparison.OrdinalIgnoreCase) || chatGroup.CreatorId == userId)
        {
            var deleteResult = await DeleteChatGroupAsync(chatGroupId, cancellationToken);
            return deleteResult.IsSuccess
                ? Result.Success("Leaving as owner deleted the Chat Group.")
                : deleteResult;
        }

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
        member.DeletedBy = userId;

        var user = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        var userName = !string.IsNullOrWhiteSpace(user?.DisplayName) ? user.DisplayName : (user?.UserName ?? "User");

        var systemMsg = new TblChatGroupMessage
        {
            ChatGroupId = chatGroupId,
            SenderId = userId,
            Content = $"{userName} left the group",
            MessageType = "SYSTEM",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        dbContext.TblChatGroupMessages.Add(systemMsg);

        await dbContext.SaveChangesAsync(cancellationToken);

        var msgModel = new ChatGroupMessageModel(
            ChatGroupMessageId: systemMsg.ChatGroupMessageId,
            ChatGroupId: chatGroupId,
            SenderId: userId,
            SenderName: userName,
            SenderDisplayName: userName,
            SenderAvatar: user?.AvatarUrl,
            Content: systemMsg.Content,
            CreatedAt: systemMsg.CreatedAt,
            MessageType: "SYSTEM");

        await BroadcastAsync(chatGroupId, "ReceiveChatGroupMessage", new object[] { msgModel }, cancellationToken);

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

        // The roster is member-only. Existence is checked first so a missing group still 404s
        // rather than being masked as a 403, and so the two failure modes stay distinguishable.
        var membership = await IsActiveMemberAsync(chatGroupId, cancellationToken);
        if (membership.IsError)
        {
            return Result<IReadOnlyList<ChatGroupMemberModel>>.Failure(
                membership.Message, ResultStatus.Forbidden);
        }

        // The roster is also the surface the owner edits the permission matrix on, so it is only
        // served to a member who can act on what they see.

        var members = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && !m.IsDeleted)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new
            {
                m.ChatGroupMemberId,
                m.ChatGroupId,
                m.UserId,
                UserName = m.User.UserName,
                DisplayName = m.User.DisplayName ?? m.User.UserName,
                AvatarUrl = m.User.AvatarUrl,
                m.Role,
                m.JoinedAt,
                m.IsMuted,
                m.CanDeleteMessages,
                m.CanRemoveMembers,
                m.CanBanMembers,
                m.CanManageInviteLinks,
                m.CanPinMessages,
                m.User.LastActiveAt
            })
            .ToListAsync(cancellationToken);

        var result = members
            .Select(m => new ChatGroupMemberModel(
                m.ChatGroupMemberId,
                m.ChatGroupId,
                m.UserId,
                m.UserName,
                m.DisplayName,
                m.AvatarUrl,
                m.Role,
                m.JoinedAt,
                m.IsMuted,
                ToPermissionSet(
                    m.Role,
                    m.CanDeleteMessages,
                    m.CanRemoveMembers,
                    m.CanBanMembers,
                    m.CanManageInviteLinks,
                    m.CanPinMessages),
                IsOnline: presenceTracker.IsUserOnline(m.UserId),
                LastActiveAt: m.LastActiveAt))
            .ToList();

        return Result<IReadOnlyList<ChatGroupMemberModel>>.Success(result);
    }

    public async Task<Result> IsActiveMemberAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var isMember = await IsActiveMemberAsync(chatGroupId, currentUser.UserId.Value, cancellationToken);
        if (!isMember.IsSuccess || isMember.Data != true)
        {
            return Result.Failure(
                "You must be a member of this Chat Group to view its members.",
                ResultStatus.Forbidden);
        }

        return Result.Success();
    }

    public async Task<Result<bool>> IsActiveMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default) =>
        await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken)
            ? Result<bool>.Success(true)
            : Result<bool>.Success(false);

    public async Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyMembershipsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var memberGroups = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId && !m.IsDeleted && !m.ChatGroup.IsDeleted && m.ChatGroup.IsActive)
            .Select(m => new
            {
                Group = m.ChatGroup,
                CreatorName = m.ChatGroup.Creator.DisplayName ?? m.ChatGroup.Creator.UserName,
                MemberCount = m.ChatGroup.TblChatGroupMembers.Count(x => !x.IsDeleted),
                UserRole = m.Role,
                IsBanned = false
            })
            .ToListAsync(cancellationToken);

        var bannedGroups = await dbContext.TblChatGroupBans
            .AsNoTracking()
            .Where(b => b.UserId == userId && !b.IsDeleted && !b.ChatGroup.IsDeleted && b.ChatGroup.IsActive)
            .Select(b => new
            {
                Group = b.ChatGroup,
                CreatorName = b.ChatGroup.Creator.DisplayName ?? b.ChatGroup.Creator.UserName,
                MemberCount = b.ChatGroup.TblChatGroupMembers.Count(x => !x.IsDeleted),
                UserRole = "NONE",
                IsBanned = true
            })
            .ToListAsync(cancellationToken);

        var memberGroupIds = memberGroups.Select(x => x.Group.ChatGroupId).ToHashSet();
        var allGroups = memberGroups.Concat(bannedGroups.Where(b => !memberGroupIds.Contains(b.Group.ChatGroupId))).ToList();

        var result = allGroups.Select(x => new ChatGroupModel(
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
            IsJoined: !x.IsBanned,
            UserRole: x.UserRole,
            IsBanned: x.IsBanned
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

        // A ban blocks the paid path too, and is checked before the wallet is touched so a
        // banned user is never charged.
        if (await IsBannedAsync(chatGroupId, userId, cancellationToken))
        {
            return Result.Failure("You are banned from this Chat Group.", ResultStatus.Forbidden);
        }

        var existingMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId, cancellationToken);

        if (existingMember != null && !existingMember.IsDeleted)
        {
            return Result.Failure("Already joined this Chat Group.", ResultStatus.Conflict);
        }

        // An open invite is honoured at the price it was quoted, so an owner raising the fee after
        // inviting cannot reprice an offer the invitee has already seen. Without an invite this is
        // the ordinary self-service join and the live fee applies.
        var pendingInvite = await dbContext.TblChatGroupInvites
            .AsNoTracking()
            .Where(i => i.ChatGroupId == chatGroupId && i.UserId == userId && i.Status == InviteStatusPending)
            .Select(i => new { i.ChatGroupInviteId, i.FeeAtInviteLinkDrops })
            .FirstOrDefaultAsync(cancellationToken);

        var userWallet = await dbContext.TblLinkDropWallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        long fee = pendingInvite?.FeeAtInviteLinkDrops ?? chatGroup.JoinFeeLinkDrops;

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

        // The payment is the only thing that converts an invite, so the invite is resolved here
        // rather than on the add. The row is retained as ACCEPTED so the creator can see which
        // invitations turned into revenue.
        var acceptedInvite = await dbContext.TblChatGroupInvites
            .FirstOrDefaultAsync(
                i => i.ChatGroupId == chatGroupId && i.UserId == userId && i.Status == InviteStatusPending,
                cancellationToken);

        if (acceptedInvite != null)
        {
            // Only the status changes. Soft-deleting the row here would erase it from the audit
            // trail the owner relies on to see which invitations converted to revenue, and the
            // filtered unique index keys on PENDING so a resolved row can never block a re-invite.
            acceptedInvite.Status = InviteStatusAccepted;
            acceptedInvite.RespondedAt = DateTime.UtcNow;
            acceptedInvite.UpdatedAt = DateTime.UtcNow;
            acceptedInvite.UpdatedBy = userId;
        }

        var joiningUser = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        var joiningUserName = !string.IsNullOrWhiteSpace(joiningUser?.DisplayName) ? joiningUser.DisplayName : (joiningUser?.UserName ?? "User");

        var systemMsg = new TblChatGroupMessage
        {
            ChatGroupId = chatGroupId,
            SenderId = userId,
            Content = $"{joiningUserName} joined the group",
            MessageType = "SYSTEM",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        dbContext.TblChatGroupMessages.Add(systemMsg);

        await dbContext.SaveChangesAsync(cancellationToken);

        var msgModel = new ChatGroupMessageModel(
            ChatGroupMessageId: systemMsg.ChatGroupMessageId,
            ChatGroupId: chatGroupId,
            SenderId: userId,
            SenderName: joiningUserName,
            SenderDisplayName: joiningUserName,
            SenderAvatar: joiningUser?.AvatarUrl,
            Content: systemMsg.Content,
            CreatedAt: systemMsg.CreatedAt,
            MessageType: "SYSTEM");

        await BroadcastAsync(chatGroupId, "ReceiveChatGroupMessage", new object[] { msgModel }, cancellationToken);

        return Result.Success("Successfully joined Paid Chat Group.");
    }

    public async Task<Result<IReadOnlyList<ChatGroupMessageModel>>> GetMessagesAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupMessageModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        if (await IsBannedAsync(chatGroupId, userId, cancellationToken))
        {
            return Result<IReadOnlyList<ChatGroupMessageModel>>.Failure("You are banned from this Chat Group.", ResultStatus.Forbidden);
        }

        var isMember = await dbContext.TblChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (!isMember)
        {
            return Result<IReadOnlyList<ChatGroupMessageModel>>.Failure("You are not a member of this Chat Group.", ResultStatus.Forbidden);
        }

        // Resolved once here so the client is never offered a moderation action the service
        // would reject.
        var canModerate = await CanDeleteMessagesAsync(chatGroupId, userId, cancellationToken);

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
                m.ReplyToChatGroupMessageId,
                m.MessageType,
                m.AttachmentUrl,
                m.FileName,
                m.FileSizeByte
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
                m.SenderId == userId || canModerate,
                m.MessageType ?? "TEXT",
                m.AttachmentUrl,
                m.FileName,
                m.FileSizeByte,
                FileSizeFormatter.FormatFileSize(m.FileSizeByte)
            );
        }).ToList();

        return Result<IReadOnlyList<ChatGroupMessageModel>>.Success(result);
    }

    /// <summary>
    /// Whether the user holds an active (not revoked) ban in the group. Consulted by both join
    /// paths and by the owner's direct add.
    /// </summary>
    private async Task<bool> IsBannedAsync(int chatGroupId, int userId, CancellationToken cancellationToken) =>
        await dbContext.TblChatGroupBans
            .AsNoTracking()
            .AnyAsync(b => b.ChatGroupId == chatGroupId && b.UserId == userId && !b.IsDeleted, cancellationToken);

    /// <summary>
    /// Whether the user may moderate other members' messages. An admin needs the owner-granted
    /// CanDeleteMessages flag; a plain member never qualifies.
    /// </summary>
    private Task<bool> CanDeleteMessagesAsync(int chatGroupId, int userId, CancellationToken cancellationToken) =>
        HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.DeleteMessages, cancellationToken);

    /// <summary>
    /// Single authorization point for every admin capability. The owner short-circuits to true:
    /// ownership is not transferable and the owner is the group's only revenue recipient, so the
    /// owner is deliberately not configurable. Everyone else must be an ADMIN holding the
    /// specific flag the caller is asking about.
    /// <para>
    /// The row is read untracked as a small projection rather than per-flag queries, so a single
    /// call costs one round trip no matter how many permissions a request needs.
    /// </para>
    /// </summary>
    private async Task<bool> HasPermissionAsync(
        int chatGroupId,
        int userId,
        ChatGroupPermissionKind permission,
        CancellationToken cancellationToken)
    {
        var member = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted)
            .Select(m => new
            {
                m.Role,
                m.CanDeleteMessages,
                m.CanRemoveMembers,
                m.CanBanMembers,
                m.CanManageInviteLinks,
                m.CanPinMessages
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (member == null)
        {
            return false;
        }

        if (string.Equals(member.Role, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(member.Role, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return permission switch
        {
            ChatGroupPermissionKind.DeleteMessages => member.CanDeleteMessages,
            ChatGroupPermissionKind.RemoveMembers => member.CanRemoveMembers,
            ChatGroupPermissionKind.BanMembers => member.CanBanMembers,
            ChatGroupPermissionKind.ManageInviteLinks => member.CanManageInviteLinks,
            ChatGroupPermissionKind.PinMessages => member.CanPinMessages,
            _ => false
        };
    }

    /// <summary>
    /// Projects the stored flags into the contract for one viewer/member.
    /// <para>
    /// The owner is reported as holding everything rather than reading the stored flags, because
    /// ownership is not configurable and the owner must never be handed an empty set that the UI
    /// would render as "cannot moderate". A plain member is reported empty since its flags are
    /// never consulted.
    /// </para>
    /// </summary>
    private static ChatGroupPermissionSet ToPermissionSet(
        string role,
        bool canDeleteMessages,
        bool canRemoveMembers,
        bool canBanMembers,
        bool canManageInviteLinks,
        bool canPinMessages)
    {
        if (string.Equals(role, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return ChatGroupPermissionSet.All;
        }

        if (!string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return ChatGroupPermissionSet.None;
        }

        return new ChatGroupPermissionSet(
            canDeleteMessages,
            canRemoveMembers,
            canBanMembers,
            canManageInviteLinks,
            canPinMessages);
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

        var text = request?.Content?.Trim() ?? string.Empty;
        var msgType = string.IsNullOrWhiteSpace(request?.MessageType) ? "TEXT" : request.MessageType.Trim().ToUpperInvariant();
        var attachmentUrl = request?.AttachmentUrl?.Trim();
        var fileName = request?.FileName?.Trim();
        var fileSizeByte = request?.FileSizeByte;

        if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(attachmentUrl))
        {
            return Result<ChatGroupMessageModel>.Failure("Message content or attachment is required.");
        }

        var userId = currentUser.UserId.Value;

        if (await IsBannedAsync(chatGroupId, userId, cancellationToken))
        {
            return Result<ChatGroupMessageModel>.Failure("You are banned from this Chat Group.", ResultStatus.Forbidden);
        }

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
            MessageType = msgType,
            AttachmentUrl = attachmentUrl,
            FileName = fileName,
            FileSizeByte = fileSizeByte,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            ReplyToChatGroupMessageId = replyToId
        };

        dbContext.TblChatGroupMessages.Add(msg);
        await dbContext.SaveChangesAsync(cancellationToken);

        var sender = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        // Dispatch notifications to active, non-muted group members (excluding the sender)
        var recipients = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId != userId && !m.IsDeleted && !m.IsMuted)
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

        if (recipients.Count > 0)
        {
            var chatGroup = await dbContext.TblChatGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId, cancellationToken);

            var groupName = chatGroup?.Name ?? "Chat Group";
            var senderName = sender?.DisplayName ?? sender?.UserName ?? "Someone";
            var previewText = string.IsNullOrWhiteSpace(text) ? $"[Sent a {msgType.ToLowerInvariant()}]" : text;

            var notifications = recipients.Select(recipientId => new TblNotification
            {
                RecipientUserId = recipientId,
                ActorUserId = userId,
                NotificationType = "GROUP_CHAT_MESSAGE",
                Title = groupName,
                Message = $"{senderName}: {previewText}",
                TargetEntityName = "TblChatGroup",
                TargetEntityId = chatGroupId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            }).ToList();

            dbContext.TblNotifications.AddRange(notifications);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

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
            CanDeleteForEveryone: true,
            msg.MessageType ?? "TEXT",
            msg.AttachmentUrl,
            msg.FileName,
            msg.FileSizeByte,
            FileSizeFormatter.FormatFileSize(msg.FileSizeByte)
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

    public async Task<Result<ChatGroupPinnedMessageModel?>> GetPinnedMessageAsync(
        int chatGroupId, CancellationToken cancellationToken = default)
    {
        var membership = await IsActiveMemberAsync(chatGroupId, cancellationToken);
        if (membership.IsError)
        {
            return Result<ChatGroupPinnedMessageModel?>.Failure(
                membership.Message, membership.Status);
        }

        // Membership, not permission: every member sees the banner, only moderators may change it.
        // IsDeleted is filtered so a retracted message stops being pinned without needing a
        // separate cleanup step when it is deleted.
        var pinned = await dbContext.TblChatGroupMessages
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.IsPinned && !m.IsDeleted)
            .OrderByDescending(m => m.PinnedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (pinned is null)
        {
            return Result<ChatGroupPinnedMessageModel?>.Success(null);
        }

        return Result<ChatGroupPinnedMessageModel?>.Success(await ToPinnedMessageModelAsync(pinned, cancellationToken));
    }

    public async Task<Result<ChatGroupPinnedMessageModel>> PinMessageAsync(
        int chatGroupId, int messageId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupPinnedMessageModel>.Failure(
                "User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        // The author may pin their own message; anyone else needs CanPinMessages.
        var message = await dbContext.TblChatGroupMessages
            .FirstOrDefaultAsync(
                m => m.ChatGroupMessageId == messageId && m.ChatGroupId == chatGroupId && !m.IsDeleted,
                cancellationToken);

        if (message is null)
        {
            return Result<ChatGroupPinnedMessageModel>.Failure("Message not found.", ResultStatus.NotFound);
        }

        if (message.SenderId != userId
            && !await HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.PinMessages, cancellationToken))
        {
            return Result<ChatGroupPinnedMessageModel>.Failure(
                "You do not have permission to pin messages in this group.", ResultStatus.Forbidden);
        }

        // Clear every other pin rather than just the current one. The group has at most one pinned
        // message, so loading all pinned rows is at most one row in practice, and this way a stale
        // row left behind by an earlier version can never surface a second banner.
        var previousPins = await dbContext.TblChatGroupMessages
            .Where(m => m.ChatGroupId == chatGroupId && m.IsPinned && m.ChatGroupMessageId != messageId)
            .ToListAsync(cancellationToken);

        foreach (var previous in previousPins)
        {
            previous.IsPinned = false;
            previous.PinnedAt = null;
            previous.PinnedByUserId = null;
            previous.UpdatedAt = DateTime.UtcNow;
            previous.UpdatedBy = userId;
        }

        var now = DateTime.UtcNow;
        message.IsPinned = true;
        message.PinnedAt = now;
        message.PinnedByUserId = userId;
        message.UpdatedAt = now;
        message.UpdatedBy = userId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var pinnedModel = await ToPinnedMessageModelAsync(message, cancellationToken);

        // Broadcast rather than only returning, so every member's banner updates without a reload.
        // The group id is included because the handler is shared and must know which thread changed.
        await BroadcastAsync(chatGroupId, "ChatGroupPinnedMessageChanged", new object?[] { chatGroupId, pinnedModel }, cancellationToken);

        return Result<ChatGroupPinnedMessageModel>.Success(pinnedModel);
    }

    public async Task<Result> UnpinMessageAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var pinnedMessages = await dbContext.TblChatGroupMessages
            .Where(m => m.ChatGroupId == chatGroupId && m.IsPinned)
            .ToListAsync(cancellationToken);

        if (pinnedMessages.Count == 0)
        {
            // Nothing to clear. Treated as success so the client can call this to reconcile after a
            // message was deleted while pinned, without having to distinguish the two cases.
            return Result.Success("No message was pinned.");
        }

        // Unpinning a message the caller did not author is moderation, so it needs the flag. This is
        // checked before mutating anything.
        var needsPermission = pinnedMessages.Any(m => m.SenderId != userId);
        if (needsPermission
            && !await HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.PinMessages, cancellationToken))
        {
            return Result.Failure(
                "You do not have permission to unpin messages in this group.", ResultStatus.Forbidden);
        }

        var now = DateTime.UtcNow;
        foreach (var pinned in pinnedMessages)
        {
            pinned.IsPinned = false;
            pinned.PinnedAt = null;
            pinned.PinnedByUserId = null;
            pinned.UpdatedAt = now;
            pinned.UpdatedBy = userId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await BroadcastAsync(chatGroupId, "ChatGroupPinnedMessageChanged", new object?[] { chatGroupId, null }, cancellationToken);

        return Result.Success("Message unpinned.");
    }

    /// <summary>
    /// Resolves the sender's display name for the banner, falling back to the handle when the
    /// account has no display name or has since been removed.
    /// </summary>
    private async Task<ChatGroupPinnedMessageModel> ToPinnedMessageModelAsync(
        TblChatGroupMessage message, CancellationToken cancellationToken)
    {
        var senderName = await dbContext.TblUsers
            .AsNoTracking()
            .Where(u => u.UserId == message.SenderId)
            .Select(u => u.DisplayName ?? u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return new ChatGroupPinnedMessageModel(
            message.ChatGroupMessageId,
            message.ChatGroupId,
            message.SenderId,
            senderName ?? "Unknown",
            message.Content,
            message.MessageType ?? "TEXT",
            message.AttachmentUrl,
            message.PinnedAt ?? message.CreatedAt,
            message.PinnedByUserId ?? message.SenderId);
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

        // The author may always retract; an owner or a flagged admin may moderate anyone.
        if (msg.SenderId != userId && !await CanDeleteMessagesAsync(chatGroupId, userId, cancellationToken))
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

    public async Task<Result> ToggleMuteAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var member = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (member == null)
        {
            return Result.Failure("You are not a member of this Chat Group.", ResultStatus.NotFound);
        }

        member.IsMuted = !member.IsMuted;
        member.UpdatedAt = DateTime.UtcNow;
        member.UpdatedBy = userId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var statusMessage = member.IsMuted ? "Muted group notifications." : "Unmuted group notifications.";
        return Result.Success(statusMessage);
    }

    public async Task<Result> PromoteMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;
        var callerRole = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == currentUserId && !m.IsDeleted)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.Equals(callerRole, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Only the Group Owner can promote members to Admin.", ResultStatus.Forbidden);
        }

        var targetMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == targetUserId && !m.IsDeleted, cancellationToken);

        if (targetMember == null)
        {
            return Result.Failure("Target user is not a member of this group.", ResultStatus.NotFound);
        }

        if (string.Equals(targetMember.Role, "ADMIN", StringComparison.OrdinalIgnoreCase) || string.Equals(targetMember.Role, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("User is already an Admin or Owner.", ResultStatus.ValidationError);
        }

        targetMember.Role = "ADMIN";

        // Grant every permission on promotion so the new admin is useful immediately, rather than
        // being an admin who can do nothing until the owner opens the matrix. The owner unticks
        // whatever they don't want afterwards.
        targetMember.CanDeleteMessages = true;
        targetMember.CanRemoveMembers = true;
        targetMember.CanBanMembers = true;
        targetMember.CanManageInviteLinks = true;
        targetMember.CanPinMessages = true;

        targetMember.UpdatedAt = DateTime.UtcNow;
        targetMember.UpdatedBy = currentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        // The new admin's own open client caches the viewer's powers, so only a targeted permission
        // event lets the moderation controls appear without a manual reload.
        await BroadcastAsync(chatGroupId, "ChatGroupPermissionsChanged",
            new object?[] { chatGroupId, targetUserId, ChatGroupPermissionSet.All }, cancellationToken);

        return Result.Success("Member promoted to Admin successfully.");
    }

    public async Task<Result> DemoteMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;
        var callerRole = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == currentUserId && !m.IsDeleted)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.Equals(callerRole, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Only the Group Owner can demote Admins.", ResultStatus.Forbidden);
        }

        var targetMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == targetUserId && !m.IsDeleted, cancellationToken);

        if (targetMember == null)
        {
            return Result.Failure("Target user is not a member of this group.", ResultStatus.NotFound);
        }

        if (string.Equals(targetMember.Role, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Cannot demote Group Owner.", ResultStatus.ValidationError);
        }

        targetMember.Role = "MEMBER";

        // Revoke every permission on demotion. A plain member's flags are never consulted, but
        // clearing them means a later re-promotion cannot silently inherit powers the owner had
        // since decided to withdraw.
        targetMember.CanDeleteMessages = false;
        targetMember.CanRemoveMembers = false;
        targetMember.CanBanMembers = false;
        targetMember.CanManageInviteLinks = false;
        targetMember.CanPinMessages = false;

        targetMember.UpdatedAt = DateTime.UtcNow;
        targetMember.UpdatedBy = currentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        // Tell the demoted client its powers are gone so it stops offering moderation controls
        // without waiting for a reload.
        await BroadcastAsync(chatGroupId, "ChatGroupPermissionsChanged",
            new object?[] { chatGroupId, targetUserId, ChatGroupPermissionSet.None }, cancellationToken);

        return Result.Success("Admin demoted to Member successfully.");
    }

    public async Task<Result> RemoveMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        // An admin needs the owner-granted CanRemoveMembers flag; the owner always qualifies.
        if (!await HasPermissionAsync(chatGroupId, currentUserId, ChatGroupPermissionKind.RemoveMembers, cancellationToken))
        {
            return Result.Failure(
                "You do not have permission to remove members from this group.", ResultStatus.Forbidden);
        }

        var callerRole = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == currentUserId && !m.IsDeleted)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(cancellationToken);

        var targetMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == targetUserId && !m.IsDeleted, cancellationToken);

        if (targetMember == null)
        {
            return Result.Failure("Target user is not a member of this group.", ResultStatus.NotFound);
        }

        if (string.Equals(targetMember.Role, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Group Owner cannot be removed from the group.", ResultStatus.ValidationError);
        }

        // Telegram rule: an Admin moderates plain members but cannot remove a peer Admin.
        // Only the Owner may do that, otherwise two Admins could expel each other and lock
        // the group down without the Owner's involvement.
        if (string.Equals(targetMember.Role, "ADMIN", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(callerRole, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Only the Group Owner can remove an Admin.", ResultStatus.Forbidden);
        }

        targetMember.IsDeleted = true;
        targetMember.DeletedAt = DateTime.UtcNow;
        targetMember.DeletedBy = currentUserId;

        var targetUser = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == targetUserId, cancellationToken);
        var targetUserName = !string.IsNullOrWhiteSpace(targetUser?.DisplayName) ? targetUser.DisplayName : (targetUser?.UserName ?? "User");

        var systemMsg = new TblChatGroupMessage
        {
            ChatGroupId = chatGroupId,
            SenderId = currentUserId,
            Content = $"{targetUserName} was removed from the group",
            MessageType = "SYSTEM",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };
        dbContext.TblChatGroupMessages.Add(systemMsg);

        await dbContext.SaveChangesAsync(cancellationToken);

        var msgModel = new ChatGroupMessageModel(
            ChatGroupMessageId: systemMsg.ChatGroupMessageId,
            ChatGroupId: chatGroupId,
            SenderId: currentUserId,
            SenderName: targetUserName,
            SenderDisplayName: targetUserName,
            SenderAvatar: targetUser?.AvatarUrl,
            Content: systemMsg.Content,
            CreatedAt: systemMsg.CreatedAt,
            MessageType: "SYSTEM");

        await BroadcastAsync(chatGroupId, "ReceiveChatGroupMessage", new object[] { msgModel }, cancellationToken);

        // The targeted event lets the removed client leave the SignalR room and drop its
        // message history immediately; a plain roster refresh would leave it in the room
        // still receiving group messages it can no longer load.
        await BroadcastAsync(chatGroupId, "ChatGroupMemberRemoved", [chatGroupId, targetUserId], cancellationToken);
        await BroadcastAsync(chatGroupId, "GroupMemberUpdated", [chatGroupId], cancellationToken);

        return Result.Success("Member removed from group successfully.");
    }

    public async Task<Result<ChatGroupPermissionSet>> UpdateMemberPermissionsAsync(
        int chatGroupId, int targetUserId, ChatGroupPermissionSet permissions,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupPermissionSet>.Failure(
                "User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        // Editing the matrix decides who can ban, remove and moderate, so it is owner-only. An
        // admin must not be able to grant themselves or a peer extra powers.
        if (!await IsOwnerAsync(chatGroupId, currentUserId, cancellationToken))
        {
            return Result<ChatGroupPermissionSet>.Failure(
                "Only the Group Owner can change admin permissions.", ResultStatus.Forbidden);
        }

        var targetMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(
                m => m.ChatGroupId == chatGroupId && m.UserId == targetUserId && !m.IsDeleted,
                cancellationToken);

        if (targetMember == null)
        {
            return Result<ChatGroupPermissionSet>.Failure(
                "Target user is not a member of this group.", ResultStatus.NotFound);
        }

        // The matrix is only consulted for an ADMIN, so refusing a MEMBER keeps the stored row
        // honest: promoting later grants everything explicitly rather than inheriting a set that
        // was written while the target could not act on it.
        if (!string.Equals(targetMember.Role, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ChatGroupPermissionSet>.Failure(
                "Permissions can only be set on an Admin. Promote the member first.", ResultStatus.ValidationError);
        }

        targetMember.CanDeleteMessages = permissions.CanDeleteMessages;
        targetMember.CanRemoveMembers = permissions.CanRemoveMembers;
        targetMember.CanBanMembers = permissions.CanBanMembers;
        targetMember.CanManageInviteLinks = permissions.CanManageInviteLinks;
        targetMember.CanPinMessages = permissions.CanPinMessages;
        targetMember.UpdatedAt = DateTime.UtcNow;
        targetMember.UpdatedBy = currentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var applied = ToPermissionSet(
            targetMember.Role,
            targetMember.CanDeleteMessages,
            targetMember.CanRemoveMembers,
            targetMember.CanBanMembers,
            targetMember.CanManageInviteLinks,
            targetMember.CanPinMessages);

        // The target's own client caches the viewer's permissions, so a roster refresh alone would
        // leave it rendering controls it no longer holds (or hiding ones it just gained) until the
        // page is reloaded.
        await BroadcastAsync(chatGroupId, "ChatGroupPermissionsChanged",
            new object?[] { chatGroupId, targetUserId, applied }, cancellationToken);
        await BroadcastAsync(chatGroupId, "GroupMemberUpdated", new object?[] { chatGroupId }, cancellationToken);

        return Result<ChatGroupPermissionSet>.Success(applied);
    }

    // ------------------------------------------------------------------
    // Creator/Admin group management
    // ------------------------------------------------------------------

    public async Task<Result<ChatGroupImageUploadResponse>> UpdateImageAsync(
        int chatGroupId, Stream imageStream, string fileName, string? contentType, long declaredLength,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupImageUploadResponse>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result<ChatGroupImageUploadResponse>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!await IsOwnerAsync(chatGroupId, currentUserId, cancellationToken))
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                "Only the Group Owner can change the group image.", ResultStatus.Forbidden);
        }

        var saveRes = await imageStorage.SaveAsync(imageStream, fileName, contentType, declaredLength, cancellationToken);
        if (!saveRes.IsSuccess || saveRes.Data == null)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                saveRes.Message, ResultStatus.ValidationError);
        }

        var previousUrl = chatGroup.AvatarUrl;
        chatGroup.AvatarUrl = saveRes.Data.Url;
        chatGroup.UpdatedAt = DateTime.UtcNow;
        chatGroup.UpdatedBy = currentUserId;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // The row still points at the old image, so the orphaned new file must go back.
            await imageStorage.DeleteAsync(saveRes.Data.Url, cancellationToken);
            throw;
        }

        // Only after the commit: deleting first would leave a brief window where the
        // persisted URL 404s for every client that has already cached the group model.
        await imageStorage.DeleteAsync(previousUrl, cancellationToken);

        // Members render the avatar in the header, thread list and info panel, so the new
        // URL is pushed rather than a bare "changed" signal they would have to refetch.
        await BroadcastAsync(chatGroupId, "ChatGroupImageUpdated", [chatGroupId, saveRes.Data.Url], cancellationToken);

        return Result<ChatGroupImageUploadResponse>.Success(saveRes.Data);
    }

    public async Task BroadcastAsync(int chatGroupId, string method, object?[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            var groupName = ChatGroupHub.GetGroupName(chatGroupId);
            await hubContext.Clients.Group(groupName).SendCoreAsync(method, args, cancellationToken);
        }
        catch
        {
            // A realtime push is a convenience for clients that are already connected. Failing
            // the business operation because a broadcast could not be delivered would be wrong:
            // the caller would see an error for a change that was in fact committed.
        }
    }

    public async Task<Result<ChatGroupModel>> UpdateInfoAsync(int chatGroupId, UpdateChatGroupRequestModel request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result<ChatGroupModel>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!await IsOwnerAsync(chatGroupId, currentUserId, cancellationToken))
        {
            return Result<ChatGroupModel>.Failure(
                "Only the Group Owner can update group settings.", ResultStatus.Forbidden);
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

        var description = request.Description?.Trim();
        if (description != null && description.Length > 2000)
        {
            return Result<ChatGroupModel>.Failure("Chat Group description cannot exceed 2000 characters.");
        }

        var chatType = string.Equals(request.ChatType, "PAID", StringComparison.OrdinalIgnoreCase) ? "PAID" : "FREE";
        var joinFee = chatType == "PAID" ? Math.Max(0, request.JoinFeeLinkDrops) : 0;

        if (joinFee < 0)
        {
            return Result<ChatGroupModel>.Failure("Join fee cannot be negative.");
        }

        // Changing the fee does not touch existing members: whoever already paid keeps access,
        // and the new fee applies only to future joiners. No membership rows are rewritten here.
        chatGroup.Name = name;
        chatGroup.Description = description;
        chatGroup.ChatType = chatType;
        chatGroup.JoinFeeLinkDrops = joinFee;
        chatGroup.UpdatedAt = DateTime.UtcNow;
        chatGroup.UpdatedBy = currentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var model = await BuildModelAsync(chatGroup, cancellationToken);

        await BroadcastAsync(chatGroupId, "ChatGroupUpdated", [model.Data!], cancellationToken);

        return model;
    }

    public async Task<Result> AddMembersAsync(int chatGroupId, IReadOnlyList<int> userIds, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!await IsOwnerAsync(chatGroupId, currentUserId, cancellationToken))
        {
            return Result.Failure("Only the Group Owner can add members.", ResultStatus.Forbidden);
        }

        if (userIds == null || userIds.Count == 0)
        {
            return Result.Failure("Select at least one user to add.");
        }

        // De-duplicate so a repeated id in the request cannot produce a duplicate insert
        // against UQ_TblChatGroupMember_Group_User.
        var targetIds = userIds.Distinct().ToList();

        var bannedIds = await dbContext.TblChatGroupBans
            .AsNoTracking()
            .Where(b => b.ChatGroupId == chatGroupId && !b.IsDeleted && targetIds.Contains(b.UserId))
            .Select(b => b.UserId)
            .ToListAsync(cancellationToken);

        // A group that charges a real fee cannot be entered through an owner's add: doing so
        // would hand the invitee the paid content for free and credit the creator nothing. Those
        // users get a PENDING invite instead and must pay through JoinPaidChatGroupAsync.
        if (RequiresPaidInvite(chatGroup))
        {
            return await InviteUsersAsync(chatGroup, currentUserId, targetIds, bannedIds, cancellationToken);
        }

        var existing = await dbContext.TblChatGroupMembers
            .Where(m => m.ChatGroupId == chatGroupId && targetIds.Contains(m.UserId))
            .ToListAsync(cancellationToken);

        var added = 0;
        foreach (var targetId in targetIds)
        {
            // A ban is a hard block on the direct-add path, not just on self-service join.
            if (bannedIds.Contains(targetId))
            {
                continue;
            }

            var member = existing.FirstOrDefault(m => m.UserId == targetId);
            if (member != null)
            {
                // Previously removed members are restored rather than duplicated.
                if (member.IsDeleted)
                {
                    member.IsDeleted = false;
                    member.JoinedAt = DateTime.UtcNow;
                    member.Role = "MEMBER";
                    member.UpdatedAt = DateTime.UtcNow;
                    member.UpdatedBy = currentUserId;
                }

                continue;
            }

            dbContext.TblChatGroupMembers.Add(new TblChatGroupMember
            {
                ChatGroupId = chatGroupId,
                UserId = targetId,
                Role = "MEMBER",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });

            added++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Only announce when the roster actually changed, so a no-op request does not
        // force every connected client to refetch an identical member list.
        if (added > 0)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", [chatGroupId], cancellationToken);
        }

        var skipped = targetIds.Count - added;
        return Result.Success(
            skipped > 0
                ? $"Added {added} member(s) to the group. Skipped {skipped} who are banned or already members."
                : $"Added {added} member(s) to the group.");
    }

    /// <summary>
    /// Issues PENDING invites into a PAID group. The invitee gets no membership row, so every
    /// membership-gated read (history, previews, notification fan-out, the hub room) keeps
    /// treating them as a non-member until they pay. The quoted fee is snapshotted onto the
    /// invite so a later fee change cannot reprice an open invitation.
    /// </summary>
    private async Task<Result> InviteUsersAsync(
        TblChatGroup chatGroup,
        int currentUserId,
        IReadOnlyList<int> targetIds,
        IReadOnlyCollection<int> bannedIds,
        CancellationToken cancellationToken)
    {
        var chatGroupId = chatGroup.ChatGroupId;
        var now = DateTime.UtcNow;

        // Already a member? Nothing to offer. Soft-deleted rows count as members so the invite
        // path cannot be used to smuggle a former member back in without paying.
        var memberIds = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && targetIds.Contains(m.UserId))
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

        // Open invites. A user with a live invite is skipped rather than duplicated, which is
        // also what keeps UQ_TblChatGroupInvite_Group_User satisfied.
        var alreadyInvitedIds = await dbContext.TblChatGroupInvites
            .AsNoTracking()
            .Where(i => i.ChatGroupId == chatGroupId
                && i.Status == InviteStatusPending
                && targetIds.Contains(i.UserId))
            .Select(i => i.UserId)
            .ToListAsync(cancellationToken);

        var invited = 0;
        foreach (var targetId in targetIds)
        {
            if (bannedIds.Contains(targetId)
                || memberIds.Contains(targetId)
                || alreadyInvitedIds.Contains(targetId))
            {
                continue;
            }

            dbContext.TblChatGroupInvites.Add(new TblChatGroupInvite
            {
                ChatGroupId = chatGroupId,
                UserId = targetId,
                InvitedByUserId = currentUserId,
                Status = InviteStatusPending,
                FeeAtInviteLinkDrops = chatGroup.JoinFeeLinkDrops,
                InvitedAt = now,
                IsDeleted = false,
                CreatedAt = now,
                CreatedBy = currentUserId
            });

            invited++;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (invited > 0)
        {
            // currentUser exposes the login name only, so the inviter is looked up for their
            // display name to match how every other notification addresses a person.
            var inviterName = await dbContext.TblUsers
                .AsNoTracking()
                .Where(u => u.UserId == currentUserId)
                .Select(u => u.DisplayName ?? u.UserName)
                .FirstOrDefaultAsync(cancellationToken) ?? currentUser.UserName ?? "The group owner";
            var fee = chatGroup.JoinFeeLinkDrops;

            // Routed through the notification service so the row lands in the recipient's bell
            // with a resolved deep link. Without this the invitee only discovers the invite by
            // opening the app, because the group-room broadcast below never reaches them: they
            // are not in the room yet.
            await notificationService.CreateBulkNotificationsAsync(
                targetIds.Where(t => !bannedIds.Contains(t) && !memberIds.Contains(t) && !alreadyInvitedIds.Contains(t)),
                currentUserId,
                "CHAT_GROUP_INVITE",
                chatGroup.Name,
                $"{inviterName} invited you to join. Unlock it for {fee} LinkDrops to take part.",
                "TblChatGroup",
                chatGroupId,
                cancellationToken);
        }

        var skipped = targetIds.Count - invited;
        var plural = invited == 1 ? "person" : "people";
        return Result.Success(
            skipped > 0
                ? $"Invited {invited} {plural} to the group. They must unlock it for {chatGroup.JoinFeeLinkDrops} LinkDrops to join. Skipped {skipped} who are banned, already members or already invited."
                : $"Invited {invited} {plural} to the group. They must unlock it for {chatGroup.JoinFeeLinkDrops} LinkDrops to join.");
    }

    public async Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyInvitationsAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var invitations = await dbContext.TblChatGroupInvites
            .AsNoTracking()
            .Where(i => i.UserId == userId && i.Status == InviteStatusPending && !i.ChatGroup.IsDeleted && i.ChatGroup.IsActive)
            .OrderByDescending(i => i.InvitedAt)
            .Select(i => new
            {
                Group = i.ChatGroup,
                CreatorName = i.ChatGroup.Creator.DisplayName ?? i.ChatGroup.Creator.UserName,
                MemberCount = i.ChatGroup.TblChatGroupMembers.Count(m => !m.IsDeleted),
                i.FeeAtInviteLinkDrops,
                InviterName = i.InvitedByUser.DisplayName ?? i.InvitedByUser.UserName
            })
            .ToListAsync(cancellationToken);

        // IsJoined stays false: an invite is an offer, and the paywall must still apply until the
        // invitee actually pays. The quoted fee wins over the group's current fee so an open
        // invite cannot be repriced after the fact.
        var result = invitations.Select(x => new ChatGroupModel(
            x.Group.ChatGroupId,
            x.Group.Name,
            x.Group.Description,
            x.Group.AvatarUrl,
            x.Group.CreatorId,
            x.CreatorName,
            x.Group.ChatType,
            x.FeeAtInviteLinkDrops,
            x.Group.CommissionPercentageSnapshot,
            x.MemberCount,
            x.Group.CreatedAt,
            IsJoined: false,
            UserRole: "NONE",
            IsInvited: true,
            InviteFeeLinkDrops: x.FeeAtInviteLinkDrops,
            InvitedByName: x.InviterName
        )).ToList();

        return Result<IReadOnlyList<ChatGroupModel>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<ChatGroupInviteModel>>> GetPendingInvitationsAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupInviteModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var groupExists = await dbContext.TblChatGroups
            .AnyAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (!groupExists)
        {
            return Result<IReadOnlyList<ChatGroupInviteModel>>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        // Revoking an outstanding invite is the same power as issuing one, so it rides the same
        // owner-granted flag: CanManageInviteLinks.
        if (!await HasPermissionAsync(chatGroupId, currentUserId, ChatGroupPermissionKind.ManageInviteLinks, cancellationToken))
        {
            return Result<IReadOnlyList<ChatGroupInviteModel>>.Failure(
                "You do not have permission to manage invitations for this Chat Group.",
                ResultStatus.Forbidden);
        }

        // Only PENDING rows are actionable. Accepted/revoked/declined invites are retained for
        // audit but are not outstanding, so they are deliberately not returned here.
        var invites = await dbContext.TblChatGroupInvites
            .AsNoTracking()
            .Where(i => i.ChatGroupId == chatGroupId && i.Status == InviteStatusPending)
            .OrderByDescending(i => i.InvitedAt)
            .Select(i => new ChatGroupInviteModel(
                i.ChatGroupInviteId,
                i.ChatGroupId,
                i.UserId,
                i.User.UserName,
                i.User.DisplayName ?? i.User.UserName,
                i.User.AvatarUrl,
                i.Status,
                i.FeeAtInviteLinkDrops,
                i.InvitedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ChatGroupInviteModel>>.Success(invites);
    }

    public async Task<Result> RevokeInvitationAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var groupExists = await dbContext.TblChatGroups
            .AnyAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (!groupExists)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!await HasPermissionAsync(chatGroupId, currentUserId, ChatGroupPermissionKind.ManageInviteLinks, cancellationToken))
        {
            return Result.Failure(
                "You do not have permission to revoke invitations in this group.", ResultStatus.Forbidden);
        }

        var invite = await dbContext.TblChatGroupInvites
            .FirstOrDefaultAsync(
                i => i.ChatGroupId == chatGroupId && i.UserId == targetUserId && i.Status == InviteStatusPending,
                cancellationToken);

        if (invite == null)
        {
            return Result.Failure("There is no pending invitation for that user.", ResultStatus.NotFound);
        }

        // Retained as a terminal state rather than deleted, so the creator keeps a record of the
        // invite they withdrew and the user becomes re-invitable. IsDeleted is deliberately left
        // false: soft-deleting would hide the audit row from the queries that read it.
        invite.Status = InviteStatusRevoked;
        invite.RespondedAt = DateTime.UtcNow;
        invite.UpdatedAt = DateTime.UtcNow;
        invite.UpdatedBy = currentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Invitation revoked.");
    }

    public async Task<Result> DeclineInvitationAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var invite = await dbContext.TblChatGroupInvites
            .FirstOrDefaultAsync(
                i => i.ChatGroupId == chatGroupId && i.UserId == userId && i.Status == InviteStatusPending,
                cancellationToken);

        if (invite == null)
        {
            return Result.Failure("You have no pending invitation to this Chat Group.", ResultStatus.NotFound);
        }

        // Kept as DECLINED rather than soft-deleted so the owner can see the invite was turned
        // down, and so the row keeps its place in the invite history.
        invite.Status = InviteStatusDeclined;
        invite.RespondedAt = DateTime.UtcNow;
        invite.UpdatedAt = DateTime.UtcNow;
        invite.UpdatedBy = userId;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Invitation declined.");
    }

    public async Task<Result> BanMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        // An admin needs the owner-granted CanBanMembers flag; the owner always qualifies.
        if (!await HasPermissionAsync(chatGroupId, currentUserId, ChatGroupPermissionKind.BanMembers, cancellationToken))
        {
            return Result.Failure(
                "You do not have permission to ban members in this group.", ResultStatus.Forbidden);
        }

        var callerRole = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == currentUserId && !m.IsDeleted)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (targetUserId == currentUserId)
        {
            return Result.Failure("You cannot ban yourself.", ResultStatus.ValidationError);
        }

        var targetMember = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == targetUserId && !m.IsDeleted, cancellationToken);

        // A ban on the Owner is never allowed, and an Admin may only ban plain members.
        if (targetMember != null && string.Equals(targetMember.Role, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("The Group Owner cannot be banned.", ResultStatus.Forbidden);
        }

        if (targetMember != null && string.Equals(targetMember.Role, "ADMIN", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(callerRole, "OWNER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Only the Group Owner can ban an Admin.", ResultStatus.Forbidden);
        }

        var alreadyBanned = await dbContext.TblChatGroupBans
            .AnyAsync(b => b.ChatGroupId == chatGroupId && b.UserId == targetUserId && !b.IsDeleted, cancellationToken);

        if (alreadyBanned)
        {
            return Result.Failure("User is already banned from this group.", ResultStatus.Conflict);
        }

        var ban = new TblChatGroupBan
        {
            ChatGroupId = chatGroupId,
            UserId = targetUserId,
            BannedByUserId = currentUserId,
            IsDeleted = false,
            BannedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId
        };

        dbContext.TblChatGroupBans.Add(ban);

        // Banning also ends the membership. Soft-deleting the row rather than deleting it
        // preserves the join history the ban is layered on top of.
        if (targetMember != null)
        {
            var memberToRemove = await dbContext.TblChatGroupMembers
                .FirstOrDefaultAsync(m => m.ChatGroupMemberId == targetMember.ChatGroupMemberId, cancellationToken);

            if (memberToRemove != null)
            {
                memberToRemove.IsDeleted = true;
                memberToRemove.DeletedAt = DateTime.UtcNow;
                memberToRemove.DeletedBy = currentUserId;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // The removal notice goes first: the affected client has to leave the room before the
        // roster refresh arrives, otherwise it reloads a list it is no longer part of.
        await BroadcastAsync(chatGroupId, "ChatGroupMemberRemoved", [chatGroupId, targetUserId], cancellationToken);
        await BroadcastAsync(chatGroupId, "GroupMemberUpdated", [chatGroupId], cancellationToken);

        return Result.Success("Member banned and removed from group successfully.");
    }

    public async Task<Result> UnbanMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        // Lifting a ban is the counterpart of imposing one, so it rides the same owner-granted
        // flag: an admin who cannot ban cannot unban.
        if (!await HasPermissionAsync(chatGroupId, currentUserId, ChatGroupPermissionKind.BanMembers, cancellationToken))
        {
            return Result.Failure(
                "You do not have permission to unban members in this group.", ResultStatus.Forbidden);
        }

        var ban = await dbContext.TblChatGroupBans
            .FirstOrDefaultAsync(b => b.ChatGroupId == chatGroupId && b.UserId == targetUserId && !b.IsDeleted, cancellationToken);

        if (ban == null)
        {
            return Result.Failure("User is not banned from this group.", ResultStatus.NotFound);
        }

        // Soft delete, not a hard delete: the ban history is audit evidence and the filtered
        // unique index frees this pair for a future ban.
        ban.IsDeleted = true;
        ban.RevokedAt = DateTime.UtcNow;
        ban.RevokedByUserId = currentUserId;
        ban.UpdatedAt = DateTime.UtcNow;
        ban.UpdatedBy = currentUserId;

        // Restore membership row if member was soft-deleted when banned
        var previousMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == chatGroupId && m.UserId == targetUserId, cancellationToken);

        if (previousMember != null && previousMember.IsDeleted)
        {
            previousMember.IsDeleted = false;
            previousMember.DeletedAt = null;
            previousMember.DeletedBy = null;
            previousMember.UpdatedAt = DateTime.UtcNow;
            previousMember.UpdatedBy = currentUserId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await BroadcastAsync(chatGroupId, "GroupMemberUpdated", [chatGroupId], cancellationToken);

        return Result.Success("User unbanned. They may join again.");
    }

    public async Task<Result<ChatGroupModel>> SetJoinFeeAsync(int chatGroupId, long joinFeeLinkDrops, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result<ChatGroupModel>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!await IsOwnerAsync(chatGroupId, currentUserId, cancellationToken))
        {
            return Result<ChatGroupModel>.Failure("Only the Group Owner can change the join fee.", ResultStatus.Forbidden);
        }

        if (joinFeeLinkDrops < 0)
        {
            return Result<ChatGroupModel>.Failure("Join fee cannot be negative.");
        }

        // Future joiners only. Existing members keep their access and their commission
        // snapshot, so no membership or payment rows are modified.
        chatGroup.JoinFeeLinkDrops = joinFeeLinkDrops;
        chatGroup.ChatType = joinFeeLinkDrops > 0 ? "PAID" : chatGroup.ChatType;
        chatGroup.UpdatedAt = DateTime.UtcNow;
        chatGroup.UpdatedBy = currentUserId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var model = await BuildModelAsync(chatGroup, cancellationToken);

        await BroadcastAsync(chatGroupId, "ChatGroupUpdated", [model.Data!], cancellationToken);

        return model;
    }

    public async Task<Result> DeleteChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (!await IsOwnerAsync(chatGroupId, currentUserId, cancellationToken))
        {
            return Result.Failure("Only the Group Owner can delete the group.", ResultStatus.Forbidden);
        }

        var now = DateTime.UtcNow;

        // Soft-delete the group and end every membership. Messages and payment transactions
        // are deliberately left in place: they are the financial and moderation audit trail.
        chatGroup.IsDeleted = true;
        chatGroup.IsActive = false;
        chatGroup.DeletedAt = now;
        chatGroup.DeletedBy = currentUserId;
        chatGroup.UpdatedAt = now;
        chatGroup.UpdatedBy = currentUserId;

        var members = await dbContext.TblChatGroupMembers
            .Where(m => m.ChatGroupId == chatGroupId && !m.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var member in members)
        {
            member.IsDeleted = true;
            member.DeletedAt = now;
            member.DeletedBy = currentUserId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await BroadcastAsync(chatGroupId, "ChatGroupDeleted", [chatGroupId], cancellationToken);

        return Result.Success("Chat Group deleted successfully.");
    }

    /// <summary>OWNER-only check used by the settings, fee, image and delete operations.</summary>
    private async Task<bool> IsOwnerAsync(int chatGroupId, int userId, CancellationToken cancellationToken)
    {
        var role = await dbContext.TblChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ChatGroupId == chatGroupId && m.UserId == userId && !m.IsDeleted)
            .Select(m => m.Role)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(role, "OWNER", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Materialises a group row into the client model, resolving the viewer's join state and
    /// role from their own membership row.
    /// </summary>
    private async Task<Result<ChatGroupModel>> BuildModelAsync(TblChatGroup chatGroup, CancellationToken cancellationToken)
    {
        var viewerId = currentUser.UserId;
        var role = "NONE";
        var isJoined = false;

        // Non-members and plain members are resolved to the empty set. An owner is reported as
        // holding every permission without needing a stored flag, since it is not configurable.
        var viewerPermissions = ChatGroupPermissionSet.None;

        if (viewerId.HasValue)
        {
            var viewerMember = await dbContext.TblChatGroupMembers
                .AsNoTracking()
                .Where(m => m.ChatGroupId == chatGroup.ChatGroupId && m.UserId == viewerId.Value && !m.IsDeleted)
                .Select(m => new
                {
                    m.Role,
                    m.CanDeleteMessages,
                    m.CanRemoveMembers,
                    m.CanBanMembers,
                    m.CanManageInviteLinks,
                    m.CanPinMessages
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (viewerMember != null)
            {
                role = viewerMember.Role;
                isJoined = true;
                viewerPermissions = ToPermissionSet(
                    viewerMember.Role,
                    viewerMember.CanDeleteMessages,
                    viewerMember.CanRemoveMembers,
                    viewerMember.CanBanMembers,
                    viewerMember.CanManageInviteLinks,
                    viewerMember.CanPinMessages);
            }
        }

        var creator = await dbContext.TblUsers
            .AsNoTracking()
            .Where(u => u.UserId == chatGroup.CreatorId)
            .Select(u => u.DisplayName ?? u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        var memberCount = await dbContext.TblChatGroupMembers
            .CountAsync(m => m.ChatGroupId == chatGroup.ChatGroupId && !m.IsDeleted, cancellationToken);

        return Result<ChatGroupModel>.Success(new ChatGroupModel(
            chatGroup.ChatGroupId,
            chatGroup.Name,
            chatGroup.Description,
            chatGroup.AvatarUrl,
            chatGroup.CreatorId,
            creator ?? "Unknown",
            chatGroup.ChatType,
            chatGroup.JoinFeeLinkDrops,
            chatGroup.CommissionPercentageSnapshot,
            memberCount,
            chatGroup.CreatedAt,
            isJoined,
            role,
            ViewerPermissions: viewerPermissions));
    }

    public async Task<Result<IReadOnlyList<ChatGroupBannedMemberModel>>> GetBannedMembersAsync(
        int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupBannedMemberModel>>.Failure(
                "User is not authenticated.", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        var chatGroup = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.ChatGroupId == chatGroupId && !cg.IsDeleted, cancellationToken);

        if (chatGroup == null)
        {
            return Result<IReadOnlyList<ChatGroupBannedMemberModel>>.Failure(
                "Chat Group not found.", ResultStatus.NotFound);
        }

        // The banned list is what an admin acts on to lift a ban, so it rides CanBanMembers
        // rather than being visible to any admin.
        if (!await HasPermissionAsync(chatGroupId, currentUserId, ChatGroupPermissionKind.BanMembers, cancellationToken))
        {
            return Result<IReadOnlyList<ChatGroupBannedMemberModel>>.Failure(
                "You do not have permission to view the banned members list.", ResultStatus.Forbidden);
        }

        // Only active bans. Revoked rows stay in the table as audit history but are not
        // actionable, so they must not appear in the list.
        var bans = await dbContext.TblChatGroupBans
            .AsNoTracking()
            .Where(b => b.ChatGroupId == chatGroupId && !b.IsDeleted)
            .OrderByDescending(b => b.BannedAt)
            .Select(b => new ChatGroupBannedMemberModel(
                b.ChatGroupBanId,
                b.ChatGroupId,
                b.UserId,
                b.User.UserName,
                b.User.DisplayName,
                b.User.AvatarUrl,
                b.BannedByUserId,
                b.BannedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ChatGroupBannedMemberModel>>.Success(bans);
    }

    // ------------------------------------------------------------------
    // Shareable invite links
    // ------------------------------------------------------------------

    /// <summary>Generates a URL-safe random token (11 chars, base-62).</summary>
    private static string GenerateInviteToken()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        // Base64url without padding, trimmed to a short, shareable length.
        return Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")[..12];
    }

    private ChatGroupInviteLinkModel ToInviteLinkModel(TblChatGroupInviteLink link) =>
        new(
            ChatGroupInviteLinkId: link.ChatGroupInviteLinkId,
            ChatGroupId: link.ChatGroupId,
            Token: link.Token,
            FullUrl: $"/invite/{link.Token}",
            Name: link.Name,
            IsPrimary: link.IsPrimary,
            ExpiresAt: link.ExpiresAt,
            MaxUses: link.MaxUses,
            UseCount: link.UseCount,
            IsRevoked: link.IsRevoked,
            CreatedAt: link.CreatedAt);

    public async Task<Result<IReadOnlyList<ChatGroupInviteLinkModel>>> GetInviteLinksAsync(
        int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<IReadOnlyList<ChatGroupInviteLinkModel>>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;
        if (!await HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.ManageInviteLinks, cancellationToken))
        {
            return Result<IReadOnlyList<ChatGroupInviteLinkModel>>.Failure(
                "You do not have permission to view invite links for this group.", ResultStatus.Forbidden);
        }

        var links = await dbContext.TblChatGroupInviteLinks
            .AsNoTracking()
            .Where(l => l.ChatGroupId == chatGroupId && !l.IsDeleted)
            .OrderByDescending(l => l.IsPrimary)
            .ThenByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var models = links.Select(ToInviteLinkModel).ToList();
        return Result<IReadOnlyList<ChatGroupInviteLinkModel>>.Success(models);
    }

    public async Task<Result<ChatGroupInviteLinkModel>> GetOrCreatePrimaryInviteLinkAsync(
        int chatGroupId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupInviteLinkModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;
        if (!await HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.ManageInviteLinks, cancellationToken))
        {
            return Result<ChatGroupInviteLinkModel>.Failure(
                "You do not have permission to manage invite links for this group.", ResultStatus.Forbidden);
        }

        var group = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.ChatGroupId == chatGroupId && !g.IsDeleted, cancellationToken);
        if (group == null)
        {
            return Result<ChatGroupInviteLinkModel>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        // Look for an existing primary link.
        var primary = await dbContext.TblChatGroupInviteLinks
            .FirstOrDefaultAsync(l => l.ChatGroupId == chatGroupId && l.IsPrimary && !l.IsDeleted && !l.IsRevoked, cancellationToken);

        if (primary != null)
        {
            return Result<ChatGroupInviteLinkModel>.Success(ToInviteLinkModel(primary));
        }

        // Create the primary link.
        primary = new TblChatGroupInviteLink
        {
            ChatGroupId = chatGroupId,
            Token = GenerateInviteToken(),
            CreatedByUserId = userId,
            Name = "Primary link",
            IsPrimary = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        dbContext.TblChatGroupInviteLinks.Add(primary);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<ChatGroupInviteLinkModel>.Success(ToInviteLinkModel(primary));
    }

    public async Task<Result<ChatGroupInviteLinkModel>> CreateInviteLinkAsync(
        int chatGroupId, CreateInviteLinkRequestModel request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<ChatGroupInviteLinkModel>.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;
        if (!await HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.ManageInviteLinks, cancellationToken))
        {
            return Result<ChatGroupInviteLinkModel>.Failure(
                "You do not have permission to create invite links for this group.", ResultStatus.Forbidden);
        }

        var group = await dbContext.TblChatGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.ChatGroupId == chatGroupId && !g.IsDeleted, cancellationToken);
        if (group == null)
        {
            return Result<ChatGroupInviteLinkModel>.Failure("Chat Group not found.", ResultStatus.NotFound);
        }

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return Result<ChatGroupInviteLinkModel>.Failure("Expiration date must be in the future.", ResultStatus.ValidationError);
        }

        if (request.MaxUses.HasValue && request.MaxUses.Value < 1)
        {
            return Result<ChatGroupInviteLinkModel>.Failure("Max uses must be at least 1.", ResultStatus.ValidationError);
        }

        var link = new TblChatGroupInviteLink
        {
            ChatGroupId = chatGroupId,
            Token = GenerateInviteToken(),
            CreatedByUserId = userId,
            Name = request.Name?.Trim(),
            IsPrimary = false,
            ExpiresAt = request.ExpiresAt,
            MaxUses = request.MaxUses,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        dbContext.TblChatGroupInviteLinks.Add(link);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<ChatGroupInviteLinkModel>.Success(ToInviteLinkModel(link));
    }

    public async Task<Result> RevokeInviteLinkAsync(
        int chatGroupId, int linkId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;
        if (!await HasPermissionAsync(chatGroupId, userId, ChatGroupPermissionKind.ManageInviteLinks, cancellationToken))
        {
            return Result.Failure(
                "You do not have permission to revoke invite links for this group.", ResultStatus.Forbidden);
        }

        var link = await dbContext.TblChatGroupInviteLinks
            .FirstOrDefaultAsync(l => l.ChatGroupInviteLinkId == linkId && l.ChatGroupId == chatGroupId && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            return Result.Failure("Invite link not found.", ResultStatus.NotFound);
        }

        if (link.IsRevoked)
        {
            return Result.Failure("Invite link is already revoked.", ResultStatus.Conflict);
        }

        link.IsRevoked = true;
        link.UpdatedAt = DateTime.UtcNow;
        link.UpdatedBy = userId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Invite link revoked.");
    }

    public async Task<Result<InviteLinkPreviewModel>> GetInviteLinkPreviewAsync(
        string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result<InviteLinkPreviewModel>.Failure("Token is required.", ResultStatus.ValidationError);
        }

        var link = await dbContext.TblChatGroupInviteLinks
            .AsNoTracking()
            .Include(l => l.ChatGroup)
            .FirstOrDefaultAsync(l => l.Token == token && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            return Result<InviteLinkPreviewModel>.Failure("Invite link not found.", ResultStatus.NotFound);
        }

        var group = link.ChatGroup;
        if (group == null || group.IsDeleted || !group.IsActive)
        {
            return Result<InviteLinkPreviewModel>.Success(new InviteLinkPreviewModel(
                ChatGroupId: 0, GroupName: "Unknown", GroupDescription: null, GroupAvatar: null,
                MemberCount: 0, ChatType: "FREE", JoinFeeLinkDrops: 0,
                IsValid: false, InvalidReason: "This group no longer exists."));
        }

        var memberCount = await dbContext.TblChatGroupMembers
            .CountAsync(m => m.ChatGroupId == group.ChatGroupId && !m.IsDeleted, cancellationToken);

        // Check link validity.
        string? invalidReason = null;
        if (link.IsRevoked)
        {
            invalidReason = "This invite link has been revoked.";
        }
        else if (link.ExpiresAt.HasValue && link.ExpiresAt.Value <= DateTime.UtcNow)
        {
            invalidReason = "This invite link has expired.";
        }
        else if (link.MaxUses.HasValue && link.UseCount >= link.MaxUses.Value)
        {
            invalidReason = "This invite link has reached its usage limit.";
        }

        // Check caller context (may be unauthenticated).
        bool isAlreadyMember = false;
        bool isBanned = false;
        if (currentUser.IsAuthenticated && currentUser.UserId.HasValue)
        {
            var uid = currentUser.UserId.Value;
            isAlreadyMember = await dbContext.TblChatGroupMembers
                .AnyAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == uid && !m.IsDeleted, cancellationToken);
            isBanned = await IsBannedAsync(group.ChatGroupId, uid, cancellationToken);
        }

        return Result<InviteLinkPreviewModel>.Success(new InviteLinkPreviewModel(
            ChatGroupId: group.ChatGroupId,
            GroupName: group.Name,
            GroupDescription: group.Description,
            GroupAvatar: group.AvatarUrl,
            MemberCount: memberCount,
            ChatType: group.ChatType,
            JoinFeeLinkDrops: group.JoinFeeLinkDrops,
            IsValid: invalidReason == null,
            InvalidReason: invalidReason,
            IsAlreadyMember: isAlreadyMember,
            IsBanned: isBanned));
    }

    public async Task<Result> JoinViaInviteLinkAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result.Failure("User is not authenticated.", ResultStatus.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Failure("Token is required.", ResultStatus.ValidationError);
        }

        var userId = currentUser.UserId.Value;

        var link = await dbContext.TblChatGroupInviteLinks
            .Include(l => l.ChatGroup)
            .FirstOrDefaultAsync(l => l.Token == token && !l.IsDeleted, cancellationToken);

        if (link == null)
        {
            return Result.Failure("Invite link not found.", ResultStatus.NotFound);
        }

        // Validate link.
        if (link.IsRevoked)
        {
            return Result.Failure("This invite link has been revoked.", ResultStatus.ValidationError);
        }
        if (link.ExpiresAt.HasValue && link.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return Result.Failure("This invite link has expired.", ResultStatus.ValidationError);
        }
        if (link.MaxUses.HasValue && link.UseCount >= link.MaxUses.Value)
        {
            return Result.Failure("This invite link has reached its usage limit.", ResultStatus.ValidationError);
        }

        var group = link.ChatGroup;
        if (group == null || group.IsDeleted || !group.IsActive)
        {
            return Result.Failure("Chat Group not found or inactive.", ResultStatus.NotFound);
        }

        // Ban check.
        if (await IsBannedAsync(group.ChatGroupId, userId, cancellationToken))
        {
            return Result.Failure("You are banned from this Chat Group.", ResultStatus.Forbidden);
        }

        // PAID group with fee: redirect to paid flow — the invite link grants access to
        // preview/attempt joining, but the fee must still be paid.
        if (RequiresPaidInvite(group))
        {
            return Result.Failure("This is a paid group. Please complete payment to join.", ResultStatus.ValidationError);
        }

        // Already a member?
        var existingMember = await dbContext.TblChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == group.ChatGroupId && m.UserId == userId, cancellationToken);

        if (existingMember != null && !existingMember.IsDeleted)
        {
            return Result.Failure("You are already a member of this Chat Group.", ResultStatus.Conflict);
        }

        // Add the member.
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
                ChatGroupId = group.ChatGroupId,
                UserId = userId,
                Role = "MEMBER",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            dbContext.TblChatGroupMembers.Add(newMember);
        }

        // Increment usage count.
        link.UseCount++;
        link.UpdatedAt = DateTime.UtcNow;

        // System message.
        var user = await dbContext.TblUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        var userName = !string.IsNullOrWhiteSpace(user?.DisplayName) ? user.DisplayName : (user?.UserName ?? "User");

        var systemMsg = new TblChatGroupMessage
        {
            ChatGroupId = group.ChatGroupId,
            SenderId = userId,
            Content = $"{userName} joined the group via invite link",
            MessageType = "SYSTEM",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        dbContext.TblChatGroupMessages.Add(systemMsg);

        await dbContext.SaveChangesAsync(cancellationToken);

        var msgModel = new ChatGroupMessageModel(
            ChatGroupMessageId: systemMsg.ChatGroupMessageId,
            ChatGroupId: group.ChatGroupId,
            SenderId: userId,
            SenderName: userName,
            SenderDisplayName: userName,
            SenderAvatar: user?.AvatarUrl,
            Content: systemMsg.Content,
            CreatedAt: systemMsg.CreatedAt,
            MessageType: "SYSTEM");

        await BroadcastAsync(group.ChatGroupId, "ReceiveChatGroupMessage", new object[] { msgModel }, cancellationToken);
        await BroadcastAsync(group.ChatGroupId, "GroupMemberUpdated", new object[] { group.ChatGroupId }, cancellationToken);

        return Result.Success("Joined Chat Group successfully via invite link.");
    }
}
