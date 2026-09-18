using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Community;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Community;

[Route("api/communities")]
public sealed class CommunityController(ICommunityService communityService) : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommunities([FromQuery] string? search, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.GetCommunitiesAsync(search, cancellationToken));

    [HttpGet("{communityId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommunityById(int communityId, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.GetCommunityByIdAsync(communityId, cancellationToken));

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateCommunity([FromBody] CreateCommunityRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.CreateCommunityAsync(request, cancellationToken));

    [HttpPost("{communityId:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinCommunity(int communityId, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.JoinCommunityAsync(communityId, cancellationToken));
}