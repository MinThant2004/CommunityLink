namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// Hand-written additions to the scaffolded <see cref="TblChatGroupMessage"/>, kept in a
/// separate partial so re-scaffolding the database-first entity does not discard them.
/// </summary>
public partial class TblChatGroupMessage
{
    /// <summary>
    /// The group message this one replies to, or null. See
    /// <see cref="TblChatMessage.ReplyToMessageId"/> for why nothing lazy-loads it.
    /// </summary>
    public int? ReplyToChatGroupMessageId { get; set; }

    public virtual TblChatGroupMessage? ReplyToChatGroupMessage { get; set; }
}
