using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Domain.Features.Premium;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;
using CommunityLink.Shared.Security;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/plans")]
public class SubscriptionPlansController(ISubscriptionPlanService planService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActivePlans(CancellationToken cancellationToken)
    {
        var result = await planService.GetActivePlansAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetPlanById(int id, CancellationToken cancellationToken)
    {
        var result = await planService.GetPlanByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("admin/all")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    public async Task<IActionResult> GetAllPlansAdmin(CancellationToken cancellationToken)
    {
        var result = await planService.GetAllPlansAdminAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("admin")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserManage)]
    public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken)
    {
        var result = await planService.CreatePlanAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("admin/{id:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserManage)]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdateSubscriptionPlanRequestDto request, CancellationToken cancellationToken)
    {
        request.PlanId = id;
        var result = await planService.UpdatePlanAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("admin/{id:int}/toggle-status")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserManage)]
    public async Task<IActionResult> TogglePlanStatus(int id, CancellationToken cancellationToken)
    {
        var result = await planService.TogglePlanStatusAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("admin/{id:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserManage)]
    public async Task<IActionResult> DeletePlan(int id, CancellationToken cancellationToken)
    {
        var result = await planService.DeletePlanAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
