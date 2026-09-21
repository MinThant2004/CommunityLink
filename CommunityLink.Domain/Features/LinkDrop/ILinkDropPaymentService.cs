using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityLink.Shared.Features.LinkDrop;

namespace CommunityLink.Domain.Features.LinkDrop;

public interface ILinkDropPaymentService
{
    // Buyer methods
    Task<List<LinkDropPackageDto>> GetActivePackagesAsync();
    Task<List<PaymentMethodDto>> GetActivePaymentMethodsAsync();
    Task<PurchaseResponseDto> SubmitPurchaseAsync(int userId, CreatePurchaseRequestDto request, Stream? proofStream = null, string? fileName = null, string? contentType = null, long fileSize = 0);
    Task<List<PurchaseResponseDto>> GetUserPurchasesAsync(int userId, int page = 1, int pageSize = 20);
    Task<PurchaseResponseDto?> GetPurchaseByIdAsync(int userId, int purchaseId);
    Task<LinkDropWalletDto> GetUserWalletAsync(int userId);
    Task<List<LinkDropTransactionDto>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20);

    // Admin review methods
    Task<List<PurchaseResponseDto>> GetPendingPurchasesAsync(int page = 1, int pageSize = 20);
    Task<PurchaseResponseDto> ApprovePurchaseAsync(int adminId, int purchaseId, string? notes = null);
    Task<PurchaseResponseDto> RejectPurchaseAsync(int adminId, int purchaseId, string rejectionReason);

    // Admin catalog & method management
    Task<List<LinkDropPackageDto>> GetAllPackagesAsync();
    Task<LinkDropPackageDto> CreatePackageAsync(int adminId, CreatePackageRequestDto request);
    Task<LinkDropPackageDto> UpdatePackageAsync(int adminId, int packageId, UpdatePackageRequestDto request);
    Task<bool> TogglePackageStatusAsync(int adminId, int packageId);

    Task<List<PaymentMethodDto>> GetAllPaymentMethodsAsync();
    Task<PaymentMethodDto> CreatePaymentMethodAsync(int adminId, CreatePaymentMethodRequestDto request);
    Task<PaymentMethodDto> UpdatePaymentMethodAsync(int adminId, int paymentMethodId, UpdatePaymentMethodRequestDto request);
    Task<bool> TogglePaymentMethodStatusAsync(int adminId, int paymentMethodId);
}
