using CommunityLink.App.Services;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Tests.Services;

public class AuthSessionServiceTests
{
    [Fact]
    public void BuildPrincipal_IncludesIdentityAndPermissionClaims()
    {
        var response = new LoginResponseModel(
            AccessToken: "jwt-token",
            RefreshToken: "refresh-token",
            UserId: 42,
            FullName: "Jane Doe",
            Email: "jane@example.com",
            RoleCode: "MEMBER",
            RoleId: 3,
            MustChangePassword: false,
            Permissions: ["POST.VIEW", "CHAT.ACCESS"],
            SessionId: "sess-123");

        var service = new AuthSessionService(new HttpContextAccessor());
        var principal = service.BuildPrincipal(response);

        Assert.Equal(CookieAuthenticationDefaults.AuthenticationScheme, principal.Identity?.AuthenticationType);
        Assert.Equal("42", principal.FindFirst("sub")?.Value);
        Assert.Equal("Jane Doe", principal.Identity?.Name);
        Assert.Equal("jane@example.com", principal.FindFirst("email")?.Value);
        Assert.Equal("MEMBER", principal.FindFirst("role")?.Value);
        Assert.Equal("3", principal.FindFirst("role_id")?.Value);
        Assert.Equal("jwt-token", principal.FindFirst("access_token")?.Value);
        Assert.Equal("false", principal.FindFirst("must_change_password")?.Value);
        Assert.Equal(2, principal.FindAll("permission").Count());
    }
}