using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.RoleAndPermission;
using CommunityLink.Shared.Security;
using CommunityLink.Domain.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public interface IRoleAndPermissionService
{
    Task<Result<IReadOnlyList<RoleModel>>> GetRolesAsync(int? roleType = null, CancellationToken cancellationToken = default);
    Task<Result<RolePermissionMatrixResponseModel>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<RoleModel>> CreateRoleAsync(CreateRoleRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<string>>> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken = default);
}

public sealed class RoleAndPermissionService(AppDbContext dbContext, ICurrentUserContext currentUser) : IRoleAndPermissionService
{
    public async Task<Result<IReadOnlyList<string>>> GetCurrentUserPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.RoleId is null)
            return Result<IReadOnlyList<string>>.Success([]);

        if (string.Equals(currentUser.RoleCode, "SYSTEM_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentUser.RoleCode, "SYSTEMADMIN", StringComparison.OrdinalIgnoreCase))
        {
            var adminPerms = await dbContext.TblPermissions
                .Where(p => !p.IsDeleted && p.Type == 1)
                .Select(p => p.PermissionCode)
                .ToListAsync(cancellationToken);
            return Result<IReadOnlyList<string>>.Success(adminPerms);
        }

        var perms = await dbContext.TblRolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == currentUser.RoleId.Value && !rp.IsDeleted)
            .Select(rp => rp.Permission.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<string>>.Success(perms);
    }

    public async Task<Result<IReadOnlyList<RoleModel>>> GetRolesAsync(int? roleType = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblRoles
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (roleType.HasValue)
        {
            query = query.Where(r => r.RoleType == roleType.Value);
        }

        var roles = await query
            .Select(r => new RoleModel(
                r.RoleId,
                r.RoleCode,
                r.RoleName,
                r.Description,
                r.IsSystemRole,
                r.RoleType == 1 ? r.TblAdminRoles.Count : r.TblUserRoles.Count,
                r.RoleType))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<RoleModel>>.Success(roles);
    }

    public async Task<Result<RolePermissionMatrixResponseModel>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.TblRoles.FindAsync([roleId], cancellationToken);
        if (role is null || role.IsDeleted) return Result<RolePermissionMatrixResponseModel>.Failure("Role not found.", ResultStatus.NotFound);

        // Filter available permissions strictly matching the role's RoleType
        var permissionsForType = await dbContext.TblPermissions
            .Where(p => !p.IsDeleted && p.Type == role.RoleType)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // If SYSTEM_ADMIN, all admin permissions are always granted and read-only
        if (string.Equals(role.RoleCode, "SYSTEM_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleCode, "SYSTEMADMIN", StringComparison.OrdinalIgnoreCase))
        {
            var systemAdminItems = permissionsForType.Select(p => new RolePermissionItemModel(
                p.PermissionCode,
                p.PermissionName,
                p.Module,
                true,
                p.Type)).ToList();

            return Result<RolePermissionMatrixResponseModel>.Success(new(role.RoleId, role.RoleCode, role.RoleName, role.RoleType, systemAdminItems));
        }

        var activePermissionIds = await dbContext.TblRolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var items = permissionsForType.Select(p => new RolePermissionItemModel(
            p.PermissionCode,
            p.PermissionName,
            p.Module,
            activePermissionIds.Contains(p.PermissionId),
            p.Type)).ToList();

        return Result<RolePermissionMatrixResponseModel>.Success(new(role.RoleId, role.RoleCode, role.RoleName, role.RoleType, items));
    }

    public async Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsRequestModel request, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.TblRoles.FindAsync([request.RoleId], cancellationToken);
        if (role is null || role.IsDeleted) return Result.Failure("Role not found.", ResultStatus.NotFound);

        if (string.Equals(role.RoleCode, "SYSTEM_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleCode, "SYSTEMADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Permissions for SYSTEM_ADMIN are locked and cannot be modified.", ResultStatus.Forbidden);
        }

        // Only allow permissions matching the role's RoleType
        var availablePermissions = await dbContext.TblPermissions
            .Where(p => !p.IsDeleted && p.Type == role.RoleType)
            .ToListAsync(cancellationToken);

        var existingMappings = await dbContext.TblRolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        var previousCodes = existingMappings
            .Where(m => !m.IsDeleted)
            .Join(availablePermissions, m => m.PermissionId, p => p.PermissionId, (m, p) => p.PermissionCode)
            .OrderBy(c => c)
            .ToList();

        var grantedSet = new HashSet<string>(request.GrantedPermissionCodes, StringComparer.OrdinalIgnoreCase);

        foreach (var permission in availablePermissions)
        {
            var mapping = existingMappings.FirstOrDefault(m => m.PermissionId == permission.PermissionId);
            var shouldGrant = grantedSet.Contains(permission.PermissionCode);

            if (mapping is null && shouldGrant)
            {
                dbContext.TblRolePermissions.Add(new TblRolePermission
                {
                    RoleId = request.RoleId,
                    PermissionId = permission.PermissionId,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (mapping is not null)
            {
                mapping.IsDeleted = !shouldGrant;
                mapping.UpdatedAt = DateTime.UtcNow;
            }
        }

        // Audit Log for PERMISSION CHANGE
        dbContext.TblAuditLogs.Add(new TblAuditLog
        {
            ActorType = currentUser.RoleCode ?? "ADMIN",
            ActorId = currentUser.UserId,
            Action = "CHANGE",
            EntityName = "PERMISSION",
            EntityId = role.RoleId,
            OldValues = $"Role: {role.RoleName} ({role.RoleCode}) | Granted ({previousCodes.Count}): {string.Join(", ", previousCodes)}",
            NewValues = $"Role: {role.RoleName} ({role.RoleCode}) | Granted ({request.GrantedPermissionCodes.Count}): {string.Join(", ", request.GrantedPermissionCodes.OrderBy(x => x))}",
            ChangedColumns = "Permissions",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Role permissions updated successfully.");
    }

    public async Task<Result<RoleModel>> CreateRoleAsync(CreateRoleRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RoleCode))
            return Result<RoleModel>.Failure("Role Code is required.", ResultStatus.ValidationError);

        if (string.IsNullOrWhiteSpace(request.RoleName))
            return Result<RoleModel>.Failure("Role Name is required.", ResultStatus.ValidationError);

        var normalizedCode = request.RoleCode.Trim().ToUpperInvariant();
        var normalizedName = request.RoleName.Trim();
        var normalizedDesc = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var roleType = request.RoleType == 1 ? 1 : 0; // 0 = User, 1 = Admin

        var codeExists = await dbContext.TblRoles.AnyAsync(r => !r.IsDeleted && r.RoleCode.ToUpper() == normalizedCode, cancellationToken);
        if (codeExists)
            return Result<RoleModel>.Failure($"Role Code '{normalizedCode}' already exists.", ResultStatus.Conflict);

        var nameExists = await dbContext.TblRoles.AnyAsync(r => !r.IsDeleted && r.RoleName.ToLower() == normalizedName.ToLower(), cancellationToken);
        if (nameExists)
            return Result<RoleModel>.Failure($"Role Name '{normalizedName}' already exists.", ResultStatus.Conflict);

        if (normalizedDesc != null)
        {
            var descExists = await dbContext.TblRoles.AnyAsync(r => !r.IsDeleted && r.Description != null && r.Description.ToLower() == normalizedDesc.ToLower(), cancellationToken);
            if (descExists)
                return Result<RoleModel>.Failure($"Role Description '{normalizedDesc}' is already in use by another role.", ResultStatus.Conflict);
        }

        var newRole = new TblRole
        {
            RoleCode = normalizedCode,
            RoleName = normalizedName,
            Description = normalizedDesc,
            IsSystemRole = false,
            RoleType = roleType,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblRoles.Add(newRole);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Assign default permissions matching role domain
        var defaultRoleLookup = roleType == 1 ? "ADMIN" : "DEFAULT_USER";
        var defaultCodes = PermissionCatalog.DefaultForRole(defaultRoleLookup);
        var defaultPermissions = await dbContext.TblPermissions
            .Where(p => !p.IsDeleted && p.Type == roleType && defaultCodes.Contains(p.PermissionCode))
            .ToListAsync(cancellationToken);

        foreach (var p in defaultPermissions)
        {
            dbContext.TblRolePermissions.Add(new TblRolePermission
            {
                RoleId = newRole.RoleId,
                PermissionId = p.PermissionId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Audit Log for ROLE CREATE
        dbContext.TblAuditLogs.Add(new TblAuditLog
        {
            ActorType = currentUser.RoleCode ?? "ADMIN",
            ActorId = currentUser.UserId,
            Action = "CREATE",
            EntityName = "ROLE",
            EntityId = newRole.RoleId,
            OldValues = null,
            NewValues = $"Code: {newRole.RoleCode}, Name: {newRole.RoleName}, RoleType: {(newRole.RoleType == 1 ? "Admin" : "User")}, Desc: {newRole.Description ?? "N/A"}",
            ChangedColumns = "RoleCode, RoleName, RoleType, Description",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var resultModel = new RoleModel(
            newRole.RoleId,
            newRole.RoleCode,
            newRole.RoleName,
            newRole.Description,
            newRole.IsSystemRole,
            0,
            newRole.RoleType);

        return Result<RoleModel>.Success(resultModel, $"Role '{newRole.RoleName}' created successfully.");
    }

    public async Task<Result> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.TblRoles.FindAsync([roleId], cancellationToken);
        if (role is null || role.IsDeleted)
            return Result.Failure("Role not found.", ResultStatus.NotFound);

        // System protected roles: SYSTEM_ADMIN, ADMIN, DEFAULT_USER (and legacy USER)
        if (role.IsSystemRole ||
            string.Equals(role.RoleCode, "SYSTEM_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleCode, "SYSTEMADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleCode, "DEFAULT_USER", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role.RoleCode, "USER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure($"Protected roles (SYSTEM_ADMIN, ADMIN, and DEFAULT_USER) cannot be deleted.", ResultStatus.Forbidden);
        }

        string fallbackRoleCode;
        if (role.RoleType == 1)
        {
            // Admin Domain Role deleted -> Fallback to ADMIN (never to SYSTEM_ADMIN)
            var defaultAdminRole = await dbContext.TblRoles
                .FirstOrDefaultAsync(r => !r.IsDeleted && r.RoleCode == "ADMIN" && r.RoleType == 1, cancellationToken);

            if (defaultAdminRole is null)
                return Result.Failure("Default 'ADMIN' fallback role could not be found to reassign admins.", ResultStatus.NotFound);

            var adminRoles = await dbContext.TblAdminRoles.Where(ar => ar.RoleId == roleId).ToListAsync(cancellationToken);
            foreach (var ar in adminRoles)
            {
                ar.RoleId = defaultAdminRole.RoleId;
                ar.UpdatedAt = DateTime.UtcNow;
            }
            fallbackRoleCode = "ADMIN";
        }
        else
        {
            // User Domain Role deleted -> Fallback to DEFAULT_USER
            var defaultUserRole = await dbContext.TblRoles
                .FirstOrDefaultAsync(r => !r.IsDeleted && (r.RoleCode == "DEFAULT_USER" || r.RoleCode == "USER"), cancellationToken);

            if (defaultUserRole is null)
                return Result.Failure("Default 'DEFAULT_USER' fallback role could not be found to reassign users.", ResultStatus.NotFound);

            var userRoles = await dbContext.TblUserRoles.Where(ur => ur.RoleId == roleId).ToListAsync(cancellationToken);
            foreach (var ur in userRoles)
            {
                ur.RoleId = defaultUserRole.RoleId;
                ur.UpdatedAt = DateTime.UtcNow;
            }
            fallbackRoleCode = "DEFAULT_USER";
        }

        // Soft-delete the role
        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;

        // Soft-delete role permissions
        var rolePermissions = await dbContext.TblRolePermissions.Where(rp => rp.RoleId == roleId && !rp.IsDeleted).ToListAsync(cancellationToken);
        foreach (var rp in rolePermissions)
        {
            rp.IsDeleted = true;
            rp.DeletedAt = DateTime.UtcNow;
        }

        // Audit Log for ROLE DELETE
        dbContext.TblAuditLogs.Add(new TblAuditLog
        {
            ActorType = currentUser.RoleCode ?? "ADMIN",
            ActorId = currentUser.UserId,
            Action = "DELETE",
            EntityName = "ROLE",
            EntityId = role.RoleId,
            OldValues = $"Code: {role.RoleCode}, Name: {role.RoleName}, RoleType: {role.RoleType}",
            NewValues = $"Deleted, Reassigned to {fallbackRoleCode}",
            ChangedColumns = "IsDeleted",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success($"Role '{role.RoleName}' deleted successfully and associated accounts were reassigned to '{fallbackRoleCode}'.");
    }
}