using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.Administration;
using CommunityLink.Shared;
using CommunityLink.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdministrationController : ControllerBase
{
    private readonly IAdministrationService _adminService;

    public AdministrationController(IAdministrationService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("stats")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardStatsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("dashboard")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("users")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetUsersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("users/page")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetUsersPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUsersPageAsync(page, pageSize, search, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("audits")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetAudits(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAuditLogsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("audits/page")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetAuditLogsPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetAuditLogsPageAsync(page, pageSize, search, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("join-requests")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.CommunityModerate)]
    public async Task<IActionResult> GetPendingJoinRequests(CancellationToken cancellationToken)
    {
        var result = await _adminService.GetPendingJoinRequestsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("join-requests/{id:int}/approve")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.CommunityModerate)]
    public async Task<IActionResult> ApproveJoinRequest(int id, CancellationToken cancellationToken)
    {
        var result = await _adminService.ApproveJoinRequestAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("join-requests/{id:int}/reject")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.CommunityModerate)]
    public async Task<IActionResult> RejectJoinRequest(int id, CancellationToken cancellationToken)
    {
        var result = await _adminService.RejectJoinRequestAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
