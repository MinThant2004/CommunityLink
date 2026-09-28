using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Activity;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.Activity;

public sealed class UserActivityService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser) : IUserActivityService
{
    public async Task<Result<IReadOnlyList<UserActivityModel>>> GetMyActivitiesAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<IReadOnlyList<UserActivityModel>>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var activities = await dbContext.TblUserActivities
            .AsNoTracking()
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(150)
            .Select(a => new UserActivityModel(
                a.ActivityId,
                a.UserId,
                a.ActivityType,
                a.Description,
                a.TargetEntityType,
                a.TargetEntityId,
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<UserActivityModel>>.Success(activities);
    }

    public async Task<Result> DeleteActivityAsync(long activityId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var activity = await dbContext.TblUserActivities
            .FirstOrDefaultAsync(a => a.ActivityId == activityId && a.UserId == userId && !a.IsDeleted, cancellationToken);

        if (activity is null)
        {
            return Result.Failure("Activity record not found.", ResultStatus.NotFound);
        }

        activity.IsDeleted = true;
        activity.DeletedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success("Activity deleted successfully.");
    }

    public async Task RecordActivityAsync(
        int userId,
        string activityType,
        string description,
        string? targetEntityType = null,
        int? targetEntityId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activity = new TblUserActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                TargetEntityType = targetEntityType,
                TargetEntityId = targetEntityId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            dbContext.TblUserActivities.Add(activity);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Do not break caller transaction if non-essential activity recording fails
        }
    }
}
