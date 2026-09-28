using System;
using System.Collections.Generic;

namespace CommunityLink.Shared.Features.Premium;

public sealed record SubscriptionPlanDto(
    int PlanId,
    string TargetRoleCode,
    string PlanName,
    string BillingInterval,
    int DurationDays,
    decimal PriceAmount,
    long LinkDropCost,
    IReadOnlyList<string> Perks,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record CreateSubscriptionPlanRequestDto
{
    public string TargetRoleCode { get; set; } = "DOMAIN_PRO";
    public string PlanName { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = "Monthly";
    public int DurationDays { get; set; } = 30;
    public decimal PriceAmount { get; set; }
    public long LinkDropCost { get; set; }
    public List<string> Perks { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public sealed record UpdateSubscriptionPlanRequestDto
{
    public int PlanId { get; set; }
    public string TargetRoleCode { get; set; } = "DOMAIN_PRO";
    public string PlanName { get; set; } = string.Empty;
    public string BillingInterval { get; set; } = "Monthly";
    public int DurationDays { get; set; } = 30;
    public decimal PriceAmount { get; set; }
    public long LinkDropCost { get; set; }
    public List<string> Perks { get; set; } = new();
    public bool IsActive { get; set; } = true;
}
