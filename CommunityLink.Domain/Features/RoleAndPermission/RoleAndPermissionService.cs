using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.RoleAndPermission;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public interface IRoleAndPermissionService
{
    Task<Result<IReadOnlyList<RoleModel>>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<Result<RolePermissionMatrixResponseModel>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
    Task<Result> UpdateRolePermissionsAsync(UpdateRolePermissionsRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class RoleAndPermissionService(AppDbContext dbContext) : IRoleAndPermissionService
{
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

        var activePermissionIds = await dbContext.TblRolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var allPermissions = await dbContext.TblPermissions.Where(p => !p.IsDeleted).AsNoTracking().ToListAsync(cancellationToken);

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
}