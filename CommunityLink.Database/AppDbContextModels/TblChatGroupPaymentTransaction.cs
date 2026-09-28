using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblChatGroupPaymentTransaction
{
    public long PaymentTransactionId { get; set; }

    public int ChatGroupId { get; set; }

    public int UserId { get; set; }

    public int CreatorUserId { get; set; }

    public long GrossAmount { get; set; }

    public decimal CommissionPercentage { get; set; }

    public long CommissionAmount { get; set; }

    public long NetAmount { get; set; }

    public long PurchasedAmountDeducted { get; set; }

    public long EarnedAmountDeducted { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatGroup ChatGroup { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;

    public virtual TblUser CreatorUser { get; set; } = null!;
}
