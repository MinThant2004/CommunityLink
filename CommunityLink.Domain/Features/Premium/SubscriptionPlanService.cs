using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;

namespace CommunityLink.Domain.Features.Premium;

public interface ISubscriptionPlanService
{
    Task<Result<IReadOnlyList<SubscriptionPlanDto>>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<SubscriptionPlanDto>>> GetAllPlansAdminAsync(CancellationToken cancellationToken = default);
    Task<Result<SubscriptionPlanDto>> GetPlanByIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<Result<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<SubscriptionPlanDto>> UpdatePlanAsync(UpdateSubscriptionPlanRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<bool>> TogglePlanStatusAsync(int planId, CancellationToken cancellationToken = default);
    Task<Result> DeletePlanAsync(int planId, CancellationToken cancellationToken = default);
}

public sealed class SubscriptionPlanService(AppDbContext dbContext) : ISubscriptionPlanService
{
    public async Task<Result<IReadOnlyList<SubscriptionPlanDto>>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await dbContext.TblSubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.TargetRoleCode)
            .ThenBy(p => p.DurationDays)
            .ToListAsync(cancellationToken);

        var dtos = plans.Select(MapToDto).ToList();
        return Result<IReadOnlyList<SubscriptionPlanDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<SubscriptionPlanDto>>> GetAllPlansAdminAsync(CancellationToken cancellationToken = default)
    {
        var plans = await dbContext.TblSubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.TargetRoleCode)
            .ThenBy(p => p.DurationDays)
            .ToListAsync(cancellationToken);

        var dtos = plans.Select(MapToDto).ToList();
        return Result<IReadOnlyList<SubscriptionPlanDto>>.Success(dtos);
    }

    public async Task<Result<SubscriptionPlanDto>> GetPlanByIdAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.TblSubscriptionPlans.FindAsync([planId], cancellationToken);
        if (plan is null) return Result<SubscriptionPlanDto>.Failure("Subscription plan not found.", ResultStatus.NotFound);

        return Result<SubscriptionPlanDto>.Success(MapToDto(plan));
    }

    public async Task<Result<SubscriptionPlanDto>> CreatePlanAsync(CreateSubscriptionPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.PlanName))
            return Result<SubscriptionPlanDto>.Failure("Plan name is required.", ResultStatus.ValidationError);

        if (request.DurationDays <= 0)
            return Result<SubscriptionPlanDto>.Failure("Duration days must be at least 1.", ResultStatus.ValidationError);

        if (request.LinkDropCost <= 0)
            return Result<SubscriptionPlanDto>.Failure("LinkDrop points cost must be at least 1 point.", ResultStatus.ValidationError);

        var normalizedTarget = request.TargetRoleCode.Trim().ToUpperInvariant();
        if (normalizedTarget != "DOMAIN_PRO" && normalizedTarget != "PUBLIC_FIGURE")
        {
            return Result<SubscriptionPlanDto>.Failure("Target role code must be 'DOMAIN_PRO' or 'PUBLIC_FIGURE'.", ResultStatus.ValidationError);
        }

        var perksJson = request.Perks != null && request.Perks.Count > 0 
            ? JsonSerializer.Serialize(request.Perks.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList()) 
            : "[]";

        var entity = new TblSubscriptionPlan
        {
            TargetRoleCode = normalizedTarget,
            PlanName = request.PlanName.Trim(),
            BillingInterval = request.BillingInterval.Trim(),
            DurationDays = request.DurationDays,
            PriceAmount = request.PriceAmount,
            LinkDropCost = request.LinkDropCost < 0 ? 0 : request.LinkDropCost,
            PerksJson = perksJson,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblSubscriptionPlans.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionPlanDto>.Success(MapToDto(entity), "Subscription plan created successfully.");
    }

    public async Task<Result<SubscriptionPlanDto>> UpdatePlanAsync(UpdateSubscriptionPlanRequestDto request, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.TblSubscriptionPlans.FindAsync([request.PlanId], cancellationToken);
        if (plan is null) return Result<SubscriptionPlanDto>.Failure("Subscription plan not found.", ResultStatus.NotFound);

        if (string.IsNullOrWhiteSpace(request.PlanName))
            return Result<SubscriptionPlanDto>.Failure("Plan name is required.", ResultStatus.ValidationError);

        if (request.DurationDays <= 0)
            return Result<SubscriptionPlanDto>.Failure("Duration days must be at least 1.", ResultStatus.ValidationError);

        if (request.LinkDropCost <= 0)
            return Result<SubscriptionPlanDto>.Failure("LinkDrop points cost must be at least 1 point.", ResultStatus.ValidationError);

        var normalizedTarget = request.TargetRoleCode.Trim().ToUpperInvariant();
        if (normalizedTarget != "DOMAIN_PRO" && normalizedTarget != "PUBLIC_FIGURE")
        {
            return Result<SubscriptionPlanDto>.Failure("Target role code must be 'DOMAIN_PRO' or 'PUBLIC_FIGURE'.", ResultStatus.ValidationError);
        }

        var perksJson = request.Perks != null && request.Perks.Count > 0 
            ? JsonSerializer.Serialize(request.Perks.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).ToList()) 
            : "[]";

        plan.TargetRoleCode = normalizedTarget;
        plan.PlanName = request.PlanName.Trim();
        plan.BillingInterval = request.BillingInterval.Trim();
        plan.DurationDays = request.DurationDays;
        plan.PriceAmount = request.PriceAmount;
        plan.LinkDropCost = request.LinkDropCost < 0 ? 0 : request.LinkDropCost;
        plan.PerksJson = perksJson;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<SubscriptionPlanDto>.Success(MapToDto(plan), "Subscription plan updated successfully.");
    }

    public async Task<Result<bool>> TogglePlanStatusAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.TblSubscriptionPlans.FindAsync([planId], cancellationToken);
        if (plan is null) return Result<bool>.Failure("Subscription plan not found.", ResultStatus.NotFound);

        plan.IsActive = !plan.IsActive;
        plan.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(plan.IsActive, $"Plan status set to {(plan.IsActive ? "Active" : "Inactive")}.");
    }

    public async Task<Result> DeletePlanAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.TblSubscriptionPlans.FindAsync([planId], cancellationToken);
        if (plan is null) return Result.Failure("Subscription plan not found.", ResultStatus.NotFound);

        var hasSubscriptions = await dbContext.TblUserSubscriptions.AnyAsync(s => s.PlanId == planId, cancellationToken);
        if (hasSubscriptions)
        {
            plan.IsActive = false;
            plan.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success("Plan cannot be hard-deleted because it has associated member subscriptions. It has been deactivated instead.");
        }

        dbContext.TblSubscriptionPlans.Remove(plan);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Subscription plan deleted successfully.");
    }

    private static SubscriptionPlanDto MapToDto(TblSubscriptionPlan plan)
    {
        var perks = new List<string>();
        if (!string.IsNullOrWhiteSpace(plan.PerksJson))
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<List<string>>(plan.PerksJson);
                if (deserialized != null) perks = deserialized;
            }
            catch
            {
                // Fallback parsing in case of non-json string
                perks = plan.PerksJson.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }
        }

        return new SubscriptionPlanDto(
            plan.PlanId,
            plan.TargetRoleCode,
            plan.PlanName,
            plan.BillingInterval,
            plan.DurationDays,
            plan.PriceAmount,
            plan.LinkDropCost,
            perks,
            plan.IsActive,
            plan.CreatedAt,
            plan.UpdatedAt
        );
    }
}
