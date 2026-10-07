using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblChatGroupBan
{
    public int ChatGroupBanId { get; set; }

    public int ChatGroupId { get; set; }

    public int UserId { get; set; }

    public int BannedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime BannedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? RevokedAt { get; set; }

    public int? RevokedByUserId { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblUser BannedByUser { get; set; } = null!;

    public virtual TblChatGroup ChatGroup { get; set; } = null!;

    public virtual TblUser? RevokedByUser { get; set; }

    public virtual TblUser User { get; set; } = null!;
}
