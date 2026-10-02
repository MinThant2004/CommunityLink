namespace CommunityLink.Domain.Features.ChatGroup;

using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Features.ChatGroup;

/// <summary>
/// The owner-configurable moderation powers an admin can hold in one group. The OWNER always
/// holds all of them and is not configurable; a plain MEMBER holds none.
/// </summary>
public enum ChatGroupPermissionKind
{
    /// <summary>Soft-delete another member's message for the whole group.</summary>
    DeleteMessages,

    /// <summary>Remove a plain member. The member may rejoin freely afterwards.</summary>
    RemoveMembers,

    /// <summary>Ban and unban a plain member. A ban also removes them and blocks rejoining.</summary>
    BanMembers,

    /// <summary>Create, list and revoke the group's shareable invite links.</summary>
    ManageInviteLinks,

    /// <summary>Pin and unpin the group's single pinned message.</summary>
    PinMessages
}

public interface IChatGroupService
{
    Task<Result<ChatGroupModel>> CreateChatGroupAsync(CreateChatGroupRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetChatGroupsAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyChatGroupsAsync(CancellationToken cancellationToken = default);
    Task<Result<ChatGroupModel>> GetChatGroupByIdAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result> JoinChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result> JoinPaidChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Leaves the group. Ownership is not transferable, so an owner leaving instead soft-deletes
    /// the group and every membership in it, exactly as <see cref="DeleteChatGroupAsync"/> would.
    /// </summary>
    Task<Result> LeaveChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Full member roster. Restricted to active members of the group: a non-member gets
    /// Forbidden rather than an empty list, so callers must not fall back to it as a
    /// membership probe. Use <see cref="IsActiveMemberAsync(int, CancellationToken)"/> for that.
    /// </summary>
    Task<Result<IReadOnlyList<ChatGroupMemberModel>>> GetMembersAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether the caller holds a non-deleted membership row in the group. Used by the hub
    /// room join guard, which must not load the whole roster just to test one membership.
    /// </summary>
    Task<Result> IsActiveMemberAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>Whether a specific user holds a non-deleted membership row in the group.</summary>
    Task<Result<bool>> IsActiveMemberAsync(int chatGroupId, int userId, CancellationToken cancellationToken = default);

    /// <summary>Pushes a payload to every member currently connected to the group room.</summary>
    Task BroadcastAsync(int chatGroupId, string method, object?[] args, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyMembershipsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupMessageModel>>> GetMessagesAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result<ChatGroupMessageModel>> SendMessageAsync(int chatGroupId, SendChatGroupMessageRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>Soft-deletes for all members. Sender, or a group OWNER/ADMIN as moderator.</summary>
    Task<Result> DeleteMessageAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>Hides the message for the caller only; the rest of the group still sees it.</summary>
    Task<Result> DeleteMessageForSelfAsync(int chatGroupId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The group's single pinned message, or <c>null</c> when nothing is pinned. Returned
    /// separately from the message list because the banner must render for every member,
    /// including one who has loaded an older page of history. Membership-gated.
    /// </summary>
    Task<Result<ChatGroupPinnedMessageModel?>> GetPinnedMessageAsync(
        int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pins a message, replacing whatever was pinned before, and returns the new pin. OWNER, or an
    /// ADMIN holding CanPinMessages.
    /// </summary>
    Task<Result<ChatGroupPinnedMessageModel>> PinMessageAsync(
        int chatGroupId, int messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the group's pin. OWNER, or an ADMIN holding CanPinMessages. Succeeds even when
    /// nothing is pinned, so the client can call it to reconcile after a delete.
    /// </summary>
    Task<Result> UnpinMessageAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>Sets or clears the caller's single reaction; returns the resulting list.</summary>
    Task<Result<IReadOnlyList<MessageReactionModel>>> SetReactionAsync(
        int chatGroupId, int messageId, string? emoji, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ChatGroupPreviewModel>>> GetPreviewsAsync(CancellationToken cancellationToken = default);

    Task<Result> ToggleMuteAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result> PromoteMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default);
    Task<Result> DemoteMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default);
    Task<Result> RemoveMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// OWNER. Replaces an admin's permission set wholesale rather than toggling one column, so the
    /// client's optimistic edit and the stored row cannot drift apart. Only meaningful for an
    /// ADMIN: the owner is not configurable and a member's flags are never consulted. Admins are
    /// notified so their own client can pick the new powers up without a manual reload.
    /// </summary>
    Task<Result<ChatGroupPermissionSet>> UpdateMemberPermissionsAsync(
        int chatGroupId, int targetUserId, ChatGroupPermissionSet permissions,
        CancellationToken cancellationToken = default);

    // ---- Creator/Admin group management (owner unless noted) ----

    /// <summary>
    /// Replaces the group avatar with an already-validated upload. Stores the new file first and
    /// deletes the replaced one only after the row is committed, so a failed write cannot leave
    /// the group pointing at a file that no longer exists.
    /// </summary>
    Task<Result<ChatGroupImageUploadResponse>> UpdateImageAsync(
        int chatGroupId, Stream imageStream, string fileName, string? contentType, long declaredLength,
        CancellationToken cancellationToken = default);

    /// <summary>OWNER. Updates Name/Description/ChatType/JoinFee and notifies members.</summary>
    Task<Result<ChatGroupModel>> UpdateInfoAsync(int chatGroupId, UpdateChatGroupRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// OWNER. Adds users to the group. In a FREE group (or a PAID group with a zero fee) this
    /// creates the membership immediately. In a PAID group with a non-zero fee it issues a
    /// PENDING invite instead, which the invitee must pay to convert; the invite never bypasses
    /// the join fee. Banned users are refused on both paths.
    /// </summary>
    Task<Result> AddMembersAsync(int chatGroupId, IReadOnlyList<int> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// PENDING invites addressed to the caller, across all groups. Surfaced in the chat list so an
    /// invitee can accept, and priced from the invite snapshot rather than the group's current fee.
    /// </summary>
    Task<Result<IReadOnlyList<ChatGroupModel>>> GetMyInvitationsAsync(CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Outstanding invites for the group, newest first, for the Manage panel.</summary>
    Task<Result<IReadOnlyList<ChatGroupInviteModel>>> GetPendingInvitationsAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// OWNER/ADMIN. Revokes a pending invite. The row is retained with Status = 'REVOKED' for
    /// audit, and the user becomes re-invitable.
    /// </summary>
    Task<Result> RevokeInvitationAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// INVITEE. Declines their own pending invite into a PAID group. Retained as Status =
    /// 'DECLINED' for audit, and the user becomes re-invitable.
    /// </summary>
    Task<Result> DeclineInvitationAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Bans a user, which also removes them from the group.</summary>
    Task<Result> BanMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Lifts a ban so the user may join or be re-added.</summary>
    Task<Result> UnbanMemberAsync(int chatGroupId, int targetUserId, CancellationToken cancellationToken = default);

    /// <summary>OWNER. Changes the join fee. Applies to future joins only; existing members are unaffected.</summary>
    Task<Result<ChatGroupModel>> SetJoinFeeAsync(int chatGroupId, long joinFeeLinkDrops, CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Returns all active bans for the group, newest first.</summary>
    Task<Result<IReadOnlyList<ChatGroupBannedMemberModel>>> GetBannedMembersAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>OWNER. Soft-deletes the group and its memberships; messages and payments are retained for audit.</summary>
    Task<Result> DeleteChatGroupAsync(int chatGroupId, CancellationToken cancellationToken = default);

    // ---- Shareable invite links (Owner/Admin) ----

    /// <summary>OWNER/ADMIN. Returns all non-deleted invite links for the group, newest first.</summary>
    Task<Result<IReadOnlyList<ChatGroupInviteLinkModel>>> GetInviteLinksAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Returns the primary link, creating one if it doesn't exist.</summary>
    Task<Result<ChatGroupInviteLinkModel>> GetOrCreatePrimaryInviteLinkAsync(int chatGroupId, CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Creates a new invite link with optional name/expiry/max-uses.</summary>
    Task<Result<ChatGroupInviteLinkModel>> CreateInviteLinkAsync(int chatGroupId, CreateInviteLinkRequestModel request, CancellationToken cancellationToken = default);

    /// <summary>OWNER/ADMIN. Revokes an invite link so it can no longer be used.</summary>
    Task<Result> RevokeInviteLinkAsync(int chatGroupId, int linkId, CancellationToken cancellationToken = default);

    /// <summary>Public. Returns a preview of the group behind the invite token, including validity.</summary>
    Task<Result<InviteLinkPreviewModel>> GetInviteLinkPreviewAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Authenticated. Joins the group via a valid invite link token.</summary>
    Task<Result> JoinViaInviteLinkAsync(string token, CancellationToken cancellationToken = default);

    // ---- Private Group Join Requests ----
    Task<Result<ChatGroupJoinRequestModel>> SubmitJoinRequestAsync(int chatGroupId, SubmitJoinRequestModel? request = null, CancellationToken cancellationToken = default);
    Task<Result<ChatGroupJoinRequestModel?>> GetMyJoinRequestStatusAsync(int chatGroupId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatGroupJoinRequestModel>>> GetJoinRequestsAsync(int chatGroupId, string? status = null, CancellationToken cancellationToken = default);
    Task<Result> ApproveJoinRequestAsync(int chatGroupId, int requestId, CancellationToken cancellationToken = default);
    Task<Result> RejectJoinRequestAsync(int chatGroupId, int requestId, string? reason = null, CancellationToken cancellationToken = default);
    Task<Result> PayAndJoinApprovedRequestAsync(int chatGroupId, CancellationToken cancellationToken = default);
}
