namespace CommunityLink.Domain.Features.Creator;

using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Creator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/creator/earnings")]
[Authorize]
public sealed class CreatorEarningsController(ICreatorEarningsService creatorEarningsService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetEarnings(
        [FromQuery] string? filterType,
        [FromQuery] int? chatGroupId,
        CancellationToken cancellationToken) =>
        ToActionResult(await creatorEarningsService.GetCreatorEarningsAsync(filterType, chatGroupId, cancellationToken));
}
