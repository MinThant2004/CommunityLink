using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Authentication;

namespace CommunityLink.Domain.Features.Authentication;

public interface IAuthenticationService
{
    Task<Result<LoginResponseModel>> RegisterUserAsync(RegisterRequestModel request);
    Task<Result<LoginResponseModel>> RegisterAdminAsync(RegisterAdminRequestModel request);
    Task<Result<LoginResponseModel>> LoginUserAsync(LoginRequestModel request);
    Task<Result<LoginResponseModel>> LoginAdminAsync(LoginRequestModel request);
    Task<Result<UserInfoModel>> GetCurrentUserAsync(int userId);
    Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestModel request);
    Task<Result<bool>> SendPasswordResetOtpAsync(SendOtpRequestModel request);
    Task<Result<bool>> VerifyPasswordResetOtpAsync(VerifyOtpRequestModel request);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestModel request);
}