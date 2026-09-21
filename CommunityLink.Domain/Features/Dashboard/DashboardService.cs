using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Domain.Features.Chat;
using CommunityLink.Domain.Features.Community;
using CommunityLink.Domain.Features.Poll;
using CommunityLink.Domain.Features.Post;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Dashboard;

namespace CommunityLink.Domain.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<UserDashboardModel>> GetUserDashboardAsync(CancellationToken cancellationToken = default);
}

public sealed class DashboardService(
    IAuthenticationService authService,
    ICommunityService communityService,
    IPostService postService,
    IPollService pollService,
    IChatService chatService,
    ICurrentUserContext currentUser) : IDashboardService
{
    public async Task<Result<UserDashboardModel>> GetUserDashboardAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUser.UserId.HasValue)
            return Result<UserDashboardModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var userId = currentUser.UserId.Value;

        var profileRes = await authService.GetCurrentUserAsync(userId);
        if (!profileRes.IsSuccess || profileRes.Data is null)
            return Result<UserDashboardModel>.Failure(profileRes.Message, profileRes.Status);

        var joinedRes = await communityService.GetJoinedCommunitiesAsync(userId, 10, cancellationToken);
        var recommendedRes = await communityService.GetRecommendedCommunitiesAsync(userId, 6, cancellationToken);
        var feedRes = await postService.GetFeedPostsAsync(null, null, cancellationToken);
        var pollsRes = await pollService.GetPollsAsync(null, null, cancellationToken);
        var unreadCount = await chatService.GetUnreadMessageCountAsync(cancellationToken);

        var model = new UserDashboardModel(
            profileRes.Data,
            joinedRes.Data ?? [],
            recommendedRes.Data ?? [],
            feedRes.Data ?? [],
            pollsRes.Data ?? [],
            unreadCount);

        return Result<UserDashboardModel>.Success(model);
    }
}
