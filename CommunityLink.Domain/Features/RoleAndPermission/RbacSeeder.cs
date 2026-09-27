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
                    -- Every value listed in the CHECK constraint below must also appear here, otherwise a
                    -- constraint created by an older build is left in place and rejects newer ledger types.
                    IF EXISTS (
                        SELECT 1
                        FROM sys.check_constraints
                        WHERE name = N'CK_TblLinkDropTransaction_TransactionType'
                          AND parent_object_id = OBJECT_ID(N'dbo.TblLinkDropTransaction')
                          AND (
                              definition NOT LIKE '%SPEND_GROUP_JOIN%'
                              OR definition NOT LIKE '%SPEND_CHAT%'
                              OR definition NOT LIKE '%REFUND%'
                              OR definition NOT LIKE '%BONUS%'
                              OR definition NOT LIKE '%PURCHASE%'
                              OR definition NOT LIKE '%CHAT_GROUP_JOIN%'
                              OR definition NOT LIKE '%CHAT_GROUP_EARNING%'
                              OR definition NOT LIKE '%CREATOR_PAYOUT%'
                              OR definition NOT LIKE '%PRIVATE_CHAT_UNLOCK%'
                              OR definition NOT LIKE '%PRIVATE_CHAT_EARNING%'
                              OR definition NOT LIKE '%TOP_UP%'
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
                                'CREATOR_PAYOUT',
                                'PRIVATE_CHAT_UNLOCK',
                                'PRIVATE_CHAT_EARNING',
                                'TOP_UP'
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

        // 5. Seed companion TblAdmin rows for admins that live in TblUser.
        await SeedAdminCompanionsAsync(db);
    }

    // TblUser.UserId and TblAdmin.AdminId are treated as a single shared id space by the
    // auth pipeline (AuthenticationService resolves a token subject against both tables).
    // Audit columns such as TblLinkDropPurchase.ReviewedByAdminId are FKs to TblAdmin, so an
    // administrator that only exists in TblUser cannot be recorded as the reviewer.
    //
    // IsSuperAdmin stays 0 so the token keeps the plain ADMIN role code. LoginAsync derives
    // the role claim from it, and CurrentUserContext.IsAdmin only recognises "ADMIN".
    private static async Task SeedAdminCompanionsAsync(AppDbContext db)
    {
        if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync(@"
            SET IDENTITY_INSERT [dbo].[TblAdmin] ON;

            INSERT INTO [dbo].[TblAdmin]
                ([AdminId], [Email], [NormalizedEmail], [FullName], [PasswordHash],
                 [IsSuperAdmin], [IsActive], [LastLoginAt], [CreatedAt],
                 [CreatedBy], [UpdatedAt], [UpdatedBy], [IsDeleted], [DeletedAt], [DeletedBy])
            SELECT u.[UserId], u.[Email], u.[NormalizedEmail], u.[DisplayName], u.[PasswordHash],
                   CAST(0 AS BIT), u.[IsActive], u.[LastLoginAt], u.[CreatedAt],
                   NULL, NULL, NULL, CAST(0 AS BIT), NULL, NULL
            FROM [dbo].[TblUser] u
            INNER JOIN [dbo].[TblUserRole] ur ON ur.[UserId] = u.[UserId]
            INNER JOIN [dbo].[TblRole] r ON r.[RoleId] = ur.[RoleId]
            WHERE r.[RoleCode] = 'ADMIN'
              AND ur.[IsDeleted] = CAST(0 AS BIT)
              AND u.[IsDeleted] = CAST(0 AS BIT)
              AND NOT EXISTS (SELECT 1 FROM [dbo].[TblAdmin] a WHERE a.[AdminId] = u.[UserId])
              AND NOT EXISTS (SELECT 1 FROM [dbo].[TblAdmin] a2 WHERE a2.[Email] = u.[Email]);

            SET IDENTITY_INSERT [dbo].[TblAdmin] OFF;

            INSERT INTO [dbo].[TblAdminRole] ([AdminId], [RoleId], [CreatedAt], [IsDeleted])
            SELECT a.[AdminId], r.[RoleId], GETUTCDATE(), CAST(0 AS BIT)
            FROM [dbo].[TblAdmin] a
            INNER JOIN [dbo].[TblRole] r ON r.[RoleCode] = 'ADMIN'
            WHERE NOT EXISTS (SELECT 1 FROM [dbo].[TblAdminRole] ar WHERE ar.[AdminId] = a.[AdminId]);
        ");

        // SQL Server does not advance the identity seed for explicit IDENTITY_INSERT values,
        // so realign it or the next self-registered admin would collide with a seeded one.
        await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.TblAdmin') WITH NO_INFOMSGS;");
    }
}