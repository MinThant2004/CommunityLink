namespace CommunityLink.Domain.Features.ChatGroup;

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

[Authorize]
public class ChatGroupHub : Hub
{
    private readonly IServiceProvider _serviceProvider;

    public ChatGroupHub(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task JoinChatGroup(int chatGroupId)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
        {
            throw new HubException("Unauthorized connection context.");
        }

        using var scope = _serviceProvider.CreateScope();
        var chatGroupService = scope.ServiceProvider.GetRequiredService<IChatGroupService>();

        var memberRes = await chatGroupService.GetMembersAsync(chatGroupId);
        if (!memberRes.IsSuccess || memberRes.Data == null || !memberRes.Data.Any(m => m.UserId == userId))
        {
            throw new HubException("User is not an active member of this Chat Group.");
        }

        string groupName = GetGroupName(chatGroupId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveChatGroup(int chatGroupId)
    {
        string groupName = GetGroupName(chatGroupId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public static string GetGroupName(int chatGroupId) => $"chat-group-{chatGroupId}";
}
