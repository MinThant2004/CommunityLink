using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Domain.Services;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

    // Admin Account Management
    Task<Result<IReadOnlyList<AdminAccountModel>>> GetAdminAccountsAsync(CancellationToken cancellationToken = default);
    Task<Result<string>> CreateAdminInviteAsync(CreateAdminInviteRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<VerifyAdminInviteResponseModel>> VerifyAdminInviteTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Result> SetupAdminPasswordAsync(SetupAdminPasswordRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> ToggleAdminStatusAsync(int adminId, CancellationToken cancellationToken = default);

    // Content Moderation & Reporting
    Task<Result> SubmitContentReportAsync(CreateContentReportRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ContentReportModel>>> GetContentReportsAsync(string? status = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ContentItemAdminModel>>> GetModeratedContentListAsync(string? filter = "ALL", CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<GroupItemAdminModel>>> GetModeratedGroupsListAsync(CancellationToken cancellationToken = default);
    Task<Result> ModerateContentAsync(ModerateContentRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class AdministrationService(
    AppDbContext dbContext,
    IEmailSender emailSender,
    IConfiguration configuration,
    ICurrentUserContext currentUser,
    CommunityLink.Domain.Features.Notification.INotificationService notificationService) : IAdministrationService
{
    public async Task<Result> AssignUserRoleAsync(AssignUserRoleRequestModel request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId && !u.IsDeleted, cancellationToken);
        if (user is null) return Result.Failure("User not found.", ResultStatus.NotFound);

        var role = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId && !r.IsDeleted, cancellationToken);
        if (role is null) return Result.Failure("Role not found.", ResultStatus.NotFound);

        var existingUserRoles = await dbContext.TblUserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var oldRoleName = existingUserRoles.Select(ur => ur.Role?.RoleName ?? ur.Role?.RoleCode).FirstOrDefault() ?? "None";
        dbContext.TblUserRoles.RemoveRange(existingUserRoles);

        dbContext.TblUserRoles.Add(new TblUserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            CreatedAt = DateTime.UtcNow
        });

        // Audit Log for USER role change
        dbContext.TblAuditLogs.Add(new TblAuditLog
        {
            ActorType = currentUser.RoleCode ?? "ADMIN",
            ActorId = currentUser.UserId,
            Action = "CHANGE",
            EntityName = "USER",
            EntityId = user.UserId,
            OldValues = $"Role: {oldRoleName}",
            NewValues = $"Role: {role.RoleName} ({role.RoleCode})",
            ChangedColumns = "RoleId",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success($"Role assigned to '{role.RoleName}' successfully.");
    }
    public async Task<Result<AdminDashboardStatsModel>> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await dbContext.TblUsers.CountAsync(u => !u.IsDeleted, cancellationToken);
        var totalCommunities = await dbContext.TblCommunities.CountAsync(c => !c.IsDeleted && c.ParentCommunityId == null, cancellationToken);
        var totalPosts = await dbContext.TblPosts.CountAsync(p => !p.IsDeleted, cancellationToken);
        var totalPolls = await dbContext.TblPolls.CountAsync(p => !p.IsDeleted, cancellationToken);
        var activeConversations = await dbContext.TblConversations.CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalAuditLogs = await dbContext.TblAuditLogs.CountAsync(cancellationToken);

        // Modernized LinkDrop Economy & Ecosystem Metrics
        var totalCirculation = await dbContext.TblLinkDropWallets.SumAsync(w => (long?)w.Balance, cancellationToken) ?? 0L;
        var approvedPurchases = dbContext.TblLinkDropPurchases.Where(p => !p.IsDeleted && p.Status == "APPROVED");
        var totalPurchasesCount = await approvedPurchases.CountAsync(cancellationToken);
        var totalRevenueMmk = await approvedPurchases.SumAsync(p => (decimal?)p.SnapshotRealMoneyAmount, cancellationToken) ?? 0m;

        // Current month LinkDrop sold
        var nowUtc = DateTime.UtcNow;
        var currentMonthStart = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthName = nowUtc.ToString("MMMM");
        var currentMonthLinkDropSold = await approvedPurchases
            .Where(p => p.CreatedAt >= currentMonthStart)
            .SumAsync(p => (long?)p.SnapshotLinkDropAmount, cancellationToken) ?? 0L;

        var totalGroups = await dbContext.TblGroups.CountAsync(g => !g.IsDeleted, cancellationToken);
        var totalCreatorPayoutsPending = await dbContext.TblCreatorPayoutRequests.CountAsync(r => !r.IsDeleted && r.Status == "PENDING", cancellationToken);
        var totalVerificationsPending = await dbContext.TblIdentityVerifications.CountAsync(v => v.Status == "PENDING", cancellationToken);
        var totalChatGroups = await dbContext.TblChatGroups.CountAsync(g => !g.IsDeleted, cancellationToken);

        var model = new AdminDashboardStatsModel(
            totalUsers,
            totalCommunities,
            totalPosts,
            totalPolls,
            activeConversations,
            totalAuditLogs,
            totalCirculation,
            totalPurchasesCount,
            totalRevenueMmk,
            totalGroups,
            totalCreatorPayoutsPending,
            totalVerificationsPending,
            totalChatGroups,
            currentMonthLinkDropSold,
            currentMonthName);

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

        var linkDropTxGrouped = await dbContext.TblLinkDropTransactions
            .Where(tx => tx.CreatedAt >= thirtyDaysAgo)
            .GroupBy(tx => tx.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Volume = g.Sum(tx => (long)tx.Amount) })
            .ToListAsync(cancellationToken);

        var trends = new List<TrendPointModel>();
        for (int i = 29; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var usersCount = newUsersGrouped.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            var postsCount = newPostsGrouped.FirstOrDefault(x => x.Date == date)?.Count ?? 0;
            var txVolume = linkDropTxGrouped.FirstOrDefault(x => x.Date == date)?.Volume ?? 0L;
            trends.Add(new TrendPointModel(date, usersCount, postsCount, usersCount + postsCount, txVolume));
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
        var adminMap = await dbContext.TblAdmins
            .Where(a => actorIds.Contains(a.AdminId))
            .ToDictionaryAsync(a => a.AdminId, a => string.IsNullOrWhiteSpace(a.FullName) ? a.Email : a.FullName, cancellationToken);

        var items = rawLogs.Select(a => {
            string actorName = "System";
            if (a.ActorId.HasValue)
            {
                if (adminMap.TryGetValue(a.ActorId.Value, out var admName))
                    actorName = admName;
                else if (userMap.TryGetValue(a.ActorId.Value, out var usrName))
                    actorName = usrName;
                else
                    actorName = $"User #{a.ActorId.Value}";
            }

            return new AuditLogModel(
                a.AuditLogId,
                a.ActorId,
                actorName,
                a.ActorType ?? "User",
                a.Action,
                a.EntityName,
                a.EntityId != null ? a.EntityId.ToString() : null,
                a.OldValues,
                a.NewValues,
                a.ChangedColumns,
                a.NewValues ?? a.OldValues,
                a.IpAddress,
                a.CreatedAt,
                a.UpdatedAt);
        }).ToList();

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

    public async Task<Result<IReadOnlyList<AdminAccountModel>>> GetAdminAccountsAsync(CancellationToken cancellationToken = default)
    {
        var admins = await dbContext.TblAdmins
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .Include(a => a.TblAdminRoles)
            .ThenInclude(ar => ar.Role)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AdminAccountModel(
                a.AdminId,
                a.Email,
                a.FullName,
                a.TblAdminRoles.Where(ar => !ar.IsDeleted).Select(ar => ar.Role.RoleCode).FirstOrDefault() ?? "ADMIN",
                a.TblAdminRoles.Where(ar => !ar.IsDeleted).Select(ar => ar.Role.RoleName).FirstOrDefault() ?? "Administrator",
                a.IsSuperAdmin,
                a.IsActive,
                a.LastLoginAt,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AdminAccountModel>>.Success(admins);
    }

    public async Task<Result<string>> CreateAdminInviteAsync(CreateAdminInviteRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result<string>.Failure("A valid email address is required.", ResultStatus.ValidationError);

        var normalizedEmail = request.Email.ToUpperInvariant();

        // Check if admin already exists
        if (await dbContext.TblAdmins.AnyAsync(a => a.NormalizedEmail == normalizedEmail && !a.IsDeleted, cancellationToken))
            return Result<string>.Failure("An administrator with this email already exists.", ResultStatus.Conflict);

        // All invited accounts are given the ADMIN system role; distinction is made by IsSuperAdmin (SYSTEMADMIN vs ADMIN)
        var role = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN" && !r.IsDeleted, cancellationToken);
        if (role == null)
            return Result<string>.Failure("ADMIN role was not found in system.", ResultStatus.NotFound);

        // Generate a 15-minute secure invitation token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(15);

        // Invalidate any previous unconsumed invites for this email
        var pendingInvites = await dbContext.TblAdminInvites
            .Where(i => i.Email == normalizedEmail && !i.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var inv in pendingInvites)
        {
            inv.IsUsed = true;
        }

        var newInvite = new TblAdminInvite
        {
            Email = request.Email.Trim(),
            Token = token,
            RoleId = role.RoleId,
            IsSuperAdmin = request.IsSuperAdmin,
            ExpiresAtUtc = expiresAtUtc,
            IsUsed = false,
            CreatedBy = currentUser.UserId,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.TblAdminInvites.Add(newInvite);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Generate setup link (expires in 15 minutes)
        var appBaseUrl = configuration["AppBaseUrl"] ?? "https://localhost:56537";
        var setupLink = $"{appBaseUrl.TrimEnd('/')}/admin/portal-entry/setup-password?token={token}";

        // Send Email
        var adminTypeLabel = request.IsSuperAdmin ? "System Administrator (SYSTEMADMIN)" : "Administrator (ADMIN)";
        var emailSubject = "CommunityLink — Admin Account Creation & Setup";
        var emailBody = $@"
            <div style=""font-family: 'Inter', -apple-system, sans-serif; max-width: 560px; margin: 0 auto; padding: 28px; border: 1px solid #E2E8F0; border-radius: 12px; background: #ffffff;"">
                <div style=""margin-bottom: 20px;"">
                    <span style=""font-size: 11px; font-weight: bold; letter-spacing: 1px; color: #557392; text-transform: uppercase;"">COMMUNITYLINK OPERATOR GATEWAY</span>
                    <h2 style=""color: #0A1B2E; margin: 8px 0 4px 0; font-size: 22px;"">Admin Account Provisioned</h2>
                    <p style=""color: #557392; font-size: 14px; margin: 0;"">You have been granted administrator access as <strong>{adminTypeLabel}</strong>.</p>
                </div>
                <div style=""background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 8px; padding: 18px; margin: 20px 0;"">
                    <p style=""margin: 0 0 12px 0; font-size: 13px; color: #0A1B2E;"">To complete your account initialization, please create your password below. This link is cryptographically signed and valid for <strong>15 minutes</strong>.</p>
                    <div style=""text-align: center; margin: 18px 0;"">
                        <a href=""{setupLink}"" style=""display: inline-block; background-color: #0A1B2E; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 6px; font-weight: 600; font-size: 14px;"">Set Up Admin Password &rarr;</a>
                    </div>
                    <p style=""margin: 10px 0 0 0; font-size: 11px; color: #BA1A1A; text-align: center;"">⚠️ Note: Link will expire strictly in 15 minutes.</p>
                </div>
                <p style=""font-size: 12px; color: #7A8CA6; margin-top: 24px;"">If you did not anticipate this invitation, please disregard this email or report to security.</p>
            </div>";

        try
        {
            await emailSender.SendEmailAsync(request.Email.Trim(), emailSubject, emailBody, cancellationToken);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send invitation email: {ex.Message}");
        }

        // Audit log for admin invitation / account creation
        dbContext.TblAuditLogs.Add(new TblAuditLog
        {
            ActorType = currentUser.RoleCode ?? "ADMIN",
            ActorId = currentUser.UserId,
            Action = "CREATE",
            EntityName = "USER",
            OldValues = null,
            NewValues = $"Admin Invite: {request.Email.Trim()} (SuperAdmin: {request.IsSuperAdmin})",
            ChangedColumns = "AdminInvite",
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(setupLink, "Admin account invitation link generated and sent. Valid for 15 minutes.");
    }

    public async Task<Result<VerifyAdminInviteResponseModel>> VerifyAdminInviteTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result<VerifyAdminInviteResponseModel>.Failure("Invalid invitation link.", ResultStatus.ValidationError);

        var invite = await dbContext.TblAdminInvites
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

        if (invite == null || invite.IsUsed)
            return Result<VerifyAdminInviteResponseModel>.Failure("This setup link is invalid or has already been used.", ResultStatus.ValidationError);

        if (DateTime.UtcNow > invite.ExpiresAtUtc)
            return Result<VerifyAdminInviteResponseModel>.Failure("This setup link has expired (15-minute validity). Please request a new invitation from an administrator.", ResultStatus.ValidationError);

        var adminTypeName = invite.IsSuperAdmin ? "System Administrator" : "Administrator";
        var response = new VerifyAdminInviteResponseModel(
            invite.Email,
            null,
            adminTypeName,
            invite.IsSuperAdmin,
            invite.ExpiresAtUtc);

        return Result<VerifyAdminInviteResponseModel>.Success(response);
    }

    public async Task<Result> SetupAdminPasswordAsync(SetupAdminPasswordRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure("Invalid invitation token.", ResultStatus.ValidationError);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return Result.Failure("Password must be at least 8 characters long.", ResultStatus.ValidationError);

        var confirmPassword = string.IsNullOrWhiteSpace(request.ConfirmPassword) ? request.Password : request.ConfirmPassword;
        if (request.Password != confirmPassword)
            return Result.Failure("Passwords do not match.", ResultStatus.ValidationError);

        var invite = await dbContext.TblAdminInvites
            .Include(i => i.Role)
            .FirstOrDefaultAsync(i => i.Token == request.Token, cancellationToken);

        if (invite == null || invite.IsUsed)
            return Result.Failure("This setup link is invalid or has already been used.", ResultStatus.ValidationError);

        if (DateTime.UtcNow > invite.ExpiresAtUtc)
            return Result.Failure("This setup link has expired (15-minute validity). Please request a new invitation from an administrator.", ResultStatus.ValidationError);

        var normalizedEmail = invite.Email.ToUpperInvariant();

        // Check if admin already exists
        var existingAdmin = await dbContext.TblAdmins.FirstOrDefaultAsync(a => a.NormalizedEmail == normalizedEmail, cancellationToken);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        if (existingAdmin != null)
        {
            existingAdmin.PasswordHash = passwordHash;
            existingAdmin.IsSuperAdmin = invite.IsSuperAdmin;
            existingAdmin.IsActive = true;
            existingAdmin.IsDeleted = false;
            existingAdmin.UpdatedAt = DateTime.UtcNow;

            var existingRole = await dbContext.TblAdminRoles.FirstOrDefaultAsync(ar => ar.AdminId == existingAdmin.AdminId && !ar.IsDeleted, cancellationToken);
            if (existingRole != null)
            {
                existingRole.RoleId = invite.RoleId;
                existingRole.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                dbContext.TblAdminRoles.Add(new TblAdminRole
                {
                    AdminId = existingAdmin.AdminId,
                    RoleId = invite.RoleId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            var admin = new TblAdmin
            {
                Email = invite.Email,
                NormalizedEmail = normalizedEmail,
                FullName = invite.Email.Split('@')[0],
                PasswordHash = passwordHash,
                IsSuperAdmin = invite.IsSuperAdmin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.TblAdmins.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);

            dbContext.TblAdminRoles.Add(new TblAdminRole
            {
                AdminId = admin.AdminId,
                RoleId = invite.RoleId,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Mark invite as used
        invite.IsUsed = true;
        invite.UsedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Admin password created successfully. You may now log in to the Admin Portal.");
    }

    public async Task<Result> ToggleAdminStatusAsync(int adminId, CancellationToken cancellationToken = default)
    {
        // Only SYSTEMADMIN (IsSuperAdmin) can activate or deactivate other admins
        var callerAdmin = await dbContext.TblAdmins.FirstOrDefaultAsync(a => a.AdminId == currentUser.UserId && !a.IsDeleted, cancellationToken);
        if (callerAdmin == null || !callerAdmin.IsSuperAdmin)
        {
            return Result.Failure("Only System Administrators (SYSTEMADMIN) have permission to activate or deactivate administrator accounts.", ResultStatus.Forbidden);
        }

        var targetAdmin = await dbContext.TblAdmins.FirstOrDefaultAsync(a => a.AdminId == adminId && !a.IsDeleted, cancellationToken);
        if (targetAdmin == null)
            return Result.Failure("Admin account not found.", ResultStatus.NotFound);

        if (targetAdmin.AdminId == callerAdmin.AdminId)
            return Result.Failure("You cannot deactivate your own account.", ResultStatus.Forbidden);

        if (targetAdmin.IsSuperAdmin)
            return Result.Failure("System Administrator (SYSTEMADMIN) accounts cannot be deactivated.", ResultStatus.Forbidden);

        targetAdmin.IsActive = !targetAdmin.IsActive;
        targetAdmin.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        var status = targetAdmin.IsActive ? "activated" : "deactivated";
        return Result.Success($"Admin account '{targetAdmin.Email}' has been {status}.");
    }

    #region Content Moderation & Reporting

    public async Task<Result> SubmitContentReportAsync(CreateContentReportRequestModel request, CancellationToken cancellationToken = default)
    {
        if (!currentUser.UserId.HasValue)
            return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        if (string.IsNullOrWhiteSpace(request.ReasonCategory))
            return Result.Failure("Please select a reason for reporting.", ResultStatus.ValidationError);

        var report = new TblContentReport
        {
            ContentType = request.ContentType.ToUpperInvariant(),
            ContentId = request.ContentId,
            ReporterUserId = currentUser.UserId.Value,
            ReasonCategory = request.ReasonCategory.Trim(),
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblContentReports.Add(report);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Report submitted successfully. Our administration team has been notified and will review it promptly.");
    }

    public async Task<Result<IReadOnlyList<ContentReportModel>>> GetContentReportsAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblContentReports
            .Include(r => r.ReporterUser)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
        {
            query = query.Where(r => r.Status == status);
        }

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var list = new List<ContentReportModel>();

        foreach (var r in reports)
        {
            int authorId = 0;
            string authorName = "Unknown";
            string? authorAvatar = null;
            string summary = "Content unavailable";
            string? commName = null;
            string? grpName = null;
            bool isActive = true;
            bool isPrivate = false;
            DateTime contentCreatedAt = r.CreatedAt;

            if (r.ContentType == "POST")
            {
                var post = await dbContext.TblPosts
                    .Include(p => p.Author)
                    .Include(p => p.Community)
                    .Include(p => p.Group)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PostId == r.ContentId, cancellationToken);

                if (post != null)
                {
                    authorId = post.AuthorId;
                    authorName = post.Author.DisplayName ?? post.Author.UserName;
                    authorAvatar = post.Author.AvatarUrl;
                    summary = post.Content;
                    commName = post.Community?.Name;
                    grpName = post.Group?.Name;
                    isActive = post.IsActive && !post.IsDeleted;
                    isPrivate = post.IsPrivate;
                    contentCreatedAt = post.CreatedAt;
                }
            }
            else if (r.ContentType == "POLL")
            {
                var poll = await dbContext.TblPolls
                    .Include(p => p.Post).ThenInclude(pt => pt.Author)
                    .Include(p => p.Post).ThenInclude(pt => pt.Community)
                    .Include(p => p.Post).ThenInclude(pt => pt.Group)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PollId == r.ContentId, cancellationToken);

                if (poll != null)
                {
                    authorId = poll.Post?.AuthorId ?? (poll.CreatedBy ?? 0);
                    authorName = poll.Post?.Author?.DisplayName ?? poll.Post?.Author?.UserName ?? "Author";
                    authorAvatar = poll.Post?.Author?.AvatarUrl;
                    summary = $"Q: {poll.Question}" + (string.IsNullOrWhiteSpace(poll.Post?.Content) ? "" : $" — {poll.Post.Content}");
                    commName = poll.Post?.Community?.Name;
                    grpName = poll.Post?.Group?.Name;
                    isActive = poll.IsActive && !poll.IsDeleted;
                    isPrivate = poll.IsPrivate;
                    contentCreatedAt = poll.CreatedAt;
                }
            }
            else if (r.ContentType == "GROUP")
            {
                var grp = await dbContext.TblGroups
                    .Include(g => g.Creator)
                    .Include(g => g.SubCommunity)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupId == r.ContentId, cancellationToken);

                if (grp != null)
                {
                    authorId = grp.CreatorId;
                    authorName = grp.Creator?.DisplayName ?? grp.Creator?.UserName ?? "Group Creator";
                    authorAvatar = grp.AvatarUrl;
                    summary = $"Group: {grp.Name}" + (string.IsNullOrWhiteSpace(grp.Description) ? "" : $" — {grp.Description}");
                    commName = grp.SubCommunity?.Name;
                    grpName = grp.Name;
                    isActive = grp.IsActive && !grp.IsDeleted;
                    isPrivate = grp.Visibility == "PRIVATE";
                    contentCreatedAt = grp.CreatedAt;
                }
            }

            list.Add(new ContentReportModel(
                r.ContentReportId,
                r.ContentType,
                r.ContentId,
                r.ReporterUserId,
                r.ReporterUser?.DisplayName ?? r.ReporterUser?.UserName ?? "Reporter",
                r.ReporterUser?.AvatarUrl,
                r.ReasonCategory,
                r.Details,
                r.Status,
                r.HandledByAdminId,
                r.AdminNote,
                r.HandledAt,
                r.CreatedAt,
                authorId,
                authorName,
                authorAvatar,
                summary,
                commName,
                grpName,
                isActive,
                isPrivate,
                contentCreatedAt
            ));
        }

        return Result<IReadOnlyList<ContentReportModel>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<ContentItemAdminModel>>> GetModeratedContentListAsync(string? filter = "ALL", CancellationToken cancellationToken = default)
    {
        var list = new List<ContentItemAdminModel>();

        // Query posts (Note: When a post/poll is Private, even admins cannot view it)
        if (filter == "ALL" || filter == "POST")
        {
            var posts = await dbContext.TblPosts
                .Include(p => p.Author)
                .Include(p => p.Community)
                .Include(p => p.Group)
                .Include(p => p.TblPostImages)
                .Where(p => !p.IsDeleted && !p.HasPoll && !p.IsPrivate && p.GroupId == null)
                .OrderByDescending(p => p.CreatedAt)
                .Take(60)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var postIds = posts.Select(p => p.PostId).ToList();
            var reportCounts = await dbContext.TblContentReports
                .Where(r => r.ContentType == "POST" && postIds.Contains(r.ContentId))
                .GroupBy(r => r.ContentId)
                .Select(g => new { ContentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.ContentId, g => g.Count, cancellationToken);

            foreach (var p in posts)
            {
                list.Add(new ContentItemAdminModel(
                    p.PostId,
                    "POST",
                    p.AuthorId,
                    p.Author?.DisplayName ?? p.Author?.UserName ?? "User",
                    p.Author?.AvatarUrl,
                    p.Content,
                    null,
                    p.CreatedAt,
                    p.Community?.Name,
                    p.Group?.Name,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason,
                    reportCounts.GetValueOrDefault(p.PostId, 0),
                    p.TblPostImages.Where(i => !i.IsDeleted).Select(i => i.ImageUrl).ToList(),
                    p.CodeSnippet,
                    p.CodeFileName,
                    null
                ));
            }
        }

        // Query polls (Note: When a poll is Private, even admins cannot view it)
        if (filter == "ALL" || filter == "POLL")
        {
            var polls = await dbContext.TblPolls
                .Include(p => p.TblPollOptions)
                .Include(p => p.Post).ThenInclude(pt => pt!.Author)
                .Include(p => p.Post).ThenInclude(pt => pt!.Community)
                .Include(p => p.Post).ThenInclude(pt => pt!.Group)
                .Where(p => !p.IsDeleted && !p.IsPrivate && (p.Post == null || p.Post.GroupId == null))
                .OrderByDescending(p => p.CreatedAt)
                .Take(60)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var pollIds = polls.Select(p => p.PollId).ToList();
            var reportCounts = await dbContext.TblContentReports
                .Where(r => r.ContentType == "POLL" && pollIds.Contains(r.ContentId))
                .GroupBy(r => r.ContentId)
                .Select(g => new { ContentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.ContentId, g => g.Count, cancellationToken);

            foreach (var p in polls)
            {
                list.Add(new ContentItemAdminModel(
                    p.PollId,
                    "POLL",
                    p.Post?.AuthorId ?? (p.CreatedBy ?? 0),
                    p.Post?.Author?.DisplayName ?? p.Post?.Author?.UserName ?? "Author",
                    p.Post?.Author?.AvatarUrl,
                    p.Post?.Content ?? string.Empty,
                    p.Question,
                    p.CreatedAt,
                    p.Post?.Community?.Name,
                    p.Post?.Group?.Name,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason,
                    reportCounts.GetValueOrDefault(p.PollId, 0),
                    ImageUrls: null,
                    CodeSnippet: null,
                    CodeFileName: null,
                    PollOptions: p.TblPollOptions.Where(o => !o.IsDeleted).OrderBy(o => o.PollOptionId).Select(o => o.OptionText).ToList()
                ));
            }
        }

        var ordered = list.OrderByDescending(x => x.ReportCount).ThenByDescending(x => x.CreatedAt).ToList();
        return Result<IReadOnlyList<ContentItemAdminModel>>.Success(ordered);
    }

    public async Task<Result<IReadOnlyList<GroupItemAdminModel>>> GetModeratedGroupsListAsync(CancellationToken cancellationToken = default)
    {
        var groups = await dbContext.TblGroups
            .Include(g => g.Creator)
            .Include(g => g.SubCommunity)
            .Where(g => !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .Take(100)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var groupIds = groups.Select(g => g.GroupId).ToList();
        var reportCounts = await dbContext.TblContentReports
            .Where(r => r.ContentType == "GROUP" && groupIds.Contains(r.ContentId))
            .GroupBy(r => r.ContentId)
            .Select(g => new { ContentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ContentId, g => g.Count, cancellationToken);

        var list = groups.Select(g => new GroupItemAdminModel(
            g.GroupId,
            g.Name,
            g.Slug,
            g.Description,
            g.AvatarUrl,
            g.SubCommunity?.Name,
            g.CreatorId,
            g.Creator?.DisplayName ?? g.Creator?.UserName ?? "Admin",
            g.Visibility,
            g.MemberCount,
            g.PostCount,
            g.IsActive,
            g.ModerationReason,
            reportCounts.GetValueOrDefault(g.GroupId, 0),
            g.CreatedAt
        ))
        .OrderByDescending(x => x.ReportCount)
        .ThenByDescending(x => x.CreatedAt)
        .ToList();

        return Result<IReadOnlyList<GroupItemAdminModel>>.Success(list);
    }

    public async Task<Result> ModerateContentAsync(ModerateContentRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Action))
            return Result.Failure("Action is required.", ResultStatus.ValidationError);

        var adminUserId = currentUser.UserId;

        // If target was identified via a ReportId
        string contentType = "";
        int contentId = 0;

        TblContentReport? report = null;
        if (request.ReportId.HasValue && request.ReportId.Value > 0)
        {
            report = await dbContext.TblContentReports.FirstOrDefaultAsync(r => r.ContentReportId == request.ReportId.Value, cancellationToken);
            if (report != null)
            {
                contentType = report.ContentType;
                contentId = report.ContentId;
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.ContentType) && request.ContentId.HasValue)
        {
            contentType = request.ContentType.ToUpperInvariant();
            contentId = request.ContentId.Value;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Result.Failure("Content report reference or target not found.", ResultStatus.NotFound);
        }

        int authorId = 0;
        string contentTitle = contentType == "POST" ? "Post" : (contentType == "POLL" ? "Poll" : "Group");

        var actionUpper = request.Action.ToUpperInvariant();
        var reasonText = !string.IsNullOrWhiteSpace(request.ReasonNote)
            ? request.ReasonNote.Trim()
            : (!string.IsNullOrWhiteSpace(request.ReasonCategory) ? request.ReasonCategory.Trim() : "Violation of community safety standards");

        if (contentType == "POST")
        {
            var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == contentId, cancellationToken);
            if (post == null) return Result.Failure("Post not found.", ResultStatus.NotFound);

            authorId = post.AuthorId;

            switch (actionUpper)
            {
                case "BAN":
                case "DEACTIVATE":
                    post.IsActive = false;
                    post.ModeratedBy = adminUserId;
                    post.ModeratedAt = DateTime.UtcNow;
                    post.ModerationReason = reasonText;
                    break;
                case "UNBAN":
                case "ACTIVATE":
                    post.IsActive = true;
                    post.ModerationReason = null;
                    break;
                case "SET_PRIVATE":
                    post.IsPrivate = true;
                    post.ModeratedBy = adminUserId;
                    post.ModeratedAt = DateTime.UtcNow;
                    post.ModerationReason = reasonText;
                    break;
                case "SET_PUBLIC":
                    post.IsPrivate = false;
                    post.ModerationReason = null;
                    break;
            }
        }
        else if (contentType == "POLL")
        {
            var poll = await dbContext.TblPolls
                .Include(p => p.Post)
                .FirstOrDefaultAsync(p => p.PollId == contentId, cancellationToken);
            if (poll == null) return Result.Failure("Poll not found.", ResultStatus.NotFound);

            authorId = poll.Post?.AuthorId ?? (poll.CreatedBy ?? 0);

            switch (actionUpper)
            {
                case "BAN":
                case "DEACTIVATE":
                    poll.IsActive = false;
                    poll.ModeratedBy = adminUserId;
                    poll.ModeratedAt = DateTime.UtcNow;
                    poll.ModerationReason = reasonText;
                    if (poll.Post != null) { poll.Post.IsActive = false; poll.Post.ModerationReason = reasonText; }
                    break;
                case "UNBAN":
                case "ACTIVATE":
                    poll.IsActive = true;
                    poll.ModerationReason = null;
                    if (poll.Post != null) { poll.Post.IsActive = true; poll.Post.ModerationReason = null; }
                    break;
                case "SET_PRIVATE":
                    poll.IsPrivate = true;
                    poll.ModeratedBy = adminUserId;
                    poll.ModeratedAt = DateTime.UtcNow;
                    poll.ModerationReason = reasonText;
                    if (poll.Post != null) { poll.Post.IsPrivate = true; poll.Post.ModerationReason = reasonText; }
                    break;
                case "SET_PUBLIC":
                    poll.IsPrivate = false;
                    poll.ModerationReason = null;
                    if (poll.Post != null) { poll.Post.IsPrivate = false; poll.Post.ModerationReason = null; }
                    break;
            }
        }
        else if (contentType == "GROUP")
        {
            var grp = await dbContext.TblGroups.FirstOrDefaultAsync(g => g.GroupId == contentId && !g.IsDeleted, cancellationToken);
            if (grp == null) return Result.Failure("Group not found.", ResultStatus.NotFound);

            authorId = grp.CreatorId;

            switch (actionUpper)
            {
                case "DEACTIVATE":
                case "BAN":
                    grp.IsActive = false;
                    grp.ModeratedBy = adminUserId;
                    grp.ModeratedAt = DateTime.UtcNow;
                    grp.ModerationReason = reasonText;
                    grp.UpdatedAt = DateTime.UtcNow;
                    grp.UpdatedBy = adminUserId;
                    break;
                case "ACTIVATE":
                case "UNBAN":
                    grp.IsActive = true;
                    grp.ModerationReason = null;
                    grp.UpdatedAt = DateTime.UtcNow;
                    grp.UpdatedBy = adminUserId;
                    break;
            }
        }

        // Mark report as resolved or dismissed
        if (report != null)
        {
            report.Status = actionUpper == "DISMISS" ? "DISMISSED" : "RESOLVED";
            report.HandledByAdminId = adminUserId;
            report.AdminNote = reasonText;
            report.HandledAt = DateTime.UtcNow;
        }

        // Also resolve any other pending reports for the same content if action taken
        if (actionUpper != "DISMISS")
        {
            var otherReports = await dbContext.TblContentReports
                .Where(r => r.ContentType == contentType && r.ContentId == contentId && r.Status == "PENDING")
                .ToListAsync(cancellationToken);

            foreach (var r in otherReports)
            {
                r.Status = "RESOLVED";
                r.HandledByAdminId = adminUserId;
                r.AdminNote = reasonText;
                r.HandledAt = DateTime.UtcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Notify content/group owner if moderated into inactive or banned mode
        if (authorId > 0 && (actionUpper == "BAN" || actionUpper == "DEACTIVATE" || actionUpper == "SET_PRIVATE"))
        {
            var actionTitle = actionUpper switch
            {
                "BAN" => $"Your {contentTitle} was banned by moderators",
                "DEACTIVATE" => contentType == "GROUP" ? "Your group has been made inactive by administration" : $"Your {contentTitle} has been deactivated",
                "SET_PRIVATE" => $"{contentTitle} set to Private Mode",
                _ => $"{contentTitle} Moderation Update"
            };

            var actionDesc = contentType == "GROUP"
                ? $"Your group has been set to Inactive status by administration due to: \"{reasonText}\". Members cannot post or poll, but existing content remains available."
                : (actionUpper == "BAN"
                    ? $"Your {contentTitle.ToLower()} was banned by administration: \"{reasonText}\". Only you and administrators can view it."
                    : $"Your {contentTitle.ToLower()} was moderated: \"{reasonText}\".");

            await notificationService.CreateNotificationAsync(
                authorId,
                adminUserId,
                "MODERATION",
                actionTitle,
                actionDesc,
                contentType,
                contentId,
                cancellationToken);
        }

        return Result.Success($"Action '{actionUpper}' applied successfully.");
    }

    #endregion
}