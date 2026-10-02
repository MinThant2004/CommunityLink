using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;
using CommunityLink.Domain.Features.Notification;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Domain.Features.Activity;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Post;

public interface IPostService
{
    Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, int? groupId = null, int page = 1, int pageSize = 15, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> UpdatePostAsync(int postId, UpdatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default);
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
                        p.CodeLanguage
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                foreach (var r in fetchedRoots)
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
                    origSummary);
            }).ToList();

            // 3-Tier Sorting Hierarchy:
            // Tier 1: Following user's posts & shares (newest first)
            // Tier 2: Joined group posts (newest first)
            // Tier 3: Non-following public posts (newest first)
            int GetPostTier(PostModel item)
            {
                // If the post author is followed, or if the sharer is followed
                if (followedAuthorIds.Contains(item.AuthorId)) return 1;
                // If it's a shared post where current user follows the sharer
                if (!string.IsNullOrWhiteSpace(item.SharedByUserName) && directItems.Any(d => d.PostId == item.PostId && followedAuthorIds.Contains(d.AuthorId))) return 1;
                // If it belongs to a joined group
                if (item.GroupId.HasValue && joinedGroupIds.Contains(item.GroupId.Value)) return 2;
                // Everything else
                return 3;
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