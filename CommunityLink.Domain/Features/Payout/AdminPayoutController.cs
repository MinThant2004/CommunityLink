namespace CommunityLink.Domain.Features.Payout;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Payout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/admin/payouts")]
[Authorize]
public sealed class AdminPayoutController(
    ICreatorPayoutService creatorPayoutService,
    ICurrentUserContext currentUser) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAdminPayouts(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<IReadOnlyList<AdminPayoutModel>>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await creatorPayoutService.GetAdminPayoutsAsync(status, cancellationToken));
    }

    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> ApprovePayout(
        long id,
        [FromBody] ReviewPayoutRequestModel? request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<AdminPayoutModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await creatorPayoutService.ApprovePayoutAsync(id, request?.AdminNote, cancellationToken));
    }

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> RejectPayout(
        long id,
        [FromBody] ReviewPayoutRequestModel? request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return ToActionResult(Result<AdminPayoutModel>.Failure("Administrator privileges required.", ResultStatus.Forbidden));
        }

        return ToActionResult(await creatorPayoutService.RejectPayoutAsync(id, request?.AdminNote, cancellationToken));
    }
}
