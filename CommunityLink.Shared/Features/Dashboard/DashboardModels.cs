using System.Collections.Generic;
using CommunityLink.Shared.Features.Authentication;
using CommunityLink.Shared.Features.Community;
using CommunityLink.Shared.Features.Poll;
using CommunityLink.Shared.Features.Post;

namespace CommunityLink.Shared.Features.Dashboard;

public sealed record UserDashboardModel(
    UserInfoModel Profile,
    IReadOnlyList<CommunityModel> JoinedCommunities,
    IReadOnlyList<CommunityModel> RecommendedCommunities,
    IReadOnlyList<PostModel> RecentPosts,
    IReadOnlyList<PollModel> ActivePolls,
    int UnreadMessageCount);
