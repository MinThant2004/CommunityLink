using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAdmin> TblAdmins { get; set; }

    public virtual DbSet<TblAdminInvite> TblAdminInvites { get; set; }

    public virtual DbSet<TblAdminRole> TblAdminRoles { get; set; }

    public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }

    public virtual DbSet<TblChatGroup> TblChatGroups { get; set; }

    public virtual DbSet<TblChatGroupBan> TblChatGroupBans { get; set; }

    public virtual DbSet<TblChatGroupInvite> TblChatGroupInvites { get; set; }

    public virtual DbSet<TblChatGroupInviteLink> TblChatGroupInviteLinks { get; set; }

    public virtual DbSet<TblChatGroupJoinRequest> TblChatGroupJoinRequests { get; set; }

    public virtual DbSet<TblChatGroupMember> TblChatGroupMembers { get; set; }

    public virtual DbSet<TblChatGroupMessage> TblChatGroupMessages { get; set; }

    public virtual DbSet<TblChatGroupMessageReaction> TblChatGroupMessageReactions { get; set; }

    public virtual DbSet<TblChatGroupMessageUserState> TblChatGroupMessageUserStates { get; set; }

    public virtual DbSet<TblChatGroupPaymentTransaction> TblChatGroupPaymentTransactions { get; set; }

    public virtual DbSet<TblChatMessage> TblChatMessages { get; set; }

    public virtual DbSet<TblChatMessageReaction> TblChatMessageReactions { get; set; }

    public virtual DbSet<TblChatMessageUserState> TblChatMessageUserStates { get; set; }

    public virtual DbSet<TblComment> TblComments { get; set; }

    public virtual DbSet<TblCommunity> TblCommunities { get; set; }

    public virtual DbSet<TblCommunityJoinRequest> TblCommunityJoinRequests { get; set; }

    public virtual DbSet<TblCommunityMember> TblCommunityMembers { get; set; }

    public virtual DbSet<TblCommunityRating> TblCommunityRatings { get; set; }

    public virtual DbSet<TblConversation> TblConversations { get; set; }

    public virtual DbSet<TblCreatorChatSetting> TblCreatorChatSettings { get; set; }

    public virtual DbSet<TblCreatorPayoutRequest> TblCreatorPayoutRequests { get; set; }

    public virtual DbSet<TblGroup> TblGroups { get; set; }

    public virtual DbSet<TblGroupJoinRequest> TblGroupJoinRequests { get; set; }

    public virtual DbSet<TblGroupMember> TblGroupMembers { get; set; }

    public virtual DbSet<TblGroupRating> TblGroupRatings { get; set; }

    public virtual DbSet<TblIdentityVerification> TblIdentityVerifications { get; set; }

    public virtual DbSet<TblLinkDropPackage> TblLinkDropPackages { get; set; }

    public virtual DbSet<TblLinkDropPurchase> TblLinkDropPurchases { get; set; }

    public virtual DbSet<TblLinkDropPurchaseProof> TblLinkDropPurchaseProofs { get; set; }

    public virtual DbSet<TblLinkDropTransaction> TblLinkDropTransactions { get; set; }

    public virtual DbSet<TblLinkDropWallet> TblLinkDropWallets { get; set; }

    public virtual DbSet<TblNotification> TblNotifications { get; set; }

    public virtual DbSet<TblPasswordResetOtp> TblPasswordResetOtps { get; set; }

    public virtual DbSet<TblPaymentMethod> TblPaymentMethods { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblPlatformSetting> TblPlatformSettings { get; set; }

    public virtual DbSet<TblPoll> TblPolls { get; set; }

    public virtual DbSet<TblPollOption> TblPollOptions { get; set; }

    public virtual DbSet<TblPollVote> TblPollVotes { get; set; }

    public virtual DbSet<TblPost> TblPosts { get; set; }

    public virtual DbSet<TblPostImage> TblPostImages { get; set; }

    public virtual DbSet<TblPostLike> TblPostLikes { get; set; }

    public virtual DbSet<TblPostShare> TblPostShares { get; set; }

    public virtual DbSet<TblPrivateChatPaymentTransaction> TblPrivateChatPaymentTransactions { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblRolePermission> TblRolePermissions { get; set; }

    public virtual DbSet<TblSavedAccount> TblSavedAccounts { get; set; }

    public virtual DbSet<TblSavedPost> TblSavedPosts { get; set; }

    public virtual DbSet<TblSkillEndorsement> TblSkillEndorsements { get; set; }

    public virtual DbSet<TblSubscriptionPlan> TblSubscriptionPlans { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserActivity> TblUserActivities { get; set; }

    public virtual DbSet<TblUserBlock> TblUserBlocks { get; set; }

    public virtual DbSet<TblUserFollow> TblUserFollows { get; set; }

    public virtual DbSet<TblUserRating> TblUserRatings { get; set; }

    public virtual DbSet<TblUserRole> TblUserRoles { get; set; }

    public virtual DbSet<TblUserSkill> TblUserSkills { get; set; }

    public virtual DbSet<TblUserSubscription> TblUserSubscriptions { get; set; }

    public virtual DbSet<TblUserVerificationAudit> TblUserVerificationAudits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAdmin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__TblAdmin__719FE4886930D3D8");

            entity.ToTable("TblAdmin");

            entity.HasIndex(e => e.Email, "UQ__TblAdmin__A9D10534F396F02B").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblAdminInvite>(entity =>
        {
            entity.HasKey(e => e.InviteId);

            entity.ToTable("TblAdminInvite");

            entity.HasIndex(e => e.Token, "IX_TblAdminInvite_Token").IsUnique();

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_TblAdminInvite_CreatedAtUtc");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Token).HasMaxLength(200);

            entity.HasOne(d => d.Role).WithMany(p => p.TblAdminInvites)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblAdminInvite_Role");
        });

        modelBuilder.Entity<TblAdminRole>(entity =>
        {
            entity.HasKey(e => e.AdminRoleId).HasName("PK__TblAdmin__D0B2ED0667D388FC");

            entity.ToTable("TblAdminRole");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Admin).WithMany(p => p.TblAdminRoles)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblAdminRole_Admin");

            entity.HasOne(d => d.Role).WithMany(p => p.TblAdminRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblAdminRole_Role");
        });

        modelBuilder.Entity<TblAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__TblAudit__EB5F6CBD2BAEE256");

            entity.ToTable("TblAuditLog");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.ActorType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblChatGroup>(entity =>
        {
            entity.HasKey(e => e.ChatGroupId).HasName("PK__TblChatG__435956989E78C6F8");

            entity.ToTable("TblChatGroup");

            entity.HasIndex(e => e.ChatType, "IX_TblChatGroup_ChatType");

            entity.HasIndex(e => e.CreatorId, "IX_TblChatGroup_CreatorId");

            entity.Property(e => e.ChatType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("FREE", "DF_TblChatGroup_ChatType");
            entity.Property(e => e.AccessMode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PUBLIC", "DF_TblChatGroup_AccessMode");
            entity.Property(e => e.CommissionPercentageSnapshot).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroup_CreatedAt");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_TblChatGroup_IsActive");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Creator).WithMany(p => p.TblChatGroups)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroup_TblUser_Creator");
        });

        modelBuilder.Entity<TblChatGroupJoinRequest>(entity =>
        {
            entity.HasKey(e => e.ChatGroupJoinRequestId);

            entity.ToTable("TblChatGroupJoinRequest");

            entity.HasIndex(e => new { e.UserId, e.Status }, "IX_TblChatGroupJoinRequest_User_Status");

            entity.HasIndex(e => new { e.ChatGroupId, e.UserId }, "UQ_TblChatGroupJoinRequest_Group_User")
                .IsUnique()
                .HasFilter("([Status]='PENDING_APPROVAL' OR [Status]='APPROVED_WAITING_PAYMENT')");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupJoinRequest_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValue("PENDING_APPROVAL");

            entity.HasOne(d => d.ChatGroup).WithMany(p => p.TblChatGroupJoinRequests)
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupJoinRequest_ChatGroup");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.TblChatGroupJoinRequestReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_TblChatGroupJoinRequest_Reviewer");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupJoinRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupJoinRequest_User");
        });

        modelBuilder.Entity<TblChatGroupBan>(entity =>
        {
            entity.HasKey(e => e.ChatGroupBanId);

            entity.ToTable("TblChatGroupBan");

            entity.HasIndex(e => new { e.ChatGroupId, e.UserId }, "UQ_TblChatGroupBan_Group_User")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.BannedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupBan_BannedAt");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupBan_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.BannedByUser).WithMany(p => p.TblChatGroupBanBannedByUsers)
                .HasForeignKey(d => d.BannedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupBan_TblUser_BannedBy");

            entity.HasOne(d => d.ChatGroup).WithMany(p => p.TblChatGroupBans)
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupBan_TblChatGroup");

            entity.HasOne(d => d.RevokedByUser).WithMany(p => p.TblChatGroupBanRevokedByUsers)
                .HasForeignKey(d => d.RevokedByUserId)
                .HasConstraintName("FK_TblChatGroupBan_TblUser_RevokedBy");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupBanUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupBan_TblUser_User");
        });

        modelBuilder.Entity<TblChatGroupInvite>(entity =>
        {
            entity.HasKey(e => e.ChatGroupInviteId);

            entity.ToTable("TblChatGroupInvite");

            entity.HasIndex(e => new { e.UserId, e.Status, e.InvitedAt }, "IX_TblChatGroupInvite_User_Status").IsDescending(false, false, true);

            entity.HasIndex(e => new { e.ChatGroupId, e.UserId }, "UQ_TblChatGroupInvite_Group_User")
                .IsUnique()
                .HasFilter("([Status]='PENDING')");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupInvite_CreatedAt");
            entity.Property(e => e.InvitedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupInvite_InvitedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ChatGroup).WithMany(p => p.TblChatGroupInvites)
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupInvite_TblChatGroup");

            entity.HasOne(d => d.DeletedByNavigation).WithMany(p => p.TblChatGroupInviteDeletedByNavigations).HasForeignKey(d => d.DeletedBy);

            entity.HasOne(d => d.InvitedByUser).WithMany(p => p.TblChatGroupInviteInvitedByUsers)
                .HasForeignKey(d => d.InvitedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupInvite_TblUser_InvitedBy");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupInviteUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupInvite_TblUser_User");
        });

        modelBuilder.Entity<TblChatGroupInviteLink>(entity =>
        {
            entity.HasKey(e => e.ChatGroupInviteLinkId);

            entity.ToTable("TblChatGroupInviteLink");

            entity.HasIndex(e => e.ChatGroupId, "UQ_TblChatGroupInviteLink_Primary")
                .IsUnique()
                .HasFilter("([IsPrimary]=(1) AND [IsDeleted]=(0) AND [IsRevoked]=(0))");

            entity.HasIndex(e => e.Token, "UQ_TblChatGroupInviteLink_Token").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupInviteLink_CreatedAt");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Token)
                .HasMaxLength(32)
                .IsUnicode(false);

            entity.HasOne(d => d.ChatGroup).WithOne(p => p.TblChatGroupInviteLink)
                .HasForeignKey<TblChatGroupInviteLink>(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupInviteLink_TblChatGroup");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.TblChatGroupInviteLinks)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupInviteLink_TblUser_CreatedBy");
        });

        modelBuilder.Entity<TblChatGroupMember>(entity =>
        {
            entity.HasKey(e => e.ChatGroupMemberId).HasName("PK__TblChatG__E7AD15399B895E33");

            entity.ToTable("TblChatGroupMember");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupMember_CreatedAt");
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupMember_JoinedAt");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("MEMBER", "DF_TblChatGroupMember_Role");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ChatGroup).WithMany(p => p.TblChatGroupMembers)
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMember_TblChatGroup");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMember_TblUser");
        });

        modelBuilder.Entity<TblChatGroupMessage>(entity =>
        {
            entity.HasKey(e => e.ChatGroupMessageId).HasName("PK__TblChatG__4403F2C8F35967F2");

            entity.ToTable("TblChatGroupMessage");

            entity.HasIndex(e => new { e.ChatGroupId, e.PinnedAt }, "IX_TblChatGroupMessage_ChatGroupId_PinnedAt")
                .IsDescending(false, true)
                .HasFilter("([IsPinned]=(1))");

            entity.HasIndex(e => new { e.ChatGroupId, e.CreatedAt }, "IX_TblChatGroupMessage_Group_CreatedAt");

            entity.HasIndex(e => e.ReplyToChatGroupMessageId, "IX_TblChatGroupMessage_ReplyToChatGroupMessageId");

            entity.HasIndex(e => e.SenderId, "IX_TblChatGroupMessage_SenderId");

            entity.Property(e => e.Content).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupMessage_CreatedAt");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.MessageType)
                .HasMaxLength(20)
                .HasDefaultValue("TEXT", "DF_TblChatGroupMessage_MessageType");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ChatGroup).WithMany(p => p.TblChatGroupMessages)
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMessage_TblChatGroup");

            entity.HasOne(d => d.PinnedByUser).WithMany(p => p.TblChatGroupMessagePinnedByUsers)
                .HasForeignKey(d => d.PinnedByUserId)
                .HasConstraintName("FK_TblChatGroupMessage_PinnedBy");

            entity.HasOne(d => d.ReplyToChatGroupMessage).WithMany(p => p.InverseReplyToChatGroupMessage)
                .HasForeignKey(d => d.ReplyToChatGroupMessageId)
                .HasConstraintName("FK_TblChatGroupMessage_ReplyTo");

            entity.HasOne(d => d.Sender).WithMany(p => p.TblChatGroupMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMessage_TblUser_Sender");
        });

        modelBuilder.Entity<TblChatGroupMessageReaction>(entity =>
        {
            entity.HasKey(e => e.ChatGroupMessageReactionId);

            entity.ToTable("TblChatGroupMessageReaction");

            entity.HasIndex(e => new { e.ChatGroupMessageId, e.UserId }, "UQ_TblChatGroupMessageReaction_Message_User").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupMessageReaction_CreatedAt");
            entity.Property(e => e.Emoji).HasMaxLength(16);
            entity.Property(e => e.RowVersion)
                .HasMaxLength(8)
                .HasDefaultValueSql("(CONVERT([varbinary](8),newid()))", "DF_TblChatGroupMessageReaction_RowVersion");

            entity.HasOne(d => d.ChatGroupMessage).WithMany(p => p.TblChatGroupMessageReactions)
                .HasForeignKey(d => d.ChatGroupMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMessageReaction_TblChatGroupMessage");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupMessageReactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMessageReaction_TblUser");
        });

        modelBuilder.Entity<TblChatGroupMessageUserState>(entity =>
        {
            entity.HasKey(e => e.ChatGroupMessageUserStateId);

            entity.ToTable("TblChatGroupMessageUserState");

            entity.HasIndex(e => new { e.ChatGroupMessageId, e.UserId }, "UQ_TblChatGroupMessageUserState_Message_User").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatGroupMessageUserState_CreatedAt");
            entity.Property(e => e.RowVersion)
                .HasMaxLength(8)
                .HasDefaultValueSql("(CONVERT([varbinary](8),newid()))", "DF_TblChatGroupMessageUserState_RowVersion");

            entity.HasOne(d => d.ChatGroupMessage).WithMany(p => p.TblChatGroupMessageUserStates)
                .HasForeignKey(d => d.ChatGroupMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMessageUserState_TblChatGroupMessage");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupMessageUserStates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupMessageUserState_TblUser");
        });

        modelBuilder.Entity<TblChatGroupPaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PaymentTransactionId).HasName("PK__TblChatG__C22AEFE031278210");

            entity.ToTable("TblChatGroupPaymentTransaction");

            entity.Property(e => e.CommissionPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("COMPLETED");

            entity.HasOne(d => d.ChatGroup).WithMany(p => p.TblChatGroupPaymentTransactions)
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupPaymentTransaction_TblChatGroup");

            entity.HasOne(d => d.CreatorUser).WithMany(p => p.TblChatGroupPaymentTransactionCreatorUsers)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupPaymentTransaction_TblUser_Creator");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatGroupPaymentTransactionUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupPaymentTransaction_TblUser");
        });

        modelBuilder.Entity<TblChatMessage>(entity =>
        {
            entity.HasKey(e => e.ChatMessageId).HasName("PK__TblChatM__9AB6103533EA5020");

            entity.ToTable("TblChatMessage");

            entity.HasIndex(e => e.ReplyToMessageId, "IX_TblChatMessage_ReplyToMessageId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.MessageType)
                .HasMaxLength(20)
                .HasDefaultValue("TEXT", "DF_TblChatMessage_MessageType");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Conversation).WithMany(p => p.TblChatMessages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessage_Conversation");

            entity.HasOne(d => d.ReplyToMessage).WithMany(p => p.InverseReplyToMessage)
                .HasForeignKey(d => d.ReplyToMessageId)
                .HasConstraintName("FK_TblChatMessage_ReplyTo");

            entity.HasOne(d => d.Sender).WithMany(p => p.TblChatMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessage_Sender");
        });

        modelBuilder.Entity<TblChatMessageReaction>(entity =>
        {
            entity.HasKey(e => e.ChatMessageReactionId);

            entity.ToTable("TblChatMessageReaction");

            entity.HasIndex(e => new { e.ChatMessageId, e.UserId }, "UQ_TblChatMessageReaction_Message_User").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatMessageReaction_CreatedAt");
            entity.Property(e => e.Emoji).HasMaxLength(16);
            entity.Property(e => e.RowVersion)
                .HasMaxLength(8)
                .HasDefaultValueSql("(CONVERT([varbinary](8),newid()))", "DF_TblChatMessageReaction_RowVersion");

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.TblChatMessageReactions)
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessageReaction_TblChatMessage");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatMessageReactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessageReaction_TblUser");
        });

        modelBuilder.Entity<TblChatMessageUserState>(entity =>
        {
            entity.HasKey(e => e.ChatMessageUserStateId);

            entity.ToTable("TblChatMessageUserState");

            entity.HasIndex(e => new { e.ChatMessageId, e.UserId }, "UQ_TblChatMessageUserState_Message_User").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblChatMessageUserState_CreatedAt");
            entity.Property(e => e.RowVersion)
                .HasMaxLength(8)
                .HasDefaultValueSql("(CONVERT([varbinary](8),newid()))", "DF_TblChatMessageUserState_RowVersion");

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.TblChatMessageUserStates)
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessageUserState_TblChatMessage");

            entity.HasOne(d => d.User).WithMany(p => p.TblChatMessageUserStates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessageUserState_TblUser");
        });

        modelBuilder.Entity<TblComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__TblComme__C3B4DFCAE30EAF3E");

            entity.ToTable("TblComment");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment)
                .HasForeignKey(d => d.ParentCommentId)
                .HasConstraintName("FK_TblComment_Parent");

            entity.HasOne(d => d.Post).WithMany(p => p.TblComments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblComment_Post");

            entity.HasOne(d => d.User).WithMany(p => p.TblComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblComment_User");
        });

        modelBuilder.Entity<TblCommunity>(entity =>
        {
            entity.HasKey(e => e.CommunityId).HasName("PK__TblCommu__CCAA5B693E10C3CB");

            entity.ToTable("TblCommunity");

            entity.HasIndex(e => e.Slug, "UQ__TblCommu__BC7B5FB64FD3781D").IsUnique();

            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.JoinPolicy)
                .HasMaxLength(50)
                .HasDefaultValue("INSTANT");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Visibility)
                .HasMaxLength(50)
                .HasDefaultValue("PUBLIC");

            entity.HasOne(d => d.Owner).WithMany(p => p.TblCommunities)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunity_Owner");

            entity.HasOne(d => d.ParentCommunity).WithMany(p => p.InverseParentCommunity)
                .HasForeignKey(d => d.ParentCommunityId)
                .HasConstraintName("FK_TblCommunity_Parent");
        });

        modelBuilder.Entity<TblCommunityJoinRequest>(entity =>
        {
            entity.HasKey(e => e.CommunityJoinRequestId).HasName("PK__TblCommu__8D64072E6D9C0681");

            entity.ToTable("TblCommunityJoinRequest");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("PENDING");

            entity.HasOne(d => d.Community).WithMany(p => p.TblCommunityJoinRequests)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunityJoinRequest_Community");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.TblCommunityJoinRequestReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_TblCommunityJoinRequest_Reviewer");

            entity.HasOne(d => d.User).WithMany(p => p.TblCommunityJoinRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunityJoinRequest_User");
        });

        modelBuilder.Entity<TblCommunityMember>(entity =>
        {
            entity.HasKey(e => e.CommunityMemberId).HasName("PK__TblCommu__F315FAD6B77D123F");

            entity.ToTable("TblCommunityMember");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Community).WithMany(p => p.TblCommunityMembers)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunityMember_Community");

            entity.HasOne(d => d.User).WithMany(p => p.TblCommunityMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunityMember_User");
        });

        modelBuilder.Entity<TblCommunityRating>(entity =>
        {
            entity.HasKey(e => e.CommunityRatingId).HasName("PK__TblCommu__799F90ED91C28EDE");

            entity.ToTable("TblCommunityRating");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Community).WithMany(p => p.TblCommunityRatings)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunityRating_Community");

            entity.HasOne(d => d.User).WithMany(p => p.TblCommunityRatings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCommunityRating_User");
        });

        modelBuilder.Entity<TblConversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__TblConve__C050D877B5A4E319");

            entity.ToTable("TblConversation");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LastMessagePreview).HasMaxLength(500);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.UserOne).WithMany(p => p.TblConversationUserOnes)
                .HasForeignKey(d => d.UserOneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblConversation_UserOne");

            entity.HasOne(d => d.UserTwo).WithMany(p => p.TblConversationUserTwos)
                .HasForeignKey(d => d.UserTwoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblConversation_UserTwo");
        });

        modelBuilder.Entity<TblCreatorChatSetting>(entity =>
        {
            entity.HasKey(e => e.CreatorChatSettingId);

            entity.ToTable("TblCreatorChatSetting");

            entity.HasIndex(e => e.CreatorUserId, "UQ_TblCreatorChatSetting_CreatorUserId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblCreatorChatSetting_CreatedAt");
            entity.Property(e => e.RowVersion)
                .HasMaxLength(8)
                .HasDefaultValueSql("(CONVERT([varbinary](8),newid()))", "DF_TblCreatorChatSetting_RowVersion");

            entity.HasOne(d => d.CreatorUser).WithOne(p => p.TblCreatorChatSetting)
                .HasForeignKey<TblCreatorChatSetting>(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCreatorChatSetting_TblUser_Creator");
        });

        modelBuilder.Entity<TblCreatorPayoutRequest>(entity =>
        {
            entity.HasKey(e => e.CreatorPayoutRequestId);

            entity.ToTable("TblCreatorPayoutRequest");

            entity.HasIndex(e => e.CreatorUserId, "IX_TblCreatorPayoutRequest_CreatorUserId");

            entity.HasIndex(e => e.Status, "IX_TblCreatorPayoutRequest_Status");

            entity.Property(e => e.AmountMmk)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("AmountMMK");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PaymentAccountName).HasMaxLength(100);
            entity.Property(e => e.PaymentAccountNumber).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING");

            entity.HasOne(d => d.CreatorUser).WithMany(p => p.TblCreatorPayoutRequests)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCreatorPayoutRequest_TblUser");
        });

        modelBuilder.Entity<TblGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.ToTable("TblGroup");

            entity.HasIndex(e => e.SubCommunityId, "IX_TblGroup_SubCommunityId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.Slug, "UQ_TblGroup_Slug")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CommissionPercentageSnapshot).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.GroupType)
                .HasMaxLength(20)
                .HasDefaultValue("FREE");
            entity.Property(e => e.JoinPolicy)
                .HasMaxLength(50)
                .HasDefaultValue("INSTANT");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Visibility)
                .HasMaxLength(50)
                .HasDefaultValue("PUBLIC");

            entity.HasOne(d => d.Creator).WithMany(p => p.TblGroups)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroup_Creator");

            entity.HasOne(d => d.SubCommunity).WithMany(p => p.TblGroups)
                .HasForeignKey(d => d.SubCommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroup_SubCommunity");
        });

        modelBuilder.Entity<TblGroupJoinRequest>(entity =>
        {
            entity.HasKey(e => e.GroupJoinRequestId);

            entity.ToTable("TblGroupJoinRequest");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("PENDING");

            entity.HasOne(d => d.Group).WithMany(p => p.TblGroupJoinRequests)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroupJoinRequest_Group");

            entity.HasOne(d => d.ReviewedByNavigation).WithMany(p => p.TblGroupJoinRequestReviewedByNavigations)
                .HasForeignKey(d => d.ReviewedBy)
                .HasConstraintName("FK_TblGroupJoinRequest_Reviewer");

            entity.HasOne(d => d.User).WithMany(p => p.TblGroupJoinRequestUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroupJoinRequest_User");
        });

        modelBuilder.Entity<TblGroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId);

            entity.ToTable("TblGroupMember");

            entity.HasIndex(e => e.UserId, "IX_TblGroupMember_UserId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.GroupId, e.UserId }, "UQ_TblGroupMember_GroupUser")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasDefaultValue("Member");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Group).WithMany(p => p.TblGroupMembers)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroupMember_Group");

            entity.HasOne(d => d.User).WithMany(p => p.TblGroupMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroupMember_User");
        });

        modelBuilder.Entity<TblGroupRating>(entity =>
        {
            entity.HasKey(e => e.GroupRatingId);

            entity.ToTable("TblGroupRating");

            entity.HasIndex(e => new { e.GroupId, e.CreatedAt }, "IX_TblGroupRating_GroupId_CreatedAt")
                .IsDescending(false, true)
                .HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.GroupId, e.UserId }, "IX_TblGroupRating_Group_User")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TblGroupRating_CreatedAt");
            entity.Property(e => e.ReviewText).HasMaxLength(1000);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Group).WithMany(p => p.TblGroupRatings)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroupRating_Group");

            entity.HasOne(d => d.User).WithMany(p => p.TblGroupRatings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblGroupRating_User");
        });

        modelBuilder.Entity<TblIdentityVerification>(entity =>
        {
            entity.HasKey(e => e.VerificationId);

            entity.ToTable("TblIdentityVerification");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_TblIdVerif_CreatedAtUtc");
            entity.Property(e => e.FullLegalName).HasMaxLength(200);
            entity.Property(e => e.IdCardBackUrl).HasMaxLength(1000);
            entity.Property(e => e.IdCardFrontUrl).HasMaxLength(1000);
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("LinkDropPoints", "DF_TblIdVerif_PaymentMethod");
            entity.Property(e => e.ProfessionalUrl).HasMaxLength(500);
            entity.Property(e => e.ReviewNotes).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("PendingReview", "DF_TblIdVerif_Status");
            entity.Property(e => e.TargetRoleCode).HasMaxLength(50);
            entity.Property(e => e.WorkEmail).HasMaxLength(256);

            entity.HasOne(d => d.Plan).WithMany(p => p.TblIdentityVerifications)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblIdentityVerification_Plan");

            entity.HasOne(d => d.ReviewedByAdmin).WithMany(p => p.TblIdentityVerifications)
                .HasForeignKey(d => d.ReviewedByAdminId)
                .HasConstraintName("FK_TblIdentityVerification_Admin");

            entity.HasOne(d => d.User).WithMany(p => p.TblIdentityVerifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblIdentityVerification_User");
        });

        modelBuilder.Entity<TblLinkDropPackage>(entity =>
        {
            entity.HasKey(e => e.PackageId);

            entity.ToTable("TblLinkDropPackage");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("USD");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.RealMoneyAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblLinkDropPurchase>(entity =>
        {
            entity.HasKey(e => e.PurchaseId);

            entity.ToTable("TblLinkDropPurchase");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "IX_TblLinkDropPurchase_Status_CreatedAt");

            entity.HasIndex(e => e.TransactionReferenceNo, "IX_TblLinkDropPurchase_TransactionReferenceNo");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_TblLinkDropPurchase_UserId_CreatedAt").IsDescending(false, true);

            entity.HasIndex(e => e.PurchaseNumber, "UQ_TblLinkDropPurchase_PurchaseNumber").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PurchaseNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SnapshotConversionRate).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.SnapshotCurrency)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SnapshotPackageName).HasMaxLength(100);
            entity.Property(e => e.SnapshotRealMoneyAmount).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING");
            entity.Property(e => e.TransactionReferenceNo).HasMaxLength(100);
            entity.Property(e => e.UserNotes).HasMaxLength(500);

            entity.HasOne(d => d.Package).WithMany(p => p.TblLinkDropPurchases)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK_TblLinkDropPurchase_TblLinkDropPackage");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.TblLinkDropPurchases)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblLinkDropPurchase_TblPaymentMethod");

            entity.HasOne(d => d.ReviewedByAdmin).WithMany(p => p.TblLinkDropPurchases)
                .HasForeignKey(d => d.ReviewedByAdminId)
                .HasConstraintName("FK_TblLinkDropPurchase_TblAdmin");

            entity.HasOne(d => d.User).WithMany(p => p.TblLinkDropPurchases)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblLinkDropPurchase_TblUser");
        });

        modelBuilder.Entity<TblLinkDropPurchaseProof>(entity =>
        {
            entity.HasKey(e => e.ProofId);

            entity.ToTable("TblLinkDropPurchaseProof");

            entity.HasIndex(e => e.PurchaseId, "UQ_TblLinkDropPurchaseProof_PurchaseId").IsUnique();

            entity.Property(e => e.ContentType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.OriginalFileName).HasMaxLength(255);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Purchase).WithOne(p => p.TblLinkDropPurchaseProof)
                .HasForeignKey<TblLinkDropPurchaseProof>(d => d.PurchaseId)
                .HasConstraintName("FK_TblLinkDropPurchaseProof_TblLinkDropPurchase");
        });

        modelBuilder.Entity<TblLinkDropTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);

            entity.ToTable("TblLinkDropTransaction");

            entity.HasIndex(e => new { e.WalletId, e.CreatedAt }, "IX_TblLinkDropTransaction_WalletId_CreatedAt").IsDescending(false, true);

            entity.HasIndex(e => new { e.ReferenceType, e.ReferenceId }, "UX_TblLinkDropTransaction_Purchase_Reference")
                .IsUnique()
                .HasFilter("([ReferenceType]='TblLinkDropPurchase')");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransactionType)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.TblLinkDropTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblLinkDropTransaction_TblUser");

            entity.HasOne(d => d.Wallet).WithMany(p => p.TblLinkDropTransactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblLinkDropTransaction_TblLinkDropWallet");
        });

        modelBuilder.Entity<TblLinkDropWallet>(entity =>
        {
            entity.HasKey(e => e.WalletId);

            entity.ToTable("TblLinkDropWallet");

            entity.HasIndex(e => e.UserId, "UQ_TblLinkDropWallet_UserId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.User).WithOne(p => p.TblLinkDropWallet)
                .HasForeignKey<TblLinkDropWallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblLinkDropWallet_TblUser");
        });

        modelBuilder.Entity<TblNotification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__TblNotif__20CF2E127C2455D3");

            entity.ToTable("TblNotification");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.TargetEntityName).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.ActorUser).WithMany(p => p.TblNotificationActorUsers)
                .HasForeignKey(d => d.ActorUserId)
                .HasConstraintName("FK_TblNotification_Actor");

            entity.HasOne(d => d.RecipientUser).WithMany(p => p.TblNotificationRecipientUsers)
                .HasForeignKey(d => d.RecipientUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblNotification_Recipient");
        });

        modelBuilder.Entity<TblPasswordResetOtp>(entity =>
        {
            entity.HasKey(e => e.OtpId);

            entity.ToTable("TblPasswordResetOtp");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(getutcdate())", "DF_TblPasswordResetOtp_CreatedAtUtc");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.OtpCode).HasMaxLength(200);
        });

        modelBuilder.Entity<TblPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId);

            entity.ToTable("TblPaymentMethod");

            entity.Property(e => e.AccountName).HasMaxLength(150);
            entity.Property(e => e.AccountNumber).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MethodName).HasMaxLength(100);
            entity.Property(e => e.QrCodeImageUrl).HasMaxLength(500);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__TblPermi__EFA6FB2F45870323");

            entity.ToTable("TblPermission");

            entity.HasIndex(e => e.PermissionCode, "UQ__TblPermi__91FE5750CD12F393").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.Property(e => e.PermissionCode).HasMaxLength(100);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblPlatformSetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PK__TblPlatf__01E719ACE2914897");

            entity.ToTable("TblPlatformSetting");

            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.DataType)
                .HasMaxLength(50)
                .HasDefaultValue("STRING");
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<TblPoll>(entity =>
        {
            entity.HasKey(e => e.PollId).HasName("PK__TblPoll__E1949E6AA43170C3");

            entity.ToTable("TblPoll");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.TblPolls)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPoll_Post");
        });

        modelBuilder.Entity<TblPollOption>(entity =>
        {
            entity.HasKey(e => e.PollOptionId).HasName("PK__TblPollO__03F0E924C8101156");

            entity.ToTable("TblPollOption");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Poll).WithMany(p => p.TblPollOptions)
                .HasForeignKey(d => d.PollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPollOption_Poll");
        });

        modelBuilder.Entity<TblPollVote>(entity =>
        {
            entity.HasKey(e => e.PollVoteId).HasName("PK__TblPollV__7D67EFB4615FEC65");

            entity.ToTable("TblPollVote");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Poll).WithMany(p => p.TblPollVotes)
                .HasForeignKey(d => d.PollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPollVote_Poll");

            entity.HasOne(d => d.PollOption).WithMany(p => p.TblPollVotes)
                .HasForeignKey(d => d.PollOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPollVote_Option");

            entity.HasOne(d => d.User).WithMany(p => p.TblPollVotes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPollVote_User");
        });

        modelBuilder.Entity<TblPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__TblPost__AA126018BF17BAD0");

            entity.ToTable("TblPost");

            entity.HasIndex(e => e.GroupId, "IX_TblPost_GroupId").HasFilter("([GroupId] IS NOT NULL AND [IsDeleted]=(0))");

            entity.Property(e => e.CodeFileName).HasMaxLength(100);
            entity.Property(e => e.CodeLanguage).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DiagramCaption).HasMaxLength(250);
            entity.Property(e => e.DiagramImageUrl).HasMaxLength(500);
            entity.Property(e => e.PostType)
                .HasMaxLength(50)
                .HasDefaultValue("STANDARD");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Subtitle).HasMaxLength(300);

            entity.HasOne(d => d.Author).WithMany(p => p.TblPosts)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPost_Author");

            entity.HasOne(d => d.Community).WithMany(p => p.TblPosts)
                .HasForeignKey(d => d.CommunityId)
                .HasConstraintName("FK_TblPost_Community");

            entity.HasOne(d => d.Group).WithMany(p => p.TblPosts)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_TblPost_Group");
        });

        modelBuilder.Entity<TblPostImage>(entity =>
        {
            entity.HasKey(e => e.PostImageId).HasName("PK__TblPostI__BCD3CCD0AD3B59E8");

            entity.ToTable("TblPostImage");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.TblPostImages)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPostImage_Post");
        });

        modelBuilder.Entity<TblPostLike>(entity =>
        {
            entity.HasKey(e => e.PostLikeId).HasName("PK__TblPostL__4CF65C19B5A33F1B");

            entity.ToTable("TblPostLike");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.TblPostLikes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPostLike_Post");

            entity.HasOne(d => d.User).WithMany(p => p.TblPostLikes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPostLike_User");
        });

        modelBuilder.Entity<TblPostShare>(entity =>
        {
            entity.HasKey(e => e.PostShareId).HasName("PK__TblPostS__D336F0FF4DE78263");

            entity.ToTable("TblPostShare");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.TblPostShares)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPostShare_Post");

            entity.HasOne(d => d.TargetCommunity).WithMany(p => p.TblPostShares)
                .HasForeignKey(d => d.TargetCommunityId)
                .HasConstraintName("FK_TblPostShare_Community");

            entity.HasOne(d => d.User).WithMany(p => p.TblPostShares)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPostShare_User");
        });

        modelBuilder.Entity<TblPrivateChatPaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PrivateChatPaymentTransactionId);

            entity.ToTable("TblPrivateChatPaymentTransaction");

            entity.HasIndex(e => e.BuyerUserId, "IX_TblPrivateChatPaymentTransaction_BuyerUserId");

            entity.HasIndex(e => e.ConversationId, "IX_TblPrivateChatPaymentTransaction_ConversationId");

            entity.HasIndex(e => e.CreatorUserId, "IX_TblPrivateChatPaymentTransaction_CreatorUserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblPrivateChatPaymentTransaction_CreatedAt");
            entity.Property(e => e.RowVersion)
                .HasMaxLength(8)
                .HasDefaultValueSql("(CONVERT([varbinary](8),newid()))", "DF_TblPrivateChatPaymentTransaction_RowVersion");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("COMPLETED", "DF_TblPrivateChatPaymentTransaction_Status");

            entity.HasOne(d => d.BuyerUser).WithMany(p => p.TblPrivateChatPaymentTransactionBuyerUsers)
                .HasForeignKey(d => d.BuyerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPrivateChatPaymentTransaction_TblUser_Buyer");

            entity.HasOne(d => d.Conversation).WithMany(p => p.TblPrivateChatPaymentTransactions)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPrivateChatPaymentTransaction_TblConversation");

            entity.HasOne(d => d.CreatorUser).WithMany(p => p.TblPrivateChatPaymentTransactionCreatorUsers)
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPrivateChatPaymentTransaction_TblUser_Creator");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__TblRole__8AFACE1A6BD796E4");

            entity.ToTable("TblRole");

            entity.HasIndex(e => e.RoleCode, "UQ__TblRole__D62CB59CE0351375").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RoleCode).HasMaxLength(100);
            entity.Property(e => e.RoleName).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<TblRolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__TblRoleP__120F46BADCF8CEC8");

            entity.ToTable("TblRolePermission");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Permission).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRolePermission_Permission");

            entity.HasOne(d => d.Role).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblRolePermission_Role");
        });

        modelBuilder.Entity<TblSavedAccount>(entity =>
        {
            entity.HasKey(e => e.SavedAccountId).HasName("PK__TblSaved__A2463A2BA482362F");

            entity.ToTable("TblSavedAccount");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.SavedUser).WithMany(p => p.TblSavedAccountSavedUsers)
                .HasForeignKey(d => d.SavedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblSavedAccount_SavedUser");

            entity.HasOne(d => d.User).WithMany(p => p.TblSavedAccountUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblSavedAccount_User");
        });

        modelBuilder.Entity<TblSavedPost>(entity =>
        {
            entity.HasKey(e => e.SavedPostId).HasName("PK__TblSaved__7B99A00BA9185125");

            entity.ToTable("TblSavedPost");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Post).WithMany(p => p.TblSavedPosts)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblSavedPost_Post");

            entity.HasOne(d => d.User).WithMany(p => p.TblSavedPosts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblSavedPost_User");
        });

        modelBuilder.Entity<TblSkillEndorsement>(entity =>
        {
            entity.HasKey(e => e.EndorsementId).HasName("PK__TblSkill__DBB8336F73611CFF");

            entity.ToTable("TblSkillEndorsement");

            entity.HasIndex(e => new { e.SkillId, e.EndorserUserId }, "UX_TblSkillEndorsement")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.EndorserUser).WithMany(p => p.TblSkillEndorsements)
                .HasForeignKey(d => d.EndorserUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblSkillEndorsement_TblUser");

            entity.HasOne(d => d.Skill).WithMany(p => p.TblSkillEndorsements)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK_TblSkillEndorsement_TblUserSkill");
        });

        modelBuilder.Entity<TblSubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId);

            entity.ToTable("TblSubscriptionPlan");

            entity.Property(e => e.BillingInterval)
                .HasMaxLength(20)
                .HasDefaultValue("Monthly", "DF_TblSubPlan_BillingInterval");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TblSubPlan_CreatedAt");
            entity.Property(e => e.DurationDays).HasDefaultValue(30, "DF_TblSubPlan_DurationDays");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_TblSubPlan_IsActive");
            entity.Property(e => e.PlanName).HasMaxLength(150);
            entity.Property(e => e.PriceAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TargetRoleCode).HasMaxLength(50);
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__TblUser__1788CC4C528B3DC4");

            entity.ToTable("TblUser");

            entity.HasIndex(e => e.Email, "UQ__TblUser__A9D10534CBAEDD36").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__TblUser__C9F2845608BEC37A").IsUnique();

            entity.Property(e => e.AvailabilityStatus).HasMaxLength(200);
            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Headline).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(100);
            entity.Property(e => e.PercentileBadgeText)
                .HasMaxLength(100)
                .HasDefaultValue("Top 1% Percentile");
            entity.Property(e => e.Pronouns).HasMaxLength(50);
            entity.Property(e => e.ResponseSlaText)
                .HasMaxLength(100)
                .HasDefaultValue("< 2 hrs Response");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblUserActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId);

            entity.ToTable("TblUserActivity");

            entity.HasIndex(e => e.UserId, "IX_TblUserActivity_UserId");

            entity.HasIndex(e => new { e.UserId, e.IsDeleted, e.CreatedAt }, "IX_TblUserActivity_User_Status").IsDescending(false, false, true);

            entity.Property(e => e.ActivityType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TblUserActivity_CreatedAt");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TargetEntityType).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.TblUserActivities)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserActivity_TblUser");
        });

        modelBuilder.Entity<TblUserBlock>(entity =>
        {
            entity.HasKey(e => e.UserBlockId);

            entity.ToTable("TblUserBlock");

            entity.HasIndex(e => new { e.BlockerUserId, e.BlockedUserId }, "UQ_TblUserBlock_Blocker_Blocked").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())", "DF_TblUserBlock_CreatedAt");

            entity.HasOne(d => d.BlockedUser).WithMany(p => p.TblUserBlockBlockedUsers)
                .HasForeignKey(d => d.BlockedUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserBlock_BlockedUser");

            entity.HasOne(d => d.BlockerUser).WithMany(p => p.TblUserBlockBlockerUsers)
                .HasForeignKey(d => d.BlockerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserBlock_BlockerUser");
        });

        modelBuilder.Entity<TblUserFollow>(entity =>
        {
            entity.HasKey(e => e.FollowId);

            entity.ToTable("TblUserFollow");

            entity.HasIndex(e => new { e.FolloweeId, e.CreatedAt }, "IX_TblUserFollow_FolloweeId")
                .IsDescending(false, true)
                .HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => new { e.FollowerId, e.FolloweeId }, "IX_TblUserFollow_Follower_Followee")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_TblUserFollow_CreatedAt");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Followee).WithMany(p => p.TblUserFollowFollowees)
                .HasForeignKey(d => d.FolloweeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserFollow_Followee");

            entity.HasOne(d => d.Follower).WithMany(p => p.TblUserFollowFollowers)
                .HasForeignKey(d => d.FollowerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserFollow_Follower");
        });

        modelBuilder.Entity<TblUserRating>(entity =>
        {
            entity.HasKey(e => e.UserRatingId).HasName("PK__TblUserR__9E5FEA8A0B1071FD");

            entity.ToTable("TblUserRating");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.RaterUser).WithMany(p => p.TblUserRatingRaterUsers)
                .HasForeignKey(d => d.RaterUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserRating_Rater");

            entity.HasOne(d => d.TargetUser).WithMany(p => p.TblUserRatingTargetUsers)
                .HasForeignKey(d => d.TargetUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserRating_Target");
        });

        modelBuilder.Entity<TblUserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__TblUserR__3D978A3534EA201A");

            entity.ToTable("TblUserRole");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Role).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserRole_Role");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserRole_User");
        });

        modelBuilder.Entity<TblUserSkill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__TblUserS__DFA09187469766C6");

            entity.ToTable("TblUserSkill");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.SkillName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.TblUserSkills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TblUserSkill_TblUser");
        });

        modelBuilder.Entity<TblUserSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId);

            entity.ToTable("TblUserSubscription");

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_TblUserSub_CreatedAtUtc");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("LinkDropPoints", "DF_TblUserSub_PaymentMethod");
            entity.Property(e => e.StartDateUtc).HasDefaultValueSql("(sysutcdatetime())", "DF_TblUserSub_StartDateUtc");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Active", "DF_TblUserSub_Status");

            entity.HasOne(d => d.Plan).WithMany(p => p.TblUserSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserSubscription_Plan");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUserSubscriptions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserSubscription_Role");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserSubscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserSubscription_User");
        });

        modelBuilder.Entity<TblUserVerificationAudit>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__TblUserV__A17F2398A9B03547");

            entity.ToTable("TblUserVerificationAudit");

            entity.Property(e => e.AuditCode).HasMaxLength(100);
            entity.Property(e => e.AuditTitle).HasMaxLength(200);
            entity.Property(e => e.AuditedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Authority).HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("ACTIVE");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserVerificationAudits)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TblUserVerificationAudit_TblUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
