using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Administration;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Api.Tests.Features;

public class AdminDashboardEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
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
    public async Task GetDashboard_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDashboard_AsAdmin_ReturnsAdminDashboardModel()
    {
        await AuthenticateAdminAsync();

        var response = await _client.GetAsync("/api/admin/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<AdminDashboardModel>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Stats);
        Assert.NotNull(result.Data.Trends);
        Assert.NotNull(result.Data.PendingJoinRequests);
    }

    [Fact]
    public async Task GetUsersPage_AsAdmin_ReturnsPagedUsers()
    {
        await AuthenticateAdminAsync();

        var response = await _client.GetAsync("/api/admin/users/page?page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<PagedResult<UserInfoModel>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.TotalCount >= 0);
    }

    [Fact]
    public async Task GetAuditLogsPage_AsAdmin_ReturnsPagedAuditLogs()
    {
        await AuthenticateAdminAsync();

        var response = await _client.GetAsync("/api/admin/audits/page?page=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<PagedResult<AuditLogModel>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.TotalCount >= 0);
    }
}
