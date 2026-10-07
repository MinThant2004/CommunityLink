using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Community;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Community;

[Route("api/communities")]
public sealed class CommunityController(ICommunityService communityService) : BaseController
{
    [HttpPost("upload-banner")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadBanner([FromForm] IFormFile? banner, CancellationToken cancellationToken)
    {
        if (banner == null || banner.Length == 0)
            return BadRequest(Result.Failure("Banner image file is required.", ResultStatus.ValidationError));

        if (banner.Length > 5 * 1024 * 1024)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, Result.Failure("Banner image cannot exceed 5 MB.", ResultStatus.ValidationError));

        using var stream = banner.OpenReadStream();
        var result = await communityService.UploadBannerAsync(stream, banner.FileName, banner.ContentType, cancellationToken);
        return ToActionResult(result);
    }
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

    [HttpPut("{communityId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateCommunity(int communityId, [FromBody] EditCommunityRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.UpdateCommunityAsync(communityId, request, cancellationToken));

    [HttpGet("{communityId:int}/audits")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommunityAudits(int communityId, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.GetCommunityAuditsAsync(communityId, cancellationToken));

    [HttpPost("{communityId:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinCommunity(int communityId, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.JoinCommunityAsync(communityId, cancellationToken));

    [HttpPost("{communityId:int}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveCommunity(int communityId, CancellationToken cancellationToken) =>
        ToActionResult(await communityService.LeaveCommunityAsync(communityId, cancellationToken));
}