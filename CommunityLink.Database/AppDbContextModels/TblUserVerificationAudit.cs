using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserVerificationAudit
{
    public int AuditId { get; set; }

    public int UserId { get; set; }

    public string AuditCode { get; set; } = null!;

    public string AuditTitle { get; set; } = null!;

    public string AuditDescription { get; set; } = null!;

    public string Authority { get; set; } = null!;

    public string Status { get; set; } = "ACTIVE";

    public DateTime AuditedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual TblUser User { get; set; } = null!;
}
