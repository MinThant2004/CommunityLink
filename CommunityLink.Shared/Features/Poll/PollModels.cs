namespace CommunityLink.Shared.Features.Poll;

public sealed record PollOptionModel(int OptionId, string Text, int VoteCount, double Percentage, bool IsVotedByCurrentUser);

public sealed record PollModel(
    int PollId,
    int PostId,
    string Question,
    bool IsMultipleChoice,
    DateTime? ExpiresAt,
    int TotalVotes,
    bool HasVoted,
    IReadOnlyList<PollOptionModel> Options,
    DateTime CreatedAt);

public sealed record CreatePollRequestModel(
    int? CommunityId,
    string Content,
    string Question,
    bool IsMultipleChoice,
    DateTime? ExpiresAt,
    IReadOnlyList<string> Options);

public sealed record VoteRequestModel(int PollId, IReadOnlyList<int> OptionIds);