using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Post;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Post;

[Route("api/posts")]
public sealed class PostController(IPostService postService) : BaseController
{
    [HttpGet("feed")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.PostView)]
    public async Task<IActionResult> GetFeed([FromQuery] int? communityId, CancellationToken cancellationToken) =>
        ToActionResult(await postService.GetFeedPostsAsync(communityId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.PostCreate)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await postService.CreatePostAsync(request, cancellationToken));

    [HttpPost("{postId:int}/like")]
    [Authorize]
    public async Task<IActionResult> LikePost(int postId, CancellationToken cancellationToken) =>
        ToActionResult(await postService.LikePostAsync(postId, cancellationToken));

    [HttpPut("{postId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost(int postId, [FromBody] UpdatePostRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await postService.UpdatePostAsync(postId, request, cancellationToken));

    [HttpDelete("{postId:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int postId, CancellationToken cancellationToken) =>
        ToActionResult(await postService.DeletePostAsync(postId, cancellationToken));

    [HttpPost("{postId:int}/share")]
    [Authorize]
    public async Task<IActionResult> SharePost(int postId, [FromBody] SharePostRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await postService.SharePostAsync(postId, request, cancellationToken));

    [HttpPost("comments")]
    [Authorize]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await postService.AddCommentAsync(request, cancellationToken));

    [HttpGet("{postId:int}/comments")]
    [Authorize]
    public async Task<IActionResult> GetComments(int postId, CancellationToken cancellationToken) =>
        ToActionResult(await postService.GetCommentsAsync(postId, cancellationToken));
}