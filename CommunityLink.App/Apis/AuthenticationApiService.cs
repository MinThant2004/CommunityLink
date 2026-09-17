using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class AuthenticationApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginResponseModel, LoginRequestModel>("api/auth/login", request, cancellationToken);

    public Task<Result<UserInfoModel>> RegisterAsync(RegisterRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<UserInfoModel, RegisterRequestModel>("api/auth/register", request, cancellationToken);

    public Task<Result> ChangePasswordAsync(ChangePasswordRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync("api/auth/change-password", request, cancellationToken);

    public Task<Result<UserInfoModel>> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetAsync<UserInfoModel>("api/auth/me", cancellationToken);
}