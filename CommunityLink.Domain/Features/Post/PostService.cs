using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;

using CommunityLink.Domain.Features.Notification;

namespace CommunityLink.Domain.Features.Post;

public interface IPostService
{
    Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, int? groupId = null, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> UpdatePostAsync(int postId, UpdatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result> SharePostAsync(int postId, SharePostRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class PostService(AppDbContext dbContext, ICurrentUserContext currentUser, INotificationService notificationService) : IPostService
{
    public async Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, int? groupId = null, CancellationToken cancellationToken = default)
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
            .Include(p => p.Author)
            .Include(p => p.Community)
            .Include(p => p.Group)
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Include(p => p.TblPostShares)
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

        var posts = await query.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync(cancellationToken);

        var list = posts.Select(p => new PostModel(
            p.PostId,
            p.CommunityId,
            p.Community?.Name,
            p.GroupId,
            p.Group?.Name,
            p.AuthorId,
            p.Author != null ? (string.IsNullOrWhiteSpace(p.Author.DisplayName) ? p.Author.UserName : p.Author.DisplayName) : "Unknown",
            p.Author?.AvatarUrl,
            p.Content,
            p.HasPoll,
            p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToArray(),
            p.TblPostLikes.Count,
            p.TblComments.Count(c => !c.IsDeleted),
            p.TblPostShares.Count,
            currentUserId.HasValue && p.TblPostLikes.Any(l => l.UserId == currentUserId.Value),
            false,
            p.CreatedAt)).ToList();

        return Result<IReadOnlyList<PostModel>>.Success(list);
    }

    public async Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PostModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

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
            post.CreatedAt);

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
            post.CreatedAt);

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
        await notificationService.CreateNotificationAsync(
            post.AuthorId,
            currentUser.UserId.Value,
            "LIKE",
            "New Like",
            $"{actorName} gave a Like to your post",
            "POST",
            post.PostId,
            cancellationToken);

        return Result.Success("Post liked.");
    }

    public async Task<Result> SharePostAsync(int postId, SharePostRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var post = await dbContext.TblPosts.FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted, cancellationToken);
        if (post == null) return Result.Failure("Post not found.", ResultStatus.NotFound);

        var share = new TblPostShare
        {
            PostId = postId,
            UserId = currentUser.UserId.Value,
            ShareNote = string.IsNullOrWhiteSpace(request.ShareNote) ? null : request.ShareNote.Trim(),
            TargetCommunityId = request.TargetCommunityId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser.UserId.Value
        };

        dbContext.TblPostShares.Add(share);
        post.ShareCount += 1;

        await dbContext.SaveChangesAsync(cancellationToken);

        // Notify post author
        var actorUser = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        var actorName = actorUser?.DisplayName ?? actorUser?.UserName ?? "Someone";
        await notificationService.CreateNotificationAsync(
            post.AuthorId,
            currentUser.UserId.Value,
            "SHARE",
            "Post Shared",
            $"{actorName} just shared your post",
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
}