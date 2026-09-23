using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Domain.Features.Chat;
using CommunityLink.Domain.Features.Community;
using CommunityLink.Domain.Features.Poll;
using CommunityLink.Domain.Features.Post;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Dashboard;
using Microsoft.EntityFrameworkCore;

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
    ICurrentUserContext currentUser,
    AppDbContext dbContext) : IDashboardService
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
        var allCommunitiesRes = await communityService.GetCommunitiesAsync(null, cancellationToken);
        var feedRes = await postService.GetFeedPostsAsync(null, null, cancellationToken);
        var pollsRes = await pollService.GetPollsAsync(null, null, cancellationToken);
        var unreadCount = await chatService.GetUnreadMessageCountAsync(cancellationToken);

        var allCommunities = allCommunitiesRes.Data ?? [];
        var rootCommunities = allCommunities.Where(c => c.ParentCommunityId == null).ToList();
        var subCommunities = allCommunities.Where(c => c.ParentCommunityId != null).ToList();

        var people = await dbContext.TblUsers
            .Include(u => u.TblUserRoles).ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .Where(u => u.IsActive && !u.IsDeleted && u.UserId != userId)
            .OrderByDescending(u => u.AverageRating ?? 0)
            .Take(9)
            .Select(u => new DirectoryPersonModel(
                u.UserId,
                u.UserName,
                u.DisplayName,
                u.AvatarUrl,
                u.Bio,
                u.TblUserRoles.Select(r => r.Role.RoleCode).FirstOrDefault() ?? "MEMBER",
                u.IsVerified,
                u.TblCommunityMembers.Count(m => !m.IsDeleted),
                u.TblPosts.Count(p => !p.IsDeleted),
                u.AverageRating))
            .ToListAsync(cancellationToken);

        var model = new UserDashboardModel(
            profileRes.Data,
            joinedRes.Data ?? [],
            recommendedRes.Data ?? [],
            rootCommunities,
            subCommunities,
            people,
            feedRes.Data ?? [],
            pollsRes.Data ?? [],
            unreadCount);

        return Result<UserDashboardModel>.Success(model);
    }
}
