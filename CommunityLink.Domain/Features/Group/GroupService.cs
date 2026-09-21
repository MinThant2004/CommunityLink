using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Group;

namespace CommunityLink.Domain.Features.Group;

public interface IGroupService
{
    Task<Result<IReadOnlyList<GroupModel>>> GetGroupsAsync(int? subCommunityId, string? search, CancellationToken cancellationToken = default);
    Task<Result<GroupModel>> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Result<GroupModel>> CreateGroupAsync(CreateGroupRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> JoinGroupAsync(int groupId, string? requestNote = null, CancellationToken cancellationToken = default);
    Task<Result> LeaveGroupAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<GroupMemberModel>>> GetGroupMembersAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<GroupJoinRequestModel>>> GetGroupJoinRequestsAsync(int groupId, CancellationToken cancellationToken = default);
    Task<Result> ReviewJoinRequestAsync(int requestId, bool approve, CancellationToken cancellationToken = default);
}

public sealed class GroupService(AppDbContext dbContext, ICurrentUserContext currentUser) : IGroupService
{
    public async Task<Result<IReadOnlyList<GroupModel>>> GetGroupsAsync(int? subCommunityId, string? search, CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUser.UserId;

        var query = dbContext.TblGroups
            .Include(g => g.Creator)
            .Include(g => g.SubCommunity)
            .Include(g => g.TblGroupMembers)
            .Include(g => g.TblGroupJoinRequests)
            .Include(g => g.TblPosts)
            .Where(g => !g.IsDeleted)
            .AsNoTracking();

        if (subCommunityId.HasValue && subCommunityId.Value > 0)
        {
            query = query.Where(g => g.SubCommunityId == subCommunityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(g => g.Name.Contains(s) || g.Slug.Contains(s));
        }

        var groups = await query.ToListAsync(cancellationToken);

        var list = groups.Select(g =>
        {
            var isMember = currentUserId.HasValue && g.TblGroupMembers.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
            var isPending = currentUserId.HasValue && g.TblGroupJoinRequests.Any(r => r.UserId == currentUserId.Value && r.Status == "PENDING" && !r.IsDeleted);
            var joinStatus = isMember ? "JOINED" : (isPending ? "PENDING" : "NONE");

            return new GroupModel(
                g.GroupId,
                g.SubCommunityId,
                g.SubCommunity != null ? g.SubCommunity.Name : null,
                g.CreatorId,
                g.Creator != null ? (string.IsNullOrWhiteSpace(g.Creator.DisplayName) ? g.Creator.UserName : g.Creator.DisplayName) : "Admin",
                g.Name,
                g.Slug,
                g.Description,
                g.AvatarUrl,
                g.BannerUrl,
                g.Visibility,
                g.JoinPolicy,
                g.TblGroupMembers.Count(m => !m.IsDeleted),
                g.TblPosts.Count(p => !p.IsDeleted),
                g.CreatedAt,
                isMember,
                joinStatus
            );
        }).ToList();

        return Result<IReadOnlyList<GroupModel>>.Success(list);
    }

    public async Task<Result<GroupModel>> GetGroupByIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var g = await dbContext.TblGroups
            .Include(g => g.Creator)
            .Include(g => g.SubCommunity)
            .Include(g => g.TblGroupMembers)
            .Include(g => g.TblGroupJoinRequests)
            .Include(g => g.TblPosts)
            .FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted, cancellationToken);

        if (g is null) return Result<GroupModel>.Failure("Group not found.", ResultStatus.NotFound);

        var currentUserId = currentUser.UserId;
        var isMember = currentUserId.HasValue && g.TblGroupMembers.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
        var isPending = currentUserId.HasValue && g.TblGroupJoinRequests.Any(r => r.UserId == currentUserId.Value && r.Status == "PENDING" && !r.IsDeleted);
        var joinStatus = isMember ? "JOINED" : (isPending ? "PENDING" : "NONE");

        var model = new GroupModel(
            g.GroupId,
            g.SubCommunityId,
            g.SubCommunity != null ? g.SubCommunity.Name : null,
            g.CreatorId,
            g.Creator != null ? (string.IsNullOrWhiteSpace(g.Creator.DisplayName) ? g.Creator.UserName : g.Creator.DisplayName) : "Admin",
            g.Name,
            g.Slug,
            g.Description,
            g.AvatarUrl,
            g.BannerUrl,
            g.Visibility,
            g.JoinPolicy,
            g.TblGroupMembers.Count(m => !m.IsDeleted),
            g.TblPosts.Count(p => !p.IsDeleted),
            g.CreatedAt,
            isMember,
            joinStatus
        );

        return Result<GroupModel>.Success(model);
    }

    public async Task<Result<GroupModel>> CreateGroupAsync(CreateGroupRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<GroupModel>.Failure("Group name is required.", ResultStatus.ValidationError);
        }

        if (request.SubCommunityId <= 0)
        {
            return Result<GroupModel>.Failure("A valid parent sub-community is required.", ResultStatus.ValidationError);
        }

        // Verify parent sub-community exists
        var subCommunity = await dbContext.TblCommunities
            .FirstOrDefaultAsync(c => c.CommunityId == request.SubCommunityId && !c.IsDeleted, cancellationToken);

        if (subCommunity is null)
        {
            return Result<GroupModel>.Failure("The selected sub-community does not exist.", ResultStatus.NotFound);
        }

        // Resolve creator ID
        int creatorId;
        bool isAdmin = currentUser.IsAdmin;

        if (currentUser.UserId.HasValue)
        {
            creatorId = currentUser.UserId.Value;
        }
        else
        {
            var fallbackUser = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted, cancellationToken);
            if (fallbackUser is null) return Result<GroupModel>.Failure("User not authenticated.", ResultStatus.Unauthorized);
            creatorId = fallbackUser.UserId;
        }

        // Business Rule: Admin OR User who joined the parent sub-community can create group
        if (!isAdmin)
        {
            var isSubCommunityMember = await dbContext.TblCommunityMembers
                .AnyAsync(m => m.CommunityId == request.SubCommunityId && m.UserId == creatorId && !m.IsDeleted, cancellationToken);

            if (!isSubCommunityMember)
            {
                return Result<GroupModel>.Failure("You must join this sub-community before you can create a group inside it.", ResultStatus.Forbidden);
            }
        }

        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToUpper();

        // Check duplicate name within the same sub-community
        var nameExists = await dbContext.TblGroups
            .AnyAsync(g => g.SubCommunityId == request.SubCommunityId && !g.IsDeleted && g.Name.ToUpper() == normalizedName, cancellationToken);

        if (nameExists)
        {
            return Result<GroupModel>.Failure("A group with this name already exists in this sub-community!", ResultStatus.Conflict);
        }

        // Generate unique slug
        var rawSlug = trimmedName.ToLowerInvariant().Replace(" ", "-");
        var slug = rawSlug;
        var slugCounter = 1;
        while (await dbContext.TblGroups.AnyAsync(g => g.Slug == slug, cancellationToken))
        {
            slug = $"{rawSlug}-{slugCounter++}";
        }

        var visibility = string.Equals(request.Visibility, "PRIVATE", StringComparison.OrdinalIgnoreCase) ? "PRIVATE" : "PUBLIC";
        var joinPolicy = visibility == "PRIVATE" ? "APPROVAL_REQUIRED" : "INSTANT";

        var group = new TblGroup
        {
            SubCommunityId = request.SubCommunityId,
            CreatorId = creatorId,
            Name = trimmedName,
            Slug = slug,
            Description = request.Description?.Trim(),
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl,
            Visibility = visibility,
            JoinPolicy = joinPolicy,
            MemberCount = 1,
            PostCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorId
        };

        dbContext.TblGroups.Add(group);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Add creator as Owner member of the group
        dbContext.TblGroupMembers.Add(new TblGroupMember
        {
            GroupId = group.GroupId,
            UserId = creatorId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = creatorId
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetGroupByIdAsync(group.GroupId, cancellationToken);
    }

    public async Task<Result> JoinGroupAsync(int groupId, string? requestNote = null, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        var userId = currentUser.UserId.Value;

        var group = await dbContext.TblGroups.FirstOrDefaultAsync(g => g.GroupId == groupId && !g.IsDeleted, cancellationToken);
        if (group is null) return Result.Failure("Group not found.", ResultStatus.NotFound);

        // Check if already a member
        var existingMember = await dbContext.TblGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, cancellationToken);

        if (existingMember is not null && !existingMember.IsDeleted)
        {
            return Result.Failure("Already a member of this group.", ResultStatus.Conflict);
        }

        // If private group, create a Join Request
        if (group.Visibility == "PRIVATE" || group.JoinPolicy == "APPROVAL_REQUIRED")
        {
            var existingRequest = await dbContext.TblGroupJoinRequests
                .FirstOrDefaultAsync(r => r.GroupId == groupId && r.UserId == userId && r.Status == "PENDING" && !r.IsDeleted, cancellationToken);

            if (existingRequest is not null)
            {
                return Result.Failure("You already have a pending join request for this group.", ResultStatus.Conflict);
            }

            dbContext.TblGroupJoinRequests.Add(new TblGroupJoinRequest
            {
                GroupId = groupId,
                UserId = userId,
                Status = "PENDING",
                RequestNote = requestNote?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success("Join request submitted. Waiting for group owner approval.");
        }

        // Public group: Instant Join
        if (existingMember is not null)
        {
            existingMember.IsDeleted = false;
            existingMember.UpdatedAt = DateTime.UtcNow;
            existingMember.UpdatedBy = userId;
        }
        else
        {
            dbContext.TblGroupMembers.Add(new TblGroupMember
            {
                GroupId = groupId,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }

        group.MemberCount = await dbContext.TblGroupMembers.CountAsync(m => m.GroupId == groupId && !m.IsDeleted, cancellationToken) + 1;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Joined group successfully.");
    }

    public async Task<Result> LeaveGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        var userId = currentUser.UserId.Value;

        var member = await dbContext.TblGroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (member is null) return Result.Failure("You are not a member of this group.", ResultStatus.NotFound);

        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;
        member.DeletedBy = userId;

        var group = await dbContext.TblGroups.FindAsync([groupId], cancellationToken);
        if (group is not null)
        {
            group.MemberCount = Math.Max(0, group.MemberCount - 1);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Left group successfully.");
    }

    public async Task<Result<IReadOnlyList<GroupMemberModel>>> GetGroupMembersAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var members = await dbContext.TblGroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId && !m.IsDeleted)
            .OrderByDescending(m => m.Role == "Owner")
            .ThenByDescending(m => m.Role == "Admin")
            .ThenBy(m => m.JoinedAt)
            .Select(m => new GroupMemberModel(
                m.GroupMemberId,
                m.GroupId,
                m.UserId,
                m.User.UserName,
                string.IsNullOrWhiteSpace(m.User.DisplayName) ? m.User.UserName : m.User.DisplayName,
                m.User.AvatarUrl,
                m.Role,
                m.JoinedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GroupMemberModel>>.Success(members);
    }

    public async Task<Result<IReadOnlyList<GroupJoinRequestModel>>> GetGroupJoinRequestsAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var requests = await dbContext.TblGroupJoinRequests
            .Include(r => r.Group)
            .Include(r => r.User)
            .Where(r => r.GroupId == groupId && r.Status == "PENDING" && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GroupJoinRequestModel(
                r.GroupJoinRequestId,
                r.GroupId,
                r.Group.Name,
                r.UserId,
                string.IsNullOrWhiteSpace(r.User.DisplayName) ? r.User.UserName : r.User.DisplayName,
                r.User.Email,
                r.RequestNote,
                r.Status,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GroupJoinRequestModel>>.Success(requests);
    }

    public async Task<Result> ReviewJoinRequestAsync(int requestId, bool approve, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.TblGroupJoinRequests
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.GroupJoinRequestId == requestId && !r.IsDeleted, cancellationToken);

        if (request is null) return Result.Failure("Request not found.", ResultStatus.NotFound);

        int reviewerId = currentUser.UserId ?? request.Group.CreatorId;

        request.Status = approve ? "APPROVED" : "REJECTED";
        request.ReviewedBy = reviewerId;
        request.ReviewedAt = DateTime.UtcNow;

        if (approve)
        {
            var existingMember = await dbContext.TblGroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == request.GroupId && m.UserId == request.UserId, cancellationToken);

            if (existingMember is not null)
            {
                existingMember.IsDeleted = false;
                existingMember.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                dbContext.TblGroupMembers.Add(new TblGroupMember
                {
                    GroupId = request.GroupId,
                    UserId = request.UserId,
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = reviewerId
                });
            }

            request.Group.MemberCount = await dbContext.TblGroupMembers.CountAsync(m => m.GroupId == request.GroupId && !m.IsDeleted, cancellationToken) + 1;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(approve ? "Join request approved!" : "Join request rejected.");
    }
}
