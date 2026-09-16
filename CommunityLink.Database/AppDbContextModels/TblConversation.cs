using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblConversation
{
    public int ConversationId { get; set; }

    public int UserOneId { get; set; }

    public int UserTwoId { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public string? LastMessagePreview { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<TblChatMessage> TblChatMessages { get; set; } = new List<TblChatMessage>();

    public virtual TblUser UserOne { get; set; } = null!;

    public virtual TblUser UserTwo { get; set; } = null!;
}
