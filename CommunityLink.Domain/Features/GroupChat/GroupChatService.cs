using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.GroupChat;

namespace CommunityLink.Domain.Features.GroupChat;

public interface IGroupChatService
{
    Task<Result<GroupChatRoomModel>> CreateGroupChatRoomAsync(CreateGroupChatRoomRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<GroupChatRoomModel>> GetGroupChatRoomByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<GroupChatMessageModel>>> GetMessagesAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Result<GroupChatMessageModel>> SendMessageAsync(int groupId, string content, CancellationToken cancellationToken = default);
}

public sealed class GroupChatService(AppDbContext dbContext, ICurrentUserContext currentUser) : IGroupChatService
{
    public async Task<Result<GroupChatRoomModel>> CreateGroupChatRoomAsync(CreateGroupChatRoomRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<GroupChatRoomModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        // STEP 3 Authorization rule: Only Premium creators (DOMAIN_PROFESSIONAL or PUBLIC_FIGURE) can create Group Chat
        if (!currentUser.IsPremiumCreator)
        {
            return Result<GroupChatRoomModel>.Failure("Only Premium users (Domain Professional or Public Figure) can create a Group Chat.", ResultStatus.Forbidden);
        }

        if (request.GroupId <= 0)
        {
            return Result<GroupChatRoomModel>.Failure("A valid GroupId is required.", ResultStatus.ValidationError);
        }

        var group = await dbContext.TblGroups
            .Include(g => g.Creator)
            .FirstOrDefaultAsync(g => g.GroupId == request.GroupId && !g.IsDeleted, cancellationToken);

        if (group is null)
        {
            return Result<GroupChatRoomModel>.Failure("Group not found.", ResultStatus.NotFound);
        }

        var existingRoom = await dbContext.TblGroupChatRooms
            .FirstOrDefaultAsync(r => r.GroupId == request.GroupId && !r.IsDeleted, cancellationToken);

        if (existingRoom is not null)
        {
            return Result<GroupChatRoomModel>.Failure("This group already has a Group Chat room.", ResultStatus.Conflict);
        }

        var chatType = string.Equals(request.ChatType, "PAID", StringComparison.OrdinalIgnoreCase) ? "PAID" : "FREE";
        var joinFee = chatType == "PAID" ? Math.Max(0, request.JoinFeeLinkDrops ?? 0) : 0;

        // Fetch current platform commission percentage
        var setting = await dbContext.TblPlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == "PlatformCommissionPercentage", cancellationToken);

        decimal commissionSnapshot = 10.00m;
        if (setting is not null && decimal.TryParse(setting.SettingValue, out var parsedPct))
        {
            commissionSnapshot = parsedPct;
        }

        var creatorId = currentUser.UserId.Value;

        var room = new TblGroupChatRoom
        {
            GroupId = request.GroupId,
            CreatorId = creatorId,
            ChatType = chatType,
            JoinFeeLinkDrops = joinFee,
            CommissionPercentageSnapshot = commissionSnapshot,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorId
        };

        dbContext.TblGroupChatRooms.Add(room);
        await dbContext.SaveChangesAsync(cancellationToken);

        var creatorUser = await dbContext.TblUsers.FindAsync([creatorId], cancellationToken);
        var creatorName = creatorUser != null
            ? (string.IsNullOrWhiteSpace(creatorUser.DisplayName) ? creatorUser.UserName : creatorUser.DisplayName)
            : "Unknown";

        var model = new GroupChatRoomModel(
            room.GroupChatRoomId,
            room.GroupId,
            group.Name,
            room.CreatorId,
            creatorName,
            room.ChatType,
            room.JoinFeeLinkDrops,
            room.CommissionPercentageSnapshot,
            room.IsActive,
            room.CreatedAt
        );

        return Result<GroupChatRoomModel>.Success(model);
    }

    public async Task<Result<GroupChatRoomModel>> GetGroupChatRoomByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<GroupChatRoomModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var group = await dbContext.TblGroups
            .Include(g => g.TblGroupMembers)
            .FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted, cancellationToken);

        if (group is null)
        {
            return Result<GroupChatRoomModel>.Failure("Group not found.", ResultStatus.NotFound);
        }

        // Must be a member of the parent Group or Admin
        bool isMember = currentUser.IsAdmin || group.TblGroupMembers.Any(m => m.UserId == userId && !m.IsDeleted);
        if (!isMember)
        {
            return Result<GroupChatRoomModel>.Failure("You must be a member of this group to view its chat room.", ResultStatus.Forbidden);
        }

        var room = await dbContext.TblGroupChatRooms
            .Include(r => r.Creator)
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.GroupId == groupId && !r.IsDeleted, cancellationToken);

        if (room is null)
        {
            return Result<GroupChatRoomModel>.Failure("Group Chat room not found for this group.", ResultStatus.NotFound);
        }

        var creatorName = room.Creator != null
            ? (string.IsNullOrWhiteSpace(room.Creator.DisplayName) ? room.Creator.UserName : room.Creator.DisplayName)
            : "Unknown";

        var model = new GroupChatRoomModel(
            room.GroupChatRoomId,
            room.GroupId,
            group.Name,
            room.CreatorId,
            creatorName,
            room.ChatType,
            room.JoinFeeLinkDrops,
            room.CommissionPercentageSnapshot,
            room.IsActive,
            room.CreatedAt
        );

        return Result<GroupChatRoomModel>.Success(model);
    }

    public async Task<Result<IReadOnlyList<GroupChatMessageModel>>> GetMessagesAsync(int groupId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<IReadOnlyList<GroupChatMessageModel>>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var group = await dbContext.TblGroups
            .Include(g => g.TblGroupMembers)
            .FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted, cancellationToken);

        if (group is null)
        {
            return Result<IReadOnlyList<GroupChatMessageModel>>.Failure("Group not found.", ResultStatus.NotFound);
        }

        bool isMember = currentUser.IsAdmin || group.TblGroupMembers.Any(m => m.UserId == userId && !m.IsDeleted);
        if (!isMember)
        {
            return Result<IReadOnlyList<GroupChatMessageModel>>.Failure("You must be a member of this group to view its messages.", ResultStatus.Forbidden);
        }

        var room = await dbContext.TblGroupChatRooms
            .FirstOrDefaultAsync(r => r.GroupId == groupId && !r.IsDeleted, cancellationToken);

        if (room is null)
        {
            return Result<IReadOnlyList<GroupChatMessageModel>>.Failure("Group Chat room not found.", ResultStatus.NotFound);
        }

        var messages = await dbContext.TblGroupChatMessages
            .Include(m => m.Sender)
            .Where(m => m.GroupChatRoomId == room.GroupChatRoomId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new GroupChatMessageModel(
                m.GroupChatMessageId,
                m.GroupChatRoomId,
                m.SenderId,
                string.IsNullOrWhiteSpace(m.Sender.DisplayName) ? m.Sender.UserName : m.Sender.DisplayName,
                m.Sender.AvatarUrl,
                m.Content,
                m.CreatedAt
            ))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GroupChatMessageModel>>.Success(messages);
    }

    public async Task<Result<GroupChatMessageModel>> SendMessageAsync(int groupId, string content, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<GroupChatMessageModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result<GroupChatMessageModel>.Failure("Message content is required.", ResultStatus.ValidationError);
        }

        var userId = currentUser.UserId.Value;

        var group = await dbContext.TblGroups
            .Include(g => g.TblGroupMembers)
            .FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted, cancellationToken);

        if (group is null)
        {
            return Result<GroupChatMessageModel>.Failure("Group not found.", ResultStatus.NotFound);
        }

        bool isMember = currentUser.IsAdmin || group.TblGroupMembers.Any(m => m.UserId == userId && !m.IsDeleted);
        if (!isMember)
        {
            return Result<GroupChatMessageModel>.Failure("You must be a member of this group to send messages.", ResultStatus.Forbidden);
        }

        var room = await dbContext.TblGroupChatRooms
            .FirstOrDefaultAsync(r => r.GroupId == groupId && !r.IsDeleted, cancellationToken);

        if (room is null)
        {
            return Result<GroupChatMessageModel>.Failure("Group Chat room not found.", ResultStatus.NotFound);
        }

        var msg = new TblGroupChatMessage
        {
            GroupChatRoomId = room.GroupChatRoomId,
            SenderId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        dbContext.TblGroupChatMessages.Add(msg);
        await dbContext.SaveChangesAsync(cancellationToken);

        var sender = await dbContext.TblUsers.FindAsync([userId], cancellationToken);
        var senderName = sender != null
            ? (string.IsNullOrWhiteSpace(sender.DisplayName) ? sender.UserName : sender.DisplayName)
            : "Unknown";

        var model = new GroupChatMessageModel(
            msg.GroupChatMessageId,
            msg.GroupChatRoomId,
            msg.SenderId,
            senderName,
            sender?.AvatarUrl,
            msg.Content,
            msg.CreatedAt
        );

        return Result<GroupChatMessageModel>.Success(model);
    }
}
