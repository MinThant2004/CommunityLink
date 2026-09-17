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
            c.Owner.DisplayName,
            c.CreatedAt)).ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CommunityModel>>.Success(list);
    }

    public async Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default)
    {
        var c = await dbContext.TblCommunities
            .Include(c => c.Owner)
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
            c.Owner.DisplayName,
            c.CreatedAt);

        return Result<CommunityModel>.Success(model);
    }

    public async Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result<CommunityModel>.Failure("Unauthorized", ResultStatus.Unauthorized);

        var slug = string.IsNullOrWhiteSpace(request.Slug) ? request.Name.ToLowerInvariant().Replace(" ", "-") : request.Slug.ToLowerInvariant();

        var community = new TblCommunity
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl,
            Visibility = request.Visibility,
            JoinPolicy = request.JoinPolicy,
            OwnerId = currentUser.UserId.Value,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblCommunities.Add(community);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.TblCommunityMembers.Add(new TblCommunityMember
        {
            CommunityId = community.CommunityId,
            UserId = currentUser.UserId.Value,
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