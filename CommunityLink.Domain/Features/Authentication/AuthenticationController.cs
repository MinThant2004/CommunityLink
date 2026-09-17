using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Domain.Features.Authentication;

[Route("api/auth")]
public sealed class AuthenticationController(IAuthenticationService authService) : BaseController
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await authService.LoginAsync(request, cancellationToken));

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await authService.RegisterAsync(request, cancellationToken));

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await authService.ChangePasswordAsync(request, cancellationToken));

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken) =>
        ToActionResult(await authService.GetCurrentUserAsync(cancellationToken));
}