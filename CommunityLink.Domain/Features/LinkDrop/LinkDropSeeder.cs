using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;

namespace CommunityLink.Domain.Features.LinkDrop;

public static class LinkDropSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

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