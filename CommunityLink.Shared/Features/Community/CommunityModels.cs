namespace CommunityLink.Shared.Features.Community;

public sealed record CommunityModel(
    int CommunityId,
    string Name,
    string Slug,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    string Visibility,
    string JoinPolicy,
    int MemberCount,
    int PostCount,
    double AverageRating,
    int OwnerId,
    string OwnerName,
    DateTime CreatedAt,
    int? ParentCommunityId = null,
    string? ParentCommunityName = null);

public sealed record CreateCommunityRequestModel(
    string Name,
    string? Slug,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    string Visibility,
    string JoinPolicy,
    int? ParentCommunityId = null);

public sealed record UpdateCommunityRequestModel(
    int CommunityId,
    string Name,
    string? Description,
    string? AvatarUrl,
    string? BannerUrl,
    string Visibility,
    string JoinPolicy);

public sealed record EditCommunityRequestModel(
    string Name,
    string? Description);

public sealed record CommunityAuditModel(
    int AuditId,
    int CommunityId,
    string TargetType,
    string FieldChanged,
    string? OldValue,
    string? NewValue,
    int EditorId,
    string EditorName,
    DateTime CreatedAt);

public sealed record CommunityMemberModel(
    int CommunityMemberId,
    int CommunityId,
    int UserId,
    string UserName,
    string DisplayName,
    string? UserAvatar,
    string Role,
    DateTime JoinedAt);

public sealed record JoinRequestModel(int CommunityId, string? Message);