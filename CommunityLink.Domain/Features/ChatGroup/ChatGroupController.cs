namespace CommunityLink.Domain.Features.ChatGroup;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

[Route("api/chat-groups")]
public sealed class ChatGroupController(IChatGroupService chatGroupService) : BaseController
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

    /// <summary>
    /// The group's single pinned message, or null when nothing is pinned. Open to any member: the
    /// banner is part of reading the group, not a moderation surface.
    /// </summary>
    [HttpGet("{chatGroupId:int}/pinned-message")]
    [Authorize]
    public async Task<IActionResult> GetPinnedMessage(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetPinnedMessageAsync(chatGroupId, cancellationToken));

    /// <summary>
    /// Pins a message, replacing whatever was pinned before. The author may pin their own; anyone
    /// else needs CanPinMessages.
    /// </summary>
    [HttpPost("{chatGroupId:int}/messages/{messageId:int}/pin")]
    [Authorize]
    public async Task<IActionResult> PinMessage(int chatGroupId, int messageId, CancellationToken cancellationToken) =>
        // The service broadcasts ChatGroupPinnedMessageChanged itself so the banner and the stored
        // pin cannot drift apart; the controller must not repeat it here.
        ToActionResult(await chatGroupService.PinMessageAsync(chatGroupId, messageId, cancellationToken));

    /// <summary>Clears the group's pin. OWNER, or an ADMIN holding CanPinMessages.</summary>
    [HttpDelete("{chatGroupId:int}/pinned-message")]
    [Authorize]
    public async Task<IActionResult> UnpinMessage(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.UnpinMessageAsync(chatGroupId, cancellationToken));

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
    public async Task<IActionResult> RemoveMember(int chatGroupId, int targetUserId, CancellationToken cancellationToken) =>
        // RemoveMemberAsync broadcasts the targeted ChatGroupMemberRemoved event and the
        // roster refresh itself, so the controller must not repeat either here.
        ToActionResult(await chatGroupService.RemoveMemberAsync(chatGroupId, targetUserId, cancellationToken));

    /// <summary>
    /// OWNER. Replaces an admin's permission set. The set is sent whole rather than as individual
    /// toggles so the client cannot leave a partially applied matrix on the server.
    /// </summary>
    [HttpPut("{chatGroupId:int}/members/{targetUserId:int}/permissions")]
    [Authorize]
    public async Task<IActionResult> UpdateMemberPermissions(
        int chatGroupId,
        int targetUserId,
        [FromBody] ChatGroupPermissionSet permissions,
        CancellationToken cancellationToken) =>
        // The service broadcasts ChatGroupPermissionsChanged and the roster refresh itself, so the
        // controller must not repeat either here.
        ToActionResult(await chatGroupService.UpdateMemberPermissionsAsync(
            chatGroupId, targetUserId, permissions, cancellationToken));

    // ------------------------------------------------------------------
    // Creator/Admin group management
    // ------------------------------------------------------------------

    // The service broadcasts for the operations below, so that a mutation and its realtime
    // notification cannot drift apart. The older member endpoints still broadcast from here.

    [HttpPut("{chatGroupId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateInfo(int chatGroupId, [FromBody] UpdateChatGroupRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.UpdateInfoAsync(chatGroupId, request, cancellationToken));

    [HttpPost("{chatGroupId:int}/image")]
    [Authorize]
    [RequestSizeLimit(ChatGroupImagePolicy.MaxBytes)]
    public async Task<IActionResult> UpdateImage(int chatGroupId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return ToActionResult(Result<ChatGroupImageUploadResponse>.Failure(
                "No image was supplied.", ResultStatus.ValidationError));
        }

        // The size cap is enforced again here: RequestSizeLimit bounds the whole request,
        // while the policy bounds the file alone.
        if (file.Length > ChatGroupImagePolicy.MaxBytes)
        {
            return ToActionResult(Result<ChatGroupImageUploadResponse>.Failure(
                "Image is too large. The maximum is 5 MB.", ResultStatus.ValidationError));
        }

        await using var stream = file.OpenReadStream();

        return ToActionResult(await chatGroupService.UpdateImageAsync(
            chatGroupId, stream, file.FileName, file.ContentType, file.Length, cancellationToken));
    }

    [HttpPost("{chatGroupId:int}/members")]
    [Authorize]
    public async Task<IActionResult> AddMembers(int chatGroupId, [FromBody] AddChatGroupMembersRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.AddMembersAsync(chatGroupId, request.UserIds, cancellationToken));

    [HttpPost("{chatGroupId:int}/members/{targetUserId:int}/ban")]
    [Authorize]
    public async Task<IActionResult> BanMember(int chatGroupId, int targetUserId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.BanMemberAsync(chatGroupId, targetUserId, cancellationToken));

    [HttpDelete("{chatGroupId:int}/members/{targetUserId:int}/ban")]
    [Authorize]
    public async Task<IActionResult> UnbanMember(int chatGroupId, int targetUserId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.UnbanMemberAsync(chatGroupId, targetUserId, cancellationToken));

    [HttpGet("{chatGroupId:int}/bans")]
    [Authorize]
    public async Task<IActionResult> GetBannedMembers(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetBannedMembersAsync(chatGroupId, cancellationToken));

    // Declared before "{chatGroupId:int}/invitations" purely for readability; the literal
    // "my-invitations" segment is what actually disambiguates it from the int route.

    /// <summary>PENDING invites addressed to the caller, so an invitee can see and accept them.</summary>
    [HttpGet("my-invitations")]
    [Authorize]
    public async Task<IActionResult> GetMyInvitations(CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetMyInvitationsAsync(cancellationToken));

    [HttpGet("{chatGroupId:int}/invitations")]
    [Authorize]
    public async Task<IActionResult> GetPendingInvitations(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetPendingInvitationsAsync(chatGroupId, cancellationToken));

    [HttpDelete("{chatGroupId:int}/invitations/{targetUserId:int}")]
    [Authorize]
    public async Task<IActionResult> RevokeInvitation(int chatGroupId, int targetUserId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.RevokeInvitationAsync(chatGroupId, targetUserId, cancellationToken));

    [HttpPost("{chatGroupId:int}/invitations/decline")]
    [Authorize]
    public async Task<IActionResult> DeclineInvitation(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.DeclineInvitationAsync(chatGroupId, cancellationToken));

    [HttpPut("{chatGroupId:int}/join-fee")]
    [Authorize]
    public async Task<IActionResult> SetJoinFee(int chatGroupId, [FromBody] SetChatGroupJoinFeeRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.SetJoinFeeAsync(chatGroupId, request.JoinFeeLinkDrops, cancellationToken));

    [HttpDelete("{chatGroupId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteChatGroup(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.DeleteChatGroupAsync(chatGroupId, cancellationToken));

    // ------------------------------------------------------------------
    // Shareable Invite Links
    // ------------------------------------------------------------------

    [HttpGet("invite/{token}/preview")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInviteLinkPreview(string token, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetInviteLinkPreviewAsync(token, cancellationToken));

    [HttpPost("invite/{token}/join")]
    [Authorize]
    public async Task<IActionResult> JoinViaInviteLink(string token, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.JoinViaInviteLinkAsync(token, cancellationToken));

    [HttpGet("{chatGroupId:int}/invite-links")]
    [Authorize]
    public async Task<IActionResult> GetInviteLinks(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetInviteLinksAsync(chatGroupId, cancellationToken));

    [HttpGet("{chatGroupId:int}/invite-links/primary")]
    [Authorize]
    public async Task<IActionResult> GetOrCreatePrimaryInviteLink(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetOrCreatePrimaryInviteLinkAsync(chatGroupId, cancellationToken));

    [HttpPost("{chatGroupId:int}/invite-links")]
    [Authorize]
    public async Task<IActionResult> CreateInviteLink(int chatGroupId, [FromBody] CreateInviteLinkRequestModel request, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.CreateInviteLinkAsync(chatGroupId, request, cancellationToken));

    [HttpDelete("{chatGroupId:int}/invite-links/{linkId:int}")]
    [Authorize]
    public async Task<IActionResult> RevokeInviteLink(int chatGroupId, int linkId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.RevokeInviteLinkAsync(chatGroupId, linkId, cancellationToken));

    // ------------------------------------------------------------------
    // Join Requests (Private Group Access Control)
    // ------------------------------------------------------------------

    [HttpPost("{chatGroupId:int}/join-requests")]
    [Authorize]
    public async Task<IActionResult> SubmitJoinRequest(int chatGroupId, [FromBody] SubmitJoinRequestModel? request, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.SubmitJoinRequestAsync(chatGroupId, request, cancellationToken));

    [HttpGet("{chatGroupId:int}/join-requests/my")]
    [Authorize]
    public async Task<IActionResult> GetMyJoinRequestStatus(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetMyJoinRequestStatusAsync(chatGroupId, cancellationToken));

    [HttpGet("{chatGroupId:int}/join-requests")]
    [Authorize]
    public async Task<IActionResult> GetJoinRequests(int chatGroupId, [FromQuery] string? status, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetJoinRequestsAsync(chatGroupId, status, cancellationToken));

    [HttpPost("{chatGroupId:int}/join-requests/{requestId:int}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveJoinRequest(int chatGroupId, int requestId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.ApproveJoinRequestAsync(chatGroupId, requestId, cancellationToken));

    [HttpPost("{chatGroupId:int}/join-requests/{requestId:int}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectJoinRequest(int chatGroupId, int requestId, [FromQuery] string? reason, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.RejectJoinRequestAsync(chatGroupId, requestId, reason, cancellationToken));

    [HttpPost("{chatGroupId:int}/pay-and-join")]
    [Authorize]
    public async Task<IActionResult> PayAndJoinApprovedRequest(int chatGroupId, CancellationToken cancellationToken)
    {
        var result = await chatGroupService.PayAndJoinApprovedRequestAsync(chatGroupId, cancellationToken);
        if (result.IsSuccess)
        {
            await BroadcastAsync(chatGroupId, "GroupMemberUpdated", cancellationToken, chatGroupId);
        }
        return ToActionResult(result);
    }

    [HttpGet("{chatGroupId:int}/media/counts")]
    [Authorize]
    public async Task<IActionResult> GetSharedMediaCounts(int chatGroupId, CancellationToken cancellationToken) =>
        ToActionResult(await chatGroupService.GetSharedMediaCountsAsync(chatGroupId, cancellationToken));

    [HttpGet("{chatGroupId:int}/media/{category}")]
    [Authorize]
    public async Task<IActionResult> GetSharedMedia(
        int chatGroupId,
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default) =>
        ToActionResult(await chatGroupService.GetSharedMediaAsync(chatGroupId, category, page, pageSize, cancellationToken));

    private async Task BroadcastAsync(
        int chatGroupId,
        string method,
        CancellationToken cancellationToken,
        params object?[] args)
    {
        try
        {
            // Routed through the service so the member endpoints broadcast through exactly the
            // same path as the management operations, rather than reaching for the hub
            // context a second way.
            await chatGroupService.BroadcastAsync(chatGroupId, method, args, cancellationToken);
        }
        catch
        {
            // SignalR broadcast failure should not break the REST response
        }
    }
}
