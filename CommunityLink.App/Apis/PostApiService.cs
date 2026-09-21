using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class PostApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<PostModel>>> GetFeedAsync(int? communityId = null, int? groupId = null, CancellationToken cancellationToken = default)
    {
        var url = groupId.HasValue ? $"api/posts/feed?groupId={groupId.Value}" : (communityId.HasValue ? $"api/posts/feed?communityId={communityId.Value}" : "api/posts/feed");
        return GetAsync<IReadOnlyList<PostModel>>(url, cancellationToken);
    }

    public Task<Result<PostModel>> CreatePostAsync(CreatePostRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<PostModel, CreatePostRequestModel>("api/posts", request, cancellationToken);

    public Task<Result<PostModel>> UpdatePostAsync(int postId, UpdatePostRequestModel request, CancellationToken cancellationToken = default) =>
        PutAsync<PostModel, UpdatePostRequestModel>($"api/posts/{postId}", request, cancellationToken);

    public Task<Result> DeletePostAsync(int postId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/posts/{postId}", cancellationToken);

    public Task<Result> SharePostAsync(int postId, SharePostRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync($"api/posts/{postId}/share", request, cancellationToken);

    public Task<Result> LikePostAsync(int postId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/posts/{postId}/like", new { }, cancellationToken);

    public Task<Result<CommentModel>> AddCommentAsync(CreateCommentRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<CommentModel, CreateCommentRequestModel>("api/posts/comments", request, cancellationToken);

    public Task<Result<IReadOnlyList<CommentModel>>> GetCommentsAsync(int postId, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CommentModel>>($"api/posts/{postId}/comments", cancellationToken);
}