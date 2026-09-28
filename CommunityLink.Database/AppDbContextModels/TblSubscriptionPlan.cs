using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblSubscriptionPlan
{
    public int PlanId { get; set; }

    public string TargetRoleCode { get; set; } = null!; // DOMAIN_PRO or PUBLIC_FIGURE

    public string PlanName { get; set; } = null!;

    public string BillingInterval { get; set; } = "Monthly"; // Monthly, Quarterly, Annual, Custom

    public int DurationDays { get; set; } = 30;

    public decimal PriceAmount { get; set; }

    public long LinkDropCost { get; set; } = 0;

    public string? PerksJson { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TblUserSubscription> TblUserSubscriptions { get; set; } = new List<TblUserSubscription>();
}
