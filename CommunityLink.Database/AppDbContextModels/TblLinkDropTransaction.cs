using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblLinkDropTransaction
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

    public virtual TblUser User { get; set; } = null!;

    public virtual TblLinkDropWallet Wallet { get; set; } = null!;
}
