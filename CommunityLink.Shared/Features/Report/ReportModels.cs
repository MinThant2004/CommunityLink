using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.Report;

public sealed record SubCommunityReportModel(
    int SubCommunityId,
    string Name,
    string? Description,
    int GroupCount,
    int MemberCount,
    DateTime CreatedAt);

public sealed record CommunityReportItemModel(
    int CommunityId,
    string Name,
    string? Description,
    string? Category,
    int SubCommunityCount,
    int GroupCount,
    int MemberCount,
    DateTime CreatedAt,
    IReadOnlyList<SubCommunityReportModel> SubCommunities);

public sealed record UserReportItemModel(
    int UserId,
    string UserName,
    string DisplayName,
    string? Email,
    string? AvatarUrl,
    int FollowersCount,
    int FollowingCount,
    int PostCount,
    int PollCount,
    DateTime CreatedAt);

public sealed record UserTopPostModel(
    int PostId,
    string Content,
    string? MediaUrl,
    int LikeCount,
    int CommentCount,
    DateTime CreatedAt,
    string? GroupName,
    string? CommunityName,
    bool HasPoll);

public sealed record UserTopPollModel(
    int PollId,
    string Question,
    int TotalVotes,
    int LikeCount,
    DateTime CreatedAt,
    string? GroupName,
    IReadOnlyList<string> Options);

public sealed record UserTopContentModel(
    UserReportItemModel User,
    IReadOnlyList<UserTopPostModel> TopPosts,
    IReadOnlyList<UserTopPollModel> TopPolls);
