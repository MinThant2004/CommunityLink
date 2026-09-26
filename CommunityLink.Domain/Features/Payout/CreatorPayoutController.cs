namespace CommunityLink.Domain.Features.Payout;

using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Payout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/creator/payouts")]
[Authorize]
public sealed class CreatorPayoutController(ICreatorPayoutService creatorPayoutService) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreatePayoutRequest(
        [FromBody] CreatePayoutRequestModel request,
        CancellationToken cancellationToken) =>
        ToActionResult(await creatorPayoutService.CreatePayoutRequestAsync(request, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> GetMyPayouts(CancellationToken cancellationToken) =>
        ToActionResult(await creatorPayoutService.GetMyPayoutsAsync(cancellationToken));

    [HttpGet("summary")]
    public async Task<IActionResult> GetMyPayoutSummary(CancellationToken cancellationToken) =>
        ToActionResult(await creatorPayoutService.GetMyPayoutSummaryAsync(cancellationToken));
}
