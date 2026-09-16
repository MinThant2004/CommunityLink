using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblCommunityRating
{
    public int CommunityRatingId { get; set; }

    public int UserId { get; set; }

    public int CommunityId { get; set; }

    public int Score { get; set; }

    public string? ReviewText { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblCommunity Community { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
