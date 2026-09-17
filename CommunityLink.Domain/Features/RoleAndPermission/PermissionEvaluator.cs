using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;

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
        if (string.Equals(currentUser.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase)) return true;

        return await dbContext.TblRolePermissions
            .AsNoTracking()
            .AnyAsync(rp => rp.RoleId == currentUser.RoleId &&
                            rp.Permission.PermissionCode == permissionCode &&
                            !rp.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsForRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.TblRolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.Permission.PermissionCode)
            .ToListAsync(cancellationToken);
    }
}