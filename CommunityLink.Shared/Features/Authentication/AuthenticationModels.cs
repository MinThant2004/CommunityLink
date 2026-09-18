namespace CommunityLink.Shared.Features.Authentication;

public sealed record LoginRequestModel(string Email, string Password, bool RememberMe = false, bool IsAdmin = false);

public sealed record LoginResponseModel(
    string AccessToken,
    string RefreshToken,
    int UserId,
    string FullName,
    string Email,
    string RoleCode,
    int RoleId,
    bool MustChangePassword,
    IReadOnlyList<string> Permissions,
    string SessionId = "");

public sealed record RegisterRequestModel(
    string FullName,
    string UserName,
    string Email,
    string Password,
    string? Bio = null,
    string? ConfirmPassword = null);

public sealed record RegisterAdminRequestModel(
    string FullName,
    string Email,
    string Password,
    string AdminInviteCode,
    string? ConfirmPassword = null);

public sealed record ChangePasswordRequestModel(string CurrentPassword, string NewPassword);

public sealed record SendOtpRequestModel(string Email, bool IsAdmin = false);

public sealed record VerifyOtpRequestModel(string OtpCode);

public sealed record ResetPasswordRequestModel(string OtpCode, string NewPassword);

public sealed record UserInfoModel(
    int UserId,
    string UserName,
    string DisplayName,
    string Email,
    string? Bio,
    string? AvatarUrl,
    string RoleCode,
    int RoleId,
    bool IsActive,
    DateTime CreatedAt);