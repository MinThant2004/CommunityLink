namespace CommunityLink.App.Components.Shared.Chat;

using CommunityLink.Shared.Features.ChatGroup;

public enum ChatThreadType
{
    Private = 0,
    Group = 1
}

/// <summary>
/// UI-level adapter over the two disjoint backend chat stacks
/// (TblConversation/TblChatMessage and TblChatGroup/TblChatGroupMessage).
/// </summary>
public sealed class ChatThreadViewModel
{
    public ChatThreadType Type { get; init; }

    public int Id { get; init; }

    public string Title { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Subtitle { get; set; }

    public string? LastMessagePreview { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public int MemberCount { get; set; }

    // Payment + membership state is mutable because 1:1 lock state is resolved lazily,
    // after the thread is opened, via GET api/chat/status/{creatorUserId}.
    public bool RequiresPayment { get; set; }

    public long FeeLinkDrops { get; set; }

    public bool IsUnlocked { get; set; }

    // Set by the server but also mutated locally when a realtime event changes it: a ban or
    // removal flips IsJoined on the open thread, and an owner settings update rewrites the
    // name, avatar and fee in place rather than reloading the thread.
    public bool IsJoined { get; set; }

    public bool IsBanned { get; set; }

    // True when the viewer holds a PENDING invite into a PAID group. An invite is an offer, not
    // access, so IsJoined stays false and the paywall still applies until the viewer pays. Used
    // only for the "X invited you" copy; it never grants read access on its own.
    public bool IsInvited { get; set; }

    // Fee this viewer was quoted when invited, and who issued the invite. Null when not invited.
    public long? InviteFeeLinkDrops { get; set; }

    public string? InvitedByName { get; set; }

    // Set by the mapper, but the "Paid" badge also depends on the lazily resolved
    // 1:1 lock state, so it cannot be init-only.
    public string? Badge { get; set; }

    public string? UserRole { get; set; }

    // The viewer's own moderation powers in this group, mirrored from the server model. The owner
    // is reported as holding everything (ownership is not configurable), a non-member as holding
    // nothing. Mutable because a realtime permission change rewrites it in place.
    public ChatGroupPermissionSet? ViewerPermissions { get; set; }

    public int? DirectUserId { get; init; }

    public bool IsBlockedByMe { get; set; }

    public bool IsBlockedByTarget { get; set; }

    public bool IsOnline { get; set; }

    public DateTime? OtherUserLastActiveAt { get; set; }

    /// <summary>
    /// The viewer may pin any message, i.e. holds CanPinMessages. The owner is already reported
    /// as holding everything, so no separate owner test is needed.
    /// </summary>
    public bool CanPinMessages => ViewerPermissions?.CanPinMessages ?? false;

    public bool CanRemoveMembers => ViewerPermissions?.CanRemoveMembers ?? false;

    public bool CanBanMembers => ViewerPermissions?.CanBanMembers ?? false;

    public bool CanManageInviteLinks => ViewerPermissions?.CanManageInviteLinks ?? false;

    public string? Description { get; set; }

    public string? CreatorName { get; set; }

    public decimal CommissionPercentage { get; set; }

    public bool IsDirect => Type == ChatThreadType.Private;

    public bool IsGroup => Type == ChatThreadType.Group;

    public string TypeQuery => IsGroup ? "group" : "direct";

    /// <summary>
    /// A 1:1 thread only has a server-side id once a message has been sent, so an
    /// unopened conversation is addressed by the other participant instead.
    /// </summary>
    public bool HasConversationId => IsGroup || Id > 0;

    public string RouteQuery => IsGroup
        ? $"?type=group&thread={Id}"
        : Id > 0
            ? $"?type=direct&thread={Id}"
            : $"?type=direct&to={DirectUserId}";

    /// <summary>
    /// Mirrors the signature the page builds from the query string, so activating a
    /// thread marks that exact route as handled and OnParametersSetAsync does not
    /// re-enter the activation it just performed.
    /// </summary>
    public string RouteSignature => IsGroup
        ? $"group|{Id}|"
        : Id > 0
            ? $"direct|{Id}|"
            : $"direct||{DirectUserId}";

    public string Initials
    {
        get
        {
            var source = string.IsNullOrWhiteSpace(Title) ? "?" : Title.Trim();
            return source.Substring(0, 1).ToUpperInvariant();
        }
    }
}
