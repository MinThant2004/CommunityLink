using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblLinkDropWallet
{
    public int WalletId { get; set; }

    public int UserId { get; set; }

    public long Balance { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public long PurchasedBalance { get; set; }

    public long EarnedBalance { get; set; }

    public virtual ICollection<TblLinkDropTransaction> TblLinkDropTransactions { get; set; } = new List<TblLinkDropTransaction>();

    public virtual TblUser User { get; set; } = null!;
}
