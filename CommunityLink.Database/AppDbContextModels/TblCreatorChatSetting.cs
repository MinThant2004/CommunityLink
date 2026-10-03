using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblCreatorChatSetting
{
    public int CreatorChatSettingId { get; set; }

    public int CreatorUserId { get; set; }

    public bool IsPrivateChatEnabled { get; set; }

    public long PrivateChatFeeLinkDrops { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual TblUser CreatorUser { get; set; } = null!;
}
