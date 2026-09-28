using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Domain.Features.Activity;

[Route("api/activity")]
[Authorize]
public sealed class UserActivityController(IUserActivityService activityService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetMyActivities(CancellationToken cancellationToken) =>
        ToActionResult(await activityService.GetMyActivitiesAsync(cancellationToken));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteActivity(long id, CancellationToken cancellationToken) =>
        ToActionResult(await activityService.DeleteActivityAsync(id, cancellationToken));
}
