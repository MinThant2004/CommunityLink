using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class PremiumUpgradeApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<UserSubscriptionStatusDto>> GetMyStatusAsync(CancellationToken cancellationToken = default) =>
        GetAsync<UserSubscriptionStatusDto>("api/premium/my-status", cancellationToken);

    public async Task<Result<IdentityVerificationDetailDto>> ApplyForUpgradeAsync(
        int planId,
        string fullLegalName,
        string? workEmail,
        string? professionalUrl,
        string? paymentMethod,
        byte[] idCardFrontBytes,
        string idCardFrontFileName,
        byte[]? idCardBackBytes,
        string? idCardBackFileName,
        CancellationToken cancellationToken = default)
    {
        var client = CreateClient();

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(planId.ToString()), "planId");
        content.Add(new StringContent(fullLegalName), "fullLegalName");
        if (!string.IsNullOrWhiteSpace(workEmail))
            content.Add(new StringContent(workEmail), "workEmail");
        if (!string.IsNullOrWhiteSpace(professionalUrl))
            content.Add(new StringContent(professionalUrl), "professionalUrl");
        if (!string.IsNullOrWhiteSpace(paymentMethod))
            content.Add(new StringContent(paymentMethod), "paymentMethod");

        var frontContent = new ByteArrayContent(idCardFrontBytes);
        frontContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(frontContent, "idCardFront", idCardFrontFileName);

        if (idCardBackBytes != null && idCardBackBytes.Length > 0 && !string.IsNullOrWhiteSpace(idCardBackFileName))
        {
            var backContent = new ByteArrayContent(idCardBackBytes);
            backContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            content.Add(backContent, "idCardBack", idCardBackFileName);
        }

        var response = await client.PostAsync("api/premium/apply", content, cancellationToken);
        return await ReadResultAsync<IdentityVerificationDetailDto>(response, cancellationToken);
    }

    public Task<Result<IReadOnlyList<IdentityVerificationDetailDto>>> GetPendingVerificationsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<IdentityVerificationDetailDto>>("api/admin/verifications/pending", cancellationToken);

    public Task<Result<IReadOnlyList<IdentityVerificationDetailDto>>> GetAllVerificationsAdminAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<IdentityVerificationDetailDto>>("api/admin/verifications/all", cancellationToken);

    public Task<Result<IdentityVerificationDetailDto>> ApproveVerificationAsync(int id, string? notes, CancellationToken cancellationToken = default) =>
        PostAsync<IdentityVerificationDetailDto, object>($"api/admin/verifications/{id}/approve", new { Notes = notes }, cancellationToken);

    public Task<Result<IdentityVerificationDetailDto>> RejectVerificationAsync(int id, string reason, CancellationToken cancellationToken = default) =>
        PostAsync<IdentityVerificationDetailDto, object>($"api/admin/verifications/{id}/reject", new { Reason = reason }, cancellationToken);
}
