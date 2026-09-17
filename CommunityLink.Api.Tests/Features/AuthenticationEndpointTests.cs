using System.Net;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Api.Tests.Features;

public class AuthenticationEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithDefaultAdminCredentials_ReturnsSuccessAndToken()
    {
        var req = new LoginRequestModel("admin@communitylink.local", "Admin@123");
        var response = await _client.PostAsJsonAsync("/api/auth/login", req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.Equal("ADMIN", result.Data.RoleCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var req = new LoginRequestModel("admin@communitylink.local", "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", req);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_NewUser_ReturnsSuccess()
    {
        var uniqueEmail = $"user_{Guid.NewGuid():N}@communitylink.local";
        var uniqueUser = $"user_{Guid.NewGuid():N}"[..12];
        var req = new RegisterRequestModel("Test User", uniqueUser, uniqueEmail, "TestPass@123", "Bio details");

        var response = await _client.PostAsJsonAsync("/api/auth/register", req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<UserInfoModel>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(uniqueEmail, result.Data.Email);
    }
}