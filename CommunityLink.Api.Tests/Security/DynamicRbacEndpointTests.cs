using System.Net;
using CommunityLink.Api.Tests.Infrastructure;

namespace CommunityLink.Api.Tests.Security;

public class DynamicRbacEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AnonymousRequest_ToProtectedAdminEndpoint_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/admin/stats");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousRequest_ToProtectedCommunitiesEndpoint_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/communities");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousRequest_ToProtectedFeedEndpoint_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/posts/feed");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}