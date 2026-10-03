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

    /// <summary>Deletes a message for both participants. Sender only.</summary>
    [HttpDelete("conversations/{conversationId:int}/messages/{messageId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> DeleteMessageForEveryone(
        int conversationId, int messageId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.DeleteMessageForEveryoneAsync(conversationId, messageId, cancellationToken));

    /// <summary>
    /// Hides a message for the caller alone. Kept off DELETE because it is a per-viewer flag
    /// rather than a mutation of the shared message, and the other participant is unaffected.
    /// </summary>
    [HttpPost("conversations/{conversationId:int}/messages/{messageId:int}/hide")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> DeleteMessageForSelf(
        int conversationId, int messageId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.DeleteMessageForSelfAsync(conversationId, messageId, cancellationToken));

    /// <summary>
    /// Sets the caller's reaction, or clears it when the same emoji is sent again. Responds
    /// with the full reaction list so the caller renders authoritative counts.
    /// </summary>
    [HttpPost("conversations/{conversationId:int}/messages/{messageId:int}/reaction")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> SetReaction(
        int conversationId,
        int messageId,
        [FromBody] SetMessageReactionRequestModel request,
        CancellationToken cancellationToken) =>
        ToActionResult(await chatService.SetReactionAsync(conversationId, messageId, request?.Emoji, cancellationToken));

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

    [HttpDelete("conversations/{conversationId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> DeleteConversationForSelf(int conversationId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.DeleteConversationForSelfAsync(conversationId, cancellationToken));

    [HttpPost("block/{targetUserId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> BlockUser(int targetUserId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.BlockUserAsync(targetUserId, cancellationToken));

    [HttpDelete("block/{targetUserId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> UnblockUser(int targetUserId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.UnblockUserAsync(targetUserId, cancellationToken));

    [HttpGet("block/status/{targetUserId:int}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetUserBlockStatus(int targetUserId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.GetUserBlockStatusAsync(targetUserId, cancellationToken));

    [HttpGet("block/list")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetBlockedUserIds(CancellationToken cancellationToken) =>
        ToActionResult(await chatService.GetBlockedUserIdsAsync(cancellationToken));

    [HttpGet("conversations/{conversationId:int}/media/counts")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetSharedMediaCounts(int conversationId, CancellationToken cancellationToken) =>
        ToActionResult(await chatService.GetSharedMediaCountsAsync(conversationId, cancellationToken));

    [HttpGet("conversations/{conversationId:int}/media/{category}")]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatAccess)]
    public async Task<IActionResult> GetSharedMedia(
        int conversationId,
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await chatService.GetSharedMediaAsync(conversationId, category, page, pageSize, cancellationToken));
}