using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class AuthenticationApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginResponseModel, LoginRequestModel>("api/auth/login", request, cancellationToken);

    public Task<Result<LoginResponseModel>> LoginAdminAsync(LoginRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginResponseModel, LoginRequestModel>("api/auth/admin/login", request, cancellationToken);

    public Task<Result<LoginResponseModel>> RegisterAsync(RegisterRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginResponseModel, RegisterRequestModel>("api/auth/register", request, cancellationToken);

    public Task<Result<LoginResponseModel>> RegisterAdminAsync(RegisterAdminRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginResponseModel, RegisterAdminRequestModel>("api/auth/admin/register", request, cancellationToken);

    public Task<Result> ChangePasswordAsync(ChangePasswordRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync("api/auth/change-password", request, cancellationToken);

    public Task<Result<UserInfoModel>> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetAsync<UserInfoModel>("api/auth/me", cancellationToken);

    public Task<Result<bool>> SendPasswordResetOtpAsync(SendOtpRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<bool, SendOtpRequestModel>("api/auth/forgot-password/send-otp", request, cancellationToken);

    public Task<Result<bool>> VerifyPasswordResetOtpAsync(VerifyOtpRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<bool, VerifyOtpRequestModel>("api/auth/forgot-password/verify-otp", request, cancellationToken);

    public Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<bool, ResetPasswordRequestModel>("api/auth/forgot-password/reset-password", request, cancellationToken);
}