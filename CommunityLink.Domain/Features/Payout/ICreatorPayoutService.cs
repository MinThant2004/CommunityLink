namespace CommunityLink.Domain.Features.Payout;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Payout;

public interface ICreatorPayoutService
{
    Task<Result<CreatorPayoutModel>> CreatePayoutRequestAsync(CreatePayoutRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CreatorPayoutModel>>> GetMyPayoutsAsync(CancellationToken cancellationToken = default);
    Task<Result<CreatorPayoutSummaryModel>> GetMyPayoutSummaryAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AdminPayoutModel>>> GetAdminPayoutsAsync(string? statusFilter = null, CancellationToken cancellationToken = default);
    Task<Result<AdminPayoutModel>> ApprovePayoutAsync(long payoutRequestId, string? adminNote = null, CancellationToken cancellationToken = default);
    Task<Result<AdminPayoutModel>> RejectPayoutAsync(long payoutRequestId, string? adminNote = null, CancellationToken cancellationToken = default);
}
