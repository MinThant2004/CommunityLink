using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;

namespace CommunityLink.Domain.Features.Chat;

public interface ICreatorChatSettingService
{
    Task<Result<CreatorChatSettingModel>> GetSettingsAsync(int? creatorUserId = null, CancellationToken cancellationToken = default);
    Task<Result<CreatorChatSettingModel>> SaveSettingsAsync(SaveCreatorChatSettingRequestModel request, CancellationToken cancellationToken = default);
}

public sealed class CreatorChatSettingService(AppDbContext dbContext, ICurrentUserContext currentUser) : ICreatorChatSettingService
{
    public async Task<Result<CreatorChatSettingModel>> GetSettingsAsync(int? creatorUserId = null, CancellationToken cancellationToken = default)
    {
        var targetUserId = creatorUserId ?? currentUser.UserId;
        if (targetUserId is null)
        {
            return Result<CreatorChatSettingModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var setting = await dbContext.TblCreatorChatSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreatorUserId == targetUserId.Value, cancellationToken);

        if (setting is null)
        {
            return Result<CreatorChatSettingModel>.Success(new CreatorChatSettingModel(
                targetUserId.Value,
                IsPrivateChatEnabled: false,
                PrivateChatFeeLinkDrops: 0,
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: null));
        }

        return Result<CreatorChatSettingModel>.Success(new CreatorChatSettingModel(
            setting.CreatorUserId,
            setting.IsPrivateChatEnabled,
            setting.PrivateChatFeeLinkDrops,
            setting.CreatedAt,
            setting.UpdatedAt));
    }

    public async Task<Result<CreatorChatSettingModel>> SaveSettingsAsync(SaveCreatorChatSettingRequestModel request, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<CreatorChatSettingModel>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var currentUserId = currentUser.UserId.Value;

        if (request.PrivateChatFeeLinkDrops < 0)
        {
            return Result<CreatorChatSettingModel>.Failure("Private chat fee cannot be negative.", ResultStatus.ValidationError);
        }

        if (request.IsPrivateChatEnabled)
        {
            var userRoles = await dbContext.TblUserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == currentUserId && !ur.IsDeleted)
                .Select(ur => ur.Role.RoleCode)
                .ToListAsync(cancellationToken);

            bool isCreatorRole = userRoles.Exists(r =>
                string.Equals(r, "DOMAIN_PROFESSIONAL", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, "PUBLIC_FIGURE", StringComparison.OrdinalIgnoreCase));

            if (!isCreatorRole && !currentUser.IsPremiumCreator)
            {
                return Result<CreatorChatSettingModel>.Failure("Only creators can enable private chat.", ResultStatus.Forbidden);
            }
        }

        var setting = await dbContext.TblCreatorChatSettings
            .FirstOrDefaultAsync(s => s.CreatorUserId == currentUserId, cancellationToken);

        if (setting is null)
        {
            setting = new TblCreatorChatSetting
            {
                CreatorUserId = currentUserId,
                IsPrivateChatEnabled = request.IsPrivateChatEnabled,
                PrivateChatFeeLinkDrops = request.PrivateChatFeeLinkDrops,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.TblCreatorChatSettings.Add(setting);
        }
        else
        {
            setting.IsPrivateChatEnabled = request.IsPrivateChatEnabled;
            setting.PrivateChatFeeLinkDrops = request.PrivateChatFeeLinkDrops;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<CreatorChatSettingModel>.Success(new CreatorChatSettingModel(
            setting.CreatorUserId,
            setting.IsPrivateChatEnabled,
            setting.PrivateChatFeeLinkDrops,
            setting.CreatedAt,
            setting.UpdatedAt));
    }
}
