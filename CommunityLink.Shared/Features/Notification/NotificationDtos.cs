using System;

namespace CommunityLink.Shared.Features.Notification;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int RecipientUserId { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string? ActorAvatar { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TargetEntityName { get; set; }
    public int? TargetEntityId { get; set; }
    public string? TargetUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationSummaryDto
{
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public IReadOnlyList<NotificationDto> Notifications { get; set; } = Array.Empty<NotificationDto>();
}
