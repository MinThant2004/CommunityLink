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

            entity.Property(e => e.IsMuted).HasDefaultValue(false);
        });



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