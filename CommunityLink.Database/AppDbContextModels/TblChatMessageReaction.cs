namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// An emoji reaction on a 1:1 chat message. The unique index on
/// (ChatMessageId, UserId) enforces the Telegram rule that a person may hold only one
/// reaction per message; picking another emoji updates this row rather than adding one.
/// </summary>
public partial class TblChatMessageReaction
{
    public int ChatMessageReactionId { get; set; }

    public int ChatMessageId { get; set; }

    public int UserId { get; set; }

    public string Emoji { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatMessage ChatMessage { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
