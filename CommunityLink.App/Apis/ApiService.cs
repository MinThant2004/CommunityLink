using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using CommunityLink.Shared;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public class ApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
{
    protected HttpClient CreateClient()
    {
        var client = clientFactory.CreateClient("CommunityApi");
        var token = httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    protected async Task<Result<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content)) return Result<T>.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Result<T>>(content, options);
            return result ?? Result<T>.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"API request failed: {ex.Message}", ResultStatus.SystemError);
        }
    }

    protected async Task<Result<T>> PostAsync<T, TRequest>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync(url, request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content)) return Result<T>.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Result<T>>(content, options);
            return result ?? Result<T>.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"API request failed: {ex.Message}", ResultStatus.SystemError);
        }
    }

    protected async Task<Result> PostAsync<TRequest>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync(url, request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content)) return Result.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Result>(content, options);
            return result ?? Result.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result.Failure($"API request failed: {ex.Message}", ResultStatus.SystemError);
        }
    }

    protected async Task<Result<T>> PutAsync<T, TRequest>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync(url, request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content)) return Result<T>.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Result<T>>(content, options);
            return result ?? Result<T>.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure($"API request failed: {ex.Message}", ResultStatus.SystemError);
        }
    }

    protected async Task<Result> PutAsync<TRequest>(string url, TRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = CreateClient();
            var response = await client.PutAsJsonAsync(url, request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content)) return Result.Failure("Empty response from API server.", ResultStatus.SystemError);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<Result>(content, options);
            return result ?? Result.Failure("Failed to deserialize API response.", ResultStatus.SystemError);
        }
        catch (Exception ex)
        {
            return Result.Failure($"API request failed: {ex.Message}", ResultStatus.SystemError);
        }
    }
}