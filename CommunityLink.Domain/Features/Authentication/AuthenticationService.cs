using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Domain.Features.Authentication;

public interface IAuthenticationService
{
    Task<Result<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<UserInfoModel>> RegisterAsync(RegisterRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(ChangePasswordRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<UserInfoModel>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}

public sealed class AuthenticationService(
    AppDbContext dbContext,
    ITokenIssuer tokenIssuer,
    IPermissionEvaluator permissionEvaluator,
    ICurrentUserContext currentUser) : IAuthenticationService
{
    public async Task<Result<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var user = await dbContext.TblUsers
            .Include(u => u.TblUserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponseModel>.Failure("Invalid email or password.", ResultStatus.Unauthorized);
        }

        if (!user.IsActive)
        {
            return Result<LoginResponseModel>.Failure("Account is deactivated.", ResultStatus.Forbidden);
        }

        var primaryRole = user.TblUserRoles.FirstOrDefault()?.Role;
        var roleCode = primaryRole?.RoleCode ?? "MEMBER";
        var roleId = primaryRole?.RoleId ?? 0;
        var sessionId = Guid.NewGuid().ToString("N");

        var token = tokenIssuer.GenerateToken(user.UserId, user.Email, user.DisplayName, roleCode, roleId, sessionId);
        var permissions = await permissionEvaluator.GetPermissionsForRoleAsync(roleId, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseModel(
            token,
            Guid.NewGuid().ToString("N"),
            user.UserId,
            user.DisplayName,
            user.Email,
            roleCode,
            roleId,
            false,
            permissions);

        return Result<LoginResponseModel>.Success(response);
    }

    public async Task<Result<UserInfoModel>> RegisterAsync(RegisterRequestModel request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var normalizedUserName = request.UserName.Trim().ToUpperInvariant();

        if (await dbContext.TblUsers.AnyAsync(u => u.NormalizedEmail == normalizedEmail || u.NormalizedUserName == normalizedUserName, cancellationToken))
        {
            return Result<UserInfoModel>.Failure("An account with this email or username already exists.", ResultStatus.Conflict);
        }

        var memberRole = await dbContext.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "MEMBER" && !r.IsDeleted, cancellationToken);
        if (memberRole is null) return Result<UserInfoModel>.Failure("Default member role not configured.", ResultStatus.SystemError);

        var newUser = new TblUser
        {
            DisplayName = request.FullName.Trim(),
            UserName = request.UserName.Trim(),
            NormalizedUserName = normalizedUserName,
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Bio = request.Bio,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblUsers.Add(newUser);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.TblUserRoles.Add(new TblUserRole
        {
            UserId = newUser.UserId,
            RoleId = memberRole.RoleId,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new UserInfoModel(
            newUser.UserId,
            newUser.UserName,
            newUser.DisplayName,
            newUser.Email,
            newUser.Bio,
            newUser.AvatarUrl,
            memberRole.RoleCode,
            memberRole.RoleId,
            newUser.IsActive,
            newUser.CreatedAt);

        return Result<UserInfoModel>.Success(response, "Account registered successfully.");
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var user = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        if (user is null || user.IsDeleted) return Result.Failure("User not found.", ResultStatus.NotFound);

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure("Current password is incorrect.", ResultStatus.ValidationError);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 12);
        user.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Password changed successfully.");
    }

    public async Task<Result<UserInfoModel>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<UserInfoModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var user = await dbContext.TblUsers
            .Include(u => u.TblUserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == currentUser.UserId.Value && !u.IsDeleted, cancellationToken);

        if (user is null) return Result<UserInfoModel>.Failure("User not found.", ResultStatus.NotFound);

        var primaryRole = user.TblUserRoles.FirstOrDefault()?.Role;
        var response = new UserInfoModel(
            user.UserId,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.Bio,
            user.AvatarUrl,
            primaryRole?.RoleCode ?? "MEMBER",
            primaryRole?.RoleId ?? 0,
            user.IsActive,
            user.CreatedAt);

        return Result<UserInfoModel>.Success(response);
    }
}