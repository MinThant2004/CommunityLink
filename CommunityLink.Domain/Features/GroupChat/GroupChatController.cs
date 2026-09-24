using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.GroupChat;

namespace CommunityLink.Domain.Features.GroupChat;

[Route("api/groups/{groupId:int}/chatroom")]
[Authorize]
public sealed class GroupChatController(IGroupChatService groupChatService) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateGroupChatRoom(int groupId, [FromBody] CreateGroupChatRoomRequestModel request, CancellationToken cancellationToken)
    {
        var model = new CreateGroupChatRoomRequestModel(groupId, request.ChatType, request.JoinFeeLinkDrops);
        return ToActionResult(await groupChatService.CreateGroupChatRoomAsync(model, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetGroupChatRoom(int groupId, CancellationToken cancellationToken) =>
        ToActionResult(await groupChatService.GetGroupChatRoomByGroupIdAsync(groupId, cancellationToken));

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(int groupId, CancellationToken cancellationToken) =>
        ToActionResult(await groupChatService.GetMessagesAsync(groupId, cancellationToken));

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage(int groupId, [FromBody] SendGroupChatMessageRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await groupChatService.SendMessageAsync(groupId, request.Content, cancellationToken));
}
