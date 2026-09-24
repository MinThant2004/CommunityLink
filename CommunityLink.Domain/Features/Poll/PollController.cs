using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Poll;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Poll;

[Route("api/polls")]
public sealed class PollController(IPollService pollService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.PollView)]
    public async Task<IActionResult> GetPolls([FromQuery] int? communityId, [FromQuery] int? groupId, CancellationToken cancellationToken) =>
        ToActionResult(await pollService.GetPollsAsync(communityId, groupId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.PollCreate)]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await pollService.CreatePollAsync(request, cancellationToken));

    [HttpPost("vote")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.PollVote)]
    public async Task<IActionResult> Vote([FromBody] VoteRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await pollService.VoteAsync(request, cancellationToken));

    [HttpPut("{pollId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePoll(int pollId, [FromBody] UpdatePollRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await pollService.UpdatePollAsync(pollId, request, cancellationToken));

    [HttpDelete("{pollId:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePoll(int pollId, CancellationToken cancellationToken) =>
        ToActionResult(await pollService.DeletePollAsync(pollId, cancellationToken));
}