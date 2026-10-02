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
        // The identity itself is still validated here so a connection without a parseable
        // claim fails fast, before any service call is made on its behalf.
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out _))
        {
            throw new HubException("Unauthorized connection context.");
        }

        using var scope = _serviceProvider.CreateScope();
        var chatGroupService = scope.ServiceProvider.GetRequiredService<IChatGroupService>();

        // Tests one membership row rather than loading the full roster. GetMembersAsync is
        // now member-gated, so using it here would reject every join, including valid members.
        var memberRes = await chatGroupService.IsActiveMemberAsync(chatGroupId);
        if (!memberRes.IsSuccess)
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

    public async Task SendGroupTyping(int chatGroupId, bool isTyping)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst("nameid")?.Value;

        if (int.TryParse(userIdStr, out int senderId))
        {
            string groupName = GetGroupName(chatGroupId);
            await Clients.OthersInGroup(groupName).SendAsync("GroupTyping", chatGroupId, senderId, isTyping);
        }
    }

    public static string GetGroupName(int chatGroupId) => $"chat-group-{chatGroupId}";
}
