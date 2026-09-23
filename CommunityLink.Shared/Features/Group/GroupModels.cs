namespace CommunityLink.Shared.Features.Group;

public sealed record GroupModel(
    int GroupId,
    int SubCommunityId,
    string? SubCommunityName,
    int CreatorId,
    string CreatorName,
    string Name,
    string Slug,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    string Visibility,
    string JoinPolicy,
    int MemberCount,
    int PostCount,
    DateTime CreatedAt,
    bool IsJoined = false,
    string UserJoinStatus = "NONE", // NONE | PENDING | JOINED
    double? AverageRating = null,
    int RatingCount = 0
);

public sealed record GroupRatingDto(
    int GroupRatingId,
    int GroupId,
    int UserId,
    string UserName,
    string DisplayName,
    string? UserAvatar,
    int Score,
    string? ReviewText,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record SubmitGroupRatingRequestModel(
    int Score,
    string? ReviewText
);

public sealed record GroupRatingSummaryDto(
    double AverageScore,
    int TotalRatings,
    GroupRatingDto? UserRating,
    IReadOnlyList<GroupRatingDto> Ratings
);

public sealed record CreateGroupRequestModel(
    int SubCommunityId,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    string Visibility, // PUBLIC | PRIVATE
    string JoinPolicy // INSTANT | APPROVAL_REQUIRED
);

public sealed record UpdateGroupRequestModel(
    int GroupId,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    string Visibility,
    string JoinPolicy
);

public sealed record GroupMemberModel(
    int GroupMemberId,
    int GroupId,
    int UserId,
    string UserName,
    string DisplayName,
    string? UserAvatar,
    string Role, // Owner | Admin | Member
    DateTime JoinedAt
);

public sealed record GroupJoinRequestModel(
    int GroupJoinRequestId,
    int GroupId,
    string GroupName,
    int UserId,
    string UserName,
    string UserEmail,
    string? RequestNote,
    string Status, // PENDING | APPROVED | REJECTED
    DateTime CreatedAt
);
