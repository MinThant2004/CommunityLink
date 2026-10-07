namespace CommunityLink.Shared.Features.Post;

public sealed record OriginalPostSummaryModel(
    int PostId,
    int AuthorId,
    string AuthorName,
    string? AuthorUserName,
    string? AuthorAvatar,
    string Content,
    DateTime CreatedAt,
    string? CommunityName = null,
    string? GroupName = null,
    IReadOnlyList<string>? ImageUrls = null,
    string? CodeSnippet = null,
    string? CodeFileName = null,
    string? CodeLanguage = null);

public sealed record PostModel(
    int PostId,
    int? CommunityId,
    string? CommunityName,
    int? GroupId,
    string? GroupName,
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
    DateTime CreatedAt,
    string? CodeSnippet = null,
    string? CodeFileName = null,
    string? CodeLanguage = null,
    string? AuthorRoleCode = null,
    bool IsAuthorVerified = false,
    string? SharedByUserName = null,
    string? SharedByDisplayName = null,
    DateTime? SharedAt = null,
    OriginalPostSummaryModel? OriginalPost = null);

public sealed record CreatePostRequestModel(
    int? CommunityId,
    string Content,
    IReadOnlyList<string>? ImageUrls,
    int? GroupId = null,
    string? CodeSnippet = null,
    string? CodeFileName = null,
    string? CodeLanguage = null);

public sealed record CommentModel(
    int CommentId,
    int PostId,
    int AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Content,
    DateTime CreatedAt,
    string? AuthorRoleCode = null,
    bool IsAuthorVerified = false);

public sealed record CreateCommentRequestModel(int PostId, string Content);
public sealed record LikePostRequestModel(int PostId);
public sealed record UpdatePostRequestModel(
    string Content,
    IReadOnlyList<string>? ImageUrls,
    string? CodeSnippet = null,
    string? CodeFileName = null,
    string? CodeLanguage = null);
public sealed record SharePostRequestModel(string? ShareNote, int? TargetCommunityId = null);