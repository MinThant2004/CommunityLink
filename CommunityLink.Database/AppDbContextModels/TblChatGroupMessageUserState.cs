namespace CommunityLink.Database.AppDbContextModels;

/// <summary>
/// Per-viewer state for a chat group message. Group counterpart of
/// <see cref="TblChatMessageUserState"/>; kept as a separate table so the two stacks stay
/// independently deployable.
/// </summary>
public partial class TblChatGroupMessageUserState
{
    public int ChatGroupMessageUserStateId { get; set; }

    public int ChatGroupMessageId { get; set; }

    public int UserId { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblChatGroupMessage ChatGroupMessage { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
