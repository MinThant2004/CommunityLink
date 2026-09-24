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

        modelBuilder.Entity<TblLinkDropTransaction>(entity =>
        {
            entity.Ignore(e => e.PurchasedBalanceBefore);
            entity.Ignore(e => e.PurchasedBalanceAfter);
            entity.Ignore(e => e.EarnedBalanceBefore);
            entity.Ignore(e => e.EarnedBalanceAfter);
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