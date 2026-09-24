using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.Administration;

public interface IAdministrationService
{
    Task<Result<AdminDashboardStatsModel>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<Result<AdminDashboardModel>> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserInfoModel>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<PagedResult<UserInfoModel>>> GetUsersPageAsync(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditLogModel>>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
    Task<Result<PagedResult<AuditLogModel>>> GetAuditLogsPageAsync(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<JoinRequestModel>>> GetPendingJoinRequestsAsync(CancellationToken cancellationToken = default);
    Task<Result> ApproveJoinRequestAsync(int requestId, CancellationToken cancellationToken = default);
    Task<Result> RejectJoinRequestAsync(int requestId, CancellationToken cancellationToken = default);
    Task<Result> AssignUserRoleAsync(AssignUserRoleRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class AdministrationService(AppDbContext dbContext) : IAdministrationService
{
    public async Task<Result> AssignUserRoleAsync(AssignUserRoleRequestModel request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId && !u.IsDeleted, cancellationToken);
        if (user is null) return Result.Failure("User not found.", ResultStatus.NotFound);

        var role = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId && !r.IsDeleted, cancellationToken);
        if (role is null) return Result.Failure("Role not found.", ResultStatus.NotFound);

        var existingUserRoles = await dbContext.TblUserRoles.Where(ur => ur.UserId == request.UserId).ToListAsync(cancellationToken);
        dbContext.TblUserRoles.RemoveRange(existingUserRoles);

        dbContext.TblUserRoles.Add(new TblUserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success($"Role assigned to '{role.RoleName}' successfully.");
    }
    public async Task<Result<AdminDashboardStatsModel>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await dbContext.TblUsers.CountAsync(u => !u.IsDeleted, cancellationToken);
        var totalCommunities = await dbContext.TblCommunities.CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalPosts = await dbContext.TblPosts.CountAsync(p => !p.IsDeleted, cancellationToken);
        var totalPolls = await dbContext.TblPolls.CountAsync(p => !p.IsDeleted, cancellationToken);
        var activeConversations = await dbContext.TblConversations.CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalAuditLogs = await dbContext.TblAuditLogs.CountAsync(cancellationToken);

        var model = new AdminDashboardStatsModel(
            totalUsers,
            totalCommunities,
            totalPosts,
            totalPolls,
            activeConversations,
            totalAuditLogs);

        return Result<AdminDashboardStatsModel>.Success(model);
    }

    public async Task<Result<AdminDashboardModel>> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var statsRes = await GetDashboardStatsAsync(cancellationToken);
        var stats = statsRes.Data!;

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var newUsersGrouped = await dbContext.TblUsers
            .Where(u => !u.IsDeleted && u.CreatedAt >= thirtyDaysAgo)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var newPostsGrouped = await dbContext.TblPosts
            .Where(p => !p.IsDeleted && p.CreatedAt >= thirtyDaysAgo)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var trends = new List<TrendPointModel>();
        for (int i = 29; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var usersCount = newUsersGrouped.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            var postsCount = newPostsGrouped.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            trends.Add(new TrendPointModel(date, usersCount, postsCount, usersCount + postsCount));
        }

        var pendingReqsRes = await GetPendingJoinRequestsAsync(cancellationToken);
        var pendingReqs = pendingReqsRes.Data ?? [];

        return Result<AdminDashboardModel>.Success(new AdminDashboardModel(stats, trends, pendingReqs));
    }

    public async Task<Result<IReadOnlyList<UserInfoModel>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var pageRes = await GetUsersPageAsync(1, 100, null, cancellationToken);
        return Result<IReadOnlyList<UserInfoModel>>.Success(pageRes.Data?.Items ?? []);
    }

    public async Task<Result<PagedResult<UserInfoModel>>> GetUsersPageAsync(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblUsers
            .Include(u => u.TblUserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.UserName.ToLower().Contains(s) ||
                                     u.DisplayName.ToLower().Contains(s) ||
                                     u.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserInfoModel(
                u.UserId,
                u.UserName,
                u.DisplayName,
                u.Email,
                u.Bio,
                u.AvatarUrl,
                u.TblUserRoles.Select(r => r.Role.RoleCode).FirstOrDefault() ?? "MEMBER",
                u.TblUserRoles.Select(r => r.Role.RoleId).FirstOrDefault(),
                u.IsActive,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<UserInfoModel>>.Success(new PagedResult<UserInfoModel>(items, totalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<AuditLogModel>>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var pageRes = await GetAuditLogsPageAsync(1, 100, null, cancellationToken);
        return Result<IReadOnlyList<AuditLogModel>>.Success(pageRes.Data?.Items ?? []);
    }

    public async Task<Result<PagedResult<AuditLogModel>>> GetAuditLogsPageAsync(int page = 1, int pageSize = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblAuditLogs
            .Where(a => !a.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(a => a.Action.ToLower().Contains(s) ||
                                     a.EntityName.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rawLogs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var actorIds = rawLogs.Where(l => l.ActorId.HasValue).Select(l => l.ActorId!.Value).Distinct().ToList();
        var userMap = await dbContext.TblUsers
            .Where(u => actorIds.Contains(u.UserId))
            .ToDictionaryAsync(u => u.UserId, u => u.DisplayName, cancellationToken);

        var items = rawLogs.Select(a => new AuditLogModel(
            a.AuditLogId,
            a.ActorId,
            a.ActorId.HasValue && userMap.TryGetValue(a.ActorId.Value, out var name) ? name : "System",
            a.ActorType ?? "User",
            a.Action,
            a.EntityName,
            a.EntityId != null ? a.EntityId.ToString() : null,
            a.NewValues ?? a.OldValues,
            a.IpAddress,
            a.CreatedAt)).ToList();

        return Result<PagedResult<AuditLogModel>>.Success(new PagedResult<AuditLogModel>(items, totalCount, page, pageSize));
    }

    public async Task<Result<IReadOnlyList<JoinRequestModel>>> GetPendingJoinRequestsAsync(CancellationToken cancellationToken = default)
    {
        var reqs = await dbContext.TblCommunityJoinRequests
            .Include(r => r.Community)
            .Include(r => r.User)
            .Where(r => r.Status == "PENDING" && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new JoinRequestModel(
                r.CommunityJoinRequestId,
                r.CommunityId,
                r.Community.Name,
                r.UserId,
                r.User.DisplayName,
                r.User.Email,
                r.Status,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<JoinRequestModel>>.Success(reqs);
    }

    public async Task<Result> ApproveJoinRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var req = await dbContext.TblCommunityJoinRequests.FindAsync([requestId], cancellationToken);
        if (req is null || req.IsDeleted) return Result.Failure("Join request not found.", ResultStatus.NotFound);

        req.Status = "APPROVED";
        req.UpdatedAt = DateTime.UtcNow;

        var existingMember = await dbContext.TblCommunityMembers
            .FirstOrDefaultAsync(m => m.CommunityId == req.CommunityId && m.UserId == req.UserId, cancellationToken);

        if (existingMember is null)
        {
            dbContext.TblCommunityMembers.Add(new TblCommunityMember
            {
                CommunityId = req.CommunityId,
                UserId = req.UserId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
        else if (existingMember.IsDeleted)
        {
            existingMember.IsDeleted = false;
            existingMember.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Join request approved.");
    }

    public async Task<Result> RejectJoinRequestAsync(int requestId, CancellationToken cancellationToken = default)
    {
        var req = await dbContext.TblCommunityJoinRequests.FindAsync([requestId], cancellationToken);
        if (req is null || req.IsDeleted) return Result.Failure("Join request not found.", ResultStatus.NotFound);

        req.Status = "REJECTED";
        req.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Join request rejected.");
    }
}