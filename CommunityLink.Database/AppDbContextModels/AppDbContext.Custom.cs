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

        modelBuilder.Entity<TblGroupRating>(entity =>
        {
            entity.HasKey(e => e.GroupRatingId);
            entity.ToTable("TblGroupRating");

            entity.Property(e => e.ReviewText).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Group)
                .WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
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
        // SQL Server handles rowversion/timestamp columns automatically on the database server.
        // Inserting an explicit value into a SQL Server timestamp column throws SqlException.
        // Only generate in-memory dummy rowversions for testing providers like InMemoryDatabase.
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
        }
    }
}