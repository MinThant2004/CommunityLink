using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblContentReport
{
    public int ContentReportId { get; set; }

    public string ContentType { get; set; } = null!; // "POST" or "POLL"

    public int ContentId { get; set; } // PostId or PollId

    public int ReporterUserId { get; set; }

    public string ReasonCategory { get; set; } = null!; // e.g. "INAPPROPRIATE_CONTENT", "MISINFORMATION", "MISLEADING"

    public string? Details { get; set; }

    public string Status { get; set; } = "PENDING"; // "PENDING", "RESOLVED", "DISMISSED"

    public int? HandledByAdminId { get; set; }

    public string? AdminNote { get; set; }

    public DateTime? HandledAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblUser ReporterUser { get; set; } = null!;
}
