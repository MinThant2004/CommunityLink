using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Administration;

[Route("api/admin")]
public sealed class AdministrationController(IAdministrationService adminService) : BaseController
{
    [HttpGet("stats")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken) =>
        ToActionResult(await adminService.GetDashboardStatsAsync(cancellationToken));

    [HttpGet("users")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken) =>
        ToActionResult(await adminService.GetUsersAsync(cancellationToken));

    [HttpGet("audit-logs")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetAuditLogs(CancellationToken cancellationToken) =>
        ToActionResult(await adminService.GetAuditLogsAsync(cancellationToken));
}