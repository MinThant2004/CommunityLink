namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// An emoji reaction on a chat group message. Group counterpart of
/// <see cref="TblChatMessageReaction"/>, with the same one-reaction-per-user rule.
/// </summary>
public partial class TblChatGroupMessageReaction
{
    public int ChatGroupMessageReactionId { get; set; }

    public int ChatGroupMessageId { get; set; }

    public int UserId { get; set; }

    public string Emoji { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatGroupMessage ChatGroupMessage { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
