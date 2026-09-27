using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CommunityLink.Domain.Features.Chat;

[Authorize]
public class ChatHub : Hub
{
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

    public async Task MarkAsRead(int conversationId, int recipientId)
    {
        var currentUserIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (int.TryParse(currentUserIdStr, out int senderId))
        {
            await Clients.User(recipientId.ToString()).SendAsync("MessageRead", conversationId, senderId);
        }
    }
}
