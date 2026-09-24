namespace CommunityLink.Shared.Features.Poll;

public sealed record PollOptionModel(int OptionId, string Text, int VoteCount, double Percentage, bool IsVotedByCurrentUser);

public sealed record PollModel(
    int PollId,
    int PostId,
    int? CommunityId,
    string? CommunityName,
    int? GroupId,
    string? GroupName,
    int AuthorId,
    string AuthorName,
    string? AuthorAvatar,
    string Question,
    string? Content,
    bool IsMultipleChoice,
    DateTime? ExpiresAt,
    bool IsExpired,
    int TotalVotes,
    bool HasVoted,
    IReadOnlyList<PollOptionModel> Options,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    bool IsLikedByCurrentUser,
    DateTime CreatedAt);

public sealed record CreatePollRequestModel(
    int? CommunityId,
    string Content,
    string Question,
    bool IsMultipleChoice,
    DateTime? ExpiresAt,
    IReadOnlyList<string> Options,
    int? GroupId = null);

public sealed record VoteRequestModel(int PollId, IReadOnlyList<int> OptionIds);
public sealed record UpdatePollRequestModel(string Question, string? Content);