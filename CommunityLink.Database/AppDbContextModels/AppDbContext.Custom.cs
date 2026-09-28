using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Database.AppDbContextModels;

public partial class AppDbContext
{
    public virtual DbSet<TblCommunityAuditLog> TblCommunityAuditLogs { get; set; }
    public virtual DbSet<TblCreatorPayoutRequest> TblCreatorPayoutRequests { get; set; }
    public virtual DbSet<TblCreatorChatSetting> TblCreatorChatSettings { get; set; }
    public virtual DbSet<TblPrivateChatPaymentTransaction> TblPrivateChatPaymentTransactions { get; set; }
    public virtual DbSet<TblChatMessageUserState> TblChatMessageUserStates { get; set; }
    public virtual DbSet<TblChatGroupMessageUserState> TblChatGroupMessageUserStates { get; set; }
    public virtual DbSet<TblChatMessageReaction> TblChatMessageReactions { get; set; }
    public virtual DbSet<TblChatGroupMessageReaction> TblChatGroupMessageReactions { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblCreatorChatSetting>(entity =>
        {
            entity.HasKey(e => e.CreatorChatSettingId);
            entity.ToTable("TblCreatorChatSetting");

            entity.HasIndex(e => e.CreatorUserId, "IX_TblCreatorChatSetting_CreatorUserId").IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.RowVersion)
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

        modelBuilder.Entity<TblChatGroup>(entity =>
        {
            entity.Property(e => e.AvatarUrl).HasColumnType("nvarchar(max)");
            entity.Property(e => e.BannerUrl).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<TblChatGroupMember>(entity =>
        {
            entity.HasIndex(e => new { e.ChatGroupId, e.UserId })
                .IsUnique()
                .HasDatabaseName("UQ_TblChatGroupMember_Group_User");
        });

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

            // One reaction per person per message: choosing a different emoji updates the row.
            entity.HasIndex(e => new { e.ChatMessageId, e.UserId }).IsUnique();

            entity.HasOne(d => d.ChatMessage).WithMany()
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TblChatMessageReaction_TblChatMessage");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_TblChatMessageReaction_TblUser");
        });

        modelBuilder.Entity<TblChatGroupMessageReaction>(entity =>
        {
            entity.HasKey(e => e.ChatGroupMessageReactionId);
            entity.ToTable("TblChatGroupMessageReaction");

            entity.Property(e => e.Emoji).HasMaxLength(16).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

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
    }
}