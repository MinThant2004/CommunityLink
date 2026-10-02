namespace CommunityLink.Shared.Features.ChatGroup;

using CommunityLink.Shared.Features.Chat;

public sealed record CreateChatGroupRequestModel(
    string Name,
    string? Description,
    string? AvatarUrl,
    string ChatType = "FREE", // FREE | PAID
    long JoinFeeLinkDrops = 0,
    string AccessMode = "PUBLIC" // PUBLIC | PRIVATE
);

/// <summary>
/// Owner-initiated adds. In a FREE group these users become members immediately. In a PAID group
/// with a non-zero fee they receive a pending invite instead and must pay before they get access,
/// so an invite never bypasses the join fee.
/// </summary>
public sealed record AddChatGroupMembersRequestModel(IReadOnlyList<int> UserIds);

/// <summary>Owner-only fee change. Applies to future joiners; existing members are untouched.</summary>
public sealed record SetChatGroupJoinFeeRequestModel(long JoinFeeLinkDrops);

/// <summary>
/// Owner-editable group fields. Fee and type changes deliberately do NOT touch existing members:
/// a member who already paid keeps access, and the change applies to future joiners only.
/// </summary>
public sealed record UpdateChatGroupRequestModel(
    string Name,
    string? Description,
    string ChatType = "FREE", // FREE | PAID
    long JoinFeeLinkDrops = 0,
    string AccessMode = "PUBLIC" // PUBLIC | PRIVATE
);

public sealed record ChatGroupModel(
    int ChatGroupId,
    string Name,
    string? Description,
    string? AvatarUrl,
    int CreatorId,
    string CreatorName,
    string ChatType,
    long JoinFeeLinkDrops,
    decimal CommissionPercentageSnapshot,
    int MemberCount,
    DateTime CreatedAt,
    bool IsJoined = false,
    string UserRole = "NONE", // OWNER | ADMIN | MEMBER | NONE
    // True when the viewer holds a PENDING invite into a PAID group. An invite is an offer, not
    // access: IsJoined stays false until the invitee pays, so the paywall still applies.
    bool IsInvited = false,
    // The fee this viewer was quoted when invited. Null when there is no pending invite. A later
    // fee change by the owner applies to new invites only and must not reprice an open invite.
    long? InviteFeeLinkDrops = null,
    // Display name of the owner who issued the pending invite, for the "X invited you" copy.
    string? InvitedByName = null,
    // The viewer's own moderation powers in this group. All-false for a non-member. The client
    // uses it to decide which controls to render, so the viewer is never offered an action the
    // server would reject. An owner always holds every permission regardless of this value.
    ChatGroupPermissionSet? ViewerPermissions = null,
    bool IsBanned = false,
    string AccessMode = "PUBLIC" // PUBLIC | PRIVATE
);

public sealed record ChatGroupMemberModel(
    int ChatGroupMemberId,
    int ChatGroupId,
    int UserId,
    string UserName,
    string DisplayName,
    string? UserAvatar,
    string Role,
    DateTime JoinedAt,
    bool IsMuted = false,
    // Owner-configurable powers for this member. Meaningful for an ADMIN; a MEMBER's is always
    // empty and the owner's is ignored, since ownership is not configurable.
    ChatGroupPermissionSet? Permissions = null,
    bool IsOnline = false,
    DateTime? LastActiveAt = null
);

/// <summary>
/// The pinned message of a group, or <c>null</c> when nothing is pinned. Returned separately
/// from the message list because the banner needs to render above the conversation for every
/// member, including those who have loaded an older page of history.
/// </summary>
public sealed record ChatGroupPinnedMessageModel(
    int ChatGroupMessageId,
    int ChatGroupId,
    int SenderId,
    string SenderDisplayName,
    string Content,
    string MessageType,
    string? AttachmentUrl,
    DateTime PinnedAt,
    int PinnedByUserId
);

/// <summary>
/// What one member is allowed to do in one group. Held on <see cref="ChatGroupMemberModel"/> so
/// the owner can see and edit each admin's powers, and on <see cref="ChatGroupModel"/> as the
/// viewer's own set so the client never offers a moderation action the server would reject.
/// <para>
/// The owner is not represented here. Ownership is not configurable and not transferable, and
/// the owner is the group's only revenue recipient, so the owner always holds every permission
/// regardless of the values in this set.
/// </para>
/// </summary>
public sealed record ChatGroupPermissionSet(
    bool CanDeleteMessages = false,
    bool CanRemoveMembers = false,
    bool CanBanMembers = false,
    bool CanManageInviteLinks = false,
    bool CanPinMessages = false
)
{
    /// <summary>Every permission granted. Applied on promotion so a new admin is useful at once.</summary>
    public static ChatGroupPermissionSet All => new(
        CanDeleteMessages: true,
        CanRemoveMembers: true,
        CanBanMembers: true,
        CanManageInviteLinks: true,
        CanPinMessages: true);

    /// <summary>Nothing granted. Applied on demotion, so no stale power survives the role change.</summary>
    public static ChatGroupPermissionSet None => new();
}

/// <summary>
/// An active ban, used by the owner/admin "Banned members" list so a ban can be lifted.
/// Revoked bans are soft-deleted and deliberately not returned.
/// </summary>
public sealed record ChatGroupBannedMemberModel(
    int ChatGroupBanId,
    int ChatGroupId,
    int BannedUserId,
    string BannedUserName,
    string BannedUserDisplayName,
    string? BannedUserAvatar,
    int BannedByUserId,
    DateTime BannedAt
);

/// <summary>
/// A pending invite as the owner sees it, so the Manage panel can list outstanding invitations and
/// revoke them. Resolved invites are deliberately not returned.
/// </summary>
public sealed record ChatGroupInviteModel(
    int ChatGroupInviteId,
    int ChatGroupId,
    int InvitedUserId,
    string InvitedUserName,
    string InvitedUserDisplayName,
    string? InvitedUserAvatar,
    string Status,
    long FeeAtInviteLinkDrops,
    DateTime InvitedAt
);

public sealed record SendChatGroupMessageRequestModel(
    string Content,
    int? ReplyToChatGroupMessageId = null,
    string MessageType = "TEXT",
    string? AttachmentUrl = null,
    string? FileName = null,
    long? FileSizeByte = null
);

public sealed record ChatGroupPreviewModel(
    int ChatGroupId,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    string? LastSenderName
);

public sealed record ChatGroupMessageModel(
    int ChatGroupMessageId,
    int ChatGroupId,
    int SenderId,
    string SenderName,
    string SenderDisplayName,
    string? SenderAvatar,
    string Content,
    DateTime CreatedAt,
    bool IsMine = false,
    // Quoted message; see ChatMessageModel for the meaning of ReplyToIsDeleted.
    int? ReplyToChatGroupMessageId = null,
    string? ReplyToSenderName = null,
    string? ReplyToPreview = null,
    bool ReplyToIsDeleted = false,
    IReadOnlyList<MessageReactionModel>? Reactions = null,
    // Sender, or a group OWNER/ADMIN acting as a moderator.
    bool CanDeleteForEveryone = false,
    string MessageType = "TEXT",
    string? AttachmentUrl = null,
    string? FileName = null,
    long? FileSizeByte = null,
    string? FormattedFileSize = null
);

// ------------------------------------------------------------------
// Shareable invite links
// ------------------------------------------------------------------

/// <summary>
/// A shareable invite link as the owner/admin sees it. Contains the full URL so the UI
/// can copy/share it without rebuilding the base address.
/// </summary>
public sealed record ChatGroupInviteLinkModel(
    int ChatGroupInviteLinkId,
    int ChatGroupId,
    string Token,
    string FullUrl,
    string? Name,
    bool IsPrimary,
    DateTime? ExpiresAt,
    int? MaxUses,
    int UseCount,
    bool IsRevoked,
    DateTime CreatedAt
);

/// <summary>Input for creating a new invite link. All fields are optional.</summary>
public sealed record CreateInviteLinkRequestModel(
    string? Name = null,
    DateTime? ExpiresAt = null,
    int? MaxUses = null
);

/// <summary>
/// Public preview of a group for the invite landing page. The viewer may not be authenticated,
/// so this contains only the data needed to render the join prompt.
/// </summary>
public sealed record InviteLinkPreviewModel(
    int ChatGroupId,
    string GroupName,
    string? GroupDescription,
    string? GroupAvatar,
    int MemberCount,
    string ChatType,
    long JoinFeeLinkDrops,
    bool IsValid,
    string? InvalidReason,
    // When the caller is authenticated:
    bool IsAlreadyMember = false,
    bool IsBanned = false,
    string AccessMode = "PUBLIC", // PUBLIC | PRIVATE
    string? RequestStatus = null // PENDING_APPROVAL | APPROVED_WAITING_PAYMENT | REJECTED | JOINED
);

public sealed record ChatGroupJoinRequestModel(
    int ChatGroupJoinRequestId,
    int ChatGroupId,
    int UserId,
    string UserName,
    string UserDisplayName,
    string? UserAvatar,
    string Status, // PENDING_APPROVAL | APPROVED_WAITING_PAYMENT | REJECTED | JOINED
    DateTime RequestedAt,
    DateTime? ReviewedAt = null,
    string? ReviewedByUserName = null,
    bool IsOnline = false
);

public sealed record SubmitJoinRequestModel(
    string? RequestNote = null
);

