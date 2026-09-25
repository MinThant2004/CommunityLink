namespace CommunityLink.Domain.Features.ChatGroup;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using CommunityLink.Shared;
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

    [HttpGet("{chatGroupId:int}")]
    [Authorize]
    public async Task<IActionResult> GetChatGroupById(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetChatGroupByIdAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinChatGroup(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.JoinChatGroupAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/join-paid")]
    [Authorize]
    public async Task<IActionResult> JoinPaidChatGroup(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.JoinPaidChatGroupAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveChatGroup(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.LeaveChatGroupAsync(chatGroupId, cancellationToken));

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
            try
            {
                var hubContext = serviceProvider.GetService<IHubContext<ChatGroupHub>>();
                if (hubContext != null)
                {
                    string groupName = ChatGroupHub.GetGroupName(chatGroupId);
                    await hubContext.Clients.Group(groupName).SendAsync("ReceiveChatGroupMessage", result.Data, cancellationToken);
                }
            }
            catch
            {
                // SignalR broadcast failure should not break REST response
            }
        }
        return ToActionResult(result);
    }

    [HttpDelete("{chatGroupId:int}/messages/{messageId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMessage(int chatGroupId, int messageId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.DeleteMessageAsync(chatGroupId, messageId, cancellationToken));
}
