using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.RoleAndPermission;
using CommunityLink.Shared.Security;
using CommunityLink.Domain.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public interface IRoleAndPermissionService
{
    Task<Result<IReadOnlyList<RoleModel>>> GetRolesAsync(CancellationToken cancellationToken = default);
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

        if (string.Equals(currentUser.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            var all = await dbContext.TblPermissions.Where(p => !p.IsDeleted).Select(p => p.PermissionCode).ToListAsync(cancellationToken);
            return Result<IReadOnlyList<string>>.Success(all);
        }

        var perms = await dbContext.TblRolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == currentUser.RoleId.Value && !rp.IsDeleted)
            .Select(rp => rp.Permission.PermissionCode)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<string>>.Success(perms);
    }

    public async Task<Result<IReadOnlyList<RoleModel>>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.TblRoles
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .Select(r => new RoleModel(
                r.RoleId,
                r.RoleCode,
                r.RoleName,
                r.Description,
                r.IsSystemRole,
                r.TblUserRoles.Count + r.TblAdminRoles.Count))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<RoleModel>>.Success(roles);
    }

    public async Task<Result<RolePermissionMatrixResponseModel>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.TblRoles.FindAsync([roleId], cancellationToken);
        if (role is null || role.IsDeleted) return Result<RolePermissionMatrixResponseModel>.Failure("Role not found.", ResultStatus.NotFound);

        var allPermissions = await dbContext.TblPermissions.Where(p => !p.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);

        // If ADMIN, all permissions are always granted and dynamic
        if (string.Equals(role.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            var adminItems = allPermissions.Select(p => new RolePermissionItemModel(
                p.PermissionCode,
                p.PermissionName,
                p.Module,
                true)).ToList();

            return Result<RolePermissionMatrixResponseModel>.Success(new(role.RoleId, role.RoleCode, role.RoleName, adminItems));
        }

        var activePermissionIds = await dbContext.TblRolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var items = allPermissions.Select(p => new RolePermissionItemModel(
            p.PermissionCode,
            p.PermissionName,
            p.Module,
            activePermissionIds.Contains(p.PermissionId))).ToList();

        return Result<RolePermissionMatrixResponseModel>.Success(new(role.RoleId, role.RoleCode, role.RoleName, items));
    }

    public async Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsRequestModel request, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.TblRoles.FindAsync([request.RoleId], cancellationToken);
        if (role is null || role.IsDeleted) return Result.Failure("Role not found.", ResultStatus.NotFound);

        if (string.Equals(role.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("ADMIN role permissions are fixed and cannot be edited.", ResultStatus.Forbidden);
        }

        var allPermissions = await dbContext.TblPermissions.Where(p => !p.IsDeleted).ToListAsync(cancellationToken);
        var existingMappings = await dbContext.TblRolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        var grantedSet = new HashSet<string>(request.GrantedPermissionCodes, StringComparer.OrdinalIgnoreCase);

        foreach (var permission in allPermissions)
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
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblRoles.Add(newRole);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Assign default USER permissions
        var defaultCodes = PermissionCatalog.DefaultForRole("USER");
        var defaultPermissions = await dbContext.TblPermissions
            .Where(p => !p.IsDeleted && defaultCodes.Contains(p.PermissionCode))
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

        await dbContext.SaveChangesAsync(cancellationToken);

        var resultModel = new RoleModel(
            newRole.RoleId,
            newRole.RoleCode,
            newRole.RoleName,
            newRole.Description,
            newRole.IsSystemRole,
            0);

        return Result<RoleModel>.Success(resultModel, $"Role '{newRole.RoleName}' created successfully.");
    }

    public async Task<Result> DeleteRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.TblRoles.FindAsync([roleId], cancellationToken);
        if (role is null || role.IsDeleted)
            return Result.Failure("Role not found.", ResultStatus.NotFound);

        if (role.IsSystemRole || string.Equals(role.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase) || string.Equals(role.RoleCode, "USER", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("System roles (ADMIN and USER) cannot be deleted.", ResultStatus.Forbidden);
        }

        // Resolve default USER role
        var defaultUserRole = await dbContext.TblRoles.FirstOrDefaultAsync(r => !r.IsDeleted && r.RoleCode == "USER", cancellationToken);
        if (defaultUserRole is null)
        {
            return Result.Failure("Default USER role could not be found to reassign users.", ResultStatus.NotFound);
        }

        // Reassign all active users assigned to this role to the default USER role
        var userRoles = await dbContext.TblUserRoles.Where(ur => ur.RoleId == roleId).ToListAsync(cancellationToken);
        foreach (var ur in userRoles)
        {
            ur.RoleId = defaultUserRole.RoleId;
            ur.UpdatedAt = DateTime.UtcNow;
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

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success($"Role '{role.RoleName}' deleted successfully and associated users were reassigned to 'USER'.");
    }
}