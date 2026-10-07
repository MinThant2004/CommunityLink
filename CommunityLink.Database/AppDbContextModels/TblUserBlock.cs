using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserBlock
{
    public int UserBlockId { get; set; }

    public int BlockerUserId { get; set; }

    public int BlockedUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblUser BlockedUser { get; set; } = null!;

    public virtual TblUser BlockerUser { get; set; } = null!;
}
