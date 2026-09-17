using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;

namespace CommunityLink.Domain.Features.Post;

public interface IPostService
{
    Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, CancellationToken cancellationToken = default);
    Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default);
    Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default);
}

public sealed class PostService(AppDbContext dbContext, ICurrentUserContext currentUser) : IPostService
{
    public async Task<Result<IReadOnlyList<PostModel>>> GetFeedPostsAsync(int? communityId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblPosts
            .Include(p => p.Author)
            .Include(p => p.Community)
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Include(p => p.TblPostShares)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (communityId.HasValue)
        {
            query = query.Where(p => p.CommunityId == communityId.Value);
        }

        var posts = await query.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync(cancellationToken);
        var currentUserId = currentUser.UserId;

        var list = posts.Select(p => new PostModel(
            p.PostId,
            p.CommunityId,
            p.Community?.Name,
            p.AuthorId,
            p.Author.DisplayName,
            p.Author.AvatarUrl,
            p.Content,
            p.HasPoll,
            p.TblPostImages.Select(i => i.ImageUrl).ToArray(),
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

        var post = new TblPost
        {
            CommunityId = request.CommunityId,
            AuthorId = currentUser.UserId.Value,
            Content = request.Content.Trim(),
            HasPoll = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblPosts.Add(post);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.ImageUrls != null)
        {
            foreach (var img in request.ImageUrls)
            {
                dbContext.TblPostImages.Add(new TblPostImage
                {
                    PostId = post.PostId,
                    ImageUrl = img,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var user = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
        var community = request.CommunityId.HasValue ? await dbContext.TblCommunities.FindAsync([request.CommunityId.Value], cancellationToken) : null;

        var response = new PostModel(
            post.PostId,
            post.CommunityId,
            community?.Name,
            post.AuthorId,
            user?.DisplayName ?? "Unknown",
            user?.AvatarUrl,
            post.Content,
            false,
            request.ImageUrls?.ToArray() ?? [],
            0,
            0,
            0,
            false,
            false,
            post.CreatedAt);

        return Result<PostModel>.Success(response, "Post created successfully.");
    }

    public async Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var existingLike = await dbContext.TblPostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == currentUser.UserId.Value, cancellationToken);

        if (existingLike is not null)
        {
            dbContext.TblPostLikes.Remove(existingLike);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success("Post unliked.");
        }

        dbContext.TblPostLikes.Add(new TblPostLike
        {
            PostId = postId,
            UserId = currentUser.UserId.Value,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Post liked.");
    }

    public async Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<CommentModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var comment = new TblComment
        {
            PostId = request.PostId,
            UserId = currentUser.UserId.Value,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblComments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
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