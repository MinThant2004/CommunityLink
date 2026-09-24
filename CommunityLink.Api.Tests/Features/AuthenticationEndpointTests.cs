using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using CommunityLink.Api.Tests.Infrastructure;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityLink.Api.Tests.Features;

public class AuthenticationEndpointTests(CommunityApiFactory factory) : IClassFixture<CommunityApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly CommunityApiFactory _factory = factory;

    [Fact]
    public async Task Login_WithDefaultAdminCredentials_ReturnsSuccessAndToken()
    {
        var req = new LoginRequestModel("admin@communitylink.local", "Admin@123");
        var response = await _client.PostAsJsonAsync("/api/auth/login", req);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
        Assert.Equal("ADMIN", result.Data.RoleCode);
        Assert.True(result.Data.RoleId > 0);
        Assert.NotEmpty(result.Data.Permissions);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var req = new LoginRequestModel("admin@communitylink.local", "WrongPassword!");
        var response = await _client.PostAsJsonAsync("/api/auth/login", req);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_NewUser_ReturnsSuccess()
    {
        var uniqueEmail = $"user_{Guid.NewGuid():N}@communitylink.local";
        var uniqueUser = $"user_{Guid.NewGuid():N}"[..12];
        var req = new RegisterRequestModel("Test User", uniqueUser, uniqueEmail, "TestPass@123", "Bio details");

        var response = await _client.PostAsJsonAsync("/api/auth/register", req);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(uniqueEmail, result.Data.Email);
        Assert.Equal("USER", result.Data.RoleCode);
        Assert.True(result.Data.RoleId > 0);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
    }

    [Fact]
    public async Task Register_ThenLogin_Roundtrip_CreatesMembership()
    {
        var uniqueEmail = $"member_{Guid.NewGuid():N}@communitylink.local";
        var uniqueUser = $"member_{Guid.NewGuid():N}"[..12];

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestModel("Roundtrip Member", uniqueUser, uniqueEmail, "MemberPass@123"));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel(uniqueEmail, "MemberPass@123"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var result = await loginResponse.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(uniqueEmail, result.Data.Email);
        Assert.Equal("USER", result.Data.RoleCode);
        Assert.False(string.IsNullOrWhiteSpace(result.Data.AccessToken));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var uniqueEmail = $"dup_{Guid.NewGuid():N}@communitylink.local";
        var userName = $"dup_{Guid.NewGuid():N}"[..12];
        var req = new RegisterRequestModel("Dup User", userName, uniqueEmail, "TestPass@123");

        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync("/api/auth/register", req)).StatusCode);

        var secondEmail = await _client.PostAsJsonAsync("/api/auth/register", req);
        Assert.Equal(HttpStatusCode.Conflict, secondEmail.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsProfile()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel("admin@communitylink.local", "Admin@123"));
        var loginResult = await login.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(loginResult?.Data?.AccessToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Data.AccessToken);

        var meResponse = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);

        var me = await meResponse.Content.ReadFromJsonAsync<Result<UserInfoModel>>();
        Assert.NotNull(me);
        Assert.True(me.IsSuccess);
        Assert.NotNull(me.Data);
        Assert.Equal("admin@communitylink.local", me.Data.Email);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var meResponse = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ThenLogin_VerifiesNewPassword()
    {
        var uniqueEmail = $"pw_{Guid.NewGuid():N}@communitylink.local";
        var userName = $"pw_{Guid.NewGuid():N}"[..12];

        var register = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestModel("Password User", userName, uniqueEmail, "OldPass@123"));
        var registerResult = await register.Content.ReadFromJsonAsync<Result<LoginResponseModel>>();
        Assert.NotNull(registerResult?.Data?.AccessToken);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", registerResult.Data.AccessToken);

        var change = await _client.PostAsJsonAsync("/api/auth/change-password",
            new ChangePasswordRequestModel("OldPass@123", "NewPass@123"));
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var oldLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(uniqueEmail, "OldPass@123"));
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestModel(uniqueEmail, "NewPass@123"));
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_SendOtp_UnknownEmail_ReturnsGenericSuccessWithoutLeak()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password/send-otp",
            new SendOtpRequestModel($"nobody_{Guid.NewGuid():N}@communitylink.local"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        Assert.Contains("If an account exists", result.Message);
    }

    [Fact]
    public async Task ForgotPassword_SendOtp_KnownEmail_ReturnsSuccessWithoutOtpInPayload()
    {
        var uniqueEmail = $"otp_{Guid.NewGuid():N}@communitylink.local";
        var uniqueUser = $"otp_{Guid.NewGuid():N}"[..12];
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestModel("Otp User", uniqueUser, uniqueEmail, "TestPass@123"));

        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password/send-otp",
            new SendOtpRequestModel(uniqueEmail));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        Assert.Contains("If an account exists", result.Message);
    }

    [Fact]
    public async Task ForgotPassword_VerifyOtp_InvalidCode_ReturnsValidationError()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password/verify-otp",
            new VerifyOtpRequestModel("000000"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_ResetPassword_WrongOtp_ReturnsValidationError()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password/reset-password",
            new ResetPasswordRequestModel("000000", "NewPass@123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_ResetPassword_UnverifiedOtp_ReturnsValidationError()
    {
        var uniqueEmail = $"unverified_{Guid.NewGuid():N}@communitylink.local";
        var uniqueUser = $"unverified_{Guid.NewGuid():N}"[..12];
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestModel("Unverified User", uniqueUser, uniqueEmail, "TestPass@123"));

        const string otp = "654321";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.TblPasswordResetOtps.Add(new TblPasswordResetOtp
            {
                Email = uniqueEmail.ToUpperInvariant(),
                OtpCode = ComputeOtpHash(otp),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
                IsUsed = false,
                CreatedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password/reset-password",
            new ResetPasswordRequestModel(otp, "NewPass@123"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_FullFlow_VerifyThenReset_PasswordChanges()
    {
        var uniqueEmail = $"reset_{Guid.NewGuid():N}@communitylink.local";
        var uniqueUser = $"reset_{Guid.NewGuid():N}"[..12];
        await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestModel("Reset User", uniqueUser, uniqueEmail, "OldPass@123"));

        var sendOtp = await _client.PostAsJsonAsync("/api/auth/forgot-password/send-otp",
            new SendOtpRequestModel(uniqueEmail));
        Assert.Equal(HttpStatusCode.OK, sendOtp.StatusCode);

        const string otp = "123456";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.TblPasswordResetOtps.Add(new TblPasswordResetOtp
            {
                Email = uniqueEmail.ToUpperInvariant(),
                OtpCode = ComputeOtpHash(otp),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
                IsUsed = false,
                CreatedAtUtc = DateTime.UtcNow.AddSeconds(1)
            });
            await db.SaveChangesAsync();
        }

        var verify = await _client.PostAsJsonAsync("/api/auth/forgot-password/verify-otp",
            new VerifyOtpRequestModel(otp));
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);

        var reset = await _client.PostAsJsonAsync("/api/auth/forgot-password/reset-password",
            new ResetPasswordRequestModel(otp, "NewPass@123"));
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var oldLogin = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel(uniqueEmail, "OldPass@123"));
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestModel(uniqueEmail, "NewPass@123"));
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    private static string ComputeOtpHash(string otp)
    {
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}