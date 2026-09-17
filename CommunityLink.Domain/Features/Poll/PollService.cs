using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Poll;

namespace CommunityLink.Domain.Features.Poll;

public interface IPollService
{
    Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> VoteAsync(VoteRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class PollService(AppDbContext dbContext, ICurrentUserContext currentUser) : IPollService
{
    public async Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblPolls
            .Include(p => p.Post).ThenInclude(post => post.Author)
            .Include(p => p.Post).ThenInclude(post => post.Community)
            .Include(p => p.TblPollOptions).ThenInclude(o => o.TblPollVotes)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (communityId.HasValue) query = query.Where(p => p.Post.CommunityId == communityId.Value);

        var polls = await query.OrderByDescending(p => p.CreatedAt).Take(30).ToListAsync(cancellationToken);
        var currentUserId = currentUser.UserId;

        var list = polls.Select(p =>
        {
            var totalVotes = p.TblPollOptions.Sum(o => o.TblPollVotes.Count);
            var hasVoted = currentUserId.HasValue && p.TblPollOptions.Any(o => o.TblPollVotes.Any(v => v.UserId == currentUserId.Value));

            var options = p.TblPollOptions.Select(o => new PollOptionModel(
                o.PollOptionId,
                o.OptionText,
                o.TblPollVotes.Count,
                totalVotes > 0 ? Math.Round((double)o.TblPollVotes.Count / totalVotes * 100, 1) : 0,
                currentUserId.HasValue && o.TblPollVotes.Any(v => v.UserId == currentUserId.Value))).ToList();

            return new PollModel(
                p.PollId,
                p.PostId,
                p.Question,
                p.IsMultipleChoice,
                p.ExpiresAt,
                totalVotes,
                hasVoted,
                options,
                p.CreatedAt);
        }).ToList();

        return Result<IReadOnlyList<PollModel>>.Success(list);
    }

    public async Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PollModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        if (request.Options.Count < 2) return Result<PollModel>.Failure("A poll must contain at least 2 options.", ResultStatus.ValidationError);

        var post = new TblPost
        {
            CommunityId = request.CommunityId,
            AuthorId = currentUser.UserId.Value,
            Content = string.IsNullOrWhiteSpace(request.Content) ? request.Question : request.Content,
            HasPoll = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblPosts.Add(post);
        await dbContext.SaveChangesAsync(cancellationToken);

        var poll = new TblPoll
        {
            PostId = post.PostId,
            Question = request.Question.Trim(),
            IsMultipleChoice = request.IsMultipleChoice,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblPolls.Add(poll);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var opt in request.Options)
        {
            dbContext.TblPollOptions.Add(new TblPollOption
            {
                PollId = poll.PollId,
                OptionText = opt.Trim(),
                CreatedAt = DateTime.UtcNow
            });
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var polls = await GetPollsAsync(request.CommunityId, cancellationToken);
        var created = polls.Data?.FirstOrDefault(p => p.PollId == poll.PollId);
        return Result<PollModel>.Success(created!);
    }

    public async Task<Result<PollModel>> VoteAsync(VoteRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PollModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var poll = await dbContext.TblPolls
            .Include(p => p.Post)
            .Include(p => p.TblPollOptions).ThenInclude(o => o.TblPollVotes)
            .FirstOrDefaultAsync(p => p.PollId == request.PollId && !p.IsDeleted, cancellationToken);

        if (poll is null) return Result<PollModel>.Failure("Poll not found.", ResultStatus.NotFound);

        var alreadyVoted = poll.TblPollOptions.Any(o => o.TblPollVotes.Any(v => v.UserId == currentUser.UserId.Value));
        if (alreadyVoted) return Result<PollModel>.Failure("You have already voted on this poll.", ResultStatus.Conflict);

        if (!poll.IsMultipleChoice && request.OptionIds.Count > 1)
        {
            return Result<PollModel>.Failure("This poll only permits a single option vote.", ResultStatus.ValidationError);
        }

        foreach (var optionId in request.OptionIds)
        {
            var opt = poll.TblPollOptions.FirstOrDefault(o => o.PollOptionId == optionId);
            if (opt is not null)
            {
                dbContext.TblPollVotes.Add(new TblPollVote
                {
                    PollOptionId = optionId,
                    UserId = currentUser.UserId.Value,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        var refreshed = await GetPollsAsync(poll.Post.CommunityId, cancellationToken);
        var model = refreshed.Data?.FirstOrDefault(p => p.PollId == poll.PollId);
        return Result<PollModel>.Success(model!, "Vote recorded.");
    }
}