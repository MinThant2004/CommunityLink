using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Poll;

using CommunityLink.Domain.Features.Notification;
using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Poll;

public interface IPollService
{
    Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId, int? groupId = null, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> VoteAsync(VoteRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<PollModel>> UpdatePollAsync(int pollId, UpdatePollRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> DeletePollAsync(int pollId, CancellationToken cancellationToken = default);
    Task<Result<bool>> TogglePollPrivacyAsync(int pollId, CancellationToken cancellationToken = default);
}

public sealed class PollService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    INotificationService notificationService,
    IPermissionEvaluator permissionEvaluator) : IPollService
{
    public async Task<Result<IReadOnlyList<PollModel>>> GetPollsAsync(int? communityId, int? groupId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = currentUser.UserId;

            if (groupId.HasValue && groupId.Value > 0)
            {
                var grp = await dbContext.TblGroups
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GroupId == groupId.Value && !g.IsDeleted, cancellationToken);

                if (grp == null)
                {
                    return Result<IReadOnlyList<PollModel>>.Failure("Group not found.", ResultStatus.NotFound);
                }

                if (grp.Visibility == "PRIVATE")
                {
                    var isMember = currentUserId.HasValue && await dbContext.TblGroupMembers
                        .AnyAsync(m => m.GroupId == grp.GroupId && m.UserId == currentUserId.Value && !m.IsDeleted, cancellationToken);
                    if (!isMember)
                    {
                        return Result<IReadOnlyList<PollModel>>.Success([]);
                    }
                }
            }

            var baseQuery = dbContext.TblPolls
                .Where(p => !p.IsDeleted);

            // Privacy & Ban Filter according to policy:
            if (currentUser.IsAdmin)
            {
                baseQuery = baseQuery.Where(p => !p.IsPrivate || (currentUserId.HasValue && ((p.Post != null && p.Post.AuthorId == currentUserId.Value) || p.CreatedBy == currentUserId.Value)));
            }
            else
            {
                if (currentUserId.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        (!p.IsPrivate || (p.Post != null && p.Post.AuthorId == currentUserId.Value) || p.CreatedBy == currentUserId.Value) &&
                        (p.IsActive || (p.Post != null && p.Post.AuthorId == currentUserId.Value) || p.CreatedBy == currentUserId.Value));
                }
                else
                {
                    baseQuery = baseQuery.Where(p => !p.IsPrivate && p.IsActive);
                }
            }

            if (groupId.HasValue && groupId.Value > 0)
            {
                baseQuery = baseQuery.Where(p => p.Post != null && p.Post.GroupId == groupId.Value);
            }
            else if (communityId.HasValue && communityId.Value > 0)
            {
                baseQuery = baseQuery.Where(p => p.Post != null && p.Post.CommunityId == communityId.Value);
            }
            else
            {
                // When in general feed, only show group polls if user has joined that group
                List<int> joinedGroupIds = [];
                if (currentUserId.HasValue)
                {
                    joinedGroupIds = await dbContext.TblGroupMembers
                        .Where(m => m.UserId == currentUserId.Value && !m.IsDeleted)
                        .Select(m => m.GroupId)
                        .ToListAsync(cancellationToken);
                }

                baseQuery = baseQuery.Where(p => p.Post == null || p.Post.GroupId == null || (currentUserId.HasValue && joinedGroupIds.Contains(p.Post.GroupId.Value)));
            }

            // 1. Fetch polls with post metadata and precomputed scalar counts
            var rawPolls = await baseQuery
                .OrderByDescending(p => p.CreatedAt)
                .Take(30)
                .Select(p => new
                {
                    p.PollId,
                    p.PostId,
                    CommunityId = p.Post != null ? p.Post.CommunityId : null,
                    CommunityName = p.Post != null && p.Post.Community != null ? p.Post.Community.Name : null,
                    GroupId = p.Post != null ? p.Post.GroupId : null,
                    GroupName = p.Post != null && p.Post.Group != null ? p.Post.Group.Name : null,
                    AuthorId = p.Post != null ? p.Post.AuthorId : (p.CreatedBy ?? 0),
                    AuthorDisplayName = p.Post != null && p.Post.Author != null ? p.Post.Author.DisplayName : null,
                    AuthorUserName = p.Post != null && p.Post.Author != null ? p.Post.Author.UserName : null,
                    AuthorAvatar = p.Post != null && p.Post.Author != null ? p.Post.Author.AvatarUrl : null,
                    p.Question,
                    Content = p.Post != null ? p.Post.Content : null,
                    p.IsMultipleChoice,
                    p.ExpiresAt,
                    LikeCount = p.Post != null ? p.Post.TblPostLikes.Count(l => !l.IsDeleted) : 0,
                    CommentCount = p.Post != null ? p.Post.TblComments.Count(c => !c.IsDeleted) : 0,
                    ShareCount = p.Post != null ? p.Post.TblPostShares.Count(s => !s.IsDeleted) : 0,
                    IsLiked = currentUserId.HasValue && p.Post != null && p.Post.TblPostLikes.Any(l => l.UserId == currentUserId.Value && !l.IsDeleted),
                    p.CreatedAt,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (rawPolls.Count == 0)
            {
                return Result<IReadOnlyList<PollModel>>.Success([]);
            }

            // 2. Fetch all options for these polls in a single, targeted batch query
            var pollIds = rawPolls.Select(p => p.PollId).ToList();

            var rawOptions = await dbContext.TblPollOptions
                .Where(o => pollIds.Contains(o.PollId) && !o.IsDeleted)
                .OrderBy(o => o.DisplayOrder)
                .Select(o => new
                {
                    o.PollId,
                    o.PollOptionId,
                    o.OptionText,
                    VoteCount = o.TblPollVotes.Count(),
                    IsVotedByCurrentUser = currentUserId.HasValue && o.TblPollVotes.Any(v => v.UserId == currentUserId.Value)
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var optionsByPoll = rawOptions.ToLookup(o => o.PollId);

            // 3. Assemble complete PollModel list in memory
            var list = rawPolls.Select(p =>
            {
                var pollOptions = optionsByPoll[p.PollId].ToList();
                var totalVotes = pollOptions.Sum(o => o.VoteCount);
                var hasVoted = currentUserId.HasValue && pollOptions.Any(o => o.IsVotedByCurrentUser);
                var isExpired = p.ExpiresAt.HasValue && p.ExpiresAt.Value <= DateTime.UtcNow;

                var options = pollOptions.Select(o => new PollOptionModel(
                    o.PollOptionId,
                    o.OptionText,
                    o.VoteCount,
                    totalVotes > 0 ? Math.Round((double)o.VoteCount / totalVotes * 100, 1) : 0,
                    o.IsVotedByCurrentUser
                )).ToList();

                var authorName = !string.IsNullOrWhiteSpace(p.AuthorDisplayName)
                    ? p.AuthorDisplayName
                    : (!string.IsNullOrWhiteSpace(p.AuthorUserName) ? p.AuthorUserName : "Unknown");

                return new PollModel(
                    p.PollId,
                    p.PostId,
                    p.CommunityId,
                    p.CommunityName,
                    p.GroupId,
                    p.GroupName,
                    p.AuthorId,
                    authorName,
                    p.AuthorAvatar,
                    p.Question,
                    p.Content,
                    p.IsMultipleChoice,
                    p.ExpiresAt,
                    isExpired,
                    totalVotes,
                    hasVoted,
                    options,
                    p.LikeCount,
                    p.CommentCount,
                    p.ShareCount,
                    p.IsLiked,
                    p.CreatedAt,
                    p.AuthorUserName,
                    p.IsActive,
                    p.IsPrivate,
                    p.ModerationReason
                );
            }).ToList();

            return Result<IReadOnlyList<PollModel>>.Success(list);
        }
        catch (Exception ex) when (ex is OperationCanceledException ||
                                   (ex is Microsoft.Data.SqlClient.SqlException sqlEx && (cancellationToken.IsCancellationRequested || sqlEx.Message.Contains("Operation cancelled by user", StringComparison.OrdinalIgnoreCase))))
        {
            return Result<IReadOnlyList<PollModel>>.Success([]);
        }
    }

    public async Task<Result<PollModel>> CreatePollAsync(CreatePollRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PollModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        // Standalone poll check
        if (!request.GroupId.HasValue)
        {
            var canPostStandalone = await permissionEvaluator.HasPermissionAsync(PermissionCatalog.PostStandaloneCreate, cancellationToken);
            if (!canPostStandalone && !currentUser.IsAdmin)
            {
                return Result<PollModel>.Failure("You can only create polls inside a group you have joined. Standalone polls are not permitted for your role.", ResultStatus.Forbidden);
            }
        }
        
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

            if (!grp.IsActive)
            {
                return Result<PollModel>.Failure("This group is currently inactive/deactivated. New polls cannot be published at this time.", ResultStatus.Forbidden);
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

    public async Task<Result<PollModel>> UpdatePollAsync(int pollId, UpdatePollRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<PollModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var poll = await dbContext.TblPolls
            .Include(p => p.Post)
            .FirstOrDefaultAsync(p => p.PollId == pollId && !p.IsDeleted, cancellationToken);

        if (poll is null || poll.Post is null) return Result<PollModel>.Failure("Poll not found.", ResultStatus.NotFound);

        if (poll.Post.AuthorId != currentUser.UserId.Value)
            return Result<PollModel>.Failure("You can only edit your own polls.", ResultStatus.Forbidden);

        if (string.IsNullOrWhiteSpace(request.Question))
            return Result<PollModel>.Failure("Poll question cannot be empty.", ResultStatus.ValidationError);

        poll.Question = request.Question.Trim();
        if (request.Content != null)
        {
            poll.Post.Content = request.Content.Trim();
            poll.Post.UpdatedAt = DateTime.UtcNow;
            poll.Post.UpdatedBy = currentUser.UserId.Value;
        }

        poll.UpdatedAt = DateTime.UtcNow;
        poll.UpdatedBy = currentUser.UserId.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        var refreshed = await GetPollsAsync(poll.Post.CommunityId, poll.Post.GroupId, cancellationToken);
        var updated = refreshed.Data?.FirstOrDefault(p => p.PollId == poll.PollId);
        return Result<PollModel>.Success(updated!, "Poll updated successfully.");
    }

    public async Task<Result> DeletePollAsync(int pollId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var poll = await dbContext.TblPolls
            .Include(p => p.Post)
            .FirstOrDefaultAsync(p => p.PollId == pollId && !p.IsDeleted, cancellationToken);

        if (poll is null || poll.Post is null) return Result.Failure("Poll not found.", ResultStatus.NotFound);

        if (poll.Post.AuthorId != currentUser.UserId.Value && !currentUser.IsAdmin)
            return Result.Failure("You can only delete your own polls.", ResultStatus.Forbidden);

        poll.IsDeleted = true;
        poll.DeletedAt = DateTime.UtcNow;
        poll.DeletedBy = currentUser.UserId.Value;

        poll.Post.IsDeleted = true;
        poll.Post.DeletedAt = DateTime.UtcNow;
        poll.Post.DeletedBy = currentUser.UserId.Value;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Poll deleted successfully.");
    }

    public async Task<Result<bool>> TogglePollPrivacyAsync(int pollId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<bool>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var poll = await dbContext.TblPolls
            .Include(p => p.Post)
            .FirstOrDefaultAsync(p => p.PollId == pollId && !p.IsDeleted, cancellationToken);

        if (poll is null)
            return Result<bool>.Failure("Poll not found.", ResultStatus.NotFound);

        var authorId = poll.Post?.AuthorId ?? poll.CreatedBy ?? 0;
        if (authorId != currentUser.UserId.Value)
            return Result<bool>.Failure("Only the poll creator can change the privacy of this poll.", ResultStatus.Forbidden);

        poll.IsPrivate = !poll.IsPrivate;
        if (poll.Post != null) poll.Post.IsPrivate = poll.IsPrivate;
        poll.UpdatedAt = DateTime.UtcNow;
        poll.UpdatedBy = currentUser.UserId.Value;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(poll.IsPrivate, poll.IsPrivate ? "Poll set to Private mode." : "Poll set to Public mode.");
    }
}