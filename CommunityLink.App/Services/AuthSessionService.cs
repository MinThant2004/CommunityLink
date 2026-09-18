using System.Security.Claims;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CommunityLink.App.Services;

public sealed class AuthSessionService(IHttpContextAccessor httpContextAccessor)
{
    public ClaimsPrincipal BuildPrincipal(LoginResponseModel response)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, response.UserId.ToString()),
            new("sub", response.UserId.ToString()),
            new(ClaimTypes.Email, response.Email),
            new("email", response.Email),
            new(ClaimTypes.Name, response.FullName),
            new("name", response.FullName),
            new(ClaimTypes.Role, response.RoleCode),
            new("role", response.RoleCode),
            new("role_id", response.RoleId.ToString()),
            new("access_token", response.AccessToken),
            new("session_id", response.SessionId),
            new("must_change_password", response.MustChangePassword.ToString().ToLowerInvariant())
        };

        foreach (var permission in response.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public async Task SignInAsync(LoginResponseModel response, bool rememberMe, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HTTP request context for cookie sign-in.");

        var principal = BuildPrincipal(response);
        var properties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            AllowRefresh = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(rememberMe ? TimeSpan.FromDays(30) : TimeSpan.FromHours(8))
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity?.IsAuthenticated == true)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}