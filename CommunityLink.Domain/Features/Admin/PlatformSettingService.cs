using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Admin;
using CommunityLink.Shared.Features.LinkDrop;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.Admin;

public interface IPlatformSettingService
{
    Task<Result<PlatformCommissionSettingDto>> GetPlatformCommissionAsync(CancellationToken cancellationToken = default);
    Task<Result<PlatformCommissionSettingDto>> UpdatePlatformCommissionAsync(UpdateCommissionSettingRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<PlatformCommissionSettingDto>> UpdatePlatformCommissionRawAsync(string? rawValue, CancellationToken cancellationToken = default);

    Task<Result<LinkDropExchangeRateSettingDto>> GetLinkDropExchangeRateAsync(CancellationToken cancellationToken = default);
    Task<Result<LinkDropExchangeRateSettingDto>> UpdateLinkDropExchangeRateAsync(UpdateExchangeRateSettingRequestDto request, CancellationToken cancellationToken = default);
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

    public const string LinkDropExchangeRateMmkKey = "LinkDropExchangeRateMMK";
    public const string LinkDropExchangeRateUsdKey = "LinkDropExchangeRateUSD";
    public const string LinkDropExchangeRateCnyKey = "LinkDropExchangeRateCNY";

    public async Task<Result<LinkDropExchangeRateSettingDto>> GetLinkDropExchangeRateAsync(CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.TblPlatformSettings
            .AsNoTracking()
            .Where(s => s.SettingKey == LinkDropExchangeRateMmkKey ||
                        s.SettingKey == LinkDropExchangeRateUsdKey ||
                        s.SettingKey == LinkDropExchangeRateCnyKey)
            .ToListAsync(cancellationToken);

        var mmkSetting = settings.FirstOrDefault(s => s.SettingKey == LinkDropExchangeRateMmkKey);
        var usdSetting = settings.FirstOrDefault(s => s.SettingKey == LinkDropExchangeRateUsdKey);
        var cnySetting = settings.FirstOrDefault(s => s.SettingKey == LinkDropExchangeRateCnyKey);

        decimal mmkRate = 1.00m;
        if (mmkSetting != null && decimal.TryParse(mmkSetting.SettingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedMmk) && parsedMmk > 0)
        {
            mmkRate = parsedMmk;
        }

        decimal usdRate = 0.0003m;
        if (usdSetting != null && decimal.TryParse(usdSetting.SettingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedUsd) && parsedUsd > 0)
        {
            usdRate = parsedUsd;
        }

        decimal cnyRate = 0.0025m;
        if (cnySetting != null && decimal.TryParse(cnySetting.SettingValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedCny) && parsedCny > 0)
        {
            cnyRate = parsedCny;
        }

        return Result<LinkDropExchangeRateSettingDto>.Success(new LinkDropExchangeRateSettingDto
        {
            MmkPerLinkDrop = mmkRate,
            UsdPerLinkDrop = usdRate,
            CnyPerLinkDrop = cnyRate,
            UpdatedAt = mmkSetting?.UpdatedAt,
            UpdatedBy = mmkSetting?.UpdatedBy
        });
    }

    public async Task<Result<LinkDropExchangeRateSettingDto>> UpdateLinkDropExchangeRateAsync(UpdateExchangeRateSettingRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.MmkPerLinkDrop <= 0)
        {
            return Result<LinkDropExchangeRateSettingDto>.Failure("MMK exchange rate per LinkDrop must be greater than zero.", ResultStatus.ValidationError);
        }

        var updatedByUserId = currentUserContext.UserId;
        var now = DateTime.UtcNow;

        // 1. MMK Rate
        var mmkSetting = await dbContext.TblPlatformSettings
            .FirstOrDefaultAsync(s => s.SettingKey == LinkDropExchangeRateMmkKey, cancellationToken);

        if (mmkSetting == null)
        {
            mmkSetting = new TblPlatformSetting
            {
                SettingKey = LinkDropExchangeRateMmkKey,
                SettingValue = request.MmkPerLinkDrop.ToString("F2", CultureInfo.InvariantCulture),
                DataType = "DECIMAL",
                Description = "Exchange rate: MMK per 1 LinkDrop point.",
                UpdatedAt = now,
                UpdatedBy = updatedByUserId
            };
            dbContext.TblPlatformSettings.Add(mmkSetting);
        }
        else
        {
            mmkSetting.SettingValue = request.MmkPerLinkDrop.ToString("F2", CultureInfo.InvariantCulture);
            mmkSetting.UpdatedAt = now;
            mmkSetting.UpdatedBy = updatedByUserId;
        }

        // 2. USD Rate
        if (request.UsdPerLinkDrop.HasValue && request.UsdPerLinkDrop.Value > 0)
        {
            var usdSetting = await dbContext.TblPlatformSettings
                .FirstOrDefaultAsync(s => s.SettingKey == LinkDropExchangeRateUsdKey, cancellationToken);

            if (usdSetting == null)
            {
                usdSetting = new TblPlatformSetting
                {
                    SettingKey = LinkDropExchangeRateUsdKey,
                    SettingValue = request.UsdPerLinkDrop.Value.ToString("F6", CultureInfo.InvariantCulture),
                    DataType = "DECIMAL",
                    Description = "Exchange rate: USD per 1 LinkDrop point.",
                    UpdatedAt = now,
                    UpdatedBy = updatedByUserId
                };
                dbContext.TblPlatformSettings.Add(usdSetting);
            }
            else
            {
                usdSetting.SettingValue = request.UsdPerLinkDrop.Value.ToString("F6", CultureInfo.InvariantCulture);
                usdSetting.UpdatedAt = now;
                usdSetting.UpdatedBy = updatedByUserId;
            }
        }

        // 3. CNY Rate
        if (request.CnyPerLinkDrop.HasValue && request.CnyPerLinkDrop.Value > 0)
        {
            var cnySetting = await dbContext.TblPlatformSettings
                .FirstOrDefaultAsync(s => s.SettingKey == LinkDropExchangeRateCnyKey, cancellationToken);

            if (cnySetting == null)
            {
                cnySetting = new TblPlatformSetting
                {
                    SettingKey = LinkDropExchangeRateCnyKey,
                    SettingValue = request.CnyPerLinkDrop.Value.ToString("F6", CultureInfo.InvariantCulture),
                    DataType = "DECIMAL",
                    Description = "Exchange rate: CNY per 1 LinkDrop point.",
                    UpdatedAt = now,
                    UpdatedBy = updatedByUserId
                };
                dbContext.TblPlatformSettings.Add(cnySetting);
            }
            else
            {
                cnySetting.SettingValue = request.CnyPerLinkDrop.Value.ToString("F6", CultureInfo.InvariantCulture);
                cnySetting.UpdatedAt = now;
                cnySetting.UpdatedBy = updatedByUserId;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetLinkDropExchangeRateAsync(cancellationToken);
    }
}
