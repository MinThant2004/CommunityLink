using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUserSubscription
{
    public int SubscriptionId { get; set; }

    public int UserId { get; set; }

    public int PlanId { get; set; }

    public int RoleId { get; set; }

    public string Status { get; set; } = "Active"; // PendingReview, Active, Expired, Rejected, Canceled

    public string PaymentMethod { get; set; } = "LinkDropPoints"; // LinkDropPoints, ManualPaymentSlip, Card

    public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual TblSubscriptionPlan Plan { get; set; } = null!;

    public virtual TblRole Role { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
