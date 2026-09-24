using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Admin;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.Admin;

public interface IPlatformSettingService
{
    Task<Result<PlatformCommissionSettingDto>> GetPlatformCommissionAsync(CancellationToken cancellationToken = default);
    Task<Result<PlatformCommissionSettingDto>> UpdatePlatformCommissionAsync(UpdateCommissionSettingRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<PlatformCommissionSettingDto>> UpdatePlatformCommissionRawAsync(string? rawValue, CancellationToken cancellationToken = default);
}

public sealed class PlatformSettingService(AppDbContext dbContext, ICurrentUserContext currentUserContext) : IPlatformSettingService
{
    public const string PlatformCommissionKey = "PlatformCommissionPercentage";
    public const decimal DefaultCommissionPercentage = 10.00m;

    public async Task<Result<PlatformCommissionSettingDto>> GetPlatformCommissionAsync(CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.TblPlatformSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == PlatformCommissionKey, cancellationToken);

        if (setting == null)
        {
            return Result<PlatformCommissionSettingDto>.Success(new PlatformCommissionSettingDto
            {
                CommissionPercentage = DefaultCommissionPercentage,
                UpdatedAt = null,
                UpdatedBy = null
            });
        }

        if (decimal.TryParse(setting.SettingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return Result<PlatformCommissionSettingDto>.Success(new PlatformCommissionSettingDto
            {
                CommissionPercentage = parsedValue,
                UpdatedAt = setting.UpdatedAt,
                UpdatedBy = setting.UpdatedBy
            });
        }

        // Fallback to default 10.00 if corrupted
        return Result<PlatformCommissionSettingDto>.Success(new PlatformCommissionSettingDto
        {
            CommissionPercentage = DefaultCommissionPercentage,
            UpdatedAt = setting.UpdatedAt,
            UpdatedBy = setting.UpdatedBy
        });
    }

    public async Task<Result<PlatformCommissionSettingDto>> UpdatePlatformCommissionAsync(UpdateCommissionSettingRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.CommissionPercentage < 0)
        {
            return Result<PlatformCommissionSettingDto>.Failure("Commission percentage cannot be negative.", ResultStatus.ValidationError);
        }

        if (request.CommissionPercentage > 100)
        {
            return Result<PlatformCommissionSettingDto>.Failure("Commission percentage cannot exceed 100%.", ResultStatus.ValidationError);
        }

        return await SaveCommissionAsync(request.CommissionPercentage, cancellationToken);
    }

    public async Task<Result<PlatformCommissionSettingDto>> UpdatePlatformCommissionRawAsync(string? rawValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawValue) || !decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
        {
            return Result<PlatformCommissionSettingDto>.Failure("Invalid numeric value for commission percentage.", ResultStatus.ValidationError);
        }

        return await UpdatePlatformCommissionAsync(new UpdateCommissionSettingRequestDto { CommissionPercentage = decimalValue }, cancellationToken);
    }

    private async Task<Result<PlatformCommissionSettingDto>> SaveCommissionAsync(decimal commissionValue, CancellationToken cancellationToken)
    {
        var updatedByUserId = currentUserContext.UserId;

        var setting = await dbContext.TblPlatformSettings
            .FirstOrDefaultAsync(s => s.SettingKey == PlatformCommissionKey, cancellationToken);

        var now = DateTime.UtcNow;

        if (setting == null)
        {
            setting = new TblPlatformSetting
            {
                SettingKey = PlatformCommissionKey,
                SettingValue = commissionValue.ToString("F2", CultureInfo.InvariantCulture),
                DataType = "DECIMAL",
                Description = "Default platform commission percentage for paid group chats.",
                UpdatedAt = now,
                UpdatedBy = updatedByUserId
            };
            dbContext.TblPlatformSettings.Add(setting);
        }
        else
        {
            setting.SettingValue = commissionValue.ToString("F2", CultureInfo.InvariantCulture);
            setting.UpdatedAt = now;
            setting.UpdatedBy = updatedByUserId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<PlatformCommissionSettingDto>.Success(new PlatformCommissionSettingDto
        {
            CommissionPercentage = commissionValue,
            UpdatedAt = setting.UpdatedAt,
            UpdatedBy = setting.UpdatedBy
        }, "Platform commission percentage updated successfully.");
    }
}
