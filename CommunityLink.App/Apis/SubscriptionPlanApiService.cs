using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class SubscriptionPlanApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<IReadOnlyList<SubscriptionPlanDto>>> GetActivePlansAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<SubscriptionPlanDto>>("api/plans", cancellationToken);

    public Task<Result<SubscriptionPlanDto>> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<SubscriptionPlanDto>($"api/plans/{id}", cancellationToken);

    public Task<Result<IReadOnlyList<SubscriptionPlanDto>>> GetAllPlansAdminAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<SubscriptionPlanDto>>("api/plans/admin/all", cancellationToken);

    public Task<Result<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<SubscriptionPlanDto, CreateSubscriptionPlanRequestDto>("api/plans/admin", request, cancellationToken);

    public Task<Result<SubscriptionPlanDto>> UpdatePlanAsync(int id, UpdateSubscriptionPlanRequestDto request, CancellationToken cancellationToken = default) =>
        PutAsync<SubscriptionPlanDto, UpdateSubscriptionPlanRequestDto>($"api/plans/admin/{id}", request, cancellationToken);

    public Task<Result<bool>> TogglePlanStatusAsync(int id, CancellationToken cancellationToken = default) =>
        PostAsync<bool, object>($"api/plans/admin/{id}/toggle-status", new { }, cancellationToken);

    public Task<Result> DeletePlanAsync(int id, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/plans/admin/{id}", cancellationToken);
}
