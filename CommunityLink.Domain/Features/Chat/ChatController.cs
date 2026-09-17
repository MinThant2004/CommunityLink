using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Chat;

[Route("api/chat")]
public sealed class ChatController(IChatService chatService) : BaseController
{
    [HttpGet("conversations")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetConversations(CancellationToken cancellationToken) =>
        ToActionResult(await chatService.GetConversationsAsync(cancellationToken));

    [HttpGet("conversations/{conversationId:int}/messages")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetMessages(int conversationId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.GetMessagesAsync(conversationId, cancellationToken));

    [HttpPost("messages")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatSend)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.SendMessageAsync(request, cancellationToken));
}