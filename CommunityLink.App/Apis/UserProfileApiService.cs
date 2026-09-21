using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.UserProfile;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class UserProfileApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<UserProfileDto>> GetOwnerProfileAsync(CancellationToken cancellationToken = default) =>
        GetAsync<UserProfileDto>("api/users/me", cancellationToken);

    public Task<Result<UserProfileDto>> GetPublicProfileAsync(string userNameOrId, CancellationToken cancellationToken = default) =>
        GetAsync<UserProfileDto>($"api/users/{userNameOrId}", cancellationToken);

    public Task<Result<UserProfileDto>> UpdateProfileAsync(UpdateUserProfileRequestDto dto, CancellationToken cancellationToken = default) =>
        PutAsync<UserProfileDto, UpdateUserProfileRequestDto>("api/users/me", dto, cancellationToken);

    public async Task<Result<UploadAvatarResponseDto>> UploadAvatarAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "avatar", fileName);

            var response = await client.PostAsync("api/users/me/avatar", content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseString))
                return Result<UploadAvatarResponseDto>.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Result<UploadAvatarResponseDto>>(responseString, options);
            return result ?? Result<UploadAvatarResponseDto>.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result<UploadAvatarResponseDto>.Failure($"Avatar upload failed: {ex.Message}", ResultStatus.SystemError);
        }
    }

    public Task<Result<bool>> ToggleSaveAccountAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        PostAsync<bool, object?>($"api/users/{targetUserId}/save", null, cancellationToken);

    public Task<Result<UserProfileDto>> RateUserAsync(int targetUserId, RateUserRequestDto dto, CancellationToken cancellationToken = default) =>
        PostAsync<UserProfileDto, RateUserRequestDto>($"api/users/{targetUserId}/rate", dto, cancellationToken);

    public Task<Result<List<UserPostItemDto>>> GetUserPostsAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        GetAsync<List<UserPostItemDto>>($"api/users/{targetUserId}/posts", cancellationToken);

    public Task<Result<List<UserPostItemDto>>> GetSavedPostsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<UserPostItemDto>>("api/users/me/saved-posts", cancellationToken);

    public Task<Result<List<SavedAccountItemDto>>> GetSavedAccountsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<SavedAccountItemDto>>("api/users/me/saved-accounts", cancellationToken);

    public Task<Result<List<UserCommunityItemDto>>> GetUserCommunitiesAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        GetAsync<List<UserCommunityItemDto>>($"api/users/{targetUserId}/communities", cancellationToken);

    public Task<Result<List<UserRatingItemDto>>> GetUserReviewsAsync(int targetUserId, CancellationToken cancellationToken = default) =>
        GetAsync<List<UserRatingItemDto>>($"api/users/{targetUserId}/reviews", cancellationToken);
}
