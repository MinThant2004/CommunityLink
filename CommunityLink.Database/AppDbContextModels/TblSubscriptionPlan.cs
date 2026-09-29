using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblSubscriptionPlan
{
    public int PlanId { get; set; }

    public string TargetRoleCode { get; set; } = null!;

    public string PlanName { get; set; } = null!;

    public string BillingInterval { get; set; } = null!;

    public int DurationDays { get; set; }

    public decimal PriceAmount { get; set; }

    public long LinkDropCost { get; set; }

    public string? PerksJson { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TblIdentityVerification> TblIdentityVerifications { get; set; } = new List<TblIdentityVerification>();

    public virtual ICollection<TblUserSubscription> TblUserSubscriptions { get; set; } = new List<TblUserSubscription>();
}
