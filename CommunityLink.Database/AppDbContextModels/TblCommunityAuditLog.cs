using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblCommunityAuditLog
{
    public int AuditId { get; set; }

    public int CommunityId { get; set; }

    public string TargetType { get; set; } = null!;

    public string FieldChanged { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int EditorId { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TblCommunity Community { get; set; } = null!;

    public virtual TblUser Editor { get; set; } = null!;
}
