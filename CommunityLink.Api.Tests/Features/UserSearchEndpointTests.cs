using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.UserProfile;

namespace CommunityLink.Api.Tests.Features;

public class UserSearchEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string Token, string UserName, int UserId)> RegisterAsync(
        string userName,
        string displayName)
    {
        var email = $"{userName}@test.local";
        var request = new RegisterRequestModel(displayName, userName, email, "Password@123", "Search Bio");

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(result?.Data?.AccessToken);
        return (result.Data.AccessToken, userName, result.Data.UserId);
    }

    private static string UniqueToken() => Guid.NewGuid().ToString("N")[..10];

    private async Task<Result<IReadOnlyList<UserSearchResultDto>>> SearchAsync(
        string token,
        string term,
        int? limit = null)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var url = $"/api/users/search?search={Uri.EscapeDataString(term)}";
        if (limit.HasValue)
        {
            url += $"&limit={limit.Value}";
        }

        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<IReadOnlyList<UserSearchResultDto>>>();
        Assert.NotNull(result);
        return result;
    }

    [Fact]
    public async Task SearchUsers_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/users/search?search=alice");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SearchUsers_ByUsernamePrefix_ReturnsMatch()
    {
        var token = UniqueToken();
        var (searcherToken, _, _) = await RegisterAsync($"probe_{token}", $"Probe {token}");
        var (_, targetUserName, targetUserId) = await RegisterAsync($"alice_{token}", "Alice Anderson");

        var result = await SearchAsync(searcherToken, $"alice_{token}");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        var match = Assert.Single(result.Data);
        Assert.Equal(targetUserId, match.UserId);
        Assert.Equal(targetUserName, match.UserName);
        Assert.Equal("Alice Anderson", match.DisplayName);
    }

    [Fact]
    public async Task SearchUsers_ByDisplayName_ReturnsMatch()
    {
        var token = UniqueToken();
        var (searcherToken, _, _) = await RegisterAsync($"probe_{token}", $"Probe {token}");
        var (_, _, targetUserId) = await RegisterAsync($"nomatch_{token}", $"Bartholomew {token}");

        var result = await SearchAsync(searcherToken, $"bartholomew {token}");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Contains(result.Data, u => u.UserId == targetUserId);
    }

    [Fact]
    public async Task SearchUsers_ExcludesSelf()
    {
        var token = UniqueToken();
        var (searcherToken, searcherUserName, searcherUserId) =
            await RegisterAsync($"self_{token}", $"Selfy {token}");

        var result = await SearchAsync(searcherToken, searcherUserName);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.DoesNotContain(result.Data, u => u.UserId == searcherUserId);
    }

    [Fact]
    public async Task SearchUsers_TermUnderTwoChars_ReturnsEmptyWithoutError()
    {
        var token = UniqueToken();
        var (searcherToken, _, _) = await RegisterAsync($"probe_{token}", $"Probe {token}");
        await RegisterAsync($"alice_{token}", "Alice Anderson");

        var result = await SearchAsync(searcherToken, "a");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task SearchUsers_RespectsLimit()
    {
        var token = UniqueToken();
        var (searcherToken, _, _) = await RegisterAsync($"probe_{token}", $"Probe {token}");

        for (var i = 0; i < 3; i++)
        {
            await RegisterAsync($"capped{i}_{token}", $"Capped {token} Number {i}");
        }

        var result = await SearchAsync(searcherToken, $"capped {token}", limit: 1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task SearchUsers_WithoutLimitParam_ReturnsMoreThanOneResult()
    {
        // An omitted `limit` must fall back to the default, not collapse to a single row.
        var token = UniqueToken();
        var (searcherToken, _, _) = await RegisterAsync($"probe_{token}", $"Probe {token}");

        for (var i = 0; i < 3; i++)
        {
            await RegisterAsync($"defaulted{i}_{token}", $"Defaulted {token} Number {i}");
        }

        var result = await SearchAsync(searcherToken, $"defaulted {token}");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count);
    }

    [Fact]
    public async Task SearchUsers_OrdersUsernamePrefixBeforeDisplayNameMatch()
    {
        var token = UniqueToken();

        // Matches the term only through the indexed username prefix.
        var (searcherToken, _, _) = await RegisterAsync($"probe{token}", $"Probe {token}");
        var (_, prefixUserName, prefixUserId) = await RegisterAsync($"zara{token}", "Zara Anchor");

        // Matches the same term only as a display-name substring.
        var (_, substringUserName, substringUserId) = await RegisterAsync($"other{token}", $"Zzara{token} Beta");

        var result = await SearchAsync(searcherToken, $"zara{token}");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // Both accounts match, so ordering is what decides which one is offered first.
        Assert.Contains(result.Data, u => u.UserId == prefixUserId);
        Assert.Contains(result.Data, u => u.UserId == substringUserId);
        Assert.Equal(prefixUserId, result.Data[0].UserId);
        Assert.Equal(prefixUserName, result.Data[0].UserName);
        Assert.NotEqual(substringUserName, result.Data[0].UserName);
    }
}
