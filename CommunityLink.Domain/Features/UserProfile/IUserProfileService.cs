using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.UserProfile;

namespace CommunityLink.Domain.Features.UserProfile;

public interface IUserProfileService
{
    Task<Result<UserProfileDto>> GetOwnerProfileAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> GetPublicProfileAsync(string userNameOrId, int? currentUserId, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(int currentUserId, UpdateUserProfileRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result<UploadAvatarResponseDto>> UploadAvatarAsync(int currentUserId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleSaveAccountAsync(int currentUserId, int targetUserId, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> RateUserAsync(int currentUserId, int targetUserId, RateUserRequestDto dto, CancellationToken cancellationToken = default);
    Task<Result<List<UserPostItemDto>>> GetUserPostsAsync(int targetUserId, int? currentUserId, CancellationToken cancellationToken = default);
    Task<Result<List<UserPostItemDto>>> GetSavedPostsAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<Result<List<SavedAccountItemDto>>> GetSavedAccountsAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<Result<List<UserCommunityItemDto>>> GetUserCommunitiesAsync(int targetUserId, CancellationToken cancellationToken = default);
    Task<Result<List<UserRatingItemDto>>> GetUserReviewsAsync(int targetUserId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleFollowUserAsync(int currentUserId, int targetUserId, CancellationToken cancellationToken = default);
    Task<Result<List<FollowUserItemDto>>> GetFollowersAsync(int targetUserId, CancellationToken cancellationToken = default);
    Task<Result<List<FollowUserItemDto>>> GetFollowingAsync(int targetUserId, CancellationToken cancellationToken = default);
    Task<Result<List<UserSharedPostItemDto>>> GetUserSharesAsync(int targetUserId, int? currentUserId, CancellationToken cancellationToken = default);
    Task<Result<bool>> EndorseSkillAsync(int currentUserId, int skillId, CancellationToken cancellationToken = default);
    Task<Result<bool>> VotePollAsync(int currentUserId, int pollId, int optionId, CancellationToken cancellationToken = default);
}
