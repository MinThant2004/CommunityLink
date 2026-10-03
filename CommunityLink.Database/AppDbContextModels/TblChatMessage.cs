using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblChatMessage
{
    public int ChatMessageId { get; set; }

    public int ConversationId { get; set; }

    public int SenderId { get; set; }

    public string MessageText { get; set; } = null!;

    public string? AttachmentUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public int? ReplyToMessageId { get; set; }

    public string MessageType { get; set; } = null!;

    public string? FileName { get; set; }

    public long? FileSizeByte { get; set; }

    public virtual TblConversation Conversation { get; set; } = null!;

    public virtual ICollection<TblChatMessage> InverseReplyToMessage { get; set; } = new List<TblChatMessage>();

    public virtual TblChatMessage? ReplyToMessage { get; set; }

    public virtual TblUser Sender { get; set; } = null!;

    public virtual ICollection<TblChatMessageReaction> TblChatMessageReactions { get; set; } = new List<TblChatMessageReaction>();

    public virtual ICollection<TblChatMessageUserState> TblChatMessageUserStates { get; set; } = new List<TblChatMessageUserState>();
}
