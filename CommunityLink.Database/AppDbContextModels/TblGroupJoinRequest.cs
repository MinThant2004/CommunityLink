using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblGroupJoinRequest
{
    public int GroupJoinRequestId { get; set; }

    public int GroupId { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = null!;

    public string? RequestNote { get; set; }

    public int? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblGroup Group { get; set; } = null!;

    public virtual TblUser? ReviewedByNavigation { get; set; }

    public virtual TblUser User { get; set; } = null!;
}
