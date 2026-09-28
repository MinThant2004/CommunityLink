using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Domain.Features.Premium;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;
using CommunityLink.Shared.Security;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/admin/verifications")]
[Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserManage)]
public class VerificationAuditController(
    IIdentityVerificationService verificationService,
    ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await verificationService.GetPendingVerificationsAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await verificationService.GetAllVerificationsAdminAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    public sealed record ApproveVerificationBody(string? Notes);

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveVerificationBody? body, CancellationToken cancellationToken)
    {
        var adminId = currentUser.UserId ?? 1;
        var result = await verificationService.ApproveVerificationAsync(id, adminId, body?.Notes, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    public sealed record RejectVerificationBody(string Reason);

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectVerificationBody body, CancellationToken cancellationToken)
    {
        var adminId = currentUser.UserId ?? 1;
        var result = await verificationService.RejectVerificationAsync(id, adminId, body.Reason, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
