using CommunityLink.Shared;
using CommunityLink.Shared.Features.Community;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class CommunityApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<CommunityModel>>> GetCommunitiesAsync(string? search = null, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CommunityModel>>(string.IsNullOrWhiteSpace(search) ? "api/communities" : $"api/communities?search={Uri.EscapeDataString(search)}", cancellationToken);

    public Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default) =>
        GetAsync<CommunityModel>($"api/communities/{communityId}", cancellationToken);

    public Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<CommunityModel, CreateCommunityRequestModel>("api/communities", request, cancellationToken);

    public Task<Result<CommunityModel>> UpdateCommunityAsync(int communityId, EditCommunityRequestModel request, CancellationToken cancellationToken = default) =>
        PutAsync<CommunityModel, EditCommunityRequestModel>($"api/communities/{communityId}", request, cancellationToken);

    public Task<Result<IReadOnlyList<CommunityAuditModel>>> GetCommunityAuditsAsync(int communityId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CommunityAuditModel>>($"api/communities/{communityId}/audits", cancellationToken);

    public Task<Result> JoinCommunityAsync(int communityId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/communities/{communityId}/join", new { }, cancellationToken);

    public async Task<Result<string>> UploadBannerAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "banner", fileName);

            var response = await client.PostAsync("api/communities/upload-banner", content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseString))
                return Result<string>.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = System.Text.Json.JsonSerializer.Deserialize<Result<string>>(responseString, options);
            return result ?? Result<string>.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Banner upload failed: {ex.Message}", ResultStatus.SystemError);
        }
    }
}