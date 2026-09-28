using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Activity;

namespace CommunityLink.Domain.Features.Activity;

public interface IUserActivityService
{
    Task<Result<IReadOnlyList<UserActivityModel>>> GetMyActivitiesAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteActivityAsync(long activityId, CancellationToken cancellationToken = default);
    Task RecordActivityAsync(int userId, string activityType, string description, string? targetEntityType = null, int? targetEntityId = null, CancellationToken cancellationToken = default);
}
