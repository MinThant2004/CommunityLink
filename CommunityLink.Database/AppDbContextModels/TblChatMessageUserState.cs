namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// Per-viewer state for a 1:1 chat message. One row per (message, user); <see cref="IsHidden"/>
/// is the server-side equivalent of Telegram's "Delete for myself", so the message stays
/// visible to the other participant and to the sender on any other device.
/// </summary>
public partial class TblChatMessageUserState
{
    public int ChatMessageUserStateId { get; set; }

    public int ChatMessageId { get; set; }

    public int UserId { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatMessage ChatMessage { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
