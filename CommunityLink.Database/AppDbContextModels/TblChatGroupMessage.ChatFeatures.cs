namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// Hand-written additions to the scaffolded <see cref="TblChatGroupMessage"/>, kept in a
/// separate partial so re-scaffolding the database-first entity does not discard them.
/// <para>
/// The pin lives on the message row rather than in a side table: a soft-deleted message then
/// stops being pinned automatically, because every read already filters <c>IsDeleted</c>, and a
/// retracted pin leaves nothing to clean up. At most one message is pinned per group; the service
/// clears any existing pin in the same transaction that sets a new one.
/// </para>
/// </summary>
public partial class TblChatGroupMessage
{
    public bool IsPinned { get; set; }

    public DateTime? PinnedAt { get; set; }

    /// <summary>Who pinned it, for the "pinned by" line in the pinned banner. Nullable.</summary>
    public int? PinnedByUserId { get; set; }
}
