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
}

public sealed class AdministrationService(
    AppDbContext dbContext,
    IEmailSender emailSender,
    IConfiguration configuration,
    ICurrentUserContext currentUser) : IAdministrationService
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
}