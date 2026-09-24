using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserFollow
{
    public int FollowId { get; set; }

    public int FollowerId { get; set; }

    public int FolloweeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblUser Follower { get; set; } = null!;

    public virtual TblUser Followee { get; set; } = null!;
}
