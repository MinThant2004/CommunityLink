using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class PostApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<PostModel>>> GetFeedAsync(int? communityId = null, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<PostModel>>(communityId.HasValue ? $"api/posts/feed?communityId={communityId.Value}" : "api/posts/feed", cancellationToken);

    public Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<PostModel, CreatePostRequestModel>("api/posts", request, cancellationToken);

    public Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/posts/{postId}/like", new { }, cancellationToken);

    public Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<CommentModel, CreateCommentRequestModel>("api/posts/comments", request, cancellationToken);

    public Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CommentModel>>($"api/posts/{postId}/comments", cancellationToken);
}