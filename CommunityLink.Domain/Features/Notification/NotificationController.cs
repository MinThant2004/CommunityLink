using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Notification;

namespace CommunityLink.Domain.Features.Notification;

[Route("api/notifications")]
[Authorize]
public sealed class NotificationController(INotificationService notificationService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int skip = 0, [FromQuery] int take = 10, CancellationToken cancellationToken = default) =>
        ToActionResult(await notificationService.GetNotificationsAsync(skip, take, cancellationToken));

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default) =>
        ToActionResult(await notificationService.GetUnreadCountAsync(cancellationToken));

    [HttpPost("{notificationId:int}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId, CancellationToken cancellationToken = default) =>
        ToActionResult(await notificationService.MarkAsReadAsync(notificationId, cancellationToken));

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default) =>
        ToActionResult(await notificationService.MarkAllAsReadAsync(cancellationToken));
}
