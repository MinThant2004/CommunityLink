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

    public virtual DbSet<TblAdminRole> TblAdminRoles { get; set; }

    public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }

    public virtual DbSet<TblChatMessage> TblChatMessages { get; set; }

    public virtual DbSet<TblComment> TblComments { get; set; }

    public virtual DbSet<TblCommunity> TblCommunities { get; set; }

    public virtual DbSet<TblCommunityJoinRequest> TblCommunityJoinRequests { get; set; }

    public virtual DbSet<TblCommunityMember> TblCommunityMembers { get; set; }

    public virtual DbSet<TblCommunityRating> TblCommunityRatings { get; set; }

    public virtual DbSet<TblConversation> TblConversations { get; set; }

    public virtual DbSet<TblGroup> TblGroups { get; set; }

    public virtual DbSet<TblGroupJoinRequest> TblGroupJoinRequests { get; set; }

    public virtual DbSet<TblGroupMember> TblGroupMembers { get; set; }

    public virtual DbSet<TblLinkDropPackage> TblLinkDropPackages { get; set; }

    public virtual DbSet<TblLinkDropPurchase> TblLinkDropPurchases { get; set; }

    public virtual DbSet<TblLinkDropPurchaseProof> TblLinkDropPurchaseProofs { get; set; }

    public virtual DbSet<TblLinkDropTransaction> TblLinkDropTransactions { get; set; }

    public virtual DbSet<TblLinkDropWallet> TblLinkDropWallets { get; set; }

    public virtual DbSet<TblNotification> TblNotifications { get; set; }

    public virtual DbSet<TblPasswordResetOtp> TblPasswordResetOtps { get; set; }

    public virtual DbSet<TblPaymentMethod> TblPaymentMethods { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblPoll> TblPolls { get; set; }

    public virtual DbSet<TblPollOption> TblPollOptions { get; set; }

    public virtual DbSet<TblPollVote> TblPollVotes { get; set; }

    public virtual DbSet<TblPost> TblPosts { get; set; }

    public virtual DbSet<TblPostImage> TblPostImages { get; set; }

    public virtual DbSet<TblPostLike> TblPostLikes { get; set; }

    public virtual DbSet<TblPostShare> TblPostShares { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblRolePermission> TblRolePermissions { get; set; }

    public virtual DbSet<TblSavedAccount> TblSavedAccounts { get; set; }

    public virtual DbSet<TblSavedPost> TblSavedPosts { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserRating> TblUserRatings { get; set; }

    public virtual DbSet<TblUserRole> TblUserRoles { get; set; }

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

        modelBuilder.Entity<TblChatMessage>(entity =>
        {
            entity.HasKey(e => e.ChatMessageId).HasName("PK__TblChatM__9AB6103533EA5020");

            entity.ToTable("TblChatMessage");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Conversation).WithMany(p => p.TblChatMessages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessage_Conversation");

            entity.HasOne(d => d.Sender).WithMany(p => p.TblChatMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatMessage_Sender");
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

        modelBuilder.Entity<TblGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId);

            entity.ToTable("TblGroup");

            entity.HasIndex(e => e.SubCommunityId, "IX_TblGroup_SubCommunityId").HasFilter("([IsDeleted]=(0))");

            entity.HasIndex(e => e.Slug, "UQ_TblGroup_Slug")
                .IsUnique()
                .HasFilter("([IsDeleted]=(0))");

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

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

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

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__TblUser__1788CC4C528B3DC4");

            entity.ToTable("TblUser");

            entity.HasIndex(e => e.Email, "UQ__TblUser__A9D10534CBAEDD36").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__TblUser__C9F2845608BEC37A").IsUnique();

            entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(100);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UserName).HasMaxLength(100);
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
