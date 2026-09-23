using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.UserProfile;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Api.Controllers;

[Route("api/users")]
public class UsersController : BaseController
{
    private readonly IUserProfileService _userProfileService;

    public UsersController(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.GetOwnerProfileAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{userNameOrId}")]
    public async Task<IActionResult> GetPublicProfile(string userNameOrId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetPublicProfileAsync(userNameOrId, CurrentUserId, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequestDto request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.UpdateProfileAsync(userId.Value, request, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("me/avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile? avatar, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        if (avatar == null || avatar.Length == 0)
            return BadRequest(Result.Failure("Avatar image file is required.", ResultStatus.ValidationError));

        // 5 MB max file size check
        if (avatar.Length > 5 * 1024 * 1024)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, Result.Failure("Avatar image cannot exceed 5 MB.", ResultStatus.ValidationError));

        using var stream = avatar.OpenReadStream();
        var result = await _userProfileService.UploadAvatarAsync(userId.Value, stream, avatar.FileName, avatar.ContentType, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("{targetUserId}/save")]
    public async Task<IActionResult> ToggleSaveAccount(int targetUserId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.ToggleSaveAccountAsync(userId.Value, targetUserId, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("{targetUserId}/rate")]
    public async Task<IActionResult> RateUser(int targetUserId, [FromBody] RateUserRequestDto request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.RateUserAsync(userId.Value, targetUserId, request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{targetUserId}/posts")]
    public async Task<IActionResult> GetUserPosts(int targetUserId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetUserPostsAsync(targetUserId, CurrentUserId, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpGet("me/saved-posts")]
    public async Task<IActionResult> GetSavedPosts(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.GetSavedPostsAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpGet("me/saved-accounts")]
    public async Task<IActionResult> GetSavedAccounts(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.GetSavedAccountsAsync(userId.Value, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{targetUserId}/communities")]
    public async Task<IActionResult> GetUserCommunities(int targetUserId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetUserCommunitiesAsync(targetUserId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{targetUserId}/reviews")]
    public async Task<IActionResult> GetUserReviews(int targetUserId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetUserReviewsAsync(targetUserId, cancellationToken);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("{targetUserId}/toggle-follow")]
    public async Task<IActionResult> ToggleFollow(int targetUserId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await _userProfileService.ToggleFollowUserAsync(userId.Value, targetUserId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{targetUserId}/followers")]
    public async Task<IActionResult> GetFollowers(int targetUserId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetFollowersAsync(targetUserId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{targetUserId}/following")]
    public async Task<IActionResult> GetFollowing(int targetUserId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetFollowingAsync(targetUserId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{targetUserId}/shares")]
    public async Task<IActionResult> GetUserShares(int targetUserId, CancellationToken cancellationToken)
    {
        var result = await _userProfileService.GetUserSharesAsync(targetUserId, CurrentUserId, cancellationToken);
        return ToActionResult(result);
    }

    private int? CurrentUserId
    {
        get
        {
            var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }
    }
}
