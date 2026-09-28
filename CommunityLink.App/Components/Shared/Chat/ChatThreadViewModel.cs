namespace CommunityLink.App.Components.Shared.Chat;

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

    public string Title { get; init; } = string.Empty;

    public string? AvatarUrl { get; init; }

    public string? Subtitle { get; init; }

    public string? LastMessagePreview { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public int MemberCount { get; init; }

    // Payment + membership state is mutable because 1:1 lock state is resolved lazily,
    // after the thread is opened, via GET api/chat/status/{creatorUserId}.
    public bool RequiresPayment { get; set; }

    public long FeeLinkDrops { get; set; }

    public bool IsUnlocked { get; set; }

    public bool IsJoined { get; init; }

    // Set by the mapper, but the "Paid" badge also depends on the lazily resolved
    // 1:1 lock state, so it cannot be init-only.
    public string? Badge { get; set; }

    public string? UserRole { get; init; }

    public int? DirectUserId { get; init; }

    public string? Description { get; init; }

    public string? CreatorName { get; init; }

    public decimal CommissionPercentage { get; init; }

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
