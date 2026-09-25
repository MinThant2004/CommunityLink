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
                .IsRowVersion()
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