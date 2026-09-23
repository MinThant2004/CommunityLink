using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Domain.Security;
using CommunityLink.Domain.Services;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CommunityLink.Domain.Features.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private const string MemberRoleCode = "MEMBER";
    private const string AdminRoleCode = "ADMIN";
    private const string SuperAdminRoleCode = "SUPERADMIN";
    private const int MinPasswordLength = 8;

    private static readonly Regex UserNamePattern = new(@"^[a-zA-Z0-9_]{3,30}$", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex OtpPattern = new(@"^[0-9]{6}$", RegexOptions.Compiled);
    private const int OtpLifetimeMinutes = 1;
    private const string OtpGenericMessage = "If an account exists for this email, a one-time password has been sent.";

    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ITokenIssuer _tokenIssuer;
    private readonly IPermissionEvaluator _permissionEvaluator;
    private readonly IEmailSender _emailSender;

    public AuthenticationService(
        AppDbContext dbContext,
        IConfiguration configuration,
        ITokenIssuer tokenIssuer,
        IPermissionEvaluator permissionEvaluator,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _tokenIssuer = tokenIssuer;
        _permissionEvaluator = permissionEvaluator;
        _emailSender = emailSender;
    }

    public async Task<Result<LoginResponseModel>> RegisterUserAsync(RegisterRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<LoginResponseModel>.Failure("Email and Password are required.", ResultStatus.ValidationError);

        if (!EmailPattern.IsMatch(request.Email))
            return Result<LoginResponseModel>.Failure("Please enter a valid email address.", ResultStatus.ValidationError);

        if (string.IsNullOrWhiteSpace(request.UserName))
            return Result<LoginResponseModel>.Failure("Username is required.", ResultStatus.ValidationError);

        if (!UserNamePattern.IsMatch(request.UserName))
            return Result<LoginResponseModel>.Failure("Username must be 3-30 characters and contain only letters, numbers or underscores.", ResultStatus.ValidationError);

        if (request.Password.Length < MinPasswordLength)
            return Result<LoginResponseModel>.Failure($"Password must be at least {MinPasswordLength} characters long.", ResultStatus.ValidationError);

        var confirmPassword = string.IsNullOrWhiteSpace(request.ConfirmPassword) ? request.Password : request.ConfirmPassword;
        if (request.Password != confirmPassword)
            return Result<LoginResponseModel>.Failure("Passwords do not match.", ResultStatus.ValidationError);

        var normalizedEmail = request.Email.ToUpperInvariant();
        var normalizedUserName = request.UserName.ToUpperInvariant();

        if (await _dbContext.TblUsers.AnyAsync(u => u.NormalizedEmail == normalizedEmail))
            return Result<LoginResponseModel>.Failure("An account with this email is already registered.", ResultStatus.Conflict);

        if (await _dbContext.TblUsers.AnyAsync(u => u.NormalizedUserName == normalizedUserName))
            return Result<LoginResponseModel>.Failure("This username is already taken.", ResultStatus.Conflict);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var user = new TblUser
        {
            UserName = request.UserName,
            NormalizedUserName = normalizedUserName,
            Email = request.Email,
            NormalizedEmail = normalizedEmail,
            DisplayName = string.IsNullOrWhiteSpace(request.FullName) ? request.UserName : request.FullName,
            PasswordHash = passwordHash,
            Bio = request.Bio,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.TblUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        var (roleCode, roleId) = await ResolveMemberRoleAsync();
        _dbContext.TblUserRoles.Add(new TblUserRole
        {
            UserId = user.UserId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var response = await BuildLoginResponseAsync(user.UserId, user.Email, user.DisplayName, roleCode, roleId);
        return Result<LoginResponseModel>.Success(response, "Account created successfully.");
    }

    public async Task<Result<LoginResponseModel>> RegisterAdminAsync(RegisterAdminRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<LoginResponseModel>.Failure("Email and Password are required.", ResultStatus.ValidationError);

        if (!EmailPattern.IsMatch(request.Email))
            return Result<LoginResponseModel>.Failure("Please enter a valid email address.", ResultStatus.ValidationError);

        if (request.Password.Length < MinPasswordLength)
            return Result<LoginResponseModel>.Failure($"Password must be at least {MinPasswordLength} characters long.", ResultStatus.ValidationError);

        var confirmPassword = string.IsNullOrWhiteSpace(request.ConfirmPassword) ? request.Password : request.ConfirmPassword;
        if (request.Password != confirmPassword)
            return Result<LoginResponseModel>.Failure("Passwords do not match.", ResultStatus.ValidationError);

        var validInviteCode = _configuration["Admin:InviteCode"] ?? "ADMIN123";
        if (!string.Equals(request.AdminInviteCode, validInviteCode, StringComparison.Ordinal))
            return Result<LoginResponseModel>.Failure("Invalid Admin Invite Code.", ResultStatus.Forbidden);

        var normalizedEmail = request.Email.ToUpperInvariant();
        if (await _dbContext.TblAdmins.AnyAsync(a => a.NormalizedEmail == normalizedEmail))
            return Result<LoginResponseModel>.Failure("An admin with this email already exists.", ResultStatus.Conflict);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        var admin = new TblAdmin
        {
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Email : request.FullName,
            Email = request.Email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHash,
            IsSuperAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.TblAdmins.Add(admin);
        await _dbContext.SaveChangesAsync();

        var (roleCode, roleId) = await ResolveAdminRoleAsync(admin.IsSuperAdmin);
        _dbContext.TblAdminRoles.Add(new TblAdminRole
        {
            AdminId = admin.AdminId,
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var response = await BuildLoginResponseAsync(admin.AdminId, admin.Email, admin.FullName, roleCode, roleId);
        return Result<LoginResponseModel>.Success(response, "Admin account registered successfully.");
    }

    public async Task<Result<LoginResponseModel>> LoginUserAsync(LoginRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<LoginResponseModel>.Failure("Email and Password are required.", ResultStatus.ValidationError);

        var user = await _dbContext.TblUsers
            .Include(u => u.TblUserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == request.Email.ToUpperInvariant());

        if (user == null || !user.IsActive || user.IsDeleted)
            return Result<LoginResponseModel>.Failure("Invalid email or password.", ResultStatus.Unauthorized);

        bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isValidPassword)
            return Result<LoginResponseModel>.Failure("Invalid email or password.", ResultStatus.Unauthorized);

        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var roleMapping = user.TblUserRoles.FirstOrDefault(ur => !ur.IsDeleted);
        var (roleCode, roleId) = roleMapping?.Role != null
            ? (roleMapping.Role.RoleCode, roleMapping.RoleId)
            : await ResolveMemberRoleAsync();

        var response = await BuildLoginResponseAsync(user.UserId, user.Email, user.DisplayName, roleCode, roleId, request.RememberMe);
        return Result<LoginResponseModel>.Success(response, "Login successful.");
    }

    public async Task<Result<LoginResponseModel>> LoginAdminAsync(LoginRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<LoginResponseModel>.Failure("Email and Password are required.", ResultStatus.ValidationError);

        var normalizedEmail = request.Email.ToUpperInvariant();

        var admin = await _dbContext.TblAdmins
            .Include(a => a.TblAdminRoles)
            .ThenInclude(ar => ar.Role)
            .FirstOrDefaultAsync(a => a.NormalizedEmail == normalizedEmail);

        if (admin != null && admin.IsActive && !admin.IsDeleted)
        {
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);
            if (!isValidPassword)
                return Result<LoginResponseModel>.Failure("Invalid admin credentials.", ResultStatus.Unauthorized);

            admin.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var roleMapping = admin.TblAdminRoles.FirstOrDefault(ar => !ar.IsDeleted);
            var roleCode = admin.IsSuperAdmin ? SuperAdminRoleCode : (roleMapping?.Role?.RoleCode ?? AdminRoleCode);
            var roleId = admin.IsSuperAdmin
                ? (roleMapping?.RoleId ?? (await ResolveAdminRoleAsync(true)).RoleId)
                : (roleMapping?.RoleId ?? (await ResolveAdminRoleAsync(false)).RoleId);

            var response = await BuildLoginResponseAsync(admin.AdminId, admin.Email, admin.FullName, roleCode, roleId, request.RememberMe);
            return Result<LoginResponseModel>.Success(response, "Admin login successful.");
        }

        var user = await _dbContext.TblUsers
            .Include(u => u.TblUserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

        if (user != null && user.IsActive && !user.IsDeleted)
        {
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isValidPassword)
                return Result<LoginResponseModel>.Failure("Invalid admin credentials.", ResultStatus.Unauthorized);

            var adminRoleId = (await ResolveAdminRoleAsync(false)).RoleId;
            var adminMapping = user.TblUserRoles.FirstOrDefault(ur => !ur.IsDeleted && ur.RoleId == adminRoleId);
            if (adminMapping == null)
                return Result<LoginResponseModel>.Failure("This account does not have administrator privileges.", ResultStatus.Unauthorized);

            user.LastLoginAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var response = await BuildLoginResponseAsync(user.UserId, user.Email, user.DisplayName, AdminRoleCode, adminRoleId, request.RememberMe);
            return Result<LoginResponseModel>.Success(response, "Admin login successful.");
        }

        return Result<LoginResponseModel>.Failure("Invalid admin credentials.", ResultStatus.Unauthorized);
    }

    public async Task<Result<UserInfoModel>> GetCurrentUserAsync(int userId)
    {
        var user = await _dbContext.TblUsers
            .Include(u => u.TblUserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user != null)
        {
            var role = user.TblUserRoles.FirstOrDefault(ur => !ur.IsDeleted)?.Role
                       ?? await _dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == MemberRoleCode);

            var userInfo = new UserInfoModel(
                user.UserId,
                user.UserName,
                user.DisplayName,
                user.Email,
                user.Bio,
                user.AvatarUrl,
                role?.RoleCode ?? MemberRoleCode,
                role?.RoleId ?? 0,
                user.IsActive,
                user.CreatedAt);

            return Result<UserInfoModel>.Success(userInfo, "Current user retrieved successfully.");
        }

        var admin = await _dbContext.TblAdmins
            .Include(a => a.TblAdminRoles)
            .ThenInclude(ar => ar.Role)
            .FirstOrDefaultAsync(a => a.AdminId == userId && !a.IsDeleted);

        if (admin != null)
        {
            var role = admin.TblAdminRoles.FirstOrDefault(ar => !ar.IsDeleted)?.Role
                       ?? await _dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == AdminRoleCode);

            var adminInfo = new UserInfoModel(
                admin.AdminId,
                admin.Email,
                admin.FullName,
                admin.Email,
                null,
                null,
                admin.IsSuperAdmin ? SuperAdminRoleCode : (role?.RoleCode ?? AdminRoleCode),
                role?.RoleId ?? 0,
                admin.IsActive,
                admin.CreatedAt);

            return Result<UserInfoModel>.Success(adminInfo, "Current user retrieved successfully.");
        }

        return Result<UserInfoModel>.Failure("Account not found.", ResultStatus.NotFound);
    }

    public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return Result<bool>.Failure("Current and New Password are required.", ResultStatus.ValidationError);

        if (request.CurrentPassword == request.NewPassword)
            return Result<bool>.Failure("New password must be different from the current password.", ResultStatus.ValidationError);

        if (request.NewPassword.Length < MinPasswordLength)
            return Result<bool>.Failure($"Password must be at least {MinPasswordLength} characters long.", ResultStatus.ValidationError);

        var user = await _dbContext.TblUsers.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
        if (user != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("Current password is incorrect.", ResultStatus.ValidationError);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return Result<bool>.Success(true, "Password changed successfully.");
        }

        var admin = await _dbContext.TblAdmins.FirstOrDefaultAsync(a => a.AdminId == userId && !a.IsDeleted);
        if (admin != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, admin.PasswordHash))
                return Result<bool>.Failure("Current password is incorrect.", ResultStatus.ValidationError);

            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
            admin.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return Result<bool>.Success(true, "Password changed successfully.");
        }

        return Result<bool>.Failure("Account not found.", ResultStatus.NotFound);
    }

    public async Task<Result<bool>> SendPasswordResetOtpAsync(SendOtpRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !EmailPattern.IsMatch(request.Email))
            return Result<bool>.Failure("Please enter a valid email address.", ResultStatus.ValidationError);

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        // Do not reveal whether an account exists for this email.
        bool userExists = await _dbContext.TblUsers.AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.IsActive && !u.IsDeleted);
        bool adminExists = await _dbContext.TblAdmins.AnyAsync(a => a.NormalizedEmail == normalizedEmail && a.IsActive && !a.IsDeleted);

        if (!userExists && !adminExists)
            return Result<bool>.Success(true, OtpGenericMessage);

        // Invalidate any previously issued, unused OTPs so only the latest one works.
        var outstanding = await _dbContext.TblPasswordResetOtps
            .Where(o => o.Email == normalizedEmail && !o.IsUsed)
            .ToListAsync();

        foreach (var record in outstanding)
        {
            record.IsUsed = true;
        }

        string otpCode = GenerateOtp();

        var otpRecord = new TblPasswordResetOtp
        {
            Email = normalizedEmail,
            OtpCode = ComputeOtpHash(otpCode),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(OtpLifetimeMinutes),
            IsUsed = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.TblPasswordResetOtps.Add(otpRecord);
        await _dbContext.SaveChangesAsync();

        var emailSubject = "Your Community Link Password Reset OTP";
        var emailBody = $@"
            <div style=""font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;"">
                <h2 style=""color: #2563eb; text-align: center;"">Community Link</h2>
                <p>Hello,</p>
                <p>You requested a password reset for your Community Link account. Use the 6-digit OTP code below to verify your request:</p>
                <div style=""background-color: #f3f4f6; text-align: center; padding: 15px; border-radius: 6px; margin: 20px 0;"">
                    <span style=""font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #1e293b;"">{otpCode}</span>
                </div>
                <p style=""color: #ef4444; font-size: 13px;""><strong>Note:</strong> This code will expire in 1 minute.</p>
                <p style=""color: #64748b; font-size: 12px; margin-top: 30px;"">If you did not request this password reset, please ignore this email.</p>
            </div>";

        try
        {
            await _emailSender.SendEmailAsync(request.Email, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send email: {ex.Message}");
        }

        return Result<bool>.Success(true, OtpGenericMessage);
    }

    public async Task<Result<bool>> VerifyPasswordResetOtpAsync(VerifyOtpRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.OtpCode) || !OtpPattern.IsMatch(request.OtpCode))
            return Result<bool>.Failure("Enter the 6-digit code from your email.", ResultStatus.ValidationError);

        var otpHash = ComputeOtpHash(request.OtpCode);

        var otpRecord = await _dbContext.TblPasswordResetOtps
            .Where(o => o.OtpCode == otpHash && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (otpRecord == null)
            return Result<bool>.Failure("The OTP is invalid or has already been used.", ResultStatus.ValidationError);

        if (DateTime.UtcNow > otpRecord.ExpiresAtUtc)
            return Result<bool>.Failure("This OTP has expired. Please click 'Resend OTP' to get a new code.", ResultStatus.ValidationError);

        // Record successful verification server-side. The 1-minute lifetime applies
        // only to entering the OTP; once verified, the user may set a new password.
        otpRecord.VerifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true, "OTP verified successfully.");
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.OtpCode) || !OtpPattern.IsMatch(request.OtpCode))
            return Result<bool>.Failure("Enter the 6-digit code from your email.", ResultStatus.ValidationError);

        var passwordError = string.Empty;
        if (string.IsNullOrWhiteSpace(request.NewPassword) || !IsValidPassword(request.NewPassword, out passwordError))
            return Result<bool>.Failure(passwordError, ResultStatus.ValidationError);

        var otpHash = ComputeOtpHash(request.OtpCode);

        var otpRecord = await _dbContext.TblPasswordResetOtps
            .Where(o => o.OtpCode == otpHash && !o.IsUsed)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (otpRecord == null)
            return Result<bool>.Failure("The OTP is invalid or has already been used.", ResultStatus.ValidationError);

        if (!otpRecord.VerifiedAt.HasValue)
            return Result<bool>.Failure("Verify your one-time password before resetting your password.", ResultStatus.ValidationError);

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);

        var user = await _dbContext.TblUsers.FirstOrDefaultAsync(u => u.NormalizedEmail == otpRecord.Email && !u.IsDeleted);
        if (user != null)
        {
            user.PasswordHash = newPasswordHash;
            user.UpdatedAt = DateTime.UtcNow;
            otpRecord.IsUsed = true;
            await _dbContext.SaveChangesAsync();
            return Result<bool>.Success(true, "Your password has been reset. You can now sign in.");
        }

        var admin = await _dbContext.TblAdmins.FirstOrDefaultAsync(a => a.NormalizedEmail == otpRecord.Email && !a.IsDeleted);
        if (admin != null)
        {
            admin.PasswordHash = newPasswordHash;
            admin.UpdatedAt = DateTime.UtcNow;
            otpRecord.IsUsed = true;
            await _dbContext.SaveChangesAsync();
            return Result<bool>.Success(true, "Your password has been reset. You can now sign in.");
        }

        return Result<bool>.Failure("This account is no longer available.", ResultStatus.NotFound);
    }

    private static bool IsValidPassword(string password, out string error)
    {
        error = string.Empty;

        if (password.Length < MinPasswordLength)
        {
            error = $"Password must be at least {MinPasswordLength} characters long.";
            return false;
        }
        if (!password.Any(char.IsUpper))
        {
            error = "Password must contain at least one uppercase letter.";
            return false;
        }
        if (!password.Any(char.IsLower))
        {
            error = "Password must contain at least one lowercase letter.";
            return false;
        }
        if (!password.Any(char.IsDigit))
        {
            error = "Password must contain at least one number.";
            return false;
        }

        return true;
    }

    private static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
    }

    private static string ComputeOtpHash(string otp)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(otp);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private async Task<(string RoleCode, int RoleId)> ResolveMemberRoleAsync()
    {
        var role = await _dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "USER" && !r.IsDeleted)
                   ?? await _dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == MemberRoleCode && !r.IsDeleted);
        return (role?.RoleCode ?? "USER", role?.RoleId ?? 3);
    }

    private async Task<(string RoleCode, int RoleId)> ResolveAdminRoleAsync(bool isSuperAdmin)
    {
        var role = await _dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == AdminRoleCode);
        return (isSuperAdmin ? SuperAdminRoleCode : AdminRoleCode, role?.RoleId ?? 1);
    }

    private async Task<LoginResponseModel> BuildLoginResponseAsync(
        int userId,
        string email,
        string fullName,
        string roleCode,
        int roleId,
        bool rememberMe = false)
    {
        var sessionId = Guid.NewGuid().ToString("N");
        var token = _tokenIssuer.GenerateToken(userId, email, fullName, roleCode, roleId, sessionId,
            expiresInMinutes: rememberMe ? (int)TimeSpan.FromDays(30).TotalMinutes : null);
        var permissions = await _permissionEvaluator.GetPermissionsForRoleAsync(roleId);

        return new LoginResponseModel(
            AccessToken: token,
            RefreshToken: GenerateRefreshToken(),
            UserId: userId,
            FullName: fullName,
            Email: email,
            RoleCode: roleCode,
            RoleId: roleId,
            MustChangePassword: false,
            Permissions: permissions,
            SessionId: sessionId);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}