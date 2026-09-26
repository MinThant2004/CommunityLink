namespace CommunityLink.Domain.Features.Payout;

using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Payout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/admin/payouts")]
[Authorize]
public sealed class AdminPayoutController(ICreatorPayoutService creatorPayoutService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAdminPayouts(
        [FromQuery] string? status,
        CancellationToken cancellationToken) =>
        ToActionResult(await creatorPayoutService.GetAdminPayoutsAsync(status, cancellationToken));

    [HttpPost("{id:long}/approve")]
    public async Task<IActionResult> ApprovePayout(
        long id,
        [FromBody] ReviewPayoutRequestModel? request,
        CancellationToken cancellationToken) =>
        ToActionResult(await creatorPayoutService.ApprovePayoutAsync(id, request?.AdminNote, cancellationToken));

    [HttpPost("{id:long}/reject")]
    public async Task<IActionResult> RejectPayout(
        long id,
        [FromBody] ReviewPayoutRequestModel? request,
        CancellationToken cancellationToken) =>
        ToActionResult(await creatorPayoutService.RejectPayoutAsync(id, request?.AdminNote, cancellationToken));
}
