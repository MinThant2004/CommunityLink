namespace CommunityLink.Shared.Features.Authentication;

public sealed record LoginRequestModel(string Email, string Password);
public sealed record LoginResponseModel(
    string AccessToken,
    string RefreshToken,
    int UserId,
    string FullName,
    string Email,
    string RoleCode,
    int RoleId,
    bool MustChangePassword,
    IReadOnlyList<string> Permissions);

public sealed record RegisterRequestModel(string FullName, string UserName, string Email, string Password, string? Bio);
public sealed record ChangePasswordRequestModel(string CurrentPassword, string NewPassword);
public sealed record LogoutRequestModel;

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