using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;
using CommunityLink.Shared.Features.Poll;
using CommunityLink.Domain.Features.Notification;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Domain.Features.Activity;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Post;

public interface IPostService
{
    Task<Result<IReadOnlyList<HomeFeedItemModel>>> GetHomeFeedAsync(int page = 1, int pageSize = 15, DateTime? sessionTime = null, string? search = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, int? groupId = null, int page = 1, int pageSize = 15, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> UpdatePostAsync(int postId, UpdatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<bool>> TogglePostPrivacyAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result> SharePostAsync(int postId, SharePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleSavePostAsync(int postId, CancellationToken cancellationToken = default);
}

public sealed class PostService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    INotificationService notificationService,
    IPermissionEvaluator permissionEvaluator,
    IUserActivityService userActivityService) : IPostService
{
    public async Task<Result<IReadOnlyList<HomeFeedItemModel>>> GetHomeFeedAsync(int page = 1, int pageSize = 15, DateTime? sessionTime = null, string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = currentUser.UserId;
            var effectiveSessionTime = sessionTime ?? DateTime.UtcNow;
            var safePageSize = Math.Clamp(pageSize, 1, 50);
            var safePage = Math.Max(1, page);

            List<int> followedAuthorIds = [];
            List<int> joinedGroupIds = [];
            List<int> joinedCommunityIds = [];
            if (currentUserId.HasValue)
            {
                followedAuthorIds = await dbContext.TblUserFollows
                    .Where(f => f.FollowerId == currentUserId.Value && !f.IsDeleted)
                    .Select(f => f.FolloweeId)
                    .ToListAsync(cancellationToken);

                joinedGroupIds = await dbContext.TblGroupMembers
                    .Where(m => m.UserId == currentUserId.Value && !m.IsDeleted)
                    .Select(m => m.GroupId)
                    .ToListAsync(cancellationToken);

                joinedCommunityIds = await dbContext.TblCommunityMembers
                    .Where(m => m.UserId == currentUserId.Value && !m.IsDeleted)
                    .Select(m => m.CommunityId)
                    .ToListAsync(cancellationToken);
            }

            var query = dbContext.TblPosts
                .Where(p => !p.IsDeleted && p.CreatedAt <= effectiveSessionTime)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(p =>
                    p.Content.Contains(s) ||
                    (p.Author != null && (p.Author.DisplayName.Contains(s) || p.Author.UserName.Contains(s))) ||
                    (p.Community != null && p.Community.Name.Contains(s)) ||
                    (p.Group != null && p.Group.Name.Contains(s)));
            }

            // Privacy & Ban Filter
            if (currentUser.IsAdmin)
            {
                query = query.Where(p => !p.IsPrivate || (currentUserId.HasValue && p.AuthorId == currentUserId.Value));
            }
            else
            {
                if (currentUserId.HasValue)
                {
                    query = query.Where(p =>
                        (!p.IsPrivate || p.AuthorId == currentUserId.Value) &&
                        (p.IsActive || p.AuthorId == currentUserId.Value));
                }
                else
                {
                    query = query.Where(p => !p.IsPrivate && p.IsActive);
                }
            }

            // General Feed Visibility Rule
            if (currentUserId.HasValue)
            {
                query = query.Where(p =>
                    p.AuthorId == currentUserId.Value ||
                    (p.GroupId.HasValue && joinedGroupIds.Contains(p.GroupId.Value)) ||
                    (p.CommunityId.HasValue && !p.GroupId.HasValue && joinedCommunityIds.Contains(p.CommunityId.Value)) ||
                    (!p.GroupId.HasValue && !p.CommunityId.HasValue && (followedAuthorIds.Contains(p.AuthorId) || p.AuthorId == currentUserId.Value)));
            }
            else
            {
                query = query.Where(p => p.GroupId == null && p.CommunityId == null);
            }

            bool hasFollowed = followedAuthorIds.Count > 0;
            bool hasGroups = joinedGroupIds.Count > 0;
            bool hasCommunities = joinedCommunityIds.Count > 0;
            bool hasUser = currentUserId.HasValue;
            int uid = currentUserId ?? 0;

            query = query
                .OrderBy(p =>
                    (hasFollowed && followedAuthorIds.Contains(p.AuthorId)) ? 1 :
                    (hasGroups && p.GroupId.HasValue && joinedGroupIds.Contains(p.GroupId.Value)) ? 2 :
                    (hasCommunities && p.CommunityId.HasValue && joinedCommunityIds.Contains(p.CommunityId.Value)) ? 3 :
                    (hasUser && p.AuthorId == uid) ? 4 : 5)
                .ThenByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.PostId);

            var rawPosts = await query
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(p => new
                {
                    p.PostId,
                    p.CommunityId,
                    CommunityName = p.Community != null ? p.Community.Name : null,
                    p.GroupId,
                    GroupName = p.Group != null ? p.Group.Name : null,
                    p.AuthorId,
                    AuthorDisplayName = p.Author != null ? p.Author.DisplayName : null,
                    AuthorUserName = p.Author != null ? p.Author.UserName : null,
                    AuthorAvatar = p.Author != null ? p.Author.AvatarUrl : null,
                    AuthorIsVerified = p.Author != null && p.Author.IsVerified,
                    AuthorRoleCode = p.Author != null
                        ? p.Author.TblUserRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Role.RoleCode).FirstOrDefault()
                        : null,
                    p.Content,
                    p.HasPoll,
                    Images = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToArray(),
                    LikeCount = p.TblPostLikes.Count(l => !l.IsDeleted),
                    CommentCount = p.TblComments.Count(c => !c.IsDeleted),
                    ShareCount = p.TblPostShares.Count(s => !s.IsDeleted),
                    IsLiked = currentUserId.HasValue && p.TblPostLikes.Any(l => l.UserId == currentUserId.Value && !l.IsDeleted),
                    IsSaved = currentUserId.HasValue && p.TblSavedPosts.Any(s => s.UserId == currentUserId.Value && !s.IsDeleted),
                    p.CreatedAt,
                    p.CodeSnippet,
                    p.CodeFileName,
                    p.CodeLanguage,
                    p.PostType,
                    p.Subtitle,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason
                })
                .ToListAsync(cancellationToken);

            var rootPostIdsToFetch = new HashSet<int>();
            foreach (var rp in rawPosts)
            {
                if (rp.PostType == "SHARED" && !string.IsNullOrWhiteSpace(rp.Subtitle) && rp.Subtitle.StartsWith("ROOT:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(rp.Subtitle["ROOT:".Length..], out int rId))
                    {
                        rootPostIdsToFetch.Add(rId);
                    }
                }
            }

            var rootPostsDict = new Dictionary<int, OriginalPostSummaryModel>();
            if (rootPostIdsToFetch.Count > 0)
            {
                var fetchedRoots = await dbContext.TblPosts
                    .Where(p => rootPostIdsToFetch.Contains(p.PostId) && !p.IsDeleted)
                    .Select(p => new
                    {
                        p.PostId,
                        p.AuthorId,
                        AuthorDisplayName = p.Author != null ? p.Author.DisplayName : null,
                        AuthorUserName = p.Author != null ? p.Author.UserName : null,
                        AuthorAvatar = p.Author != null ? p.Author.AvatarUrl : null,
                        p.Content,
                        p.CreatedAt,
                        CommunityName = p.Community != null ? p.Community.Name : null,
                        GroupName = p.Group != null ? p.Group.Name : null,
                        Images = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToArray(),
                        p.CodeSnippet,
                        p.CodeFileName,
                        p.CodeLanguage,
                        p.IsActive,
                        p.IsPrivate
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (var r in fetchedRoots)
                {
                    bool isHidden = (!r.IsActive || r.IsPrivate) && !currentUser.IsAdmin && (!currentUserId.HasValue || currentUserId.Value != r.AuthorId);
                    if (isHidden)
                    {
                        rootPostsDict[r.PostId] = new OriginalPostSummaryModel(
                            r.PostId,
                            r.AuthorId,
                            "CommunityLink",
                            null,
                            null,
                            "This content is no longer available. When this happens, it's usually because the owner only shared it with a small group of people, changed who can see it or it's been taken down by moderators.",
                            r.CreatedAt);
                    }
                    else
                    {
                        var aName = !string.IsNullOrWhiteSpace(r.AuthorDisplayName)
                            ? r.AuthorDisplayName
                            : (!string.IsNullOrWhiteSpace(r.AuthorUserName) ? r.AuthorUserName : "Unknown");

                        rootPostsDict[r.PostId] = new OriginalPostSummaryModel(
                            r.PostId,
                            r.AuthorId,
                            aName,
                            r.AuthorUserName,
                            r.AuthorAvatar,
                            r.Content,
                            r.CreatedAt,
                            r.CommunityName,
                            r.GroupName,
                            r.Images,
                            r.CodeSnippet,
                            r.CodeFileName,
                            r.CodeLanguage);
                    }
                }
            }

            var pollPostIds = rawPosts.Where(p => p.HasPoll).Select(p => p.PostId).ToList();
            var pollsByPostId = new Dictionary<int, PollModel>();
            if (pollPostIds.Count > 0)
            {
                var rawPolls = await dbContext.TblPolls
                    .Where(pl => pollPostIds.Contains(pl.PostId) && !pl.IsDeleted)
                    .Select(pl => new
                    {
                        pl.PollId,
                        pl.PostId,
                        pl.Question,
                        pl.IsMultipleChoice,
                        pl.ExpiresAt,
                        pl.IsActive,
                        pl.IsPrivate,
                        pl.ModerationReason
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var pollIds = rawPolls.Select(pl => pl.PollId).ToList();
                var rawOptions = await dbContext.TblPollOptions
                    .Where(o => pollIds.Contains(o.PollId) && !o.IsDeleted)
                    .OrderBy(o => o.DisplayOrder)
                    .Select(o => new
                    {
                        o.PollId,
                        o.PollOptionId,
                        o.OptionText,
                        VoteCount = o.TblPollVotes.Count(),
                        IsVotedByCurrentUser = currentUserId.HasValue && o.TblPollVotes.Any(v => v.UserId == currentUserId.Value)
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var optionsByPoll = rawOptions.ToLookup(o => o.PollId);
                var rawPollsByPostId = rawPolls.ToDictionary(pl => pl.PostId);

                foreach (var p in rawPosts.Where(p => p.HasPoll))
                {
                    if (rawPollsByPostId.TryGetValue(p.PostId, out var pl))
                    {
                        var pollOptions = optionsByPoll[pl.PollId].ToList();
                        var totalVotes = pollOptions.Sum(o => o.VoteCount);
                        var hasVoted = currentUserId.HasValue && pollOptions.Any(o => o.IsVotedByCurrentUser);
                        var isExpired = pl.ExpiresAt.HasValue && pl.ExpiresAt.Value <= DateTime.UtcNow;

                        var options = pollOptions.Select(o => new PollOptionModel(
                            o.PollOptionId,
                            o.OptionText,
                            o.VoteCount,
                            totalVotes > 0 ? Math.Round((double)o.VoteCount / totalVotes * 100, 1) : 0,
                            o.IsVotedByCurrentUser
                        )).ToList();

                        var authorName = !string.IsNullOrWhiteSpace(p.AuthorDisplayName)
                            ? p.AuthorDisplayName
                            : (!string.IsNullOrWhiteSpace(p.AuthorUserName) ? p.AuthorUserName : "Unknown");

                        pollsByPostId[p.PostId] = new PollModel(
                            pl.PollId,
                            pl.PostId,
                            p.CommunityId,
                            p.CommunityName,
                            p.GroupId,
                            p.GroupName,
                            p.AuthorId,
                            authorName,
                            p.AuthorAvatar,
                            pl.Question,
                            p.Content,
                            pl.IsMultipleChoice,
                            pl.ExpiresAt,
                            isExpired,
                            totalVotes,
                            hasVoted,
                            options,
                            p.LikeCount,
                            p.CommentCount,
                            p.ShareCount,
                            p.IsLiked,
                            p.CreatedAt,
                            p.AuthorUserName,
                            pl.IsActive,
                            pl.IsPrivate,
                            pl.ModerationReason,
                            p.AuthorIsVerified,
                            p.AuthorRoleCode);
                    }
                }
            }

            int CalculateTier(int authorId, int? gId, int? cId)
            {
                if (hasFollowed && followedAuthorIds.Contains(authorId)) return 1;
                if (hasGroups && gId.HasValue && joinedGroupIds.Contains(gId.Value)) return 2;
                if (hasCommunities && cId.HasValue && joinedCommunityIds.Contains(cId.Value)) return 3;
                if (hasUser && authorId == uid) return 4;
                return 5;
            }

            var items = new List<HomeFeedItemModel>(rawPosts.Count);
            foreach (var p in rawPosts)
            {
                var tier = CalculateTier(p.AuthorId, p.GroupId, p.CommunityId);
                if (p.HasPoll && pollsByPostId.TryGetValue(p.PostId, out var pollModel))
                {
                    items.Add(new HomeFeedItemModel(
                        UniqueKey: $"poll-{pollModel.PollId}",
                        Post: null,
                        Poll: pollModel,
                        Tier: tier,
                        CreatedAt: p.CreatedAt
                    ));
                }
                else
                {
                    var roleCode = p.AuthorRoleCode;
                    var isVerified = p.AuthorIsVerified || (roleCode == "DOMAIN_PRO" || roleCode == "PUBLIC_FIGURE");
                    var authorName = !string.IsNullOrWhiteSpace(p.AuthorDisplayName)
                        ? p.AuthorDisplayName
                        : (!string.IsNullOrWhiteSpace(p.AuthorUserName) ? p.AuthorUserName : "Unknown");

                    OriginalPostSummaryModel? origSummary = null;
                    string? sharedByUser = null;
                    string? sharedByDisplay = null;
                    DateTime? sharedTime = null;

                    if (p.PostType == "SHARED" && !string.IsNullOrWhiteSpace(p.Subtitle) && p.Subtitle.StartsWith("ROOT:", StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(p.Subtitle["ROOT:".Length..], out int rId) && rootPostsDict.TryGetValue(rId, out var rootPost))
                        {
                            origSummary = rootPost;
                            sharedByUser = p.AuthorUserName;
                            sharedByDisplay = p.AuthorDisplayName;
                            sharedTime = p.CreatedAt;
                        }
                    }

                    var postModel = new PostModel(
                        p.PostId,
                        p.CommunityId,
                        p.CommunityName,
                        p.GroupId,
                        p.GroupName,
                        p.AuthorId,
                        authorName,
                        p.AuthorAvatar,
                        p.Content,
                        p.HasPoll,
                        p.Images,
                        p.LikeCount,
                        p.CommentCount,
                        p.ShareCount,
                        p.IsLiked,
                        p.IsSaved,
                        p.CreatedAt,
                        p.CodeSnippet,
                        p.CodeFileName,
                        p.CodeLanguage,
                        roleCode,
                        isVerified,
                        sharedByUser,
                        sharedByDisplay,
                        sharedTime,
                        origSummary,
                        p.IsActive,
                        p.IsPrivate,
                        p.ModerationReason);

                    items.Add(new HomeFeedItemModel(
                        UniqueKey: $"post-{p.PostId}",
                        Post: postModel,
                        Poll: null,
                        Tier: tier,
                        CreatedAt: p.CreatedAt
                    ));
                }
            }

            return Result<IReadOnlyList<HomeFeedItemModel>>.Success(items);
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   (ex is Microsoft.Data.SqlClient.SqlException sqlEx && (cancellationToken.IsCancellationRequested || sqlEx.Message.Contains("Operation cancelled by user", StringComparison.OrdinalIgnoreCase))))
        {
            return Result<IReadOnlyList<HomeFeedItemModel>>.Success([]);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<HomeFeedItemModel>>.Failure(ex.Message, ResultStatus.SystemError);
        }
    }

    public async Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, int? groupId = null, int page = 1, int pageSize = 15, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = currentUser.UserId;

            if (groupId.HasValue && groupId.Value > 0)
            {
                var grp = await dbContext.TblGroups
                    .Include(g => g.TblGroupMembers)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupId == groupId.Value && !g.IsDeleted, cancellationToken);

                if (grp == null)
                {
                    return Result<IReadOnlyList<PostModel>>.Failure("Group not found.", ResultStatus.NotFound);
                }

                if (grp.Visibility == "PRIVATE")
                {
                    var isMember = currentUserId.HasValue && grp.TblGroupMembers.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
                    if (!isMember)
                    {
                        return Result<IReadOnlyList<PostModel>>.Success([]);
                    }
                }
            }

            var query = dbContext.TblPosts
                .Where(p => !p.IsDeleted && !p.HasPoll)
                .AsNoTracking();

            // Privacy & Ban Filter according to policy:
            // 1. If post IsPrivate: ONLY the post author can view it (even admin cannot see private user posts).
            // 2. If post IsActive == false (Banned): ONLY the post author and admin can see it.
            // 3. Normal active public posts are viewable by all.
            if (currentUser.IsAdmin)
            {
                // Admin can see public posts (active or banned), but cannot see private posts of other users
                query = query.Where(p => !p.IsPrivate || (currentUserId.HasValue && p.AuthorId == currentUserId.Value));
            }
            else
            {
                if (currentUserId.HasValue)
                {
                    query = query.Where(p =>
                        // If private, only author
                        (!p.IsPrivate || p.AuthorId == currentUserId.Value) &&
                        // If inactive (banned), only author
                        (p.IsActive || p.AuthorId == currentUserId.Value));
                }
                else
                {
                    query = query.Where(p => !p.IsPrivate && p.IsActive);
                }
            }

            if (groupId.HasValue && groupId.Value > 0)
            {
                query = query.Where(p => p.GroupId == groupId.Value);
            }
            else if (communityId.HasValue && communityId.Value > 0)
            {
                query = query.Where(p => p.CommunityId == communityId.Value);
            }

            List<int> followedAuthorIds = [];
            List<int> joinedGroupIds = [];
            List<int> joinedCommunityIds = [];
            if (currentUserId.HasValue)
            {
                followedAuthorIds = await dbContext.TblUserFollows
                    .Where(f => f.FollowerId == currentUserId.Value && !f.IsDeleted)
                    .Select(f => f.FolloweeId)
                    .ToListAsync(cancellationToken);

                joinedGroupIds = await dbContext.TblGroupMembers
                    .Where(m => m.UserId == currentUserId.Value && !m.IsDeleted)
                    .Select(m => m.GroupId)
                    .ToListAsync(cancellationToken);

                joinedCommunityIds = await dbContext.TblCommunityMembers
                    .Where(m => m.UserId == currentUserId.Value && !m.IsDeleted)
                    .Select(m => m.CommunityId)
                    .ToListAsync(cancellationToken);
            }

            // General Feed Visibility Rule:
            // 1. Group posts: only show if current user has joined the group (or is self author)
            // 2. Community/SubCommunity posts: only show if current user has joined that community/sub-community (or is self author)
            // 3. Standalone posts (no group and no community): show if followed, self, or public feed
            if (!groupId.HasValue || groupId.Value <= 0)
            {
                if (!communityId.HasValue || communityId.Value <= 0)
                {
                    // General feed (no specific group or community filtered)
                    if (currentUserId.HasValue)
                    {
                        query = query.Where(p =>
                            p.AuthorId == currentUserId.Value ||
                            (p.GroupId.HasValue && joinedGroupIds.Contains(p.GroupId.Value)) ||
                            (p.CommunityId.HasValue && !p.GroupId.HasValue && joinedCommunityIds.Contains(p.CommunityId.Value)) ||
                            (!p.GroupId.HasValue && !p.CommunityId.HasValue && (followedAuthorIds.Contains(p.AuthorId) || p.AuthorId == currentUserId.Value)));
                    }
                    else
                    {
                        // Unauthenticated or guest: only standalone public posts with no group and no community
                        query = query.Where(p => p.GroupId == null && p.CommunityId == null);
                    }
                }
                else
                {
                    // Community filtered: only if member (or author)
                    query = query.Where(p => p.GroupId == null || (currentUserId.HasValue && joinedGroupIds.Contains(p.GroupId.Value)));
                }
            }

            var safePageSize = Math.Clamp(pageSize, 1, 50);
            var safePage = Math.Max(1, page);
            var fetchLimit = Math.Max(100, safePage * safePageSize + 50);

            var rawPosts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Take(fetchLimit)
                .Select(p => new
                {
                    p.PostId,
                    p.CommunityId,
                    CommunityName = p.Community != null ? p.Community.Name : null,
                    p.GroupId,
                    GroupName = p.Group != null ? p.Group.Name : null,
                    p.AuthorId,
                    AuthorDisplayName = p.Author != null ? p.Author.DisplayName : null,
                    AuthorUserName = p.Author != null ? p.Author.UserName : null,
                    AuthorAvatar = p.Author != null ? p.Author.AvatarUrl : null,
                    AuthorIsVerified = p.Author != null && p.Author.IsVerified,
                    AuthorRoleCode = p.Author != null
                        ? p.Author.TblUserRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Role.RoleCode).FirstOrDefault()
                        : null,
                    p.Content,
                    p.HasPoll,
                    Images = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToArray(),
                    LikeCount = p.TblPostLikes.Count(l => !l.IsDeleted),
                    CommentCount = p.TblComments.Count(c => !c.IsDeleted),
                    ShareCount = p.TblPostShares.Count(s => !s.IsDeleted),
                    IsLiked = currentUserId.HasValue && p.TblPostLikes.Any(l => l.UserId == currentUserId.Value && !l.IsDeleted),
                    IsSaved = currentUserId.HasValue && p.TblSavedPosts.Any(s => s.UserId == currentUserId.Value && !s.IsDeleted),
                    p.CreatedAt,
                    p.CodeSnippet,
                    p.CodeFileName,
                    p.CodeLanguage,
                    p.PostType,
                    p.Subtitle,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason,
                    SharedByUserName = (string?)null,
                    SharedByDisplayName = (string?)null,
                    SharedAt = (DateTime?)null,
                    EffectiveDate = p.CreatedAt
                })
                .ToListAsync(cancellationToken);

            // Collect root post IDs for any SHARED posts
            var rootPostIdsToFetch = new HashSet<int>();
            foreach (var rp in rawPosts)
            {
                if (rp.PostType == "SHARED" && !string.IsNullOrWhiteSpace(rp.Subtitle) && rp.Subtitle.StartsWith("ROOT:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(rp.Subtitle["ROOT:".Length..], out int rId))
                    {
                        rootPostIdsToFetch.Add(rId);
                    }
                }
            }

            var rootPostsDict = new Dictionary<int, OriginalPostSummaryModel>();
            if (rootPostIdsToFetch.Count > 0)
            {
                var fetchedRoots = await dbContext.TblPosts
                    .Where(p => rootPostIdsToFetch.Contains(p.PostId) && !p.IsDeleted)
                    .Select(p => new
                    {
                        p.PostId,
                        p.AuthorId,
                        AuthorDisplayName = p.Author != null ? p.Author.DisplayName : null,
                        AuthorUserName = p.Author != null ? p.Author.UserName : null,
                        AuthorAvatar = p.Author != null ? p.Author.AvatarUrl : null,
                        p.Content,
                        p.CreatedAt,
                        CommunityName = p.Community != null ? p.Community.Name : null,
                        GroupName = p.Group != null ? p.Group.Name : null,
                        Images = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToArray(),
                        p.CodeSnippet,
                        p.CodeFileName,
                        p.CodeLanguage,
                        p.IsActive,
                        p.IsPrivate
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (var r in fetchedRoots)
                {
                    // If root post is made private by admin or inactive, and current user is not author or admin, show unavailable placeholder
                    bool isHidden = (!r.IsActive || r.IsPrivate) && !currentUser.IsAdmin && (!currentUserId.HasValue || currentUserId.Value != r.AuthorId);

                    if (isHidden)
                    {
                        rootPostsDict[r.PostId] = new OriginalPostSummaryModel(
                            r.PostId,
                            r.AuthorId,
                            "CommunityLink",
                            null,
                            null,
                            "This content is no longer available. When this happens, it's usually because the owner only shared it with a small group of people, changed who can see it or it's been taken down by moderators.",
                            r.CreatedAt);
                    }
                    else
                    {
                        var aName = !string.IsNullOrWhiteSpace(r.AuthorDisplayName)
                            ? r.AuthorDisplayName
                            : (!string.IsNullOrWhiteSpace(r.AuthorUserName) ? r.AuthorUserName : "Unknown");

                        rootPostsDict[r.PostId] = new OriginalPostSummaryModel(
                            r.PostId,
                            r.AuthorId,
                            aName,
                            r.AuthorUserName,
                            r.AuthorAvatar,
                            r.Content,
                            r.CreatedAt,
                            r.CommunityName,
                            r.GroupName,
                            r.Images,
                            r.CodeSnippet,
                            r.CodeFileName,
                            r.CodeLanguage);
                    }
                }
            }

            // Fetch posts shared by users the current user follows via TblPostShares (legacy shares)
            var sharedItems = new List<PostModel>();
            if (followedAuthorIds.Count > 0 && !groupId.HasValue && !communityId.HasValue)
            {
                var rawShares = await dbContext.TblPostShares
                    .Where(s => followedAuthorIds.Contains(s.UserId) && !s.IsDeleted && !s.Post.IsDeleted && !s.Post.HasPoll && !rootPostIdsToFetch.Contains(s.PostId))
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(30)
                    .Select(s => new
                    {
                        s.Post.PostId,
                        s.Post.CommunityId,
                        CommunityName = s.Post.Community != null ? s.Post.Community.Name : null,
                        s.Post.GroupId,
                        GroupName = s.Post.Group != null ? s.Post.Group.Name : null,
                        s.Post.AuthorId,
                        AuthorDisplayName = s.Post.Author != null ? s.Post.Author.DisplayName : null,
                        AuthorUserName = s.Post.Author != null ? s.Post.Author.UserName : null,
                        AuthorAvatar = s.Post.Author != null ? s.Post.Author.AvatarUrl : null,
                        AuthorIsVerified = s.Post.Author != null && s.Post.Author.IsVerified,
                        AuthorRoleCode = s.Post.Author != null
                            ? s.Post.Author.TblUserRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Role.RoleCode).FirstOrDefault()
                            : null,
                        s.Post.Content,
                        s.Post.HasPoll,
                        Images = s.Post.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToArray(),
                        LikeCount = s.Post.TblPostLikes.Count(l => !l.IsDeleted),
                        CommentCount = s.Post.TblComments.Count(c => !c.IsDeleted),
                        ShareCount = s.Post.TblPostShares.Count(sh => !sh.IsDeleted),
                        IsLiked = currentUserId.HasValue && s.Post.TblPostLikes.Any(l => l.UserId == currentUserId.Value && !l.IsDeleted),
                        IsSaved = currentUserId.HasValue && s.Post.TblSavedPosts.Any(sp => sp.UserId == currentUserId.Value && !sp.IsDeleted),
                        s.Post.CreatedAt,
                        s.Post.CodeSnippet,
                        s.Post.CodeFileName,
                        s.Post.CodeLanguage,
                        s.ShareNote,
                        SharedByUserName = s.User.UserName,
                        SharedByDisplayName = s.User.DisplayName,
                        SharedByUserId = s.UserId,
                        SharedAt = (DateTime?)s.CreatedAt,
                        EffectiveDate = s.CreatedAt
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                sharedItems = rawShares.Select(p =>
                {
                    var roleCode = p.AuthorRoleCode;
                    var isVerified = p.AuthorIsVerified || (roleCode == "DOMAIN_PRO" || roleCode == "PUBLIC_FIGURE");
                    var authorName = !string.IsNullOrWhiteSpace(p.AuthorDisplayName)
                        ? p.AuthorDisplayName
                        : (!string.IsNullOrWhiteSpace(p.AuthorUserName) ? p.AuthorUserName : "Unknown");

                    var origSummary = new OriginalPostSummaryModel(
                        p.PostId,
                        p.AuthorId,
                        authorName,
                        p.AuthorUserName,
                        p.AuthorAvatar,
                        p.Content,
                        p.CreatedAt,
                        p.CommunityName,
                        p.GroupName,
                        p.Images,
                        p.CodeSnippet,
                        p.CodeFileName,
                        p.CodeLanguage);

                    return new PostModel(
                        p.PostId,
                        p.CommunityId,
                        p.CommunityName,
                        p.GroupId,
                        p.GroupName,
                        p.AuthorId,
                        authorName,
                        p.AuthorAvatar,
                        p.ShareNote ?? string.Empty,
                        p.HasPoll,
                        [],
                        p.LikeCount,
                        p.CommentCount,
                        p.ShareCount,
                        p.IsLiked,
                        p.IsSaved,
                        p.CreatedAt,
                        null,
                        null,
                        null,
                        roleCode,
                        isVerified,
                        p.SharedByUserName,
                        p.SharedByDisplayName,
                        p.SharedAt,
                        origSummary);
                }).ToList();
            }

            var directItems = rawPosts.Select(p =>
            {
                var roleCode = p.AuthorRoleCode;
                var isVerified = p.AuthorIsVerified || (roleCode == "DOMAIN_PRO" || roleCode == "PUBLIC_FIGURE");
                var authorName = !string.IsNullOrWhiteSpace(p.AuthorDisplayName)
                    ? p.AuthorDisplayName
                    : (!string.IsNullOrWhiteSpace(p.AuthorUserName) ? p.AuthorUserName : "Unknown");

                OriginalPostSummaryModel? origSummary = null;
                string? sharedByUser = null;
                string? sharedByDisplay = null;
                DateTime? sharedTime = null;

                if (p.PostType == "SHARED" && !string.IsNullOrWhiteSpace(p.Subtitle) && p.Subtitle.StartsWith("ROOT:", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(p.Subtitle["ROOT:".Length..], out int rId) && rootPostsDict.TryGetValue(rId, out var rootPost))
                    {
                        origSummary = rootPost;
                        sharedByUser = p.AuthorUserName;
                        sharedByDisplay = p.AuthorDisplayName;
                        sharedTime = p.CreatedAt;
                    }
                }

                return new PostModel(
                    p.PostId,
                    p.CommunityId,
                    p.CommunityName,
                    p.GroupId,
                    p.GroupName,
                    p.AuthorId,
                    authorName,
                    p.AuthorAvatar,
                    p.Content,
                    p.HasPoll,
                    p.Images,
                    p.LikeCount,
                    p.CommentCount,
                    p.ShareCount,
                    p.IsLiked,
                    p.IsSaved,
                    p.CreatedAt,
                    p.CodeSnippet,
                    p.CodeFileName,
                    p.CodeLanguage,
                    roleCode,
                    isVerified,
                    sharedByUser,
                    sharedByDisplay,
                    sharedTime,
                    origSummary,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason);
            }).ToList();

            // 4-Tier Sorting Hierarchy:
            // 1. Followed user's posts & shares (newest first)
            // 2. Joined group posts (newest first)
            // 3. Joined community/sub-community posts (newest first)
            // 4. Self posts and other public posts (placed last)
            int GetPostTier(PostModel item)
            {
                // Self posts are ranked Tier 1 so current user's newly published posts appear at the top
                if (currentUserId.HasValue && item.AuthorId == currentUserId.Value) return 1;

                // Tier 1: Followed user's post or share
                if (followedAuthorIds.Contains(item.AuthorId)) return 1;
                if (!string.IsNullOrWhiteSpace(item.SharedByUserName) && directItems.Any(d => d.PostId == item.PostId && followedAuthorIds.Contains(d.AuthorId))) return 1;

                // Tier 2: Joined group posts
                if (item.GroupId.HasValue && joinedGroupIds.Contains(item.GroupId.Value)) return 2;

                // Tier 3: Joined community/sub-community posts
                if (item.CommunityId.HasValue && joinedCommunityIds.Contains(item.CommunityId.Value)) return 3;

                return 5;
            }

            var list = directItems
                .Concat(sharedItems)
                .OrderBy(p => GetPostTier(p))
                .ThenByDescending(p => p.SharedAt ?? p.CreatedAt)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToList();

            return Result<IReadOnlyList<PostModel>>.Success(list);
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   (ex is Microsoft.Data.SqlClient.SqlException sqlEx && (cancellationToken.IsCancellationRequested || sqlEx.Message.Contains("Operation cancelled by user", StringComparison.OrdinalIgnoreCase))))
        {
            return Result<IReadOnlyList<PostModel>>.Success([]);
        }
    }

    public async Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PostModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        // Standalone post check
        if (!request.GroupId.HasValue)
        {
            var canPostStandalone = await permissionEvaluator.HasPermissionAsync(PermissionCatalog.PostStandaloneCreate, cancellationToken);
            if (!canPostStandalone && !currentUser.IsAdmin)
            {
                return Result<PostModel>.Failure("You can only post inside a group you have joined. Standalone posting is not permitted for your role.", ResultStatus.Forbidden);
            }
        }

        int? communityId = request.CommunityId;
        if (request.GroupId.HasValue)
        {
            var grp = await dbContext.TblGroups
                .Include(g => g.TblGroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == request.GroupId.Value && !g.IsDeleted, cancellationToken);

            if (grp == null)
            {
                return Result<PostModel>.Failure("Group not found.", ResultStatus.NotFound);
            }

            if (!grp.IsActive)
            {
                return Result<PostModel>.Failure("This group is currently inactive/deactivated. New posts cannot be published at this time.", ResultStatus.Forbidden);
            }

            var isMember = grp.TblGroupMembers.Any(m => m.UserId == currentUser.UserId.Value && !m.IsDeleted);
            if (!isMember)
            {
                return Result<PostModel>.Failure("You must be a member of this group to create a post.", ResultStatus.Forbidden);
            }

            if (!communityId.HasValue)
            {
                communityId = grp.SubCommunityId;
            }
        }

        var post = new TblPost
        {
            CommunityId = communityId,
            GroupId = request.GroupId,
            AuthorId = currentUser.UserId.Value,
            Content = request.Content.Trim(),
            HasPoll = false,
            CodeSnippet = request.CodeSnippet,
            CodeFileName = request.CodeFileName,
            CodeLanguage = request.CodeLanguage ?? "TEXT",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblPosts.Add(post);

        if (request.GroupId.HasValue)
        {
            var g = await dbContext.TblGroups.FindAsync([request.GroupId.Value], cancellationToken);
            if (g != null) g.PostCount += 1;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.ImageUrls != null)
        {
            var order = 0;
            foreach (var img in request.ImageUrls.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                dbContext.TblPostImages.Add(new TblPostImage
                {
                    PostId = post.PostId,
                    ImageUrl = img,
                    DisplayOrder = order++,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var user = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        var community = communityId.HasValue ? await dbContext.TblCommunities.FindAsync([communityId.Value], cancellationToken) : null;
        var group = request.GroupId.HasValue ? await dbContext.TblGroups.FindAsync([request.GroupId.Value], cancellationToken) : null;

        // Record User Activity for creating a post
        var targetContextName = group?.Name ?? community?.Name ?? "public feed";
        await userActivityService.RecordActivityAsync(
            currentUser.UserId.Value,
            "POST",
            $"You published a new post to {targetContextName}",
            "POST",
            post.PostId,
            cancellationToken);

        var response = new PostModel(
            post.PostId,
            post.CommunityId,
            community?.Name,
            post.GroupId,
            group?.Name,
            post.AuthorId,
            user?.DisplayName ?? "Unknown",
            user?.AvatarUrl,
            post.Content,
            false,
            request.ImageUrls?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? [],
            0,
            0,
            0,
            false,
            false,
            post.CreatedAt,
            post.CodeSnippet,
            post.CodeFileName,
            post.CodeLanguage);

        return Result<PostModel>.Success(response, "Post created successfully.");
    }

    public async Task<Result<PostModel>> UpdatePostAsync(int postId, UpdatePostRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PostModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts
            .Include(p => p.Author)
            .Include(p => p.Community)
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Include(p => p.TblPostShares)
            .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);

        if (post == null)
            return Result<PostModel>.Failure("Post not found.", ResultStatus.NotFound);

        // Enforce only self-upload post can be edited
        if (post.AuthorId != currentUser.UserId.Value)
            return Result<PostModel>.Failure("You can only edit your own posts.", ResultStatus.Forbidden);

        post.Content = request.Content.Trim();
        post.CodeSnippet = request.CodeSnippet;
        post.CodeFileName = request.CodeFileName;
        post.CodeLanguage = request.CodeLanguage ?? "TEXT";
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = currentUser.UserId.Value;

        // Soft delete old images and insert updated ones if provided
        if (request.ImageUrls != null)
        {
            foreach (var img in post.TblPostImages.Where(i => !i.IsDeleted))
            {
                img.IsDeleted = true;
                img.DeletedAt = DateTime.UtcNow;
                img.DeletedBy = currentUser.UserId.Value;
            }

            var order = 0;
            foreach (var img in request.ImageUrls.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                dbContext.TblPostImages.Add(new TblPostImage
                {
                    PostId = post.PostId,
                    ImageUrl = img,
                    DisplayOrder = order++,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser.UserId.Value
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new PostModel(
            post.PostId,
            post.CommunityId,
            post.Community?.Name,
            post.GroupId,
            post.Group?.Name,
            post.AuthorId,
            post.Author?.DisplayName ?? "Unknown",
            post.Author?.AvatarUrl,
            post.Content,
            post.HasPoll,
            request.ImageUrls?.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray() ?? post.TblPostImages.Where(i => !i.IsDeleted).Select(i => i.ImageUrl).ToArray(),
            post.TblPostLikes.Count,
            post.TblComments.Count(c => !c.IsDeleted),
            post.TblPostShares.Count,
            post.TblPostLikes.Any(l => l.UserId == currentUser.UserId.Value),
            false,
            post.CreatedAt,
            post.CodeSnippet,
            post.CodeFileName,
            post.CodeLanguage);

        return Result<PostModel>.Success(response, "Post updated successfully.");
    }

    public async Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);
        if (post == null)
            return Result.Failure("Post not found.", ResultStatus.NotFound);

        // Only author can delete
        if (post.AuthorId != currentUser.UserId.Value)
            return Result.Failure("You can only delete your own posts.", ResultStatus.Forbidden);

        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.DeletedBy = currentUser.UserId.Value;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Post deleted successfully.");
    }

    public async Task<Result<bool>> TogglePostPrivacyAsync(int postId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<bool>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);
        if (post == null)
            return Result<bool>.Failure("Post not found.", ResultStatus.NotFound);

        // Only author can toggle their own post between Public and Private
        if (post.AuthorId != currentUser.UserId.Value)
            return Result<bool>.Failure("Only the post author can change the privacy of this post.", ResultStatus.Forbidden);

        post.IsPrivate = !post.IsPrivate;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = currentUser.UserId.Value;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(post.IsPrivate, post.IsPrivate ? "Post changed to Private mode." : "Post changed to Public mode.");
    }

    public async Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);
        if (post == null) return Result.Failure("Post not found.", ResultStatus.NotFound);

        var existingLike = await dbContext.TblPostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == currentUser.UserId.Value, cancellationToken);

        if (existingLike is not null)
        {
            dbContext.TblPostLikes.Remove(existingLike);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success("Post unliked.");
        }

        dbContext.TblPostLikes.Add(new TblPostLike
        {
            PostId = postId,
            UserId = currentUser.UserId.Value,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value
        });

        post.LikeCount += 1;
        await dbContext.SaveChangesAsync(cancellationToken);

        // Notify post author
        var actorUser = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        var actorName = actorUser?.DisplayName ?? actorUser?.UserName ?? "Someone";
        var authorUser = await dbContext.TblUsers.FindAsync([post.AuthorId], cancellationToken);
        var authorName = authorUser?.DisplayName ?? authorUser?.UserName ?? "someone";

        await notificationService.CreateNotificationAsync(
            post.AuthorId,
            currentUser.UserId.Value,
            "LIKE",
            "New Like",
            $"{actorName} gave a Like to your post",
            "POST",
            post.PostId,
            cancellationToken);

        // Record User Activity
        await userActivityService.RecordActivityAsync(
            currentUser.UserId.Value,
            "LIKE",
            $"You gave like to {authorName} post",
            "POST",
            post.PostId,
            cancellationToken);

        return Result.Success("Post liked.");
    }

    public async Task<Result> SharePostAsync(int postId, SharePostRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts
            .Include(p => p.TblPostImages)
            .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);
        if (post == null) return Result.Failure("Post not found.", ResultStatus.NotFound);

        // Identify the root original post
        int rootPostId = postId;
        if (post.PostType == "SHARED" && !string.IsNullOrWhiteSpace(post.Subtitle) && post.Subtitle.StartsWith("ROOT:", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(post.Subtitle["ROOT:".Length..], out int parsedRootId))
            {
                rootPostId = parsedRootId;
            }
        }

        var rootPost = (rootPostId == postId)
            ? post
            : await dbContext.TblPosts.Include(p => p.TblPostImages).FirstOrDefaultAsync(p => p.PostId == rootPostId && !p.IsDeleted, cancellationToken);

        if (rootPost == null)
        {
            rootPost = post;
            rootPostId = postId;
        }

        // Increment share count on the post being viewed
        post.ShareCount += 1;
        // If sharing a shared post, also increment share count on the root post
        if (rootPost.PostId != post.PostId)
        {
            rootPost.ShareCount += 1;
        }

        // Record a TblPostShare entry linking to the root post
        var share = new TblPostShare
        {
            PostId = rootPostId,
            UserId = currentUser.UserId.Value,
            ShareNote = string.IsNullOrWhiteSpace(request.ShareNote) ? null : request.ShareNote.Trim(),
            TargetCommunityId = request.TargetCommunityId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value
        };
        dbContext.TblPostShares.Add(share);

        // Create a new post representing this share (Facebook-style standalone post with independent likes/comments)
        var sharedPostRecord = new TblPost
        {
            AuthorId = currentUser.UserId.Value,
            CommunityId = request.TargetCommunityId ?? rootPost.CommunityId,
            GroupId = null,
            Content = string.IsNullOrWhiteSpace(request.ShareNote) ? string.Empty : request.ShareNote.Trim(),
            PostType = "SHARED",
            Subtitle = $"ROOT:{rootPostId}",
            HasPoll = false,
            LikeCount = 0,
            CommentCount = 0,
            ShareCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value,
            IsDeleted = false
        };
        dbContext.TblPosts.Add(sharedPostRecord);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Notify post author
        var actorUser = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        var actorName = actorUser?.DisplayName ?? actorUser?.UserName ?? "Someone";
        var authorUser = await dbContext.TblUsers.FindAsync([post.AuthorId], cancellationToken);
        var authorName = authorUser?.DisplayName ?? authorUser?.UserName ?? "someone";

        await notificationService.CreateNotificationAsync(
            post.AuthorId,
            currentUser.UserId.Value,
            "SHARE",
            "Post Shared",
            $"{actorName} just shared your post",
            "POST",
            post.PostId,
            cancellationToken);

        // Record User Activity
        await userActivityService.RecordActivityAsync(
            currentUser.UserId.Value,
            "SHARE",
            $"You shared {authorName} post",
            "POST",
            post.PostId,
            cancellationToken);

        return Result.Success("Post shared successfully.");
    }

    public async Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<CommentModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == request.PostId && !p.IsDeleted, cancellationToken);
        if (post == null) return Result<CommentModel>.Failure("Post not found.", ResultStatus.NotFound);

        var comment = new TblComment
        {
            PostId = request.PostId,
            UserId = currentUser.UserId.Value,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value
        };

        dbContext.TblComments.Add(comment);
        post.CommentCount += 1;
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        var actorName = user?.DisplayName ?? user?.UserName ?? "Someone";
        var authorUser = await dbContext.TblUsers.FindAsync([post.AuthorId], cancellationToken);
        var authorName = authorUser?.DisplayName ?? authorUser?.UserName ?? "someone";

        // Notify post author
        await notificationService.CreateNotificationAsync(
            post.AuthorId,
            currentUser.UserId.Value,
            "COMMENT",
            "New Comment",
            $"{actorName} gave you a comment to your post",
            "POST",
            post.PostId,
            cancellationToken);

        // Record User Activity
        await userActivityService.RecordActivityAsync(
            currentUser.UserId.Value,
            "COMMENT",
            $"You gave a comment to {authorName} post",
            "POST",
            post.PostId,
            cancellationToken);

        var model = new CommentModel(
            comment.CommentId,
            comment.PostId,
            comment.UserId,
            user?.DisplayName ?? "Unknown",
            user?.AvatarUrl,
            comment.Content,
            comment.CreatedAt);

        return Result<CommentModel>.Success(model);
    }

    public async Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default)
    {
        var list = await dbContext.TblComments
            .Include(c => c.User)
            .Where(c => c.PostId == postId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentModel(
                c.CommentId,
                c.PostId,
                c.UserId,
                c.User.DisplayName,
                c.User.AvatarUrl,
                c.Content,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CommentModel>>.Success(list);
    }

    public async Task<Result<bool>> ToggleSavePostAsync(int postId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<bool>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);
        if (post == null) return Result<bool>.Failure("Post not found.", ResultStatus.NotFound);

        var existingSave = await dbContext.TblSavedPosts
            .FirstOrDefaultAsync(s => s.PostId == postId && s.UserId == currentUser.UserId.Value && !s.IsDeleted, cancellationToken);

        bool isSaved;
        if (existingSave != null)
        {
            dbContext.TblSavedPosts.Remove(existingSave);
            isSaved = false;
        }
        else
        {
            dbContext.TblSavedPosts.Add(new TblSavedPost
            {
                PostId = postId,
                UserId = currentUser.UserId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUser.UserId.Value
            });
            isSaved = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(isSaved, isSaved ? "Post saved successfully." : "Post removed from saved.");
    }
}