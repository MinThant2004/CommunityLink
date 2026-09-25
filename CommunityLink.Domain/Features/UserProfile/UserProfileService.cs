using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.UserProfile;
using CommunityLink.Domain.Features.Notification;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.UserProfile;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public UserProfileService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<Result<UserProfileDto>> GetOwnerProfileAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.TblUsers
            .Include(u => u.TblUserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.Failure("User profile not found.", ResultStatus.NotFound);

        var profile = await BuildProfileDtoAsync(user, currentUserId, isOwnerView: true, cancellationToken);
        return Result<UserProfileDto>.Success(profile);
    }

    public async Task<Result<UserProfileDto>> GetPublicProfileAsync(string userNameOrId, int? currentUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userNameOrId))
            return Result<UserProfileDto>.Failure("Username or User ID is required.", ResultStatus.ValidationError);

        TblUser? user = null;
        if (int.TryParse(userNameOrId, out var parsedId))
        {
            user = await _dbContext.TblUsers
                .Include(u => u.TblUserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == parsedId && !u.IsDeleted, cancellationToken);
        }

        if (user == null)
        {
            var normalized = userNameOrId.Trim().ToUpperInvariant();
            user = await _dbContext.TblUsers
                .Include(u => u.TblUserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == normalized && !u.IsDeleted, cancellationToken);
        }

        if (user == null)
            return Result<UserProfileDto>.Failure($"User '@{userNameOrId}' does not exist.", ResultStatus.NotFound);

        bool isSelf = currentUserId.HasValue && currentUserId.Value == user.UserId;
        var profile = await BuildProfileDtoAsync(user, currentUserId, isOwnerView: isSelf, cancellationToken);
        return Result<UserProfileDto>.Success(profile);
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(int currentUserId, UpdateUserProfileRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return Result<UserProfileDto>.Failure("Request body cannot be null.", ResultStatus.ValidationError);

        var displayName = dto.DisplayName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length < 2 || displayName.Length > 100)
            return Result<UserProfileDto>.Failure("Display Name is required and must be between 2 and 100 characters.", ResultStatus.ValidationError);

        var userName = dto.UserName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName) || userName.Length < 3 || userName.Length > 50)
            return Result<UserProfileDto>.Failure("Username is required and must be between 3 and 50 characters.", ResultStatus.ValidationError);

        if (!Regex.IsMatch(userName, "^[a-zA-Z0-9_]+$"))
            return Result<UserProfileDto>.Failure("Username can only contain letters, numbers, and underscores.", ResultStatus.ValidationError);

        if (!string.IsNullOrEmpty(dto.Bio) && dto.Bio.Length > 500)
            return Result<UserProfileDto>.Failure("Bio cannot exceed 500 characters.", ResultStatus.ValidationError);

        var user = await _dbContext.TblUsers
            .Include(u => u.TblUserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == currentUserId && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Result<UserProfileDto>.Failure("User profile not found.", ResultStatus.NotFound);

        string normalizedNewUserName = userName.ToUpperInvariant();
        if (user.NormalizedUserName != normalizedNewUserName)
        {
            bool isTaken = await _dbContext.TblUsers
                .AnyAsync(u => u.NormalizedUserName == normalizedNewUserName && u.UserId != currentUserId && !u.IsDeleted, cancellationToken);

            if (isTaken)
                return Result<UserProfileDto>.Failure($"The username '{userName}' is already taken.", ResultStatus.Conflict);

            user.UserName = userName;
            user.NormalizedUserName = normalizedNewUserName;
        }

        user.DisplayName = displayName;
        user.Headline = dto.Headline?.Trim();
        user.Pronouns = dto.Pronouns?.Trim();
        user.Location = dto.Location?.Trim();
        user.AvailabilityStatus = dto.AvailabilityStatus?.Trim();
        user.Bio = dto.Bio?.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedProfile = await BuildProfileDtoAsync(user, currentUserId, isOwnerView: true, cancellationToken);
        return Result<UserProfileDto>.Success(updatedProfile, "Profile updated successfully.");
    }

    public async Task<Result<UploadAvatarResponseDto>> UploadAvatarAsync(int currentUserId, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.TblUsers.FirstOrDefaultAsync(u => u.UserId == currentUserId && !u.IsDeleted, cancellationToken);
        if (user == null)
            return Result<UploadAvatarResponseDto>.Failure("User not found.", ResultStatus.NotFound);

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            return Result<UploadAvatarResponseDto>.Failure("Only JPEG, PNG, and WebP images are allowed.", ResultStatus.ValidationError);

        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
        }

        var uniqueFileName = $"avatar_{currentUserId}_{DateTime.UtcNow.Ticks}{ext}";
        var fullPath = Path.Combine(wwwrootPath, uniqueFileName);

        using (var destStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destStream, cancellationToken);
        }

        var avatarUrl = $"/uploads/avatars/{uniqueFileName}";
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<UploadAvatarResponseDto>.Success(new UploadAvatarResponseDto { AvatarUrl = avatarUrl }, "Avatar updated successfully.");
    }

    public async Task<Result<bool>> ToggleSaveAccountAsync(int currentUserId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
            return Result<bool>.Failure("You cannot save your own profile.", ResultStatus.ValidationError);

        var targetExists = await _dbContext.TblUsers.AnyAsync(u => u.UserId == targetUserId && !u.IsDeleted, cancellationToken);
        if (!targetExists)
            return Result<bool>.Failure("Target user not found.", ResultStatus.NotFound);

        var existingSave = await _dbContext.TblSavedAccounts
            .FirstOrDefaultAsync(s => s.UserId == currentUserId && s.SavedUserId == targetUserId && !s.IsDeleted, cancellationToken);

        bool isSavedNow;
        if (existingSave != null)
        {
            _dbContext.TblSavedAccounts.Remove(existingSave);
            isSavedNow = false;
        }
        else
        {
            var newSave = new TblSavedAccount
            {
                UserId = currentUserId,
                SavedUserId = targetUserId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };
            await _dbContext.TblSavedAccounts.AddAsync(newSave, cancellationToken);
            isSavedNow = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(isSavedNow, isSavedNow ? "Saved account successfully." : "Removed saved account.");
    }

    public async Task<Result<UserProfileDto>> RateUserAsync(int currentUserId, int targetUserId, RateUserRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
            return Result<UserProfileDto>.Failure("Users cannot rate their own profile.", ResultStatus.ValidationError);

        if (dto == null || dto.Score < 1 || dto.Score > 5)
            return Result<UserProfileDto>.Failure("Rating score must be between 1 and 5 stars.", ResultStatus.ValidationError);

        if (!string.IsNullOrEmpty(dto.ReviewText) && dto.ReviewText.Length > 500)
            return Result<UserProfileDto>.Failure("Review text cannot exceed 500 characters.", ResultStatus.ValidationError);

        var targetUser = await _dbContext.TblUsers
            .Include(u => u.TblUserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserId == targetUserId && !u.IsDeleted, cancellationToken);

        if (targetUser == null)
            return Result<UserProfileDto>.Failure("Target user not found.", ResultStatus.NotFound);

        var existingRating = await _dbContext.TblUserRatings
            .FirstOrDefaultAsync(r => r.RaterUserId == currentUserId && r.TargetUserId == targetUserId && !r.IsDeleted, cancellationToken);

        if (existingRating != null)
        {
            existingRating.Score = dto.Score;
            existingRating.ReviewText = dto.ReviewText?.Trim();
            existingRating.UpdatedAt = DateTime.UtcNow;
            existingRating.UpdatedBy = currentUserId;
        }
        else
        {
            var rating = new TblUserRating
            {
                RaterUserId = currentUserId,
                TargetUserId = targetUserId,
                Score = dto.Score,
                ReviewText = dto.ReviewText?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };
            await _dbContext.TblUserRatings.AddAsync(rating, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Recalculate AverageRating & RatingCount for target user
        var ratings = await _dbContext.TblUserRatings
            .Where(r => r.TargetUserId == targetUserId && !r.IsDeleted)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);

        if (ratings.Any())
        {
            targetUser.AverageRating = Math.Round((decimal)ratings.Average(), 2);
            targetUser.RatingCount = ratings.Count;
        }
        else
        {
            targetUser.AverageRating = null;
            targetUser.RatingCount = 0;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var profile = await BuildProfileDtoAsync(targetUser, currentUserId, isOwnerView: false, cancellationToken);
        return Result<UserProfileDto>.Success(profile, "Rating submitted successfully.");
    }

    public async Task<Result<List<UserPostItemDto>>> GetUserPostsAsync(int targetUserId, int? currentUserId, CancellationToken cancellationToken = default)
    {
        var rawPosts = await _dbContext.TblPosts
            .Include(p => p.Author)
            .Include(p => p.Community)
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Include(p => p.TblPolls)
                .ThenInclude(poll => poll.TblPollOptions)
            .Include(p => p.TblPolls)
                .ThenInclude(poll => poll.TblPollVotes)
            .Where(p => p.AuthorId == targetUserId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var list = rawPosts.Select(p => MapPostToDto(p, currentUserId)).ToList();
        return Result<List<UserPostItemDto>>.Success(list);
    }

    public async Task<Result<List<UserPostItemDto>>> GetSavedPostsAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var rawPosts = await _dbContext.TblSavedPosts
            .Include(sp => sp.Post)
                .ThenInclude(p => p.Author)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.Community)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.TblPostImages)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.TblPostLikes)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.TblComments)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.TblPolls)
                    .ThenInclude(poll => poll.TblPollOptions)
            .Include(sp => sp.Post)
                .ThenInclude(p => p.TblPolls)
                    .ThenInclude(poll => poll.TblPollVotes)
            .Where(sp => sp.UserId == currentUserId && !sp.IsDeleted && sp.Post != null && !sp.Post.IsDeleted)
            .OrderByDescending(sp => sp.CreatedAt)
            .Select(sp => sp.Post)
            .ToListAsync(cancellationToken);

        var list = rawPosts.Select(p => MapPostToDto(p, currentUserId)).ToList();
        return Result<List<UserPostItemDto>>.Success(list);
    }

    public async Task<Result<List<UserPostItemDto>>> GetRecycledPostsAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var tenDaysAgo = DateTime.UtcNow.AddDays(-10);

        // Auto-purge any recycled posts older than 10 days
        var expiredPosts = await _dbContext.TblPosts
            .Where(p => p.AuthorId == currentUserId && p.IsDeleted && p.DeletedAt.HasValue && p.DeletedAt.Value < tenDaysAgo)
            .ToListAsync(cancellationToken);

        if (expiredPosts.Any())
        {
            _dbContext.TblPosts.RemoveRange(expiredPosts);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var recycledPosts = await _dbContext.TblPosts
            .Include(p => p.Author)
            .Include(p => p.Community)
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Include(p => p.TblPolls)
                .ThenInclude(poll => poll.TblPollOptions)
            .Include(p => p.TblPolls)
                .ThenInclude(poll => poll.TblPollVotes)
            .Where(p => p.AuthorId == currentUserId && p.IsDeleted && (!p.DeletedAt.HasValue || p.DeletedAt.Value >= tenDaysAgo))
            .OrderByDescending(p => p.DeletedAt ?? p.CreatedAt)
            .ToListAsync(cancellationToken);

        var list = recycledPosts.Select(p => MapPostToDto(p, currentUserId)).ToList();
        return Result<List<UserPostItemDto>>.Success(list);
    }

    public async Task<Result> RestorePostAsync(int currentUserId, int postId, CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.TblPosts
            .FirstOrDefaultAsync(p => p.PostId == postId && p.AuthorId == currentUserId && p.IsDeleted, cancellationToken);

        if (post == null)
            return Result.Failure("Recycled post not found.", ResultStatus.NotFound);

        post.IsDeleted = false;
        post.DeletedAt = null;
        post.DeletedBy = null;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Post restored successfully.");
    }

    public async Task<Result> PermanentlyDeletePostAsync(int currentUserId, int postId, CancellationToken cancellationToken = default)
    {
        var post = await _dbContext.TblPosts
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Include(p => p.TblSavedPosts)
            .Include(p => p.TblPostShares)
            .Include(p => p.TblPolls)
            .FirstOrDefaultAsync(p => p.PostId == postId && p.AuthorId == currentUserId, cancellationToken);

        if (post == null)
            return Result.Failure("Post not found.", ResultStatus.NotFound);

        // Permanently remove related child records if any exist
        if (post.TblPostImages.Any()) _dbContext.TblPostImages.RemoveRange(post.TblPostImages);
        if (post.TblPostLikes.Any()) _dbContext.TblPostLikes.RemoveRange(post.TblPostLikes);
        if (post.TblComments.Any()) _dbContext.TblComments.RemoveRange(post.TblComments);
        if (post.TblSavedPosts.Any()) _dbContext.TblSavedPosts.RemoveRange(post.TblSavedPosts);
        if (post.TblPostShares.Any()) _dbContext.TblPostShares.RemoveRange(post.TblPostShares);

        _dbContext.TblPosts.Remove(post);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Post permanently deleted.");
    }

    public async Task<Result<List<SavedAccountItemDto>>> GetSavedAccountsAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var savedAccounts = await _dbContext.TblSavedAccounts
            .Include(sa => sa.SavedUser)
            .Where(sa => sa.UserId == currentUserId && !sa.IsDeleted && sa.SavedUser != null && !sa.SavedUser.IsDeleted)
            .OrderByDescending(sa => sa.CreatedAt)
            .Select(sa => new SavedAccountItemDto
            {
                SavedAccountId = sa.SavedAccountId,
                SavedUserId = sa.SavedUserId,
                UserName = sa.SavedUser.UserName,
                DisplayName = sa.SavedUser.DisplayName,
                AvatarUrl = sa.SavedUser.AvatarUrl,
                Bio = sa.SavedUser.Bio,
                IsVerified = sa.SavedUser.IsVerified,
                AverageRating = sa.SavedUser.AverageRating,
                RatingCount = sa.SavedUser.RatingCount,
                SavedAt = sa.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<SavedAccountItemDto>>.Success(savedAccounts);
    }

    public async Task<Result<List<UserCommunityItemDto>>> GetUserCommunitiesAsync(int targetUserId, CancellationToken cancellationToken = default)
    {
        var communities = await _dbContext.TblCommunityMembers
            .Include(cm => cm.Community)
                .ThenInclude(c => c.TblCommunityMembers)
            .Where(cm => cm.UserId == targetUserId && !cm.IsDeleted && cm.Community != null && !cm.Community.IsDeleted)
            .OrderByDescending(cm => cm.CreatedAt)
            .Select(cm => new UserCommunityItemDto
            {
                CommunityId = cm.CommunityId,
                Name = cm.Community.Name,
                Description = cm.Community.Description ?? string.Empty,
                RoleName = cm.Role ?? "Member",
                MemberCount = cm.Community.TblCommunityMembers.Count(m => !m.IsDeleted),
                JoinedAt = cm.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserCommunityItemDto>>.Success(communities);
    }

    public async Task<Result<List<UserRatingItemDto>>> GetUserReviewsAsync(int targetUserId, CancellationToken cancellationToken = default)
    {
        var reviews = await _dbContext.TblUserRatings
            .Include(ur => ur.RaterUser)
            .Where(ur => ur.TargetUserId == targetUserId && !ur.IsDeleted)
            .OrderByDescending(ur => ur.CreatedAt)
            .Select(ur => new UserRatingItemDto
            {
                UserRatingId = ur.UserRatingId,
                RaterUserId = ur.RaterUserId,
                RaterUserName = ur.RaterUser != null ? ur.RaterUser.UserName : "Anonymous",
                RaterDisplayName = ur.RaterUser != null ? ur.RaterUser.DisplayName : "Anonymous User",
                RaterAvatarUrl = ur.RaterUser != null ? ur.RaterUser.AvatarUrl : null,
                Score = ur.Score,
                ReviewText = ur.ReviewText,
                CreatedAt = ur.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserRatingItemDto>>.Success(reviews);
    }

    public async Task<Result<bool>> ToggleFollowUserAsync(int currentUserId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (currentUserId == targetUserId)
            return Result<bool>.Failure("You cannot follow yourself.", ResultStatus.ValidationError);

        var targetExists = await _dbContext.TblUsers.AnyAsync(u => u.UserId == targetUserId && !u.IsDeleted, cancellationToken);
        if (!targetExists)
            return Result<bool>.Failure("Target user not found.", ResultStatus.NotFound);

        var existingFollow = await _dbContext.TblUserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FolloweeId == targetUserId && !f.IsDeleted, cancellationToken);

        bool isFollowed;
        if (existingFollow != null)
        {
            _dbContext.TblUserFollows.Remove(existingFollow);
            isFollowed = false;
        }
        else
        {
            var newFollow = new TblUserFollow
            {
                FollowerId = currentUserId,
                FolloweeId = targetUserId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            };
            await _dbContext.TblUserFollows.AddAsync(newFollow, cancellationToken);
            isFollowed = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (isFollowed)
        {
            var followerUser = await _dbContext.TblUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == currentUserId, cancellationToken);

            var followerName = followerUser != null
                ? (!string.IsNullOrWhiteSpace(followerUser.DisplayName) ? followerUser.DisplayName : followerUser.UserName)
                : "Someone";

            await _notificationService.CreateNotificationAsync(
                recipientUserId: targetUserId,
                actorUserId: currentUserId,
                notificationType: "USER_FOLLOW",
                title: "New Follower",
                message: $"{followerName} started following you.",
                targetEntityName: "USER_FOLLOWERS",
                targetEntityId: currentUserId,
                cancellationToken: cancellationToken);
        }

        return Result<bool>.Success(isFollowed, isFollowed ? "Followed user successfully." : "Unfollowed user successfully.");
    }

    public async Task<Result<List<FollowUserItemDto>>> GetFollowersAsync(int targetUserId, CancellationToken cancellationToken = default)
    {
        var followers = await _dbContext.TblUserFollows
            .Include(f => f.Follower)
            .Where(f => f.FolloweeId == targetUserId && !f.IsDeleted && f.Follower != null && !f.Follower.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowUserItemDto
            {
                UserId = f.Follower.UserId,
                UserName = f.Follower.UserName,
                DisplayName = f.Follower.DisplayName,
                AvatarUrl = f.Follower.AvatarUrl,
                Bio = f.Follower.Bio,
                IsVerified = f.Follower.IsVerified,
                FollowedAt = f.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<FollowUserItemDto>>.Success(followers);
    }

    public async Task<Result<List<FollowUserItemDto>>> GetFollowingAsync(int targetUserId, CancellationToken cancellationToken = default)
    {
        var following = await _dbContext.TblUserFollows
            .Include(f => f.Followee)
            .Where(f => f.FollowerId == targetUserId && !f.IsDeleted && f.Followee != null && !f.Followee.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FollowUserItemDto
            {
                UserId = f.Followee.UserId,
                UserName = f.Followee.UserName,
                DisplayName = f.Followee.DisplayName,
                AvatarUrl = f.Followee.AvatarUrl,
                Bio = f.Followee.Bio,
                IsVerified = f.Followee.IsVerified,
                FollowedAt = f.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<FollowUserItemDto>>.Success(following);
    }

    public async Task<Result<List<UserSharedPostItemDto>>> GetUserSharesAsync(int targetUserId, int? currentUserId, CancellationToken cancellationToken = default)
    {
        var shares = await _dbContext.TblPostShares
            .Include(ps => ps.Post)
                .ThenInclude(p => p.Author)
            .Include(ps => ps.Post)
                .ThenInclude(p => p.Community)
            .Include(ps => ps.Post)
                .ThenInclude(p => p.Group)
                    .ThenInclude(g => g!.TblGroupMembers)
            .Include(ps => ps.Post)
                .ThenInclude(p => p.TblPostImages)
            .Include(ps => ps.Post)
                .ThenInclude(p => p.TblPostLikes)
            .Include(ps => ps.Post)
                .ThenInclude(p => p.TblComments)
            .Where(ps => ps.UserId == targetUserId && !ps.IsDeleted && ps.Post != null && !ps.Post.IsDeleted)
            .OrderByDescending(ps => ps.CreatedAt)
            .ToListAsync(cancellationToken);

        var list = new List<UserSharedPostItemDto>();
        foreach (var s in shares)
        {
            var p = s.Post;
            if (p.Group != null && p.Group.Visibility == "PRIVATE")
            {
                var isMember = currentUserId.HasValue && p.Group.TblGroupMembers.Any(m => m.UserId == currentUserId.Value && !m.IsDeleted);
                if (!isMember) continue;
            }

            list.Add(new UserSharedPostItemDto
            {
                ShareId = s.PostShareId,
                PostId = s.PostId,
                ShareNote = s.ShareNote,
                SharedAt = s.CreatedAt,
                PostContent = p.Content,
                AuthorUserId = p.AuthorId,
                AuthorUserName = p.Author.UserName,
                AuthorDisplayName = p.Author.DisplayName,
                AuthorAvatarUrl = p.Author.AvatarUrl,
                CommunityName = p.Community?.Name,
                GroupName = p.Group?.Name,
                LikeCount = p.TblPostLikes.Count(l => !l.IsDeleted),
                CommentCount = p.TblComments.Count(c => !c.IsDeleted),
                ImageUrls = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
                PostCreatedAt = p.CreatedAt
            });
        }

        return Result<List<UserSharedPostItemDto>>.Success(list);
    }

    private async Task<UserProfileDto> BuildProfileDtoAsync(TblUser user, int? currentUserId, bool isOwnerView, CancellationToken cancellationToken)
    {
        // 1. Role Info & Badge
        var primaryRole = user.TblUserRoles.FirstOrDefault(ur => !ur.IsDeleted)?.Role;
        var roleCode = primaryRole?.RoleCode ?? "USER";
        var roleName = primaryRole?.RoleName ?? "Member";
        var badge = roleCode switch
        {
            "DOMAIN_PRO" => "⭐",
            "PUBLIC_FIGURE" or "ADMIN" or "SUPER_ADMIN" => "🏛️",
            _ => "👤"
        };

        // 2. Metrics
        int joinedCommunitiesCount = await _dbContext.TblCommunityMembers
            .CountAsync(cm => cm.UserId == user.UserId && !cm.IsDeleted, cancellationToken);

        int savedAccountsCount = isOwnerView ? await _dbContext.TblSavedAccounts
            .CountAsync(sa => sa.UserId == user.UserId && !sa.IsDeleted, cancellationToken) : 0;

        int postsCount = await _dbContext.TblPosts
            .CountAsync(p => p.AuthorId == user.UserId && !p.IsDeleted, cancellationToken);

        int followersCount = await _dbContext.TblUserFollows
            .CountAsync(f => f.FolloweeId == user.UserId && !f.IsDeleted, cancellationToken);

        int followingCount = await _dbContext.TblUserFollows
            .CountAsync(f => f.FollowerId == user.UserId && !f.IsDeleted, cancellationToken);

        int sharesCount = await _dbContext.TblPostShares
            .CountAsync(ps => ps.UserId == user.UserId && !ps.IsDeleted && ps.Post != null && !ps.Post.IsDeleted, cancellationToken);

        // 3. Relationship
        bool isSelf = currentUserId.HasValue && currentUserId.Value == user.UserId;
        bool isSavedByMe = false;
        bool isFollowedByMe = false;
        bool hasRated = false;
        int? myRatingScore = null;
        string? myReviewText = null;

        if (currentUserId.HasValue && !isSelf)
        {
            isSavedByMe = await _dbContext.TblSavedAccounts
                .AnyAsync(sa => sa.UserId == currentUserId.Value && sa.SavedUserId == user.UserId && !sa.IsDeleted, cancellationToken);

            isFollowedByMe = await _dbContext.TblUserFollows
                .AnyAsync(f => f.FollowerId == currentUserId.Value && f.FolloweeId == user.UserId && !f.IsDeleted, cancellationToken);

            var rating = await _dbContext.TblUserRatings
                .FirstOrDefaultAsync(r => r.RaterUserId == currentUserId.Value && r.TargetUserId == user.UserId && !r.IsDeleted, cancellationToken);

            if (rating != null)
            {
                hasRated = true;
                myRatingScore = rating.Score;
                myReviewText = rating.ReviewText;
            }
        }

        // 4. Rating Distribution Breakdown (5★ to 1★ percentages)
        var ratingScores = await _dbContext.TblUserRatings
            .Where(r => r.TargetUserId == user.UserId && !r.IsDeleted)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);

        var ratingDistribution = new RatingDistributionDto();
        if (ratingScores.Any())
        {
            int total = ratingScores.Count;
            ratingDistribution.Star5Count = ratingScores.Count(s => s == 5);
            ratingDistribution.Star4Count = ratingScores.Count(s => s == 4);
            ratingDistribution.Star3Count = ratingScores.Count(s => s == 3);
            ratingDistribution.Star2Count = ratingScores.Count(s => s == 2);
            ratingDistribution.Star1Count = ratingScores.Count(s => s == 1);

            ratingDistribution.Star5Percentage = (int)Math.Round((double)ratingDistribution.Star5Count * 100 / total);
            ratingDistribution.Star4Percentage = (int)Math.Round((double)ratingDistribution.Star4Count * 100 / total);
            ratingDistribution.Star3Percentage = (int)Math.Round((double)ratingDistribution.Star3Count * 100 / total);
            ratingDistribution.Star2Percentage = (int)Math.Round((double)ratingDistribution.Star2Count * 100 / total);
            ratingDistribution.Star1Percentage = (int)Math.Round((double)ratingDistribution.Star1Count * 100 / total);
        }

        // 5. Domain Competencies / Skills
        var userSkills = await _dbContext.TblUserSkills
            .Include(s => s.TblSkillEndorsements)
            .Where(s => s.UserId == user.UserId && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .ThenByDescending(s => s.EndorsementCount)
            .Select(s => new UserSkillDto
            {
                SkillId = s.SkillId,
                SkillName = s.SkillName,
                EndorsementCount = s.EndorsementCount,
                IsVerified = s.IsVerified,
                IsEndorsedByMe = currentUserId.HasValue && s.TblSkillEndorsements.Any(e => e.EndorserUserId == currentUserId.Value && !e.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        // 6. Identity Verification Audit
        var audit = await _dbContext.TblUserVerificationAudits
            .Where(a => a.UserId == user.UserId && !a.IsDeleted)
            .OrderByDescending(a => a.AuditedAt)
            .Select(a => new UserVerificationAuditDto
            {
                AuditCode = a.AuditCode,
                AuditTitle = a.AuditTitle,
                AuditDescription = a.AuditDescription,
                Authority = a.Authority,
                Status = a.Status,
                AuditedAt = a.AuditedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new UserProfileDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = isOwnerView ? user.Email : string.Empty,
            DisplayName = user.DisplayName,
            Headline = user.Headline,
            Pronouns = user.Pronouns,
            Location = user.Location,
            AvailabilityStatus = user.AvailabilityStatus,
            ResponseSlaText = user.ResponseSlaText ?? "< 2 hrs Response",
            PercentileBadgeText = user.PercentileBadgeText ?? "Top 1% Percentile",
            IsOnline = user.LastActiveAt.HasValue && user.LastActiveAt.Value > DateTime.UtcNow.AddMinutes(-10),
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            IsVerified = user.IsVerified,
            Role = new UserRoleInfoDto
            {
                RoleCode = roleCode,
                RoleName = roleName,
                Badge = badge
            },
            Metrics = new UserProfileMetricsDto
            {
                AverageRating = user.AverageRating,
                RatingCount = user.RatingCount,
                JoinedCommunitiesCount = joinedCommunitiesCount,
                SavedAccountsCount = savedAccountsCount,
                PostsCount = postsCount,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                SharesCount = sharesCount
            },
            Relationship = new UserRelationshipInfoDto
            {
                IsSelf = isSelf,
                IsSavedByMe = isSavedByMe,
                IsFollowedByMe = isFollowedByMe,
                CanMessage = !isSelf,
                HasRated = hasRated,
                MyRatingScore = myRatingScore,
                MyReviewText = myReviewText
            },
            RatingDistribution = ratingDistribution,
            Skills = userSkills,
            VerificationAudit = audit,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<Result<bool>> EndorseSkillAsync(int currentUserId, int skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _dbContext.TblUserSkills
            .FirstOrDefaultAsync(s => s.SkillId == skillId && !s.IsDeleted, cancellationToken);

        if (skill == null)
            return Result<bool>.Failure("Competency skill not found.", ResultStatus.NotFound);

        if (skill.UserId == currentUserId)
            return Result<bool>.Failure("You cannot endorse your own competency skill.", ResultStatus.ValidationError);

        var existing = await _dbContext.TblSkillEndorsements
            .FirstOrDefaultAsync(e => e.SkillId == skillId && e.EndorserUserId == currentUserId, cancellationToken);

        bool isEndorsed;
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                skill.EndorsementCount++;
                isEndorsed = true;
            }
            else
            {
                existing.IsDeleted = true;
                skill.EndorsementCount = Math.Max(0, skill.EndorsementCount - 1);
                isEndorsed = false;
            }
        }
        else
        {
            var endorsement = new TblSkillEndorsement
            {
                SkillId = skillId,
                EndorserUserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _dbContext.TblSkillEndorsements.AddAsync(endorsement, cancellationToken);
            skill.EndorsementCount++;
            isEndorsed = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(isEndorsed, isEndorsed ? "Skill endorsed successfully." : "Endorsement removed.");
    }

    public async Task<Result<bool>> VotePollAsync(int currentUserId, int pollId, int optionId, CancellationToken cancellationToken = default)
    {
        var poll = await _dbContext.TblPolls
            .Include(p => p.TblPollOptions)
            .Include(p => p.TblPollVotes)
            .FirstOrDefaultAsync(p => p.PollId == pollId && !p.IsDeleted, cancellationToken);

        if (poll == null)
            return Result<bool>.Failure("Poll not found.", ResultStatus.NotFound);

        bool isPollActive = !poll.IsDeleted && (!poll.ExpiresAt.HasValue || poll.ExpiresAt.Value > DateTime.UtcNow);
        if (!isPollActive)
            return Result<bool>.Failure("This poll is closed.", ResultStatus.ValidationError);

        var option = poll.TblPollOptions.FirstOrDefault(o => o.PollOptionId == optionId && !o.IsDeleted);
        if (option == null)
            return Result<bool>.Failure("Selected option is invalid.", ResultStatus.ValidationError);

        var existingVote = poll.TblPollVotes.FirstOrDefault(v => v.UserId == currentUserId && !v.IsDeleted);
        if (existingVote != null)
        {
            return Result<bool>.Failure("You have already voted in this poll.", ResultStatus.Conflict);
        }

        var vote = new TblPollVote
        {
            PollId = pollId,
            PollOptionId = optionId,
            UserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId,
            IsDeleted = false
        };

        option.VoteCount++;
        poll.TotalVotes++;

        await _dbContext.TblPollVotes.AddAsync(vote, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Vote cast successfully.");
    }

    private static UserPostItemDto MapPostToDto(TblPost p, int? currentUserId)
    {
        UserPollDetailDto? pollDto = null;
        if (p.HasPoll && p.TblPolls.Any(pl => !pl.IsDeleted))
        {
            var poll = p.TblPolls.First(pl => !pl.IsDeleted);
            var myVote = currentUserId.HasValue
                ? poll.TblPollVotes.FirstOrDefault(v => v.UserId == currentUserId.Value && !v.IsDeleted)
                : null;

            int totalVotes = poll.TotalVotes > 0 ? poll.TotalVotes : poll.TblPollOptions.Sum(o => o.VoteCount);
            bool isPollActive = !poll.IsDeleted && (!poll.ExpiresAt.HasValue || poll.ExpiresAt.Value > DateTime.UtcNow);

            pollDto = new UserPollDetailDto
            {
                PollId = poll.PollId,
                Question = poll.Question,
                TotalVotes = totalVotes,
                IsActive = isPollActive,
                ExpiresAt = poll.ExpiresAt,
                HasVoted = myVote != null,
                MyVotedOptionId = myVote?.PollOptionId,
                Options = poll.TblPollOptions.Where(o => !o.IsDeleted).OrderBy(o => o.DisplayOrder).Select(o => new UserPollOptionDto
                {
                    OptionId = o.PollOptionId,
                    OptionText = o.OptionText,
                    VoteCount = o.VoteCount,
                    VotePercentage = totalVotes > 0 ? (int)Math.Round((double)o.VoteCount * 100 / totalVotes) : 0,
                    IsSelectedByMe = myVote != null && myVote.PollOptionId == o.PollOptionId
                }).ToList()
            };
        }

        return new UserPostItemDto
        {
            PostId = p.PostId,
            Title = string.Empty,
            Subtitle = p.Subtitle,
            Content = p.Content,
            PostType = p.PostType ?? "STANDARD",
            CodeSnippet = p.CodeSnippet,
            CodeLanguage = p.CodeLanguage,
            CodeFileName = p.CodeFileName,
            DiagramImageUrl = p.DiagramImageUrl,
            DiagramCaption = p.DiagramCaption,
            HasPoll = p.HasPoll,
            Poll = pollDto,
            CommunityId = p.CommunityId ?? 0,
            CommunityName = p.Community != null ? p.Community.Name : "General",
            AuthorUserId = p.AuthorId,
            AuthorUserName = p.Author.UserName,
            AuthorDisplayName = p.Author.DisplayName,
            AuthorAvatarUrl = p.Author.AvatarUrl,
            LikeCount = p.TblPostLikes.Count(l => !l.IsDeleted),
            CommentCount = p.TblComments.Count(c => !c.IsDeleted),
            ImageUrls = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
            CreatedAt = p.CreatedAt,
            DeletedAt = p.DeletedAt
        };
    }
}
