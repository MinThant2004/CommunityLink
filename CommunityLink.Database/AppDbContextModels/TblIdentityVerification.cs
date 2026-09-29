using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblIdentityVerification
{
    public int VerificationId { get; set; }

    public int UserId { get; set; }

    public int PlanId { get; set; }

    public string TargetRoleCode { get; set; } = null!;

    public string FullLegalName { get; set; } = null!;

    public string? WorkEmail { get; set; }

    public string? ProfessionalUrl { get; set; }

    public string IdCardFrontUrl { get; set; } = null!;

    public string? IdCardBackUrl { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public long LinkDropPointsDeducted { get; set; }

    public string Status { get; set; } = null!;

    public string? ReviewNotes { get; set; }

    public int? ReviewedByAdminId { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual TblSubscriptionPlan Plan { get; set; } = null!;

    public virtual TblAdmin? ReviewedByAdmin { get; set; }

    public virtual TblUser User { get; set; } = null!;
}
