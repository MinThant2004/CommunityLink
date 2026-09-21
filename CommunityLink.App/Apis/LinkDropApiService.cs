using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.LinkDrop;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class LinkDropApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    // =========================================================================
    // USER METHODS
    // =========================================================================

    public Task<Result<IReadOnlyList<LinkDropPackageDto>>> GetActivePackagesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<LinkDropPackageDto>>("api/linkdrops/packages", cancellationToken);

    public Task<Result<decimal>> GetConversionRateAsync(CancellationToken cancellationToken = default) =>
        GetAsync<decimal>("api/linkdrops/rate", cancellationToken);

    public Task<Result<IReadOnlyList<PaymentMethodDto>>> GetActivePaymentMethodsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<PaymentMethodDto>>("api/linkdrops/payment-methods", cancellationToken);

    public async Task<Result<PurchaseResponseDto>> SubmitPurchaseAsync(
        CreatePurchaseRequestDto request,
        IBrowserFile? proofFile = null,
        CancellationToken cancellationToken = default)
    {
        using var client = CreateClient();
        using var content = new MultipartFormDataContent();

        if (request.PackageId.HasValue)
            content.Add(new StringContent(request.PackageId.Value.ToString()), "PackageId");

        content.Add(new StringContent(request.PaymentMethodId.ToString()), "PaymentMethodId");
        content.Add(new StringContent(request.IsCustomPurchase.ToString().ToLowerInvariant()), "IsCustomPurchase");

        if (request.CustomRealMoneyAmount.HasValue)
            content.Add(new StringContent(request.CustomRealMoneyAmount.Value.ToString()), "CustomRealMoneyAmount");

        if (request.CustomLinkDropAmount.HasValue)
            content.Add(new StringContent(request.CustomLinkDropAmount.Value.ToString()), "CustomLinkDropAmount");

        content.Add(new StringContent(request.TransactionReferenceNo ?? ""), "TransactionReferenceNo");

        if (!string.IsNullOrWhiteSpace(request.UserNotes))
            content.Add(new StringContent(request.UserNotes), "UserNotes");

        if (!string.IsNullOrWhiteSpace(request.ProofFileUrl))
            content.Add(new StringContent(request.ProofFileUrl), "ProofFileUrl");

        if (proofFile != null)
        {
            var stream = proofFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024, cancellationToken);
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(proofFile.ContentType ?? "image/jpeg");
            content.Add(streamContent, "proofFile", proofFile.Name);
        }

        var response = await client.PostAsync("api/linkdrops/purchases", content, cancellationToken);
        return await ReadResultAsync<PurchaseResponseDto>(response, cancellationToken);
    }

    public Task<Result<IReadOnlyList<PurchaseResponseDto>>> GetUserPurchasesAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<PurchaseResponseDto>>($"api/linkdrops/purchases?page={page}&pageSize={pageSize}", cancellationToken);

    public Task<Result<PurchaseResponseDto>> GetPurchaseByIdAsync(int purchaseId, CancellationToken cancellationToken = default) =>
        GetAsync<PurchaseResponseDto>($"api/linkdrops/purchases/{purchaseId}", cancellationToken);

    public Task<Result<LinkDropWalletDto>> GetUserWalletAsync(CancellationToken cancellationToken = default) =>
        GetAsync<LinkDropWalletDto>("api/linkdrops/wallet", cancellationToken);

    public Task<Result<IReadOnlyList<LinkDropTransactionDto>>> GetUserTransactionsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<LinkDropTransactionDto>>($"api/linkdrops/transactions?page={page}&pageSize={pageSize}", cancellationToken);

    // =========================================================================
    // ADMIN METHODS
    // =========================================================================

    public Task<Result<IReadOnlyList<PurchaseResponseDto>>> GetPendingPurchasesAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<PurchaseResponseDto>>($"api/admin/linkdrops/purchases?page={page}&pageSize={pageSize}", cancellationToken);

    public Task<Result<PurchaseResponseDto>> ApprovePurchaseAsync(int purchaseId, string? notes = null, CancellationToken cancellationToken = default) =>
        PostAsync<PurchaseResponseDto, ApprovePurchaseDto>($"api/admin/linkdrops/purchases/{purchaseId}/approve", new ApprovePurchaseDto { Notes = notes }, cancellationToken);

    public Task<Result<PurchaseResponseDto>> RejectPurchaseAsync(int purchaseId, string rejectionReason, CancellationToken cancellationToken = default) =>
        PostAsync<PurchaseResponseDto, RejectPurchaseDto>($"api/admin/linkdrops/purchases/{purchaseId}/reject", new RejectPurchaseDto { RejectionReason = rejectionReason }, cancellationToken);

    public Task<Result<IReadOnlyList<LinkDropPackageDto>>> GetAllPackagesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<LinkDropPackageDto>>("api/admin/linkdrops/packages", cancellationToken);

    public Task<Result<LinkDropPackageDto>> CreatePackageAsync(CreatePackageRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<LinkDropPackageDto, CreatePackageRequestDto>("api/admin/linkdrops/packages", request, cancellationToken);

    public Task<Result<LinkDropPackageDto>> UpdatePackageAsync(int packageId, UpdatePackageRequestDto request, CancellationToken cancellationToken = default) =>
        PutAsync<LinkDropPackageDto, UpdatePackageRequestDto>($"api/admin/linkdrops/packages/{packageId}", request, cancellationToken);

    public Task<Result<bool>> TogglePackageStatusAsync(int packageId, CancellationToken cancellationToken = default) =>
        PostAsync<bool, object>($"api/admin/linkdrops/packages/{packageId}/toggle", new { }, cancellationToken);

    public Task<Result<IReadOnlyList<PaymentMethodDto>>> GetAllPaymentMethodsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<PaymentMethodDto>>("api/admin/linkdrops/payment-methods", cancellationToken);

    public Task<Result<PaymentMethodDto>> CreatePaymentMethodAsync(CreatePaymentMethodRequestDto request, CancellationToken cancellationToken = default) =>
        PostAsync<PaymentMethodDto, CreatePaymentMethodRequestDto>("api/admin/linkdrops/payment-methods", request, cancellationToken);

    public Task<Result<PaymentMethodDto>> UpdatePaymentMethodAsync(int paymentMethodId, UpdatePaymentMethodRequestDto request, CancellationToken cancellationToken = default) =>
        PutAsync<PaymentMethodDto, UpdatePaymentMethodRequestDto>($"api/admin/linkdrops/payment-methods/{paymentMethodId}", request, cancellationToken);

    public Task<Result<bool>> TogglePaymentMethodStatusAsync(int paymentMethodId, CancellationToken cancellationToken = default) =>
        PostAsync<bool, object>($"api/admin/linkdrops/payment-methods/{paymentMethodId}/toggle", new { }, cancellationToken);
}
