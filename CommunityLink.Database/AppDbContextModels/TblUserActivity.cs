using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserActivity
{
    public long ActivityId { get; set; }

    public int UserId { get; set; }

    public string ActivityType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? TargetEntityType { get; set; }

    public int? TargetEntityId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual TblUser User { get; set; } = null!;
}
