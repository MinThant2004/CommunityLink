using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblLinkDropPurchase
{
    public int PurchaseId { get; set; }

    public string PurchaseNumber { get; set; } = null!;

    public int UserId { get; set; }

    public int? PackageId { get; set; }

    public int PaymentMethodId { get; set; }

    public bool IsCustomPurchase { get; set; }

    public string? SnapshotPackageName { get; set; }

    public decimal SnapshotRealMoneyAmount { get; set; }

    public long SnapshotLinkDropAmount { get; set; }

    public string SnapshotCurrency { get; set; } = null!;

    public decimal? SnapshotConversionRate { get; set; }

    public string TransactionReferenceNo { get; set; } = null!;

    public string? UserNotes { get; set; }

    public string Status { get; set; } = null!;

    public string? RejectionReason { get; set; }

    public int? ReviewedByAdminId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblLinkDropPackage? Package { get; set; }

    public virtual TblPaymentMethod PaymentMethod { get; set; } = null!;

    public virtual TblAdmin? ReviewedByAdmin { get; set; }

    public virtual TblLinkDropPurchaseProof? TblLinkDropPurchaseProof { get; set; }

    public virtual TblUser User { get; set; } = null!;
}
