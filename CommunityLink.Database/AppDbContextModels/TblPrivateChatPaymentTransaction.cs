using System;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblPrivateChatPaymentTransaction
{
    public long PrivateChatPaymentTransactionId { get; set; }

    public int ConversationId { get; set; }

    public int BuyerUserId { get; set; }

    public int CreatorUserId { get; set; }

    public long GrossAmountLinkDrops { get; set; }

    public long CommissionAmount { get; set; }

    public long CreatorAmount { get; set; }

    public string Status { get; set; } = "COMPLETED";

    public DateTime CreatedAt { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual TblConversation Conversation { get; set; } = null!;

    public virtual TblUser BuyerUser { get; set; } = null!;

    public virtual TblUser CreatorUser { get; set; } = null!;
}
