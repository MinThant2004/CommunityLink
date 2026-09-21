using System;
using System.Collections.Generic;

namespace CommunityLink.Database.AppDbContextModels;

public partial class TblUser
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string NormalizedUserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public bool IsVerified { get; set; }

    public bool IsActive { get; set; }

    public decimal? AverageRating { get; set; }

    public int RatingCount { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<TblChatMessage> TblChatMessages { get; set; } = new List<TblChatMessage>();

    public virtual ICollection<TblComment> TblComments { get; set; } = new List<TblComment>();

    public virtual ICollection<TblCommunity> TblCommunities { get; set; } = new List<TblCommunity>();

    public virtual ICollection<TblCommunityJoinRequest> TblCommunityJoinRequestReviewedByNavigations { get; set; } = new List<TblCommunityJoinRequest>();

    public virtual ICollection<TblCommunityJoinRequest> TblCommunityJoinRequestUsers { get; set; } = new List<TblCommunityJoinRequest>();

    public virtual ICollection<TblCommunityMember> TblCommunityMembers { get; set; } = new List<TblCommunityMember>();

    public virtual ICollection<TblCommunityRating> TblCommunityRatings { get; set; } = new List<TblCommunityRating>();

    public virtual ICollection<TblConversation> TblConversationUserOnes { get; set; } = new List<TblConversation>();

    public virtual ICollection<TblConversation> TblConversationUserTwos { get; set; } = new List<TblConversation>();

    public virtual ICollection<TblGroupJoinRequest> TblGroupJoinRequestReviewedByNavigations { get; set; } = new List<TblGroupJoinRequest>();

    public virtual ICollection<TblGroupJoinRequest> TblGroupJoinRequestUsers { get; set; } = new List<TblGroupJoinRequest>();

    public virtual ICollection<TblGroupMember> TblGroupMembers { get; set; } = new List<TblGroupMember>();

    public virtual ICollection<TblGroup> TblGroups { get; set; } = new List<TblGroup>();

    public virtual ICollection<TblLinkDropPurchase> TblLinkDropPurchases { get; set; } = new List<TblLinkDropPurchase>();

    public virtual ICollection<TblLinkDropTransaction> TblLinkDropTransactions { get; set; } = new List<TblLinkDropTransaction>();

    public virtual TblLinkDropWallet? TblLinkDropWallet { get; set; }

    public virtual ICollection<TblNotification> TblNotificationActorUsers { get; set; } = new List<TblNotification>();

    public virtual ICollection<TblNotification> TblNotificationRecipientUsers { get; set; } = new List<TblNotification>();

    public virtual ICollection<TblPollVote> TblPollVotes { get; set; } = new List<TblPollVote>();

    public virtual ICollection<TblPostLike> TblPostLikes { get; set; } = new List<TblPostLike>();

    public virtual ICollection<TblPostShare> TblPostShares { get; set; } = new List<TblPostShare>();

    public virtual ICollection<TblPost> TblPosts { get; set; } = new List<TblPost>();

    public virtual ICollection<TblSavedAccount> TblSavedAccountSavedUsers { get; set; } = new List<TblSavedAccount>();

    public virtual ICollection<TblSavedAccount> TblSavedAccountUsers { get; set; } = new List<TblSavedAccount>();

    public virtual ICollection<TblSavedPost> TblSavedPosts { get; set; } = new List<TblSavedPost>();

    public virtual ICollection<TblUserRating> TblUserRatingRaterUsers { get; set; } = new List<TblUserRating>();

    public virtual ICollection<TblUserRating> TblUserRatingTargetUsers { get; set; } = new List<TblUserRating>();

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();
}
