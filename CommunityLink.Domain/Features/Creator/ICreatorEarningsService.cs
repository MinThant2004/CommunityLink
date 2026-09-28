namespace CommunityLink.Domain.Features.Creator;

using CommunityLink.Shared;
using CommunityLink.Shared.Features.Creator;

public interface ICreatorEarningsService
{
    Task<Result<CreatorEarningsDashboardModel>> GetCreatorEarningsAsync(string? filterType = null, int? chatGroupId = null, CancellationToken cancellationToken = default);
}
