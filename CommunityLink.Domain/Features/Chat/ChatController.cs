using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Chat;

[Route("api/chat")]
public sealed class ChatController(
    IChatService chatService,
    IPrivateChatPaymentService paymentService,
    ICurrentUserContext currentUser) : BaseController
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

    [HttpPost("conversations/{conversationId:int}/read")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> MarkConversationRead(int conversationId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.MarkConversationReadAsync(conversationId, cancellationToken));

    [HttpPost("unlock/{creatorUserId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> UnlockPrivateChat(int creatorUserId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return ToActionResult(Result.Failure("Forbidden", ResultStatus.Forbidden));
        return ToActionResult(await paymentService.UnlockPrivateChatAsync(currentUser.UserId.Value, creatorUserId, cancellationToken));
    }

    [HttpGet("status/{creatorUserId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetChatStatus(int creatorUserId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null) return ToActionResult(Result.Failure("Forbidden", ResultStatus.Forbidden));

        var status = await paymentService.GetPrivateChatStatusAsync(
            creatorUserId,
            currentUser.UserId.Value,
            cancellationToken);

        return ToActionResult(Result<PrivateChatStatusModel>.Success(status));
    }
}