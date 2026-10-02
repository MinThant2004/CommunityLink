using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblChatGroupInviteLink
{
    public int ChatGroupInviteLinkId { get; set; }

    public int ChatGroupId { get; set; }

    public string Token { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public string? Name { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public int? MaxUses { get; set; }

    public int UseCount { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatGroup ChatGroup { get; set; } = null!;

    public virtual TblUser CreatedByUser { get; set; } = null!;
}
