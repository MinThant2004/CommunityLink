namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// Hand-written additions to the scaffolded <see cref="TblChatMessage"/>, kept in a separate
/// partial so re-scaffolding the database-first entity does not discard them.
/// </summary>
public partial class TblChatMessage
{
    /// <summary>
    /// The message this one replies to, or null when it is a plain message. Services resolve
    /// the quoted text for a whole page of messages in one bulk query rather than per row, so
    /// this navigation is only used by the query translator, never lazy-loaded.
    /// </summary>
    public int? ReplyToMessageId { get; set; }

    public virtual TblChatMessage? ReplyToMessage { get; set; }
}
