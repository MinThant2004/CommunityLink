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
        var posts = await _dbContext.TblPosts
            .Include(p => p.Author)
            .Include(p => p.Community)
            .Include(p => p.TblPostImages)
            .Include(p => p.TblPostLikes)
            .Include(p => p.TblComments)
            .Where(p => p.AuthorId == targetUserId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new UserPostItemDto
            {
                PostId = p.PostId,
                Title = string.Empty,
                Content = p.Content,
                CommunityId = p.CommunityId ?? 0,
                CommunityName = p.Community != null ? p.Community.Name : "General",
                AuthorUserId = p.AuthorId,
                AuthorUserName = p.Author.UserName,
                AuthorDisplayName = p.Author.DisplayName,
                AuthorAvatarUrl = p.Author.AvatarUrl,
                LikeCount = p.TblPostLikes.Count(l => !l.IsDeleted),
                CommentCount = p.TblComments.Count(c => !c.IsDeleted),
                ImageUrls = p.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserPostItemDto>>.Success(posts);
    }

    public async Task<Result<List<UserPostItemDto>>> GetSavedPostsAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var posts = await _dbContext.TblSavedPosts
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
            .Where(sp => sp.UserId == currentUserId && !sp.IsDeleted && sp.Post != null && !sp.Post.IsDeleted)
            .OrderByDescending(sp => sp.CreatedAt)
            .Select(sp => new UserPostItemDto
            {
                PostId = sp.Post.PostId,
                Title = string.Empty,
                Content = sp.Post.Content,
                CommunityId = sp.Post.CommunityId ?? 0,
                CommunityName = sp.Post.Community != null ? sp.Post.Community.Name : "General",
                AuthorUserId = sp.Post.AuthorId,
                AuthorUserName = sp.Post.Author.UserName,
                AuthorDisplayName = sp.Post.Author.DisplayName,
                AuthorAvatarUrl = sp.Post.Author.AvatarUrl,
                LikeCount = sp.Post.TblPostLikes.Count(l => !l.IsDeleted),
                CommentCount = sp.Post.TblComments.Count(c => !c.IsDeleted),
                ImageUrls = sp.Post.TblPostImages.Where(i => !i.IsDeleted).OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
                CreatedAt = sp.Post.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<UserPostItemDto>>.Success(posts);
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

        return new UserProfileDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = isOwnerView ? user.Email : string.Empty,
            DisplayName = user.DisplayName,
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
            CreatedAt = user.CreatedAt
        };
    }
}
