using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.UserProfile;

namespace CommunityLink.Api.Tests.Features;

public class UserProfileEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> LoginAsUserAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(email, password));
        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(result?.Data?.AccessToken);
        return result.Data.AccessToken;
    }

    private async Task<(string Token, string UserName, int UserId)> RegisterNewUserAsync(string prefix = "user")
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var userName = $"{prefix}_{unique}";
        var email = $"{userName}@test.local";
        var req = new RegisterRequestModel($"Display {userName}", userName, email, "Password@123", "Initial Bio");
        
        var response = await _client.PostAsJsonAsync("/api/auth/register", req);
        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(result?.Data?.AccessToken);
        return (result.Data.AccessToken, userName, result.Data.UserId);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WithToken_ReturnsOwnerProfile()
    {
        var (token, userName, userId) = await RegisterNewUserAsync("owner");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<UserProfileDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(userName, result.Data.UserName);
        Assert.Equal(userId, result.Data.UserId);
        Assert.NotNull(result.Data.Role);
        Assert.NotNull(result.Data.Metrics);
        Assert.True(result.Data.Relationship.IsSelf);
    }

    [Fact]
    public async Task GetPublicProfile_ByUserName_ReturnsProfile()
    {
        var (token, userName, userId) = await RegisterNewUserAsync("target");
        
        // Query publicly without token
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/users/{userName}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<UserProfileDto>>();
        Assert.NotNull(result?.Data);
        Assert.Equal(userName, result.Data.UserName);
        Assert.Equal(userId, result.Data.UserId);
        Assert.False(result.Data.Relationship.IsSelf);
    }

    [Fact]
    public async Task GetPublicProfile_NotFound_Returns404()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/users/non_existing_user_12345");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ValidPayload_UpdatesSuccessfully()
    {
        var (token, userName, _) = await RegisterNewUserAsync("upd");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var newDisplayName = "Updated Name";
        var newBio = "Updated bio summary for testing.";
        var updateReq = new UpdateUserProfileRequestDto
        {
            DisplayName = newDisplayName,
            UserName = userName,
            Bio = newBio
        };

        var response = await _client.PutAsJsonAsync("/api/users/me", updateReq);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<UserProfileDto>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(newDisplayName, result.Data?.DisplayName);
        Assert.Equal(newBio, result.Data?.Bio);
    }

    [Fact]
    public async Task UpdateProfile_DuplicateUsername_ReturnsConflict()
    {
        var (_, user1Name, _) = await RegisterNewUserAsync("user_first");
        var (user2Token, user2Name, _) = await RegisterNewUserAsync("user_second");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user2Token);

        // Try to take user1's username
        var updateReq = new UpdateUserProfileRequestDto
        {
            DisplayName = "Duplicate Tester",
            UserName = user1Name,
            Bio = "Trying to steal username"
        };

        var response = await _client.PutAsJsonAsync("/api/users/me", updateReq);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_InvalidUsernameFormat_ReturnsBadRequest()
    {
        var (token, _, _) = await RegisterNewUserAsync("invalid");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateReq = new UpdateUserProfileRequestDto
        {
            DisplayName = "Invalid User",
            UserName = "invalid user name with spaces!",
            Bio = "Invalid"
        };

        var response = await _client.PutAsJsonAsync("/api/users/me", updateReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ToggleSaveAccount_SuccessAndToggleBehavior()
    {
        var (user1Token, _, _) = await RegisterNewUserAsync("saver");
        var (_, _, user2Id) = await RegisterNewUserAsync("save_target");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user1Token);

        // 1. First save -> true
        var saveRes = await _client.PostAsync($"/api/users/{user2Id}/save", null);
        Assert.Equal(HttpStatusCode.OK, saveRes.StatusCode);
        var resObj = await saveRes.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.True(resObj?.Data);

        // Check saved accounts list
        var listRes = await _client.GetAsync("/api/users/me/saved-accounts");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        var listObj = await listRes.Content.ReadFromJsonAsync<Result<List<SavedAccountItemDto>>>();
        Assert.Contains(listObj?.Data ?? new(), x => x.SavedUserId == user2Id);

        // 2. Toggle again -> false (removed)
        var unsaveRes = await _client.PostAsync($"/api/users/{user2Id}/save", null);
        Assert.Equal(HttpStatusCode.OK, unsaveRes.StatusCode);
        var unsaveObj = await unsaveRes.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.False(unsaveObj?.Data);
    }

    [Fact]
    public async Task RateUser_SuccessAndRecalculatesAverage()
    {
        var (rater1Token, _, _) = await RegisterNewUserAsync("rater_one");
        var (rater2Token, _, _) = await RegisterNewUserAsync("rater_two");
        var (_, targetUserName, targetUserId) = await RegisterNewUserAsync("rated_target");

        // Rater 1 gives 5 stars
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rater1Token);
        var rate1Res = await _client.PostAsJsonAsync($"/api/users/{targetUserId}/rate", new RateUserRequestDto
        {
            Score = 5,
            ReviewText = "Excellent community member!"
        });
        Assert.Equal(HttpStatusCode.OK, rate1Res.StatusCode);

        // Rater 2 gives 3 stars
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rater2Token);
        var rate2Res = await _client.PostAsJsonAsync($"/api/users/{targetUserId}/rate", new RateUserRequestDto
        {
            Score = 3,
            ReviewText = "Good contributor."
        });
        Assert.Equal(HttpStatusCode.OK, rate2Res.StatusCode);

        // Check public profile metrics -> Average (5+3)/2 = 4.0, count = 2
        var pubRes = await _client.GetAsync($"/api/users/{targetUserName}");
        Assert.Equal(HttpStatusCode.OK, pubRes.StatusCode);
        var pubObj = await pubRes.Content.ReadFromJsonAsync<Result<UserProfileDto>>();
        Assert.Equal(4.0m, pubObj?.Data?.Metrics?.AverageRating);
        Assert.Equal(2, pubObj?.Data?.Metrics?.RatingCount);

        // Check reviews list
        var reviewsRes = await _client.GetAsync($"/api/users/{targetUserId}/reviews");
        Assert.Equal(HttpStatusCode.OK, reviewsRes.StatusCode);
        var reviewsObj = await reviewsRes.Content.ReadFromJsonAsync<Result<List<UserRatingItemDto>>>();
        Assert.Equal(2, reviewsObj?.Data?.Count);
    }

    [Fact]
    public async Task RateUser_SelfRating_ReturnsBadRequest()
    {
        var (token, _, userId) = await RegisterNewUserAsync("self_rater");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var rateRes = await _client.PostAsJsonAsync($"/api/users/{userId}/rate", new RateUserRequestDto
        {
            Score = 5,
            ReviewText = "Rating myself"
        });
        Assert.Equal(HttpStatusCode.BadRequest, rateRes.StatusCode);
    }
}
