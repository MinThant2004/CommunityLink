using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Admin;
using CommunityLink.Shared.Features.Authentication;
using Xunit;

namespace CommunityLink.Api.Tests.Features;

public class PlatformSettingEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> GetAdminTokenAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("admin@communitylink.local", "Admin@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    private async Task<string> GetMemberTokenAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("member@communitylink.local", "Password@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    [Fact]
    public async Task GetPlatformCommission_Default_ReturnsDefault10Percent()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/admin/settings/commission");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<PlatformCommissionSettingDto>>();
        Assert.NotNull(result?.Data);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data.CommissionPercentage >= 0m && result.Data.CommissionPercentage <= 100m);
    }

    [Fact]
    public async Task UpdatePlatformCommission_AsAdmin_UpdatesSettingAndSetsTimestampAndUpdatedBy()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateCommissionSettingRequestDto { CommissionPercentage = 12.50m };
        var updateRes = await _client.PutAsJsonAsync("/api/admin/settings/commission", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        var updateData = await updateRes.Content.ReadFromJsonAsync<Result<PlatformCommissionSettingDto>>();
        Assert.NotNull(updateData?.Data);
        Assert.True(updateData.IsSuccess);
        Assert.Equal(12.50m, updateData.Data.CommissionPercentage);
        Assert.NotNull(updateData.Data.UpdatedAt);
        Assert.NotNull(updateData.Data.UpdatedBy);

        // Verify GET returns updated setting
        var getRes = await _client.GetAsync("/api/admin/settings/commission");
        var getData = await getRes.Content.ReadFromJsonAsync<Result<PlatformCommissionSettingDto>>();
        Assert.NotNull(getData?.Data);
        Assert.Equal(12.50m, getData.Data.CommissionPercentage);
    }

    [Fact]
    public async Task UpdatePlatformCommission_AsNonAdmin_ReturnsForbiddenOrUnauthorized()
    {
        var token = await GetMemberTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateCommissionSettingRequestDto { CommissionPercentage = 15.00m };
        var response = await _client.PutAsJsonAsync("/api/admin/settings/commission", updateRequest);

        Assert.True(response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePlatformCommission_NegativeValue_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateCommissionSettingRequestDto { CommissionPercentage = -5.00m };
        var response = await _client.PutAsJsonAsync("/api/admin/settings/commission", updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<PlatformCommissionSettingDto>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot be negative", result.Message ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdatePlatformCommission_GreaterThan100_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new UpdateCommissionSettingRequestDto { CommissionPercentage = 105.00m };
        var response = await _client.PutAsJsonAsync("/api/admin/settings/commission", updateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<PlatformCommissionSettingDto>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("cannot exceed 100", result.Message ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdatePlatformCommission_InvalidNumericValue_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new StringContent("{\"CommissionPercentage\":\"invalid_string\"}", Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/admin/settings/commission", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
