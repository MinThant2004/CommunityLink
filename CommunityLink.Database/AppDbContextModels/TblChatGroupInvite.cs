using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblChatGroupInvite
{
    public int ChatGroupInviteId { get; set; }

    public int ChatGroupId { get; set; }

    public int UserId { get; set; }

    public int InvitedByUserId { get; set; }

    public string Status { get; set; } = null!;

    public long FeeAtInviteLinkDrops { get; set; }

    public DateTime InvitedAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatGroup ChatGroup { get; set; } = null!;

    public virtual TblUser? DeletedByNavigation { get; set; }

    public virtual TblUser InvitedByUser { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
