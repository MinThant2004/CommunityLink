using CommunityLink.Shared;
using CommunityLink.Shared.Features.Notification;
using Microsoft.AspNetCore.Http;

namespace CommunityLink.App.Apis;

public sealed class NotificationApiService(IHttpClientFactory clientFactory, IHttpContextAccessor httpContextAccessor)
    : ApiService(clientFactory, httpContextAccessor)
{
    public Task<Result<NotificationSummaryDto>> GetNotificationsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default) =>
        GetAsync<NotificationSummaryDto>($"api/notifications?skip={skip}&take={take}", cancellationToken);

    public Task<Result<int>> GetUnreadCountAsync(CancellationToken cancellationToken = default) =>
        GetAsync<int>("api/notifications/unread-count", cancellationToken);

    public Task<Result> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default) =>
        PostAsync($"api/notifications/{notificationId}/read", new { }, cancellationToken);

    public Task<Result> MarkAllAsReadAsync(CancellationToken cancellationToken = default) =>
        PostAsync("api/notifications/read-all", new { }, cancellationToken);
}
