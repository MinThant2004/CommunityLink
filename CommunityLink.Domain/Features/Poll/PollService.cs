using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Poll;

using CommunityLink.Domain.Features.Notification;

namespace CommunityLink.Domain.Features.Poll;

public interface IPollService
{
    Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId, int? groupId = null, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> VoteAsync(VoteRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class PollService(AppDbContext dbContext, ICurrentUserContext currentUser, INotificationService notificationService) : IPollService
{
    public async Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId, int? groupId = null, CancellationToken cancellationToken = default)
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
                return Result<IReadOnlyList<PollModel>>.Failure("Group not found.", ResultStatus.NotFound);
            }

            if (grp.Visibility == "PRIVATE")
            {
                var isMember = currentUserId.HasValue && grp.TblGroupMembers.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
                if (!isMember)
                {
                    return Result<IReadOnlyList<PollModel>>.Success([]);
                }
            }
        }

        var query = dbContext.TblPolls
            .Include(p => p.Post).ThenInclude(post => post.Author)
            .Include(p => p.Post).ThenInclude(post => post.Community)
            .Include(p => p.Post).ThenInclude(post => post.Group)
            .Include(p => p.Post).ThenInclude(post => post.TblPostLikes)
            .Include(p => p.Post).ThenInclude(post => post.TblComments)
            .Include(p => p.Post).ThenInclude(post => post.TblPostShares)
            .Include(p => p.TblPollOptions).ThenInclude(o => o.TblPollVotes)
            .Where(p => !p.IsDeleted)
            .AsNoTracking();

        if (groupId.HasValue && groupId.Value > 0)
        {
            query = query.Where(p => p.Post.GroupId == groupId.Value);
        }
        else if (communityId.HasValue && communityId.Value > 0)
        {
            query = query.Where(p => p.Post.CommunityId == communityId.Value);
        }

        var polls = await query.OrderByDescending(p => p.CreatedAt).Take(30).ToListAsync(cancellationToken);

        var list = polls.Select(p =>
        {
            var totalVotes = p.TblPollOptions.Sum(o => o.TblPollVotes.Count);
            var hasVoted = currentUserId.HasValue && p.TblPollOptions.Any(o => o.TblPollVotes.Any(v => v.UserId == currentUserId.Value));
            var isExpired = p.ExpiresAt.HasValue && p.ExpiresAt.Value <= DateTime.UtcNow;

            var options = p.TblPollOptions
                .OrderBy(o => o.DisplayOrder)
                .Select(o => new PollOptionModel(
                    o.PollOptionId,
                    o.OptionText,
                    o.TblPollVotes.Count,
                    totalVotes > 0 ? Math.Round((double)o.TblPollVotes.Count / totalVotes * 100, 1) : 0,
                    currentUserId.HasValue && o.TblPollVotes.Any(v => v.UserId == currentUserId.Value))).ToList();

            var likeCount = p.Post.TblPostLikes.Count;
            var commentCount = p.Post.TblComments.Count(c => !c.IsDeleted);
            var shareCount = p.Post.TblPostShares.Count;
            var isLiked = currentUserId.HasValue && p.Post.TblPostLikes.Any(l => l.UserId == currentUserId.Value);

            return new PollModel(
                p.PollId,
                p.PostId,
                p.Post.CommunityId,
                p.Post.Community?.Name,
                p.Post.GroupId,
                p.Post.Group?.Name,
                p.Post.AuthorId,
                p.Post.Author != null ? (string.IsNullOrWhiteSpace(p.Post.Author.DisplayName) ? p.Post.Author.UserName : p.Post.Author.DisplayName) : "Unknown",
                p.Post.Author?.AvatarUrl,
                p.Question,
                p.Post.Content,
                p.IsMultipleChoice,
                p.ExpiresAt,
                isExpired,
                totalVotes,
                hasVoted,
                options,
                likeCount,
                commentCount,
                shareCount,
                isLiked,
                p.CreatedAt);
        }).ToList();

        return Result<IReadOnlyList<PollModel>>.Success(list);
    }

    public async Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PollModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        
        var trimmedOptions = request.Options
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (trimmedOptions.Count < 2) 
            return Result<PollModel>.Failure("A poll must contain at least 2 unique non-empty options.", ResultStatus.ValidationError);

        if (trimmedOptions.Count > 10)
            return Result<PollModel>.Failure("A poll cannot contain more than 10 options.", ResultStatus.ValidationError);

        int? communityId = request.CommunityId;
        if (request.GroupId.HasValue)
        {
            var grp = await dbContext.TblGroups
                .Include(g => g.TblGroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == request.GroupId.Value && !g.IsDeleted, cancellationToken);

            if (grp == null)
            {
                return Result<PollModel>.Failure("Group not found.", ResultStatus.NotFound);
            }

            var isMember = grp.TblGroupMembers.Any(m => m.UserId == currentUser.UserId.Value && !m.IsDeleted);
            if (!isMember)
            {
                return Result<PollModel>.Failure("You must be a member of this group to create a poll.", ResultStatus.Forbidden);
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
            Content = string.IsNullOrWhiteSpace(request.Content) ? request.Question : request.Content.Trim(),
            HasPoll = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblPosts.Add(post);

        if (request.GroupId.HasValue)
        {
            var g = await dbContext.TblGroups.FindAsync([request.GroupId.Value], cancellationToken);
            if (g != null) g.PostCount += 1;
        }

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

        var order = 0;
        foreach (var opt in trimmedOptions)
        {
            dbContext.TblPollOptions.Add(new TblPollOption
            {
                PollId = poll.PollId,
                OptionText = opt,
                DisplayOrder = order++,
                CreatedAt = DateTime.UtcNow
            });
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var refreshedPolls = await GetPollsAsync(communityId, request.GroupId, cancellationToken);
        var created = refreshedPolls.Data?.FirstOrDefault(p => p.PollId == poll.PollId);

        if (created is null)
        {
            // Build a minimal model directly from what we saved
            created = new PollModel(
                poll.PollId,
                post.PostId,
                communityId,
                null,
                request.GroupId,
                null,
                currentUser.UserId!.Value,
                "You",
                null,
                poll.Question,
                post.Content,
                poll.IsMultipleChoice,
                poll.ExpiresAt,
                false,
                0,
                false,
                trimmedOptions.Select((o, i) => new PollOptionModel(0, o, 0, 0, false)).ToList(),
                0, 0, 0, false,
                poll.CreatedAt);
        }

        return Result<PollModel>.Success(created);
    }

    public async Task<Result<PollModel>> VoteAsync(VoteRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PollModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var poll = await dbContext.TblPolls
            .Include(p => p.Post)
            .Include(p => p.TblPollOptions).ThenInclude(o => o.TblPollVotes)
            .FirstOrDefaultAsync(p => p.PollId == request.PollId && !p.IsDeleted, cancellationToken);

        if (poll is null) return Result<PollModel>.Failure("Poll not found.", ResultStatus.NotFound);

        // Check if poll is expired
        if (poll.ExpiresAt.HasValue && poll.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return Result<PollModel>.Failure("This poll has already expired. Voting is closed.", ResultStatus.ValidationError);
        }

        var alreadyVoted = poll.TblPollOptions.Any(o => o.TblPollVotes.Any(v => v.UserId == currentUser.UserId.Value));
        if (alreadyVoted) return Result<PollModel>.Failure("You have already voted on this poll.", ResultStatus.Conflict);

        if (!poll.IsMultipleChoice && request.OptionIds.Count > 1)
        {
            return Result<PollModel>.Failure("This poll only permits a single option vote.", ResultStatus.ValidationError);
        }

        if (request.OptionIds.Count == 0)
        {
            return Result<PollModel>.Failure("Please select at least one option.", ResultStatus.ValidationError);
        }

        foreach (var optionId in request.OptionIds)
        {
            var opt = poll.TblPollOptions.FirstOrDefault(o => o.PollOptionId == optionId);
            if (opt is not null)
            {
                dbContext.TblPollVotes.Add(new TblPollVote
                {
                    PollId = poll.PollId,
                    PollOptionId = optionId,
                    UserId = currentUser.UserId.Value,
                    CreatedAt = DateTime.UtcNow
                });
                opt.VoteCount += 1;
            }
        }

        poll.TotalVotes += request.OptionIds.Count;
        await dbContext.SaveChangesAsync(cancellationToken);

        // Notify poll author
        if (poll.Post != null)
        {
            var voter = await dbContext.TblUsers.FindAsync([currentUser.UserId.Value], cancellationToken);
            var voterName = voter?.DisplayName ?? voter?.UserName ?? "Someone";
            await notificationService.CreateNotificationAsync(
                poll.Post.AuthorId,
                currentUser.UserId.Value,
                "POLL_VOTE",
                "New Poll Vote",
                $"{voterName} voted on your poll",
                "POLL",
                poll.PollId,
                cancellationToken);
        }
        
        var refreshed = await GetPollsAsync(poll.Post?.CommunityId, poll.Post?.GroupId, cancellationToken);
        var model = refreshed.Data?.FirstOrDefault(p => p.PollId == poll.PollId);
        return Result<PollModel>.Success(model!, "Vote recorded.");
    }
}