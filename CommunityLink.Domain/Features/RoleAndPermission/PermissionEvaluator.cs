using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public interface IPermissionEvaluator
{
    Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(int roleId, CancellationToken cancellationToken = default);
}

public sealed class PermissionEvaluator(AppDbContext dbContext, ICurrentUserContext currentUser) : IPermissionEvaluator
{
    public async Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default)
    {
        if (currentUser.RoleId is null) return false;
        if (string.Equals(currentUser.RoleCode, "SYSTEM_ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(currentUser.RoleCode, "SYSTEMADMIN", StringComparison.OrdinalIgnoreCase)) return true;

        var hasPerm = await dbContext.TblRolePermissions
            .AsNoTracking()
            .AnyAsync(rp => rp.RoleId == currentUser.RoleId &&
                            rp.Permission.PermissionCode == permissionCode &&
                            !rp.IsDeleted, cancellationToken);
        if (hasPerm) return true;

        // Fallback to catalog defaults only if role permissions haven't been seeded in DB (e.g. dynamic test roles)
        if (!string.IsNullOrEmpty(currentUser.RoleCode))
        {
            var roleHasAnyPerms = await dbContext.TblRolePermissions
                .AsNoTracking()
                .AnyAsync(rp => rp.RoleId == currentUser.RoleId, cancellationToken);
            if (!roleHasAnyPerms)
            {
                var defaultCodes = PermissionCatalog.DefaultForRole(currentUser.RoleCode);
                return defaultCodes.Contains(permissionCode);
            }
        }

        return false;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var dbPerms = await dbContext.TblRolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.Permission.PermissionCode)
            .ToListAsync(cancellationToken);

        if (dbPerms.Count > 0) return dbPerms;

        var role = await dbContext.TblRoles.FindAsync([roleId], cancellationToken);
        if (role != null)
        {
            return PermissionCatalog.DefaultForRole(role.RoleCode);
        }

        return [];
    }
}