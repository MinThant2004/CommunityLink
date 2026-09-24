using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblGroupChatMessage
{
    public int GroupChatMessageId { get; set; }

    public int GroupChatRoomId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public virtual TblGroupChatRoom GroupChatRoom { get; set; } = null!;

    public virtual TblUser Sender { get; set; } = null!;
}
