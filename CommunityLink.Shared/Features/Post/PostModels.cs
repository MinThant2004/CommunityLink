namespace CommunityLink.Shared.Features.Post;

public sealed record PostModel(
    int PostId,
    int? CommunityId,
    string? CommunityName,
    int AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    bool HasPoll,
    IReadOnlyList<string> ImageUrls,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    bool IsLikedByCurrentUser,
    bool IsSavedByCurrentUser,
    DateTime CreatedAt);

public sealed record CreatePostRequestModel(
    int? CommunityId,
    string Content,
    IReadOnlyList<string>? ImageUrls);

public sealed record CommentModel(
    int CommentId,
    int PostId,
    int AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    DateTime CreatedAt);

public sealed record CreateCommentRequestModel(int PostId, string Content);
public sealed record LikePostRequestModel(int PostId);
public sealed record UpdatePostRequestModel(string Content, IReadOnlyList<string>? ImageUrls);
public sealed record SharePostRequestModel(string? ShareNote, int? TargetCommunityId = null);