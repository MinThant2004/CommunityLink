using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

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
