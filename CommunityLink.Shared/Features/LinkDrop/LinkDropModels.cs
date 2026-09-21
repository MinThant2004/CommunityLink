using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.LinkDrop;

public class LinkDropPackageDto
{
    public int PackageId { get; set; }
    public string PackageName { get; set; } = null!;
    public string? Description { get; set; }
    public long LinkDropAmount { get; set; }
    public long BonusAmount { get; set; }
    public long TotalGrantedDrops => LinkDropAmount + BonusAmount;
    public decimal RealMoneyAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class PaymentMethodDto
{
    public int PaymentMethodId { get; set; }
    public string MethodName { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string? QrCodeImageUrl { get; set; }
    public string? Instructions { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePurchaseRequestDto
{
    public int? PackageId { get; set; }
    public int PaymentMethodId { get; set; }
    public bool IsCustomPurchase { get; set; }
    public decimal? CustomRealMoneyAmount { get; set; }
    public long? CustomLinkDropAmount { get; set; }
    public string TransactionReferenceNo { get; set; } = null!;
    public string? UserNotes { get; set; }
    public string? ProofFileUrl { get; set; }
}

public class PurchaseResponseDto
{
    public int PurchaseId { get; set; }
    public string PurchaseNumber { get; set; } = null!;
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int? PackageId { get; set; }
    public string? PackageName { get; set; }
    public int PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public bool IsCustomPurchase { get; set; }
    public string? SnapshotPackageName { get; set; }
    public decimal SnapshotRealMoneyAmount { get; set; }
    public long SnapshotLinkDropAmount { get; set; }
    public string SnapshotCurrency { get; set; } = "USD";
    public decimal? SnapshotConversionRate { get; set; }
    public string TransactionReferenceNo { get; set; } = null!;
    public string? UserNotes { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? RejectionReason { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public string? ReviewedByAdminName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public PurchaseProofDto? Proof { get; set; }
}

public class PurchaseProofDto
{
    public int ProofId { get; set; }
    public int PurchaseId { get; set; }
    public string FileUrl { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class ApprovePurchaseDto
{
    public string? Notes { get; set; }
}

public class RejectPurchaseDto
{
    public string RejectionReason { get; set; } = null!;
}

public class LinkDropWalletDto
{
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public long Balance { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class LinkDropTransactionDto
{
    public long TransactionId { get; set; }
    public int WalletId { get; set; }
    public int UserId { get; set; }
    public string TransactionType { get; set; } = null!;
    public long Amount { get; set; }
    public long BalanceBefore { get; set; }
    public long BalanceAfter { get; set; }
    public string ReferenceType { get; set; } = null!;
    public int ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePackageRequestDto
{
    public string PackageName { get; set; } = null!;
    public string? Description { get; set; }
    public long LinkDropAmount { get; set; }
    public long BonusAmount { get; set; }
    public decimal RealMoneyAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int DisplayOrder { get; set; }
}

public class UpdatePackageRequestDto
{
    public string PackageName { get; set; } = null!;
    public string? Description { get; set; }
    public long LinkDropAmount { get; set; }
    public long BonusAmount { get; set; }
    public decimal RealMoneyAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePaymentMethodRequestDto
{
    public string MethodName { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string? QrCodeImageUrl { get; set; }
    public string? Instructions { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdatePaymentMethodRequestDto
{
    public string MethodName { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string? QrCodeImageUrl { get; set; }
    public string? Instructions { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
