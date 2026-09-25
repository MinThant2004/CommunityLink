using System.Collections.Generic;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Community;
using CommunityLink.Shared.Features.Group;
using CommunityLink.Shared.Features.Poll;
using CommunityLink.Shared.Features.Post;

namespace CommunityLink.Shared.Features.Dashboard;

public sealed record UserDashboardModel(
    UserInfoModel Profile,
    IReadOnlyList<CommunityModel> JoinedCommunities,
    IReadOnlyList<CommunityModel> RecommendedCommunities,
    IReadOnlyList<CommunityModel> Communities,
    IReadOnlyList<CommunityModel> SubCommunities,
    IReadOnlyList<DirectoryPersonModel> People,
    IReadOnlyList<PostModel> RecentPosts,
    IReadOnlyList<PollModel> ActivePolls,
    int UnreadMessageCount,
    IReadOnlyList<GroupModel>? Groups = null);

public sealed record DirectoryPersonModel(
    int UserId,
    string UserName,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string RoleCode,
    bool IsVerified,
    int JoinedCommunitiesCount,
    int PostsCount,
    decimal? AverageRating,
    bool IsFollowedByCurrentUser = false);
