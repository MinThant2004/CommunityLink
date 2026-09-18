using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Community;

namespace CommunityLink.Domain.Features.Community;

public interface ICommunityService
{
    Task<Result<IReadOnlyList<CommunityModel>>> GetCommunitiesAsync(string? search, CancellationToken cancellationToken = default);
    Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default);
    Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default);
    Task<Result> JoinCommunityAsync(int communityId, CancellationToken cancellationToken = default);
}

public sealed class CommunityService(AppDbContext dbContext, ICurrentUserContext currentUser) : ICommunityService
{
    public async Task<Result<IReadOnlyList<CommunityModel>>> GetCommunitiesAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TblCommunities
            .Include(c => c.Owner)
            .Include(c => c.ParentCommunity)
            .Include(c => c.TblCommunityMembers)
            .Include(c => c.TblPosts)
            .Include(c => c.TblCommunityRatings)
            .Where(c => !c.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) || c.Slug.Contains(search));
        }

        var list = await query.Select(c => new CommunityModel(
            c.CommunityId,
            c.Name,
            c.Slug,
            c.Description,
            c.AvatarUrl,
            c.BannerUrl,
            c.Visibility,
            c.JoinPolicy,
            c.TblCommunityMembers.Count(m => !m.IsDeleted),
            c.TblPosts.Count(p => !p.IsDeleted),
            c.TblCommunityRatings.Any() ? (double)c.TblCommunityRatings.Average(r => r.Score) : 5.0,
            c.OwnerId,
            c.Owner != null ? c.Owner.DisplayName : "Admin",
            c.CreatedAt,
            c.ParentCommunityId,
            c.ParentCommunity != null ? c.ParentCommunity.Name : null)).ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CommunityModel>>.Success(list);
    }

    public async Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default)
    {
        var c = await dbContext.TblCommunities
            .Include(c => c.Owner)
            .Include(c => c.ParentCommunity)
            .Include(c => c.TblCommunityMembers)
            .Include(c => c.TblPosts)
            .Include(c => c.TblCommunityRatings)
            .FirstOrDefaultAsync(c => c.CommunityId == communityId && !c.IsDeleted, cancellationToken);

        if (c is null) return Result<CommunityModel>.Failure("Community not found.", ResultStatus.NotFound);

        var model = new CommunityModel(
            c.CommunityId,
            c.Name,
            c.Slug,
            c.Description,
            c.AvatarUrl,
            c.BannerUrl,
            c.Visibility,
            c.JoinPolicy,
            c.TblCommunityMembers.Count(m => !m.IsDeleted),
            c.TblPosts.Count(p => !p.IsDeleted),
            c.TblCommunityRatings.Any() ? (double)c.TblCommunityRatings.Average(r => r.Score) : 5.0,
            c.OwnerId,
            c.Owner != null ? c.Owner.DisplayName : "Admin",
            c.CreatedAt,
            c.ParentCommunityId,
            c.ParentCommunity != null ? c.ParentCommunity.Name : null);

        return Result<CommunityModel>.Success(model);
    }

    public async Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CommunityModel>.Failure("Community name is required.", ResultStatus.ValidationError);
        }

        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToUpper();

        // Check if name already exists anywhere in dbo.TblCommunity (case-insensitive)
        var nameExists = await dbContext.TblCommunities
            .AnyAsync(c => !c.IsDeleted && c.Name.ToUpper() == normalizedName, cancellationToken);

        if (nameExists)
        {
            var errorMessage = request.ParentCommunityId.HasValue
                ? "This sub-community name is already exist!"
                : "This community name is already exist!";
            return Result<CommunityModel>.Failure(errorMessage, ResultStatus.Conflict);
        }

        // Validate parent community if sub-community
        if (request.ParentCommunityId.HasValue)
        {
            var parentExists = await dbContext.TblCommunities
                .AnyAsync(c => c.CommunityId == request.ParentCommunityId.Value && !c.IsDeleted, cancellationToken);

            if (!parentExists)
            {
                return Result<CommunityModel>.Failure("The selected parent community does not exist.", ResultStatus.NotFound);
            }
        }

        // Resolve owner: use authenticated user or fallback to first available active user (e.g., admin mock data)
        int ownerId;
        if (currentUser.UserId.HasValue)
        {
            ownerId = currentUser.UserId.Value;
        }
        else
        {
            var fallbackUser = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted, cancellationToken);
            if (fallbackUser is null)
            {
                return Result<CommunityModel>.Failure("Default system user not found.", ResultStatus.SystemError);
            }
            ownerId = fallbackUser.UserId;
        }

        var rawSlug = string.IsNullOrWhiteSpace(request.Slug) ? trimmedName.ToLowerInvariant().Replace(" ", "-") : request.Slug.ToLowerInvariant();
        var slug = rawSlug;
        var slugCounter = 1;
        while (await dbContext.TblCommunities.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            slug = $"{rawSlug}-{slugCounter++}";
        }

        var community = new TblCommunity
        {
            Name = trimmedName,
            Slug = slug,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl,
            Visibility = string.IsNullOrWhiteSpace(request.Visibility) ? "PUBLIC" : request.Visibility,
            JoinPolicy = string.IsNullOrWhiteSpace(request.JoinPolicy) ? "INSTANT" : request.JoinPolicy,
            ParentCommunityId = request.ParentCommunityId,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblCommunities.Add(community);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.TblCommunityMembers.Add(new TblCommunityMember
        {
            CommunityId = community.CommunityId,
            UserId = ownerId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetCommunityByIdAsync(community.CommunityId, cancellationToken);
    }

    public async Task<Result> JoinCommunityAsync(int communityId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var community = await dbContext.TblCommunities.FindAsync([communityId], cancellationToken);
        if (community is null || community.IsDeleted) return Result.Failure("Community not found.", ResultStatus.NotFound);

        var existing = await dbContext.TblCommunityMembers
            .FirstOrDefaultAsync(m => m.CommunityId == communityId && m.UserId == currentUser.UserId.Value, cancellationToken);

        if (existing is not null)
        {
            if (!existing.IsDeleted) return Result.Failure("Already a member of this community.", ResultStatus.Conflict);
            existing.IsDeleted = false;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            dbContext.TblCommunityMembers.Add(new TblCommunityMember
            {
                CommunityId = communityId,
                UserId = currentUser.UserId.Value,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Joined community successfully.");
    }
}