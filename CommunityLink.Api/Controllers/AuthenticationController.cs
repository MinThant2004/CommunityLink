using System.Security.Claims;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Api.Controllers;

[Route("api/auth")]
public class AuthenticationController : BaseController
{
    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [HttpPost("user/register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequestModel request)
    {
        var result = await _authService.RegisterUserAsync(request);
        return ToActionResult(result);
    }

    [HttpPost("login")]
    [HttpPost("user/login")]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequestModel request)
    {
        // Try member login first; if the account flag or credentials point to an admin, fall through.
        var result = await _authService.LoginUserAsync(request);
        if (!result.IsSuccess && result.Status == ResultStatus.Unauthorized)
        {
            var adminResult = await _authService.LoginAdminAsync(request with { IsAdmin = true });
            if (adminResult.IsSuccess) return ToActionResult(adminResult);
        }
        return ToActionResult(result);
    }

    [HttpPost("admin/register")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequestModel request)
    {
        var result = await _authService.RegisterAdminAsync(request);
        return ToActionResult(result);
    }

    [HttpPost("admin/login")]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginRequestModel request)
    {
        request = request with { IsAdmin = true };
        var result = await _authService.LoginAdminAsync(request);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Invalid session token.", ResultStatus.Unauthorized));

        var result = await _authService.GetCurrentUserAsync(userId.Value);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel request)
    {
        var userId = CurrentUserId;
        if (userId == null)
            return Unauthorized(Result.Failure("Invalid session token.", ResultStatus.Unauthorized));

        var result = await _authService.ChangePasswordAsync(userId.Value, request);
        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(Result.Success("Logged out."));
    }

    [HttpPost("forgot-password/send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestModel request)
    {
        var result = await _authService.SendPasswordResetOtpAsync(request);
        return ToActionResult(result);
    }

    [HttpPost("forgot-password/verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestModel request)
    {
        var result = await _authService.VerifyPasswordResetOtpAsync(request);
        return ToActionResult(result);
    }

    [HttpPost("forgot-password/reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestModel request)
    {
        var result = await _authService.ResetPasswordAsync(request);
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