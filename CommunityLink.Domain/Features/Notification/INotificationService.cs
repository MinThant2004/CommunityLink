using CommunityLink.Shared;
using CommunityLink.Shared.Features.Notification;

namespace CommunityLink.Domain.Features.Notification;

public interface INotificationService
{
    Task<Result<NotificationSummaryDto>> GetNotificationsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
    Task<Result<int>> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task<Result> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task<Result> MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    Task CreateNotificationAsync(int recipientUserId, int? actorUserId, string notificationType, string title, string message, string? targetEntityName, int? targetEntityId, CancellationToken cancellationToken = default);
    Task CreateBulkNotificationsAsync(IEnumerable<int> recipientUserIds, int? actorUserId, string notificationType, string title, string message, string? targetEntityName, int? targetEntityId, CancellationToken cancellationToken = default);
}
