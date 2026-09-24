using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;

namespace CommunityLink.Domain.Features.LinkDrop;

public static class LinkDropSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (db.Database.IsRelational())
        {
            await db.Database.ExecuteSqlRawAsync(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TblLinkDropTransaction' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'PurchasedBalanceBefore')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD PurchasedBalanceBefore BIGINT NOT NULL CONSTRAINT DF_TblLinkDropTransaction_PurchasedBalanceBefore DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'PurchasedBalanceAfter')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD PurchasedBalanceAfter BIGINT NOT NULL CONSTRAINT DF_TblLinkDropTransaction_PurchasedBalanceAfter DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'EarnedBalanceBefore')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD EarnedBalanceBefore BIGINT NOT NULL CONSTRAINT DF_TblLinkDropTransaction_EarnedBalanceBefore DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'EarnedBalanceAfter')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD EarnedBalanceAfter BIGINT NOT NULL CONSTRAINT DF_TblLinkDropTransaction_EarnedBalanceAfter DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'PurchasedAmountDeducted')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD PurchasedAmountDeducted BIGINT NOT NULL CONSTRAINT DF_TblLinkDropTransaction_PurchasedAmountDeducted DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'EarnedAmountDeducted')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD EarnedAmountDeducted BIGINT NOT NULL CONSTRAINT DF_TblLinkDropTransaction_EarnedAmountDeducted DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'RelatedUserId')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD RelatedUserId INT NULL;

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropTransaction') AND name = 'RelatedGroupId')
                        ALTER TABLE dbo.TblLinkDropTransaction ADD RelatedGroupId INT NULL;
                END;

                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'TblLinkDropWallet' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropWallet') AND name = 'PurchasedBalance')
                        ALTER TABLE dbo.TblLinkDropWallet ADD PurchasedBalance BIGINT NOT NULL CONSTRAINT DF_TblLinkDropWallet_PurchasedBalance DEFAULT (0);

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblLinkDropWallet') AND name = 'EarnedBalance')
                        ALTER TABLE dbo.TblLinkDropWallet ADD EarnedBalance BIGINT NOT NULL CONSTRAINT DF_TblLinkDropWallet_EarnedBalance DEFAULT (0);
                END;
            ");
        }

        // ---------------------------------------------------------------------
        // Default Payment Channels (KPAY, WavePay, KBZPay, AYA Pay)
        // ---------------------------------------------------------------------
        var methodSeeds = new (string Name, string Instructions, int DisplayOrder)[]
        {
            ("KPAY", "Open your K-Pay app -> Transfer -> Enter the account number below -> Send the exact amount. Then upload the transaction screenshot along with the Transaction ID.", 1),
            ("WavePay", "Open your Wave Money app -> Transfer -> Enter the account number below -> Send the exact amount. Then upload the transaction screenshot along with the Transaction ID.", 2),
            ("KBZPay", "Open your KBZ Pay app -> Transfer -> Enter the account number below -> Send the exact amount. Then upload the transaction screenshot along with the Transaction ID.", 3),
            ("AYA Pay", "Open your AYA Pay app -> Transfer -> Enter the account number below -> Send the exact amount. Then upload the transaction screenshot along with the Transaction ID.", 4)
        };

        foreach (var (name, instructions, displayOrder) in methodSeeds)
        {
            var exists = await db.TblPaymentMethods
                .AnyAsync(m => m.MethodName == name && !m.IsDeleted);
            if (exists) continue;

            db.TblPaymentMethods.Add(new TblPaymentMethod
            {
                MethodName = name,
                AccountName = "Update Account Name",
                AccountNumber = "09-XXXXXXXXX",
                Instructions = instructions,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }
        await db.SaveChangesAsync();

        // ---------------------------------------------------------------------
        // Default Packages (100 Drops / 200 Drops at 10 MMK per Drop)
        // ---------------------------------------------------------------------
        var packageSeeds = new (string Name, string Description, long Drops, long Bonus, decimal Price, int DisplayOrder)[]
        {
            ("100 Link Drops", "100 Link Drops added to your balance for group links, posts and sharing.", 100, 0, 1000.0m, 1),
            ("200 Link Drops", "200 Link Drops added to your balance for group links, posts and sharing.", 200, 0, 2000.0m, 2)
        };

        foreach (var (name, description, drops, bonus, price, displayOrder) in packageSeeds)
        {
            var exists = await db.TblLinkDropPackages
                .AnyAsync(p => p.PackageName == name && !p.IsDeleted);
            if (exists) continue;

            db.TblLinkDropPackages.Add(new TblLinkDropPackage
            {
                PackageName = name,
                Description = description,
                LinkDropAmount = drops,
                BonusAmount = bonus,
                RealMoneyAmount = price,
                Currency = LinkDropPricing.DefaultCurrency,
                DisplayOrder = displayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }
        await db.SaveChangesAsync();
    }
}