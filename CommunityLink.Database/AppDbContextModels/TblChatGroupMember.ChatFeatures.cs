namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// Hand-written additions to the scaffolded <see cref="TblChatGroupMember"/>, kept in a
/// separate partial so re-scaffolding the database-first entity does not discard them.
/// <para>
/// These are the owner-configurable moderation powers of one member in one group. A membership
/// row is already exactly one (group, user) pair, so the matrix belongs here rather than in a
/// side table.
/// </para>
/// <para>
/// They are meaningless for a plain MEMBER, which the service never consults, and ignored for
/// the OWNER, who always holds every permission. Ownership is not transferable and the owner is
/// the group's only revenue recipient, so the owner is deliberately not configurable.
/// </para>
/// </summary>
public partial class TblChatGroupMember
{
    /// <summary>May soft-delete another member's message for the whole group.</summary>
    public bool CanDeleteMessages { get; set; }

    /// <summary>May remove a plain member from the group. The member may rejoin freely.</summary>
    public bool CanRemoveMembers { get; set; }

    /// <summary>May ban and unban a plain member. A ban also removes them and blocks rejoining.</summary>
    public bool CanBanMembers { get; set; }

    /// <summary>May create, list and revoke the group's shareable invite links.</summary>
    public bool CanManageInviteLinks { get; set; }

    /// <summary>May pin and unpin the group's single pinned message.</summary>
    public bool CanPinMessages { get; set; }
}
