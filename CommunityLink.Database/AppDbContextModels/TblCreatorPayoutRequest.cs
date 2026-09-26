using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblCreatorPayoutRequest
{
    public long CreatorPayoutRequestId { get; set; }

    public int CreatorUserId { get; set; }

    public long AmountLinkDrops { get; set; }

    public decimal AmountMMK { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentAccountName { get; set; } = null!;

    public string PaymentAccountNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedBy { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public virtual TblUser CreatorUser { get; set; } = null!;
}
