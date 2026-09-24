using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.UserProfile;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Pronouns { get; set; }
    public string? Location { get; set; }
    public string? AvailabilityStatus { get; set; }
    public string? ResponseSlaText { get; set; }
    public string? PercentileBadgeText { get; set; }
    public bool IsOnline { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public UserRoleInfoDto Role { get; set; } = new();
    public UserProfileMetricsDto Metrics { get; set; } = new();
    public UserRelationshipInfoDto Relationship { get; set; } = new();
    public RatingDistributionDto RatingDistribution { get; set; } = new();
    public List<UserSkillDto> Skills { get; set; } = new();
    public UserVerificationAuditDto? VerificationAudit { get; set; }
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

public class RatingDistributionDto
{
    public int Star5Count { get; set; }
    public int Star4Count { get; set; }
    public int Star3Count { get; set; }
    public int Star2Count { get; set; }
    public int Star1Count { get; set; }

    public int Star5Percentage { get; set; }
    public int Star4Percentage { get; set; }
    public int Star3Percentage { get; set; }
    public int Star2Percentage { get; set; }
    public int Star1Percentage { get; set; }
}

public class UserSkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int EndorsementCount { get; set; }
    public bool IsVerified { get; set; }
    public bool IsEndorsedByMe { get; set; }
}

public class UserVerificationAuditDto
{
    public string AuditCode { get; set; } = string.Empty;
    public string AuditTitle { get; set; } = string.Empty;
    public string AuditDescription { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Status { get; set; } = "ACTIVE";
    public DateTime AuditedAt { get; set; }
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
    public string? Headline { get; set; }
    public string? Pronouns { get; set; }
    public string? Location { get; set; }
    public string? AvailabilityStatus { get; set; }
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
    public string PostType { get; set; } = "STANDARD";
    public string? Subtitle { get; set; }
    public string? CodeSnippet { get; set; }
    public string? CodeLanguage { get; set; }
    public string? CodeFileName { get; set; }
    public string? DiagramImageUrl { get; set; }
    public string? DiagramCaption { get; set; }
    public bool HasPoll { get; set; }
    public UserPollDetailDto? Poll { get; set; }
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

public class UserPollDetailDto
{
    public int PollId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool HasVoted { get; set; }
    public int? MyVotedOptionId { get; set; }
    public List<UserPollOptionDto> Options { get; set; } = new();
}

public class UserPollOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public int VotePercentage { get; set; }
    public bool IsSelectedByMe { get; set; }
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
