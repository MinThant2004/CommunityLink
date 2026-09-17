using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Domain.Features.Administration;

public interface IAdministrationService
{
    Task<Result<AdminDashboardStatsModel>> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserInfoModel>>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AuditLogModel>>> GetAuditLogsAsync(CancellationToken cancellationToken = default);
}

public sealed class AdministrationService(AppDbContext dbContext) : IAdministrationService
{
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

    public async Task<Result<IReadOnlyList<UserInfoModel>>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.TblUsers
            .Include(u => u.TblUserRoles).ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
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

        return Result<IReadOnlyList<UserInfoModel>>.Success(users);
    }

    public async Task<Result<IReadOnlyList<AuditLogModel>>> GetAuditLogsAsync(CancellationToken cancellationToken = default)
    {
        var logs = await dbContext.TblAuditLogs
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .Select(a => new AuditLogModel(
                a.AuditLogId,
                a.ActorId,
                a.ActorType,
                a.Action,
                a.EntityName,
                a.EntityId != null ? a.EntityId.ToString() : null,
                a.NewValues ?? a.OldValues,
                a.IpAddress,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AuditLogModel>>.Success(logs);
    }
}