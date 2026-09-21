using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblLinkDropPurchaseProof
{
    public int ProofId { get; set; }

    public int PurchaseId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblLinkDropPurchase Purchase { get; set; } = null!;
}
