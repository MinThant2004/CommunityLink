using System;

namespace CommunityLink.Shared.Features.UserProfile;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public UserRoleInfoDto Role { get; set; } = new();
    public UserProfileMetricsDto Metrics { get; set; } = new();
    public UserRelationshipInfoDto Relationship { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class UserRoleInfoDto
{
    public string RoleCode { get; set; } = "USER";
    public string RoleName { get; set; } = "Member";
    public string Badge { get; set; } = "👤";
}

public class UserProfileMetricsDto
{
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int JoinedCommunitiesCount { get; set; }
    public int SavedAccountsCount { get; set; }
    public int PostsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int SharesCount { get; set; }
}

public class UserRelationshipInfoDto
{
    public bool IsSelf { get; set; }
    public bool IsSavedByMe { get; set; }
    public bool IsFollowedByMe { get; set; }
    public bool CanMessage { get; set; }
    public bool HasRated { get; set; }
    public int? MyRatingScore { get; set; }
    public string? MyReviewText { get; set; }
}

public class UpdateUserProfileRequestDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Bio { get; set; }
}

public class RateUserRequestDto
{
    public int Score { get; set; }
    public string? ReviewText { get; set; }
}

public class UploadAvatarResponseDto
{
    public string AvatarUrl { get; set; } = string.Empty;
}

public class UserPostItemDto
{
    public int PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CommunityId { get; set; }
    public string CommunityName { get; set; } = string.Empty;
    public int AuthorUserId { get; set; }
    public string AuthorUserName { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class UserCommunityItemDto
{
    public int CommunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RoleName { get; set; } = "Member";
    public int MemberCount { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class UserRatingItemDto
{
    public int UserRatingId { get; set; }
    public int RaterUserId { get; set; }
    public string RaterUserName { get; set; } = string.Empty;
    public string RaterDisplayName { get; set; } = string.Empty;
    public string? RaterAvatarUrl { get; set; }
    public int Score { get; set; }
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SavedAccountItemDto
{
    public int SavedAccountId { get; set; }
    public int SavedUserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public DateTime SavedAt { get; set; }
}

public class FollowUserItemDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public DateTime FollowedAt { get; set; }
}

public class UserSharedPostItemDto
{
    public int ShareId { get; set; }
    public int PostId { get; set; }
    public string? ShareNote { get; set; }
    public DateTime SharedAt { get; set; }
    public string PostContent { get; set; } = string.Empty;
    public int AuthorUserId { get; set; }
    public string AuthorUserName { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string? CommunityName { get; set; }
    public string? GroupName { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public DateTime PostCreatedAt { get; set; }
}
