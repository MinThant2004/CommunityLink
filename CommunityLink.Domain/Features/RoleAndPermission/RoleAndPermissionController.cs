using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.RoleAndPermission;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

[Route("api/roles")]
public sealed class RoleAndPermissionController(IRoleAndPermissionService roleService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminRbacManage)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken) =>
        ToActionResult(await roleService.GetRolesAsync(cancellationToken));

    [HttpGet("{roleId:int}/permissions")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminRbacManage)]
    public async Task<IActionResult> GetRolePermissions(int roleId, CancellationToken cancellationToken) =>
        ToActionResult(await roleService.GetRolePermissionsAsync(roleId, cancellationToken));

    [HttpPost("permissions")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminRbacManage)]
    public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionsRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await roleService.UpdateRolePermissionsAsync(request, cancellationToken));
}