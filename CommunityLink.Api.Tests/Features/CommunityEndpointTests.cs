using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Community;

namespace CommunityLink.Api.Tests.Features;

public class CommunityEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> GetAdminTokenAsync()
    {
        var req = new LoginRequestModel("admin@communitylink.local", "Admin@123");
        var res = await _client.PostAsJsonAsync("/api/auth/login", req);
        var result = await res.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        return result!.Data!.AccessToken;
    }

    [Fact]
    public async Task CreateAndGetCommunity_AsAdmin_ReturnsSuccess()
    {
        var token = await GetAdminTokenAsync();
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var slug = $"dotnet-{Guid.NewGuid():N}"[..15];
        var createReq = new CreateCommunityRequestModel(".NET Enthusiasts", slug, "A community for .NET devs", null, null, "Public", "Open");
        var createRes = await client.PostAsJsonAsync("/api/communities", createReq);

        Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.NotNull(created?.Data);
        Assert.Equal(".NET Enthusiasts", created.Data.Name);

        var getRes = await client.GetAsync($"/api/communities/{created.Data.CommunityId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var fetched = await getRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.NotNull(fetched?.Data);
        Assert.Equal(created.Data.CommunityId, fetched.Data.CommunityId);
    }
}