using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Notification;

namespace CommunityLink.Domain.Features.Notification;

public sealed class NotificationService(AppDbContext dbContext, ICurrentUserContext currentUser) : INotificationService
{
    public async Task<Result<NotificationSummaryDto>> GetNotificationsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<NotificationSummaryDto>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var userId = currentUser.UserId.Value;

        var baseQuery = dbContext.TblNotifications
            .Where(n => n.RecipientUserId == userId && !n.IsDeleted);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var unreadCount = await baseQuery.CountAsync(n => !n.IsRead, cancellationToken);

        var notifications = await baseQuery
            .Include(n => n.ActorUser)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                RecipientUserId = n.RecipientUserId,
                ActorUserId = n.ActorUserId,
                ActorName = n.ActorUser != null ? (string.IsNullOrWhiteSpace(n.ActorUser.DisplayName) ? n.ActorUser.UserName : n.ActorUser.DisplayName) : "System",
                ActorAvatar = n.ActorUser != null ? n.ActorUser.AvatarUrl : null,
                NotificationType = n.NotificationType,
                Title = n.Title,
                Message = n.Message,
                TargetEntityName = n.TargetEntityName,
                TargetEntityId = n.TargetEntityId,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Resolve friendly target URLs for deep linking
        foreach (var notif in notifications)
        {
            notif.TargetUrl = await ResolveTargetUrlAsync(notif.TargetEntityName, notif.TargetEntityId, cancellationToken);
        }

        var summary = new NotificationSummaryDto
        {
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            Notifications = notifications
        };

        return Result<NotificationSummaryDto>.Success(summary);
    }

    public async Task<Result<int>> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result<int>.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var unreadCount = await dbContext.TblNotifications
            .CountAsync(n => n.RecipientUserId == currentUser.UserId.Value && !n.IsRead && !n.IsDeleted, cancellationToken);

        return Result<int>.Success(unreadCount);
    }

    public async Task<Result> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var notif = await dbContext.TblNotifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientUserId == currentUser.UserId.Value && !n.IsDeleted, cancellationToken);

        if (notif is null)
        {
            return Result.Failure("Notification not found.", ResultStatus.NotFound);
        }

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            notif.ReadAt = DateTime.UtcNow;
            notif.UpdatedAt = DateTime.UtcNow;
            notif.UpdatedBy = currentUser.UserId.Value;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure("Unauthorized", ResultStatus.Unauthorized);
        }

        var unreadList = await dbContext.TblNotifications
            .Where(n => n.RecipientUserId == currentUser.UserId.Value && !n.IsRead && !n.IsDeleted)
            .ToListAsync(cancellationToken);

        if (unreadList.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var n in unreadList)
            {
                n.IsRead = true;
                n.ReadAt = now;
                n.UpdatedAt = now;
                n.UpdatedBy = currentUser.UserId.Value;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task CreateNotificationAsync(
        int recipientUserId,
        int? actorUserId,
        string notificationType,
        string title,
        string message,
        string? targetEntityName,
        int? targetEntityId,
        CancellationToken cancellationToken = default)
    {
        // Don't notify oneself
        if (actorUserId.HasValue && actorUserId.Value == recipientUserId)
        {
            return;
        }

        var notif = new TblNotification
        {
            RecipientUserId = recipientUserId,
            ActorUserId = actorUserId,
            NotificationType = notificationType,
            Title = title,
            Message = message,
            TargetEntityName = targetEntityName,
            TargetEntityId = targetEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = actorUserId
        };

        dbContext.TblNotifications.Add(notif);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateBulkNotificationsAsync(
        IEnumerable<int> recipientUserIds,
        int? actorUserId,
        string notificationType,
        string title,
        string message,
        string? targetEntityName,
        int? targetEntityId,
        CancellationToken cancellationToken = default)
    {
        var distinctRecipients = recipientUserIds
            .Where(uid => !actorUserId.HasValue || uid != actorUserId.Value)
            .Distinct()
            .ToList();

        if (distinctRecipients.Count == 0) return;

        var now = DateTime.UtcNow;
        var notifs = distinctRecipients.Select(uid => new TblNotification
        {
            RecipientUserId = uid,
            ActorUserId = actorUserId,
            NotificationType = notificationType,
            Title = title,
            Message = message,
            TargetEntityName = targetEntityName,
            TargetEntityId = targetEntityId,
            IsRead = false,
            CreatedAt = now,
            CreatedBy = actorUserId
        }).ToList();

        await dbContext.TblNotifications.AddRangeAsync(notifs, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> ResolveTargetUrlAsync(string? targetEntityName, int? targetEntityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(targetEntityName) || !targetEntityId.HasValue)
        {
            return "/groups";
        }

        try
        {
            switch (targetEntityName.ToUpperInvariant())
            {
                case "POST":
                    var post = await dbContext.TblPosts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PostId == targetEntityId.Value, cancellationToken);
                    if (post?.GroupId.HasValue == true)
                    {
                        return $"/group/{post.GroupId.Value}?tab=feed#post-{post.PostId}";
                    }
                    return "/groups";

                case "POLL":
                    var poll = await dbContext.TblPolls
                        .Include(p => p.Post)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PollId == targetEntityId.Value, cancellationToken);
                    if (poll?.Post?.GroupId.HasValue == true)
                    {
                        return $"/group/{poll.Post.GroupId.Value}?tab=polls#poll-{poll.PollId}";
                    }
                    return "/groups";

                case "GROUP":
                    return $"/group/{targetEntityId.Value}";

                case "GROUP_JOIN_REQUEST":
                    // Navigate directly to the members tab where the owner can review requests
                    return $"/group/{targetEntityId.Value}?tab=members";

                default:
                    return "/groups";
            }
        }
        catch
        {
            return "/groups";
        }
    }
}
