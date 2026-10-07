using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityLink.Domain.Features.Chat;

[Authorize]
public class ChatHub(IServiceProvider serviceProvider, PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var currentUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (int.TryParse(currentUserIdStr, out int userId))
        {
            bool isNewlyOnline = presenceTracker.UserConnected(userId, Context.ConnectionId);

            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CommunityLink.Database.AppDbContextModels.AppDbContext>();
                var user = await db.TblUsers.FindAsync(userId);
                if (user is not null)
                {
                    user.LastActiveAt = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            if (isNewlyOnline)
            {
                await Clients.All.SendAsync("UserOnline", userId);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var currentUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (int.TryParse(currentUserIdStr, out int userId))
        {
            bool isNewlyOffline = presenceTracker.UserDisconnected(userId, Context.ConnectionId);

            DateTime? lastActiveAt = null;
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CommunityLink.Database.AppDbContextModels.AppDbContext>();
                var user = await db.TblUsers.FindAsync(userId);
                if (user is not null)
                {
                    user.LastActiveAt = DateTime.UtcNow;
                    lastActiveAt = user.LastActiveAt;
                    await db.SaveChangesAsync();
                }
            }

            if (isNewlyOffline)
            {
                await Clients.All.SendAsync("UserOffline", userId, lastActiveAt);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTyping(int recipientId, bool isTyping)
    {
        var currentUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (int.TryParse(currentUserIdStr, out int senderId))
        {
            await Clients.User(recipientId.ToString()).SendAsync("Typing", senderId, isTyping);
        }
    }

    /// <summary>
    /// Marks the caller's inbound messages as read. The read state is persisted by the
    /// service (not just announced to the peer), then the peer is notified.
    /// </summary>
    public async Task MarkAsRead(int conversationId, int recipientId)
    {
        var currentUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (!int.TryParse(currentUserIdStr, out int senderId))
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

        var result = await chatService.MarkConversationReadAsync(conversationId);
        if (!result.IsSuccess)
        {
            return;
        }

        if (result.Data > 0)
        {
            await Clients.User(recipientId.ToString()).SendAsync("MessageRead", conversationId, senderId);
        }
    }
}
