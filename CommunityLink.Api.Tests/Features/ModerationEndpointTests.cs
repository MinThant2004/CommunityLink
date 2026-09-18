using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Api.Tests.Features;

public class ModerationEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task AuthenticateAdminAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("admin@communitylink.local", "Admin@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Data.AccessToken);
    }

    [Fact]
    public async Task GetPendingJoinRequests_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/admin/join-requests");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingJoinRequests_AsAdmin_ReturnsRequestsList()
    {
        await AuthenticateAdminAsync();

        var response = await _client.GetAsync("/api/admin/join-requests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<JoinRequestModel>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ApproveJoinRequest_NonExistentId_ReturnsNotFoundOrBadRequest()
    {
        await AuthenticateAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/admin/join-requests/999999/approve", new { });
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RejectJoinRequest_NonExistentId_ReturnsNotFoundOrBadRequest()
    {
        await AuthenticateAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/admin/join-requests/999999/reject", new { });
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound);
    }
}
