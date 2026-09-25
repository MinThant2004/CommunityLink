using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblChatGroup
{
    public int ChatGroupId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? AvatarUrl { get; set; }

    public string? BannerUrl { get; set; }

    public int CreatorId { get; set; }

    public string ChatType { get; set; } = null!;

    public long JoinFeeLinkDrops { get; set; }

    public decimal CommissionPercentageSnapshot { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblUser Creator { get; set; } = null!;

    public virtual ICollection<TblChatGroupMember> TblChatGroupMembers { get; set; } = new List<TblChatGroupMember>();

    public virtual ICollection<TblChatGroupMessage> TblChatGroupMessages { get; set; } = new List<TblChatGroupMessage>();
}
