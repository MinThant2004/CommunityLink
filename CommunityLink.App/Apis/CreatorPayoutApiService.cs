namespace CommunityLink.App.Apis;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Payout;
using Microsoft.AspNetCore.Http;

public sealed class CreatorPayoutApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<CreatorPayoutModel>> CreatePayoutRequestAsync(CreatePayoutRequestModel request, CancellationToken cancellationToken = default) =>
        PostAsync<CreatorPayoutModel, CreatePayoutRequestModel>("api/creator/payouts", request, cancellationToken);

    public Task<Result<IReadOnlyList<CreatorPayoutModel>>> GetMyPayoutsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CreatorPayoutModel>>("api/creator/payouts", cancellationToken);

    public Task<Result<CreatorPayoutSummaryModel>> GetMyPayoutSummaryAsync(CancellationToken cancellationToken = default) =>
        GetAsync<CreatorPayoutSummaryModel>("api/creator/payouts/summary", cancellationToken);

    public Task<Result<IReadOnlyList<AdminPayoutModel>>> GetAdminPayoutsAsync(string? status = null, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<AdminPayoutModel>>(string.IsNullOrWhiteSpace(status) ? "api/admin/payouts" : $"api/admin/payouts?status={Uri.EscapeDataString(status.Trim())}", cancellationToken);

    public Task<Result<AdminPayoutModel>> ApprovePayoutAsync(long id, string? adminNote = null, CancellationToken cancellationToken = default) =>
        PostAsync<AdminPayoutModel, ReviewPayoutRequestModel>($"api/admin/payouts/{id}/approve", new ReviewPayoutRequestModel(adminNote), cancellationToken);

    public Task<Result<AdminPayoutModel>> RejectPayoutAsync(long id, string? adminNote = null, CancellationToken cancellationToken = default) =>
        PostAsync<AdminPayoutModel, ReviewPayoutRequestModel>($"api/admin/payouts/{id}/reject", new ReviewPayoutRequestModel(adminNote), cancellationToken);
}
