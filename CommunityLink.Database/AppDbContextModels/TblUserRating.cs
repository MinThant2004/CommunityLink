using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserRating
{
    public int UserRatingId { get; set; }

    public int RaterUserId { get; set; }

    public int TargetUserId { get; set; }

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

    public virtual TblUser RaterUser { get; set; } = null!;

    public virtual TblUser TargetUser { get; set; } = null!;
}
