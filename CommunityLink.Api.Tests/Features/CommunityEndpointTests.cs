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

        var uniqueName = $".NET Enthusiasts {Guid.NewGuid():N}"[..20];
        var slug = $"dotnet-{Guid.NewGuid():N}"[..15];
        var createReq = new CreateCommunityRequestModel(uniqueName, slug, "A community for .NET devs", null, null, "Public", "Open");
        var createRes = await client.PostAsJsonAsync("/api/communities", createReq);

        Assert.Equal(HttpStatusCode.OK, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.NotNull(created?.Data);
        Assert.Equal(uniqueName, created.Data.Name);

        var getRes = await client.GetAsync($"/api/communities/{created.Data.CommunityId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var fetched = await getRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.NotNull(fetched?.Data);
        Assert.Equal(created.Data.CommunityId, fetched.Data.CommunityId);
    }

    [Fact]
    public async Task CreateCommunity_Anonymous_WithoutLogin_Succeeds()
    {
        var client = factory.CreateClient();
        var uniqueName = $"Public Hub {Guid.NewGuid():N}"[..20];
        var createReq = new CreateCommunityRequestModel(uniqueName, null, "Public community created anonymously", null, null, "PUBLIC", "INSTANT");

        var response = await client.PostAsJsonAsync("/api/communities", createReq);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.True(result?.IsSuccess);
        Assert.NotNull(result?.Data);
        Assert.Equal(uniqueName, result.Data.Name);
    }

    [Fact]
    public async Task CreateCommunity_DuplicateName_ReturnsFriendlyErrorMessage()
    {
        var client = factory.CreateClient();
        var sharedName = $"DuplicateComm_{Guid.NewGuid():N}"[..20];

        // 1. Create first community
        var firstReq = new CreateCommunityRequestModel(sharedName, null, "First community", null, null, "PUBLIC", "INSTANT");
        var firstRes = await client.PostAsJsonAsync("/api/communities", firstReq);
        Assert.Equal(HttpStatusCode.OK, firstRes.StatusCode);

        // 2. Attempt creating another with same name (different casing)
        var secondReq = new CreateCommunityRequestModel(sharedName.ToLowerInvariant(), null, "Second community", null, null, "PUBLIC", "INSTANT");
        var secondRes = await client.PostAsJsonAsync("/api/communities", secondReq);
        Assert.Equal(HttpStatusCode.Conflict, secondRes.StatusCode);

        var secondResult = await secondRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.False(secondResult?.IsSuccess);
        Assert.Equal("This community name is already exist!", secondResult?.Message);
    }

    [Fact]
    public async Task CreateSubCommunity_LinkedToParent_AndCheckDuplicates()
    {
        var client = factory.CreateClient();
        var parentName = $"Parent_{Guid.NewGuid():N}"[..18];
        var parentReq = new CreateCommunityRequestModel(parentName, null, "Parent community", null, null, "PUBLIC", "INSTANT");
        var parentRes = await client.PostAsJsonAsync("/api/communities", parentReq);
        var parentResult = await parentRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.True(parentResult?.IsSuccess);
        var parentId = parentResult.Data!.CommunityId;

        // 1. Create Sub-community under parent
        var subName = $"Sub_{Guid.NewGuid():N}"[..18];
        var subReq = new CreateCommunityRequestModel(subName, null, "Sub-community", null, null, "PUBLIC", "INSTANT", ParentCommunityId: parentId);
        var subRes = await client.PostAsJsonAsync("/api/communities", subReq);
        Assert.Equal(HttpStatusCode.OK, subRes.StatusCode);
        var subResult = await subRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.True(subResult?.IsSuccess);
        Assert.Equal(parentId, subResult.Data!.ParentCommunityId);

        // 2. Attempt creating another Sub-community with same name (case-insensitive) -> should reject with sub-community message
        var duplicateSubReq = new CreateCommunityRequestModel(subName.ToUpperInvariant(), null, "Duplicate Sub", null, null, "PUBLIC", "INSTANT", ParentCommunityId: parentId);
        var duplicateSubRes = await client.PostAsJsonAsync("/api/communities", duplicateSubReq);
        Assert.Equal(HttpStatusCode.Conflict, duplicateSubRes.StatusCode);
        var duplicateSubResult = await duplicateSubRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.Equal("This sub-community name is already exist!", duplicateSubResult?.Message);

        // 3. Attempt creating a sub-community with the parent's name -> should reject with sub-community message
        var subWithParentNameReq = new CreateCommunityRequestModel(parentName, null, "Sub with parent name", null, null, "PUBLIC", "INSTANT", ParentCommunityId: parentId);
        var subWithParentNameRes = await client.PostAsJsonAsync("/api/communities", subWithParentNameReq);
        Assert.Equal(HttpStatusCode.Conflict, subWithParentNameRes.StatusCode);
        var subWithParentNameResult = await subWithParentNameRes.Content.ReadFromJsonAsync<Result<CommunityModel>>();
        Assert.Equal("This sub-community name is already exist!", subWithParentNameResult?.Message);
    }
}