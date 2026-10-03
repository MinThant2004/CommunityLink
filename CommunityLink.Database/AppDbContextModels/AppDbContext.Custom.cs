using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Database.AppDbContextModels;

public partial class AppDbContext
{
    public virtual DbSet<TblCommunityAuditLog> TblCommunityAuditLogs { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblUserActivity>(entity =>
        {
            entity.HasKey(e => e.ActivityId);
            entity.ToTable("TblUserActivity");

            entity.Property(e => e.ActivityType).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.TargetEntityType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasIndex(e => e.UserId, "IX_TblUserActivity_UserId");
            entity.HasIndex(e => new { e.UserId, e.IsDeleted, e.CreatedAt }, "IX_TblUserActivity_User_Status");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblUserActivity_TblUser");
        });
        modelBuilder.Entity<TblCreatorChatSetting>(entity =>
        {
            entity.HasKey(e => e.CreatorChatSettingId);
            entity.ToTable("TblCreatorChatSetting");

            entity.HasIndex(e => e.CreatorUserId, "IX_TblCreatorChatSetting_CreatorUserId").IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.CreatorUser).WithMany()
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCreatorChatSetting_TblUser_Creator");
        });

        modelBuilder.Entity<TblPrivateChatPaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PrivateChatPaymentTransactionId);
            entity.ToTable("TblPrivateChatPaymentTransaction");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("COMPLETED");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasIndex(e => e.ConversationId, "IX_TblPrivateChatPaymentTransaction_ConversationId");
            entity.HasIndex(e => e.BuyerUserId, "IX_TblPrivateChatPaymentTransaction_BuyerUserId");
            entity.HasIndex(e => e.CreatorUserId, "IX_TblPrivateChatPaymentTransaction_CreatorUserId");

            entity.HasOne(d => d.Conversation).WithMany()
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPrivateChatPaymentTransaction_TblConversation");

            entity.HasOne(d => d.BuyerUser).WithMany()
                .HasForeignKey(d => d.BuyerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPrivateChatPaymentTransaction_TblUser_Buyer");

            entity.HasOne(d => d.CreatorUser).WithMany()
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblPrivateChatPaymentTransaction_TblUser_Creator");
        });

        modelBuilder.Entity<TblCreatorPayoutRequest>(entity =>
        {
            entity.HasKey(e => e.CreatorPayoutRequestId);
            entity.ToTable("TblCreatorPayoutRequest");

            entity.Property(e => e.AmountMMK).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING");

            entity.HasIndex(e => e.CreatorUserId, "IX_TblCreatorPayoutRequest_CreatorUserId");
            entity.HasIndex(e => e.Status, "IX_TblCreatorPayoutRequest_Status");

            entity.HasOne(d => d.CreatorUser).WithMany()
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCreatorPayoutRequest_TblUser");
        });

        modelBuilder.Entity<TblCommunityAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId);
            entity.ToTable("TblCommunityAuditLog");
        });

        modelBuilder.Entity<TblChatGroupMember>(entity =>
        {
            entity.HasIndex(e => new { e.ChatGroupId, e.UserId })
                .IsUnique()
                .HasDatabaseName("UQ_TblChatGroupMember_Group_User");

            entity.Property(e => e.IsMuted).HasDefaultValue(false);
        modelBuilder.Entity<TblChatGroupPaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PaymentTransactionId);

            entity.ToTable("TblChatGroupPaymentTransaction");

            entity.Property(e => e.CommissionPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("COMPLETED");
            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken();

            entity.HasOne(d => d.ChatGroup).WithMany()
                .HasForeignKey(d => d.ChatGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupPaymentTransaction_TblChatGroup");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupPaymentTransaction_TblUser");

            entity.HasOne(d => d.CreatorUser).WithMany()
                .HasForeignKey(d => d.CreatorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblChatGroupPaymentTransaction_TblUser_Creator");
        });

        modelBuilder.Entity<TblLinkDropTransaction>(entity =>
        {
            entity.ToTable("TblLinkDropTransaction", table =>
            {
                table.HasCheckConstraint(
                    "CK_TblLinkDropTransaction_TransactionType",
                    "[TransactionType] IN ('SPEND_GROUP_JOIN', 'SPEND_CHAT', 'REFUND', 'BONUS', 'PURCHASE', 'CHAT_GROUP_JOIN', 'CHAT_GROUP_EARNING', 'CREATOR_PAYOUT', 'PRIVATE_CHAT_UNLOCK', 'PRIVATE_CHAT_EARNING', 'TOP_UP')");
            });

            entity.Ignore(e => e.PurchasedAmountDeducted);
            entity.Ignore(e => e.EarnedAmountDeducted);
            entity.Ignore(e => e.RelatedUserId);
            entity.Ignore(e => e.RelatedGroupId);
        });

        // ------------------------------------------------------------------
        // Per-message actions: reply links, per-viewer hide state, reactions.
        // The reply columns are bare FKs with no navigation property on purpose; the services
        // resolve quoted text in bulk for a page of messages.
        // ------------------------------------------------------------------

        modelBuilder.Entity<TblChatMessage>(entity =>
        {
            entity.HasOne(d => d.ReplyToMessage).WithMany()
                .HasForeignKey(d => d.ReplyToMessageId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TblChatMessage_ReplyTo");
        });

        modelBuilder.Entity<TblChatGroupMessage>(entity =>
        {
            entity.HasOne(d => d.ReplyToChatGroupMessage).WithMany()
                .HasForeignKey(d => d.ReplyToChatGroupMessageId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TblChatGroupMessage_ReplyTo");
        });

        modelBuilder.Entity<TblChatMessageUserState>(entity =>
        {
            entity.HasKey(e => e.ChatMessageUserStateId);
            entity.ToTable("TblChatMessageUserState");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .HasDefaultValueSql("(CONVERT(VARBINARY(8), NEWID()))")
                .ValueGeneratedOnAdd();

            // Not a concurrency token on purpose. Two tabs toggling the same reaction or the
            // same hide flag would otherwise surface DbUpdateConcurrencyException as a 500, and
            // both outcomes are harmless: the row ends up hidden / reacted either way.
            entity.HasIndex(e => new { e.ChatMessageId, e.UserId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsHidden });

            entity.HasOne(d => d.ChatMessage).WithMany()
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TblChatMessageUserState_TblChatMessage");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TblChatMessageUserState_TblUser");
        });

        modelBuilder.Entity<TblChatGroupMessageUserState>(entity =>
        {
            entity.HasKey(e => e.ChatGroupMessageUserStateId);
            entity.ToTable("TblChatGroupMessageUserState");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .HasDefaultValueSql("(CONVERT(VARBINARY(8), NEWID()))")
                .ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.ChatGroupMessageId, e.UserId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsHidden });

            entity.HasOne(d => d.ChatGroupMessage).WithMany()
                .HasForeignKey(d => d.ChatGroupMessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TblChatGroupMessageUserState_TblChatGroupMessage");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TblChatGroupMessageUserState_TblUser");
        });

        modelBuilder.Entity<TblChatMessageReaction>(entity =>
        {
            entity.HasKey(e => e.ChatMessageReactionId);
            entity.ToTable("TblChatMessageReaction");

            entity.Property(e => e.Emoji).HasMaxLength(16).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .HasDefaultValueSql("(CONVERT(VARBINARY(8), NEWID()))")
                .ValueGeneratedOnAdd();

            // One reaction per person per message: choosing a different emoji updates the row.
            entity.HasIndex(e => new { e.ChatMessageId, e.UserId }).IsUnique();

            entity.HasOne(d => d.ChatMessage).WithMany()
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TblChatMessageReaction_TblChatMessage");


        // Per-member admin permission matrix. One membership row is already exactly one
        // (group, user) pair, so the matrix lives on it rather than in a side table. Every flag
        // defaults to false, so an admin row that predates this feature holds no powers until the
        // owner grants them.
        modelBuilder.Entity<TblChatGroupMember>(entity =>
        {
            entity.Property(e => e.CanDeleteMessages).HasDefaultValue(false, "DF_TblChatGroupMember_CanDeleteMessages");
            entity.Property(e => e.CanRemoveMembers).HasDefaultValue(false, "DF_TblChatGroupMember_CanRemoveMembers");
            entity.Property(e => e.CanBanMembers).HasDefaultValue(false, "DF_TblChatGroupMember_CanBanMembers");
            entity.Property(e => e.CanManageInviteLinks).HasDefaultValue(false, "DF_TblChatGroupMember_CanManageInviteLinks");
            entity.Property(e => e.CanPinMessages).HasDefaultValue(false, "DF_TblChatGroupMember_CanPinMessages");
            entity.HasKey(e => e.ChatGroupMessageReactionId);
            entity.ToTable("TblChatGroupMessageReaction");

            entity.Property(e => e.Emoji).HasMaxLength(16).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
                .HasDefaultValueSql("(CONVERT(VARBINARY(8), NEWID()))")
                .ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.ChatGroupMessageId, e.UserId }).IsUnique();

            entity.HasOne(d => d.ChatGroupMessage).WithMany()
                .HasForeignKey(d => d.ChatGroupMessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TblChatGroupMessageReaction_TblChatGroupMessage");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TblChatGroupMessageReaction_TblUser");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureRowVersions();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        EnsureRowVersions();
        return base.SaveChanges();
    }

    private void EnsureRowVersions()
    {
        // 1. In-memory database doesn't auto-generate rowversions for any entity.
        if (Database.IsInMemory())
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    var rowVersionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "RowVersion");
                    if (rowVersionProp != null && (rowVersionProp.CurrentValue == null || ((byte[])rowVersionProp.CurrentValue).Length == 0))
                    {
                        rowVersionProp.CurrentValue = Guid.NewGuid().ToByteArray()[..8];
                    }
                }
            }
            return;
        }

        // 2. On SQL Server, native ROWVERSION/TIMESTAMP columns must NOT be assigned values on insert.
        // However, tables created with VARBINARY(8) (like TblChatMessageReaction, TblChatMessageUserState,
        // TblChatGroupMessageReaction, TblChatGroupMessageUserState) require a non-null VARBINARY(8) value.
        // If EF Core hasn't marked them as store-generated or sends null, supply the 8-byte token.
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is TblChatMessageReaction
                    or TblChatGroupMessageReaction
                    or TblChatMessageUserState
                    or TblChatGroupMessageUserState)
                {
                    var rowVersionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "RowVersion");
                    if (rowVersionProp != null && (rowVersionProp.CurrentValue == null || ((byte[])rowVersionProp.CurrentValue).Length == 0))
                    {
                        rowVersionProp.CurrentValue = Guid.NewGuid().ToByteArray()[..8];
                    }
                }
            }
        }
    }
}