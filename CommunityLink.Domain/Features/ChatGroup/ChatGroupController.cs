namespace CommunityLink.Domain.Features.ChatGroup;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

[Route("api/chat-groups")]
public sealed class ChatGroupController(
    IChatGroupService chatGroupService,
    IServiceProvider serviceProvider) : BaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetChatGroups([FromQuery] string? search, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetChatGroupsAsync(search, cancellationToken));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateChatGroup([FromBody] CreateChatGroupRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.CreateChatGroupAsync(request, cancellationToken));

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyChatGroups(CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetMyChatGroupsAsync(cancellationToken));

    // Declared before "{chatGroupId:int}" so the literal segment wins routing.
    [HttpGet("preview")]
    [Authorize]
    public async Task<IActionResult> GetPreviews(CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetPreviewsAsync(cancellationToken));

    [HttpGet("{chatGroupId:int}")]
    [Authorize]
    public async Task<IActionResult> GetChatGroupById(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetChatGroupByIdAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinChatGroup(int chatGroupId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.JoinChatGroupAsync(chatGroupId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    [HttpPost("{chatGroupId:int}/join-paid")]
    [Authorize]
    public async Task<IActionResult> JoinPaidChatGroup(int chatGroupId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.JoinPaidChatGroupAsync(chatGroupId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    [HttpPost("{chatGroupId:int}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveChatGroup(int chatGroupId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.LeaveChatGroupAsync(chatGroupId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    [HttpGet("{chatGroupId:int}/members")]
    [Authorize]
    public async Task<IActionResult> GetMembers(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetMembersAsync(chatGroupId, cancellationToken));

    [HttpGet("my-memberships")]
    [Authorize]
    public async Task<IActionResult> GetMyMemberships(CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetMyMembershipsAsync(cancellationToken));

    [HttpGet("{chatGroupId:int}/messages")]
    [Authorize]
    public async Task<IActionResult> GetMessages(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetMessagesAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/messages")]
    [Authorize]
    public async Task<IActionResult> SendMessage(int chatGroupId, [FromBody] SendChatGroupMessageRequestModel request, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.SendMessageAsync(chatGroupId, request, cancellationToken);
        if (result.IsSuccess && result.Data != null)
        {
            await BroadcastAsync(chatGroupId, "ReceiveChatGroupMessage", cancellationToken, result.Data);
        }

        return ToActionResult(result);
    }

    /// <summary>Soft-deletes for the whole group. Sender, or an OWNER/ADMIN as moderator.</summary>
    [HttpDelete("{chatGroupId:int}/messages/{messageId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMessage(int chatGroupId, int messageId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.DeleteMessageAsync(chatGroupId, messageId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "ChatGroupMessageDeleted", cancellationToken, chatGroupId, messageId);
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Hides a message for the caller alone. The rest of the group is unaffected, so this is
    /// deliberately not broadcast.
    /// </summary>
    [HttpPost("{chatGroupId:int}/messages/{messageId:int}/hide")]
    [Authorize]
    public async Task<IActionResult> HideMessage(int chatGroupId, int messageId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.DeleteMessageForSelfAsync(chatGroupId, messageId, cancellationToken));

    /// <summary>Sets the caller's reaction, or clears it when the same emoji is sent again.</summary>
    [HttpPost("{chatGroupId:int}/messages/{messageId:int}/reaction")]
    [Authorize]
    public async Task<IActionResult> SetReaction(
        int chatGroupId,
        int messageId,
        [FromBody] SetMessageReactionRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await chatGroupService.SetReactionAsync(chatGroupId, messageId, request?.Emoji, cancellationToken);
        if (result.IsSuccess && result.Data is not null)
        {
            // The resulting state is broadcast, not the toggle, so every member's client
            // converges on the same counts without refetching the thread.
            await BroadcastAsync(
                chatGroupId,
                "ChatGroupReactionUpdated",
                cancellationToken,
                chatGroupId,
                messageId,
                result.Data);
        }

        return ToActionResult(result);
    }

    [HttpPost("{chatGroupId:int}/mute")]
    [Authorize]
    public async Task<IActionResult> ToggleMute(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.ToggleMuteAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/members/{targetUserId:int}/promote")]
    [Authorize]
    public async Task<IActionResult> PromoteMember(int chatGroupId, int targetUserId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.PromoteMemberAsync(chatGroupId, targetUserId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    [HttpPost("{chatGroupId:int}/members/{targetUserId:int}/demote")]
    [Authorize]
    public async Task<IActionResult> DemoteMember(int chatGroupId, int targetUserId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.DemoteMemberAsync(chatGroupId, targetUserId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    [HttpDelete("{chatGroupId:int}/members/{targetUserId:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveMember(int chatGroupId, int targetUserId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.RemoveMemberAsync(chatGroupId, targetUserId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    private async Task BroadcastAsync(
        int chatGroupId,
        string method,
        CancellationToken cancellationToken,
        params object?[] args)
    {
        try
        {
            var hubContext = serviceProvider.GetService<IHubContext<ChatGroupHub>>();
            if (hubContext != null)
            {
                string groupName = ChatGroupHub.GetGroupName(chatGroupId);
                await hubContext.Clients.Group(groupName).SendCoreAsync(method, args, cancellationToken);
            }
        }
        catch
        {
            // SignalR broadcast failure should not break the REST response
        }
    }
}
