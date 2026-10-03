using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

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
