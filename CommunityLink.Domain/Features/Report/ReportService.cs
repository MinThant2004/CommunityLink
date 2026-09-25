using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Report;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.Report;

public sealed class ReportService(AppDbContext dbContext) : IReportService
{
    public async Task<Result<IReadOnlyList<CommunityReportItemModel>>> GetCommunityReportAsync(CancellationToken cancellationToken = default)
    {
        // Top-level communities only (ParentCommunityId == null)
        var parentCommunities = await dbContext.TblCommunities
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.ParentCommunityId == null)
            .Include(c => c.InverseParentCommunity.Where(sc => !sc.IsDeleted))
                .ThenInclude(sc => sc.TblGroups.Where(g => !g.IsDeleted))
            .Include(c => c.TblGroups.Where(g => !g.IsDeleted))
            .ToListAsync(cancellationToken);

        var reportItems = parentCommunities
            .Select(c =>
            {
                var subCommunities = c.InverseParentCommunity
                    .Select(sc => new SubCommunityReportModel(
                        sc.CommunityId,
                        sc.Name,
                        sc.Description,
                        sc.TblGroups.Count,
                        sc.MemberCount,
                        sc.CreatedAt))
                    .OrderByDescending(sc => sc.GroupCount)
                    .ThenByDescending(sc => sc.MemberCount)
                    .ToList();

                var directGroupCount = c.TblGroups.Count;
                var totalGroups = directGroupCount + subCommunities.Sum(sc => sc.GroupCount);

                return new CommunityReportItemModel(
                    c.CommunityId,
                    c.Name,
                    c.Description,
                    null,
                    subCommunities.Count,
                    totalGroups,
                    c.MemberCount,
                    c.CreatedAt,
                    subCommunities);
            })
            // Ordered by highest sub-community count first per requirement
            .OrderByDescending(c => c.SubCommunityCount)
            .ThenByDescending(c => c.GroupCount)
            .ThenByDescending(c => c.MemberCount)
            .ToList();

        return Result<IReadOnlyList<CommunityReportItemModel>>.Success(reportItems);
    }

    public async Task<Result<IReadOnlyList<UserReportItemModel>>> GetUserReportAsync(CancellationToken cancellationToken = default)
    {
        // Users ordered by followers count (highest first)
        var users = await dbContext.TblUsers
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Select(u => new
            {
                u.UserId,
                u.UserName,
                u.DisplayName,
                u.Email,
                u.AvatarUrl,
                FollowersCount = dbContext.TblUserFollows.Count(f => f.FolloweeId == u.UserId && !f.IsDeleted),
                FollowingCount = dbContext.TblUserFollows.Count(f => f.FollowerId == u.UserId && !f.IsDeleted),
                PostCount = u.TblPosts.Count(p => !p.IsDeleted),
                PollCount = u.TblPosts.Count(p => !p.IsDeleted && p.HasPoll),
                u.CreatedAt
            })
            .OrderByDescending(u => u.FollowersCount)
            .ThenByDescending(u => u.PostCount)
            .ToListAsync(cancellationToken);

        var result = users.Select(u => new UserReportItemModel(
            u.UserId,
            u.UserName,
            u.DisplayName,
            u.Email,
            u.AvatarUrl,
            u.FollowersCount,
            u.FollowingCount,
            u.PostCount,
            u.PollCount,
            u.CreatedAt)).ToList();

        return Result<IReadOnlyList<UserReportItemModel>>.Success(result);
    }

    public async Task<Result<UserTopContentModel>> GetUserTopContentAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.TblUsers
            .AsNoTracking()
            .Where(u => u.UserId == userId && !u.IsDeleted)
            .Select(u => new UserReportItemModel(
                u.UserId,
                u.UserName,
                u.DisplayName,
                u.Email,
                u.AvatarUrl,
                dbContext.TblUserFollows.Count(f => f.FolloweeId == u.UserId && !f.IsDeleted),
                dbContext.TblUserFollows.Count(f => f.FollowerId == u.UserId && !f.IsDeleted),
                u.TblPosts.Count(p => !p.IsDeleted),
                u.TblPosts.Count(p => !p.IsDeleted && p.HasPoll),
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            return Result<UserTopContentModel>.Failure("User not found.", ResultStatus.NotFound);
        }

        // Top Posts (solo or in group) ordered by LikeCount descending
        var posts = await dbContext.TblPosts
            .AsNoTracking()
            .Where(p => p.AuthorId == userId && !p.IsDeleted)
            .Include(p => p.Group)
            .Include(p => p.Community)
            .Include(p => p.TblPostImages.Where(img => !img.IsDeleted))
            .OrderByDescending(p => p.LikeCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(p => new UserTopPostModel(
                p.PostId,
                p.Content,
                p.TblPostImages.Select(img => img.ImageUrl).FirstOrDefault(),
                p.LikeCount,
                p.CommentCount,
                p.CreatedAt,
                p.Group != null ? p.Group.Name : null,
                p.Community != null ? p.Community.Name : null,
                p.HasPoll))
            .ToListAsync(cancellationToken);

        // Top Polls created by user (solo or in group) ordered by TotalVotes / LikeCount descending
        var polls = await dbContext.TblPolls
            .AsNoTracking()
            .Where(pl => !pl.IsDeleted && pl.Post.AuthorId == userId && !pl.Post.IsDeleted)
            .Include(pl => pl.Post)
                .ThenInclude(p => p.Group)
            .Include(pl => pl.TblPollOptions.Where(o => !o.IsDeleted))
            .OrderByDescending(pl => pl.TotalVotes)
            .ThenByDescending(pl => pl.Post.LikeCount)
            .Take(10)
            .Select(pl => new UserTopPollModel(
                pl.PollId,
                pl.Question,
                pl.TotalVotes,
                pl.Post.LikeCount,
                pl.CreatedAt,
                pl.Post.Group != null ? pl.Post.Group.Name : null,
                pl.TblPollOptions.OrderBy(o => o.DisplayOrder).Select(o => o.OptionText).ToList()))
            .ToListAsync(cancellationToken);

        return Result<UserTopContentModel>.Success(new UserTopContentModel(user, posts, polls));
    }
}
