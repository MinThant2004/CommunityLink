using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Report;

namespace CommunityLink.Domain.Features.Report;

public interface IReportService
{
    Task<Result<IReadOnlyList<CommunityReportItemModel>>> GetCommunityReportAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserReportItemModel>>> GetUserReportAsync(CancellationToken cancellationToken = default);
    Task<Result<UserTopContentModel>> GetUserTopContentAsync(int userId, CancellationToken cancellationToken = default);
}
