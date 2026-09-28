using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblIdentityVerification
{
    public int VerificationId { get; set; }

    public int UserId { get; set; }

    public int PlanId { get; set; }

    public string TargetRoleCode { get; set; } = null!; // DOMAIN_PRO or PUBLIC_FIGURE

    public string FullLegalName { get; set; } = null!;

    public string? WorkEmail { get; set; }

    public string? ProfessionalUrl { get; set; } // LinkedIn, GitHub, Portfolio

    public string IdCardFrontUrl { get; set; } = null!;

    public string? IdCardBackUrl { get; set; }

    public string PaymentMethod { get; set; } = "LinkDropPoints"; // LinkDropPoints, ManualSlip

    public long LinkDropPointsDeducted { get; set; } = 0;

    public string Status { get; set; } = "PendingReview"; // PendingReview, Approved, Rejected

    public string? ReviewNotes { get; set; }

    public int? ReviewedByAdminId { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual TblUser User { get; set; } = null!;

    public virtual TblSubscriptionPlan Plan { get; set; } = null!;

    public virtual TblAdmin? ReviewedByAdmin { get; set; }
}
