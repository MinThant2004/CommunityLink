using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            await db.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupPaymentTransaction')
                BEGIN
                    CREATE TABLE [dbo].[TblChatGroupPaymentTransaction] (
                        [PaymentTransactionId] BIGINT IDENTITY(1,1) NOT NULL,
                        [ChatGroupId] INT NOT NULL,
                        [UserId] INT NOT NULL,
                        [CreatorUserId] INT NOT NULL,
                        [GrossAmount] BIGINT NOT NULL,
                        [CommissionPercentage] DECIMAL(5,2) NOT NULL,
                        [CommissionAmount] BIGINT NOT NULL,
                        [NetAmount] BIGINT NOT NULL,
                        [PurchasedAmountDeducted] BIGINT NOT NULL,
                        [EarnedAmountDeducted] BIGINT NOT NULL,
                        [Status] VARCHAR(20) NOT NULL DEFAULT 'COMPLETED',
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
                        [RowVersion] ROWVERSION NOT NULL,
                        CONSTRAINT [PK_TblChatGroupPaymentTransaction] PRIMARY KEY CLUSTERED ([PaymentTransactionId] ASC),
                        CONSTRAINT [FK_TblChatGroupPaymentTransaction_TblChatGroup] FOREIGN KEY ([ChatGroupId]) REFERENCES [dbo].[TblChatGroup] ([ChatGroupId]),
                        CONSTRAINT [FK_TblChatGroupPaymentTransaction_TblUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId]),
                        CONSTRAINT [FK_TblChatGroupPaymentTransaction_TblUser_Creator] FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[TblUser] ([UserId])
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblCreatorPayoutRequest')
                BEGIN
                    CREATE TABLE [dbo].[TblCreatorPayoutRequest] (
                        [CreatorPayoutRequestId] BIGINT IDENTITY(1,1) NOT NULL,
                        [CreatorUserId] INT NOT NULL,
                        [AmountLinkDrops] BIGINT NOT NULL,
                        [AmountMMK] DECIMAL(18,2) NOT NULL,
                        [PaymentMethod] NVARCHAR(50) NOT NULL,
                        [PaymentAccountName] NVARCHAR(100) NOT NULL,
                        [PaymentAccountNumber] NVARCHAR(100) NOT NULL,
                        [Status] VARCHAR(20) NOT NULL DEFAULT 'PENDING',
                        [AdminNote] NVARCHAR(MAX) NULL,
                        [CreatedAt] DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
                        [ReviewedAt] DATETIME2 NULL,
                        [ReviewedBy] INT NULL,
                        [CreatedBy] INT NULL,
                        [UpdatedAt] DATETIME2 NULL,
                        [UpdatedBy] INT NULL,
                        [IsDeleted] BIT NOT NULL DEFAULT 0,
                        CONSTRAINT [PK_TblCreatorPayoutRequest] PRIMARY KEY CLUSTERED ([CreatorPayoutRequestId] ASC),
                        CONSTRAINT [FK_TblCreatorPayoutRequest_TblUser] FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[TblUser] ([UserId])
                    );

                    CREATE NONCLUSTERED INDEX [IX_TblCreatorPayoutRequest_CreatorUserId] ON [dbo].[TblCreatorPayoutRequest] ([CreatorUserId] ASC);
                    CREATE NONCLUSTERED INDEX [IX_TblCreatorPayoutRequest_Status] ON [dbo].[TblCreatorPayoutRequest] ([Status] ASC);
                END
            ");

            await db.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID(N'dbo.TblLinkDropTransaction', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM sys.check_constraints
                        WHERE name = N'CK_TblLinkDropTransaction_TransactionType'
                          AND parent_object_id = OBJECT_ID(N'dbo.TblLinkDropTransaction')
                          AND (
                              definition NOT LIKE '%CREATOR_PAYOUT%'
                              OR definition NOT LIKE '%CHAT_GROUP_JOIN%'
                              OR definition NOT LIKE '%CHAT_GROUP_EARNING%'
                          )
                    )
                    BEGIN
                        ALTER TABLE dbo.TblLinkDropTransaction
                            DROP CONSTRAINT CK_TblLinkDropTransaction_TransactionType;
                    END;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM sys.check_constraints
                        WHERE name = N'CK_TblLinkDropTransaction_TransactionType'
                          AND parent_object_id = OBJECT_ID(N'dbo.TblLinkDropTransaction')
                    )
                    BEGIN
                        ALTER TABLE dbo.TblLinkDropTransaction
                            ADD CONSTRAINT CK_TblLinkDropTransaction_TransactionType
                            CHECK ([TransactionType] IN (
                                'SPEND_GROUP_JOIN',
                                'SPEND_CHAT',
                                'REFUND',
                                'BONUS',
                                'PURCHASE',
                                'CHAT_GROUP_JOIN',
                                'CHAT_GROUP_EARNING',
                                'CREATOR_PAYOUT'
                            ));
                    END;
                END
            ");
        }

        // 1. Seed Permissions from Catalog
        foreach (var def in PermissionCatalog.All)
        {
            var existing = await db.TblPermissions.FirstOrDefaultAsync(p => p.PermissionCode == def.Code);
            if (existing is null)
            {
                db.TblPermissions.Add(new TblPermission
                {
                    PermissionCode = def.Code,
                    PermissionName = def.Name,
                    Module = def.Module,
                    Description = def.Name,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await db.SaveChangesAsync();

        // 2. Seed Roles
        var rolesToSeed = new (string Code, string Name, string Description, bool IsSystem)[]
        {
            ("ADMIN", "Administrator", "Full system administrator", true),
            ("MODERATOR", "Community Moderator", "Manages community content & moderation", true),
            ("MEMBER", "Community Member", "Standard user account", true),
            ("DOMAIN_PROFESSIONAL", "Domain Professional", "Domain Professional premium creator account", true),
            ("PUBLIC_FIGURE", "Public Figure", "Public Figure premium creator account", true)
        };

        foreach (var (code, name, desc, isSystem) in rolesToSeed)
        {
            var role = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == code);
            if (role is null)
            {
                role = new TblRole
                {
                    RoleCode = code,
                    RoleName = name,
                    Description = desc,
                    IsSystemRole = isSystem,
                    CreatedAt = DateTime.UtcNow
                };
                db.TblRoles.Add(role);
                await db.SaveChangesAsync();
            }

            // Seed Role Permissions
            var defaultCodes = PermissionCatalog.DefaultForRole(code);
            var permissions = await db.TblPermissions.Where(p => defaultCodes.Contains(p.PermissionCode)).ToListAsync();

            foreach (var perm in permissions)
            {
                var mapping = await db.TblRolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == role.RoleId && rp.PermissionId == perm.PermissionId);
                if (mapping is null)
                {
                    db.TblRolePermissions.Add(new TblRolePermission
                    {
                        RoleId = role.RoleId,
                        PermissionId = perm.PermissionId,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
        await db.SaveChangesAsync();

        // 3. Seed Default Admin & Demo Users
        if (!await db.TblUsers.AnyAsync())
        {
            var adminRole = await db.TblRoles.FirstAsync(r => r.RoleCode == "ADMIN");
            var memberRole = await db.TblRoles.FirstAsync(r => r.RoleCode == "MEMBER");

            var adminUser = new TblUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                DisplayName = "System Administrator",
                Email = "admin@communitylink.local",
                NormalizedEmail = "ADMIN@COMMUNITYLINK.LOCAL",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", 12),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblUsers.Add(adminUser);
            await db.SaveChangesAsync();

            db.TblUserRoles.Add(new TblUserRole { UserId = adminUser.UserId, RoleId = adminRole.RoleId, CreatedAt = DateTime.UtcNow });

            var demoMember = new TblUser
            {
                UserName = "member1",
                NormalizedUserName = "MEMBER1",
                DisplayName = "Demo Member",
                Email = "member@communitylink.local",
                NormalizedEmail = "MEMBER@COMMUNITYLINK.LOCAL",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123", 12),
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblUsers.Add(demoMember);
            await db.SaveChangesAsync();

            db.TblUserRoles.Add(new TblUserRole { UserId = demoMember.UserId, RoleId = memberRole.RoleId, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        // 4. Seed Premium Demo Users (DOMAIN_PROFESSIONAL & PUBLIC_FIGURE)
        var premiumUsersToSeed = new (string Username, string DisplayName, string Email, string RoleCode)[]
        {
            ("pro_user", "Dr. Alex Pro", "pro_user@communitylink.local", "DOMAIN_PROFESSIONAL"),
            ("figure_user", "Sarah Public Figure", "figure_user@communitylink.local", "PUBLIC_FIGURE")
        };

        foreach (var (username, displayName, email, roleCode) in premiumUsersToSeed)
        {
            var targetRole = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == roleCode);
            if (targetRole is null) continue;

            var existingUser = await db.TblUsers.FirstOrDefaultAsync(u => u.NormalizedUserName == username.ToUpperInvariant());
            if (existingUser is null)
            {
                var newUser = new TblUser
                {
                    UserName = username,
                    NormalizedUserName = username.ToUpperInvariant(),
                    DisplayName = displayName,
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123", 12),
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                db.TblUsers.Add(newUser);
                await db.SaveChangesAsync();

                db.TblUserRoles.Add(new TblUserRole { UserId = newUser.UserId, RoleId = targetRole.RoleId, CreatedAt = DateTime.UtcNow });
                
                db.TblLinkDropWallets.Add(new TblLinkDropWallet
                {
                    UserId = newUser.UserId,
                    Balance = 500,
                    PurchasedBalance = 300,
                    EarnedBalance = 200,
                    UpdatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }
        }
    }
}