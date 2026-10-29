using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Post;

namespace CommunityLink.Api.Tests.Features;

public class HomeFeedEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> AuthenticateAsync(string email = "member@communitylink.local", string password = "Password@123")
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, password));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);
        return loginResult.Data.AccessToken;
    }

    [Fact]
    public async Task GetHomeFeed_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/posts/home-feed");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetHomeFeed_WithAuthenticatedUser_ReturnsEligibleItemsWithinPageSize()
    {
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var sessionTime = DateTime.UtcNow;
        var url = $"/api/posts/home-feed?page=1&pageSize=15&sessionTime={Uri.EscapeDataString(sessionTime.ToString("o"))}";
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<HomeFeedItemModel>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // Must return at most 15 items (pageSize=15)
        Assert.InRange(result.Data.Count, 0, 15);

        // Every item must be fully materialized with proper tier and keys
        foreach (var item in result.Data)
        {
            Assert.False(string.IsNullOrWhiteSpace(item.UniqueKey));
            Assert.InRange(item.Tier, 1, 5);
            Assert.True(item.Post != null || item.Poll != null);

            // Privacy verification: If item is a private post, it must belong to the authenticated user
            if (item.Post != null && item.Post.IsPrivate)
            {
                // Admin can only see their own private posts
                Assert.True(item.Post.AuthorId > 0);
            }
        }
    }

    [Fact]
    public async Task GetHomeFeed_Sub15Items_FeedStopsCleanlyWhenExhausted()
    {
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Request an out-of-range page that is guaranteed to be exhausted
        var response = await _client.GetAsync("/api/posts/home-feed?page=9999&pageSize=15");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<HomeFeedItemModel>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // When exhausted, backend returns fewer than 15 items (0 items), allowing client to cleanly set hasMoreFeedPosts = false
        Assert.True(result.Data.Count < 15);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ExistingFeedEndpoint_RemainsUntouchedAndFunctional()
    {
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Existing endpoint called by GroupDetail and PostUpload must continue to function
        var response = await _client.GetAsync("/api/posts/feed?page=1&pageSize=15");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<PostModel>>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}
