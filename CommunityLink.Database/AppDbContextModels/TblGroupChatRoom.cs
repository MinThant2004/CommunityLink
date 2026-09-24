using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblGroupChatRoom
{
    public int GroupChatRoomId { get; set; }

    public int GroupId { get; set; }

    public int CreatorId { get; set; }

    public string ChatType { get; set; } = "FREE";

    public int JoinFeeLinkDrops { get; set; }

    public decimal CommissionPercentageSnapshot { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public virtual TblGroup Group { get; set; } = null!;

    public virtual TblUser Creator { get; set; } = null!;

    public virtual ICollection<TblGroupChatMessage> TblGroupChatMessages { get; set; } = new List<TblGroupChatMessage>();
}
