using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Features.Authentication;
using CommunityLink.Domain.Features.Chat;
using CommunityLink.Domain.Features.Community;
using CommunityLink.Domain.Features.Group;
using CommunityLink.Domain.Features.Poll;
using CommunityLink.Domain.Features.Post;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Dashboard;
using CommunityLink.Shared.Features.Group;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.Dashboard;

public interface IDashboardService
{
    Task<Result<UserDashboardModel>> GetUserDashboardAsync(CancellationToken cancellationToken = default);
    Task<Result<PagedResult<DirectoryPersonModel>>> GetPeopleDirectoryAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? roleFilter = null,
        string? sortOption = null,
        CancellationToken cancellationToken = default);
}

public sealed class DashboardService(
    IAuthenticationService authService,
    ICommunityService communityService,
    IGroupService groupService,
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

        // NOTE: these must stay sequential. AppDbContext and every domain service below are
        // AddScoped (FeatureManager.cs:43), so they share one DbContext instance and EF Core
        // forbids overlapping operations on it. Reducing query cost is the lever here,
        // not concurrency - see GetCommunityDirectoryAsync.
        try
        {
            var now = DateTime.UtcNow;
            var joinedRes = await communityService.GetJoinedCommunitiesAsync(userId, 10, cancellationToken);
            var recommendedRes = await communityService.GetRecommendedCommunitiesAsync(userId, 6, cancellationToken);
            var directoryRes = await communityService.GetCommunityDirectoryAsync(cancellationToken);
            var feedRes = await postService.GetFeedPostsAsync(null, null, page: 1, pageSize: 15, cancellationToken: cancellationToken);
            var pollsRes = await pollService.GetPollsAsync(null, null, cancellationToken);
            var unreadCount = await chatService.GetUnreadMessageCountAsync(cancellationToken);
            var groupsRes = await groupService.GetGroupsAsync(null, null, cancellationToken);

            var totalPeople = await dbContext.TblUsers.CountAsync(u => u.IsActive && !u.IsDeleted && u.UserId != userId, cancellationToken);
            var totalDomainPro = await dbContext.TblUsers.CountAsync(u => u.IsActive && !u.IsDeleted && u.UserId != userId && u.TblUserRoles.Any(r => !r.IsDeleted && (r.Role.RoleCode == "DOMAIN_PROFESSIONAL" || r.Role.RoleCode == "DOMAIN_PRO")), cancellationToken);
            var totalPublicFigure = await dbContext.TblUsers.CountAsync(u => u.IsActive && !u.IsDeleted && u.UserId != userId && u.TblUserRoles.Any(r => !r.IsDeleted && r.Role.RoleCode == "PUBLIC_FIGURE"), cancellationToken);

            var allCommunities = directoryRes.Data ?? [];
            var rootCommunities = allCommunities.Where(c => c.ParentCommunityId == null).ToList();
            var subCommunities = allCommunities.Where(c => c.ParentCommunityId != null).ToList();

            // Fetch followed user IDs for current user
            var followedUserIds = await dbContext.TblUserFollows
                .Where(f => f.FollowerId == userId && !f.IsDeleted)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken);

            var rawPeople = await dbContext.TblUsers
                .Include(u => u.TblUserRoles).ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted && u.UserId != userId)
                .Select(u => new DirectoryPersonModel(
                    u.UserId,
                    u.UserName,
                    u.DisplayName,
                    u.AvatarUrl,
                    u.Bio,
                    u.TblUserRoles.Where(r => !r.IsDeleted).Select(r => r.Role.RoleCode).FirstOrDefault() ?? "DEFAULT_USER",
                    u.IsVerified,
                    u.TblCommunityMembers.Count(m => !m.IsDeleted),
                    u.TblPosts.Count(p => !p.IsDeleted),
                    u.AverageRating,
                    followedUserIds.Contains(u.UserId),
                    u.Headline,
                    u.IsVerified ? u.AvailabilityStatus : null,
                    u.ResponseSlaText,
                    u.PercentileBadgeText,
                    u.RatingCount,
                    null,
                    u.TblCreatorChatSetting != null && u.TblCreatorChatSetting.IsPrivateChatEnabled,
                    u.TblCreatorChatSetting != null ? u.TblCreatorChatSetting.PrivateChatFeeLinkDrops : 0,
                    false,
                    u.IsVerified ||
                    u.TblUserRoles.Any(r => !r.IsDeleted && (r.Role.RoleCode == "DOMAIN_PRO" || r.Role.RoleCode == "DOMAIN_PROFESSIONAL" || r.Role.RoleCode == "PUBLIC_FIGURE")) ||
                    u.TblUserSubscriptions.Any(s => s.Status == "ACTIVE" && s.ExpiresAtUtc > now),
                    u.TblUserFollowFollowees.Count(f => !f.IsDeleted)))
                .ToListAsync(cancellationToken);

            // Highest followers first, then premium, then followed, then ratings
            var topPeople = rawPeople
                .OrderByDescending(u => u.FollowersCount)
                .ThenByDescending(u => u.IsPremium)
                .ThenByDescending(u => u.IsFollowedByCurrentUser)
                .ThenByDescending(u => u.AverageRating ?? 0)
                .ThenBy(u => u.DisplayName)
                .Take(20)
                .ToList();

            var topPersonIds = topPeople.Select(p => p.UserId).ToList();
            var skillsMap = new Dictionary<int, IReadOnlyList<string>>();
            var unlockedUserIds = new List<int>();

            if (topPersonIds.Count > 0)
            {
                var rawSkills = await dbContext.TblUserSkills
                    .AsNoTracking()
                    .Where(s => topPersonIds.Contains(s.UserId) && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new { s.UserId, s.SkillName })
                    .ToListAsync(cancellationToken);

                skillsMap = rawSkills
                    .GroupBy(s => s.UserId)
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(s => s.SkillName).Take(4).ToList());

                unlockedUserIds = await dbContext.TblPrivateChatPaymentTransactions
                    .AsNoTracking()
                    .Where(t => ((t.BuyerUserId == userId && topPersonIds.Contains(t.CreatorUserId)) ||
                                 (t.CreatorUserId == userId && topPersonIds.Contains(t.BuyerUserId))) &&
                                t.Status == "COMPLETED")
                    .Select(t => t.BuyerUserId == userId ? t.CreatorUserId : t.BuyerUserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }

            var people = topPeople
                .Select(p => p with
                {
                    Skills = skillsMap.TryGetValue(p.UserId, out var sk) ? sk : [],
                    IsPrivateChatUnlocked = unlockedUserIds.Contains(p.UserId)
                })
                .ToList();

            // Groups: Joined first, then pending/not joined
            var allGroups = groupsRes.Data ?? [];
            var groups = allGroups
                .OrderByDescending(g => g.IsJoined)
                .ThenByDescending(g => g.UserJoinStatus == "PENDING")
                .ThenByDescending(g => g.MemberCount)
                .ToList();

            var model = new UserDashboardModel(
                profileRes.Data,
                joinedRes.Data ?? [],
                recommendedRes.Data ?? [],
                rootCommunities,
                subCommunities,
                people,
                feedRes.Data ?? [],
                pollsRes.Data ?? [],
                unreadCount,
                groups,
                totalPeople,
                totalDomainPro,
                totalPublicFigure);

            return Result<UserDashboardModel>.Success(model);
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   cancellationToken.IsCancellationRequested ||
                                   (ex is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 0 || sqlEx.Message.Contains("cancelled", StringComparison.OrdinalIgnoreCase) || sqlEx.Message.Contains("canceled", StringComparison.OrdinalIgnoreCase))))
        {
            return Result<UserDashboardModel>.Failure("Request was canceled.", ResultStatus.SystemError);
        }
    }

    public async Task<Result<PagedResult<DirectoryPersonModel>>> GetPeopleDirectoryAsync(
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? roleFilter = null,
        string? sortOption = null,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.UserId.HasValue)
            return Result<PagedResult<DirectoryPersonModel>>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var userId = currentUser.UserId.Value;
        var now = DateTime.UtcNow;
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var followedUserIds = await dbContext.TblUserFollows
                .AsNoTracking()
                .Where(f => f.FollowerId == userId && !f.IsDeleted)
                .Select(f => f.FolloweeId)
                .ToListAsync(cancellationToken);

            var query = dbContext.TblUsers
                .AsNoTracking()
                .Where(u => u.IsActive && !u.IsDeleted && u.UserId != userId);

            // Search by name, username, headline, bio, or skill
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                query = query.Where(u =>
                    u.DisplayName.Contains(q) ||
                    u.UserName.Contains(q) ||
                    (u.Headline != null && u.Headline.Contains(q)) ||
                    (u.Bio != null && u.Bio.Contains(q)) ||
                    u.TblUserSkills.Any(s => !s.IsDeleted && s.SkillName.Contains(q)));
            }

            // Role filter
            if (!string.IsNullOrWhiteSpace(roleFilter) && !string.Equals(roleFilter, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(roleFilter, "DOMAIN_PROFESSIONAL", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(u => u.TblUserRoles.Any(r => !r.IsDeleted && (r.Role.RoleCode == "DOMAIN_PROFESSIONAL" || r.Role.RoleCode == "DOMAIN_PRO")));
                }
                else
                {
                    query = query.Where(u => u.TblUserRoles.Any(r => !r.IsDeleted && r.Role.RoleCode == roleFilter));
                }
            }

            // Sort option
            bool isFollowedSort = string.Equals(sortOption, "followed", StringComparison.OrdinalIgnoreCase);
            bool isActiveSort = string.Equals(sortOption, "active", StringComparison.OrdinalIgnoreCase);

            if (isFollowedSort)
            {
                query = query.Where(u => followedUserIds.Contains(u.UserId));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            IOrderedQueryable<TblUser> ordered;
            if (isActiveSort)
            {
                ordered = query
                    .OrderByDescending(u => u.IsVerified
                        || u.TblUserRoles.Any(r => !r.IsDeleted && (r.Role.RoleCode == "DOMAIN_PRO" || r.Role.RoleCode == "DOMAIN_PROFESSIONAL" || r.Role.RoleCode == "PUBLIC_FIGURE"))
                        || u.TblUserSubscriptions.Any(s => s.Status == "ACTIVE" && s.ExpiresAtUtc > now))
                    .ThenByDescending(u => u.AvailabilityStatus != null && !u.AvailabilityStatus.Contains("busy"))
                    .ThenByDescending(u => u.TblPosts.Count(p => !p.IsDeleted))
                    .ThenByDescending(u => u.AverageRating ?? 0)
                    .ThenBy(u => u.DisplayName)
                    .ThenBy(u => u.UserId);
            }
            else
            {
                ordered = query
                    .OrderByDescending(u => u.IsVerified
                        || u.TblUserRoles.Any(r => !r.IsDeleted && (r.Role.RoleCode == "DOMAIN_PRO" || r.Role.RoleCode == "DOMAIN_PROFESSIONAL" || r.Role.RoleCode == "PUBLIC_FIGURE"))
                        || u.TblUserSubscriptions.Any(s => s.Status == "ACTIVE" && s.ExpiresAtUtc > now))
                    .ThenByDescending(u => followedUserIds.Contains(u.UserId))
                    .ThenByDescending(u => u.AverageRating ?? 0)
                    .ThenByDescending(u => u.RatingCount)
                    .ThenBy(u => u.DisplayName)
                    .ThenBy(u => u.UserId);
            }

            var rawPeople = await ordered
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.DisplayName,
                    u.AvatarUrl,
                    u.Bio,
                    RoleCode = u.TblUserRoles.Where(r => !r.IsDeleted).Select(r => r.Role.RoleCode).FirstOrDefault() ?? "DEFAULT_USER",
                    u.IsVerified,
                    JoinedCommunitiesCount = u.TblCommunityMembers.Count(m => !m.IsDeleted),
                    PostsCount = u.TblPosts.Count(p => !p.IsDeleted),
                    u.AverageRating,
                    IsFollowedByCurrentUser = followedUserIds.Contains(u.UserId),
                    u.Headline,
                    AvailabilityStatus = u.IsVerified ? u.AvailabilityStatus : null,
                    u.ResponseSlaText,
                    u.PercentileBadgeText,
                    u.RatingCount,
                    IsPrivateChatEnabled = u.TblCreatorChatSetting != null && u.TblCreatorChatSetting.IsPrivateChatEnabled,
                    PrivateChatFeeLinkDrops = u.TblCreatorChatSetting != null ? u.TblCreatorChatSetting.PrivateChatFeeLinkDrops : 0,
                    IsPremium = u.IsVerified
                        || u.TblUserRoles.Any(r => !r.IsDeleted && (r.Role.RoleCode == "DOMAIN_PRO" || r.Role.RoleCode == "DOMAIN_PROFESSIONAL" || r.Role.RoleCode == "PUBLIC_FIGURE"))
                        || u.TblUserSubscriptions.Any(s => s.Status == "ACTIVE" && s.ExpiresAtUtc > now),
                    FollowersCount = u.TblUserFollowFollowees.Count(f => !f.IsDeleted)
                })
                .ToListAsync(cancellationToken);

            var topPersonIds = rawPeople.Select(p => p.UserId).ToList();
            var skillsMap = new Dictionary<int, IReadOnlyList<string>>();
            var unlockedUserIds = new HashSet<int>();

            if (topPersonIds.Count > 0)
            {
                var rawSkills = await dbContext.TblUserSkills
                    .AsNoTracking()
                    .Where(s => topPersonIds.Contains(s.UserId) && !s.IsDeleted)
                    .OrderBy(s => s.DisplayOrder)
                    .Select(s => new { s.UserId, s.SkillName })
                    .ToListAsync(cancellationToken);

                skillsMap = rawSkills
                    .GroupBy(s => s.UserId)
                    .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(s => s.SkillName).Take(4).ToList());

                var unlocked = await dbContext.TblPrivateChatPaymentTransactions
                    .AsNoTracking()
                    .Where(t => ((t.BuyerUserId == userId && topPersonIds.Contains(t.CreatorUserId)) ||
                                 (t.CreatorUserId == userId && topPersonIds.Contains(t.BuyerUserId))) &&
                                t.Status == "COMPLETED")
                    .Select(t => t.BuyerUserId == userId ? t.CreatorUserId : t.BuyerUserId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                unlockedUserIds = new HashSet<int>(unlocked);
            }

            var people = rawPeople
                .Select(p => new DirectoryPersonModel(
                    p.UserId,
                    p.UserName,
                    p.DisplayName,
                    p.AvatarUrl,
                    p.Bio,
                    p.RoleCode,
                    p.IsVerified,
                    p.JoinedCommunitiesCount,
                    p.PostsCount,
                    p.AverageRating,
                    p.IsFollowedByCurrentUser,
                    p.Headline,
                    p.AvailabilityStatus,
                    p.ResponseSlaText,
                    p.PercentileBadgeText,
                    p.RatingCount,
                    skillsMap.TryGetValue(p.UserId, out var sk) ? sk : [],
                    p.IsPrivateChatEnabled,
                    p.PrivateChatFeeLinkDrops,
                    unlockedUserIds.Contains(p.UserId),
                    p.IsPremium,
                    p.FollowersCount
                ))
                .ToList();

            var result = new PagedResult<DirectoryPersonModel>(people, totalCount, safePage, safePageSize);
            return Result<PagedResult<DirectoryPersonModel>>.Success(result);
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   cancellationToken.IsCancellationRequested ||
                                   (ex is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 0 || sqlEx.Message.Contains("cancelled", StringComparison.OrdinalIgnoreCase) || sqlEx.Message.Contains("canceled", StringComparison.OrdinalIgnoreCase))))
        {
            return Result<PagedResult<DirectoryPersonModel>>.Failure("Request was canceled.", ResultStatus.SystemError);
        }
    }
}
