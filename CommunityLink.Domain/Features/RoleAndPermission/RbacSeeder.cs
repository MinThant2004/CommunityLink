using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Database.IsRelational())
        {
            // Startup seeding runs before SQL Server has warmed its buffer pool (pages are often
            // paged out after sleep), and the seeder issues hundreds of statements. The 30s default
            // command timeout is too tight for that cold start, so raise it for this DbContext scope.
            db.Database.SetCommandTimeout(TimeSpan.FromSeconds(300));
        }

        await db.Database.EnsureCreatedAsync();

        if (db.Database.IsRelational())
        {
            // Auto-create TblUserActivity table if it doesn't exist yet
            await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblUserActivity' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblUserActivity (
                    ActivityId       BIGINT IDENTITY(1,1) NOT NULL,
                    UserId           INT NOT NULL,
                    ActivityType     NVARCHAR(50) NOT NULL,
                    Description      NVARCHAR(500) NOT NULL,
                    TargetEntityType NVARCHAR(50) NULL,
                    TargetEntityId   INT NULL,
                    CreatedAt        DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserActivity_CreatedAt DEFAULT (SYSUTCDATETIME()),
                    IsDeleted        BIT NOT NULL CONSTRAINT DF_TblUserActivity_IsDeleted DEFAULT (0),
                    DeletedAt        DATETIME2(7) NULL,
                    CONSTRAINT PK_TblUserActivity PRIMARY KEY CLUSTERED (ActivityId ASC),
                    CONSTRAINT FK_TblUserActivity_TblUser FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId)
                );
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblUserActivity_UserId' AND object_id = OBJECT_ID('dbo.TblUserActivity'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_TblUserActivity_UserId ON dbo.TblUserActivity (UserId ASC);
                END;
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblUserActivity_User_Status' AND object_id = OBJECT_ID('dbo.TblUserActivity'))
                BEGIN
                    CREATE NONCLUSTERED INDEX IX_TblUserActivity_User_Status ON dbo.TblUserActivity (UserId ASC, IsDeleted ASC, CreatedAt DESC);
                END;
            END;");

            // Auto-create TblUserFollow table if it doesn't exist yet (for database first setups)
            await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblUserFollow' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblUserFollow (
                    FollowId    INT IDENTITY(1,1) NOT NULL,
                    FollowerId  INT NOT NULL,
                    FolloweeId  INT NOT NULL,
                    CreatedAt   DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserFollow_CreatedAt DEFAULT (SYSUTCDATETIME()),
                    CreatedBy   INT NULL,
                    IsDeleted   BIT NOT NULL CONSTRAINT DF_TblUserFollow_IsDeleted DEFAULT (0),
                    DeletedAt   DATETIME2(7) NULL,
                    DeletedBy   INT NULL,
                    RowVersion  ROWVERSION NOT NULL,
                    CONSTRAINT PK_TblUserFollow PRIMARY KEY CLUSTERED (FollowId ASC),
                    CONSTRAINT FK_TblUserFollow_Follower FOREIGN KEY (FollowerId) REFERENCES dbo.TblUser (UserId),
                    CONSTRAINT FK_TblUserFollow_Followee FOREIGN KEY (FolloweeId) REFERENCES dbo.TblUser (UserId)
                );
            END;");

            // Auto-create TblGroupRating table if it doesn't exist yet
            await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblGroupRating' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblGroupRating (
                    GroupRatingId INT IDENTITY(1,1) NOT NULL,
                    GroupId       INT NOT NULL,
                    UserId        INT NOT NULL,
                    Score         INT NOT NULL,
                    ReviewText    NVARCHAR(1000) NULL,
                    CreatedAt     DATETIME2(7) NOT NULL CONSTRAINT DF_TblGroupRating_CreatedAt DEFAULT (SYSUTCDATETIME()),
                    CreatedBy     INT NULL,
                    UpdatedAt     DATETIME2(7) NULL,
                    UpdatedBy     INT NULL,
                    IsDeleted     BIT NOT NULL CONSTRAINT DF_TblGroupRating_IsDeleted DEFAULT (0),
                    DeletedAt     DATETIME2(7) NULL,
                    DeletedBy     INT NULL,
                    RowVersion    ROWVERSION NOT NULL,
                    CONSTRAINT PK_TblGroupRating PRIMARY KEY CLUSTERED (GroupRatingId ASC),
                    CONSTRAINT FK_TblGroupRating_Group FOREIGN KEY (GroupId) REFERENCES dbo.TblGroup (GroupId),
                    CONSTRAINT FK_TblGroupRating_User FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId)
                );
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblGroupRating_Group_User' AND object_id = OBJECT_ID('dbo.TblGroupRating'))
                BEGIN
                    CREATE UNIQUE NONCLUSTERED INDEX IX_TblGroupRating_Group_User 
                        ON dbo.TblGroupRating (GroupId ASC, UserId ASC)
                        WHERE IsDeleted = 0;
                END;
            END;");

            // Auto-create TblAdminInvite table if it doesn't exist yet
            await db.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblAdminInvite' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblAdminInvite (
                    InviteId     INT IDENTITY(1,1) NOT NULL,
                    Email        NVARCHAR(256) NOT NULL,
                    Token        NVARCHAR(200) NOT NULL,
                    RoleId       INT NOT NULL,
                    ExpiresAtUtc DATETIME2(7) NOT NULL,
                    IsUsed       BIT NOT NULL CONSTRAINT DF_TblAdminInvite_IsUsed DEFAULT (0),
                    CreatedBy    INT NULL,
                    CreatedAtUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TblAdminInvite_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    UsedAtUtc    DATETIME2(7) NULL,
                    CONSTRAINT PK_TblAdminInvite PRIMARY KEY CLUSTERED (InviteId ASC),
                    CONSTRAINT FK_TblAdminInvite_Role FOREIGN KEY (RoleId) REFERENCES dbo.TblRole (RoleId)
                );
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblAdminInvite_Token' AND object_id = OBJECT_ID('dbo.TblAdminInvite'))
                BEGIN
                    CREATE UNIQUE NONCLUSTERED INDEX IX_TblAdminInvite_Token 
                        ON dbo.TblAdminInvite (Token ASC);
                END;
            END;
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblAdminInvite') AND name = 'IsSuperAdmin')
            BEGIN
                ALTER TABLE dbo.TblAdminInvite ADD IsSuperAdmin BIT NOT NULL CONSTRAINT DF_TblAdminInvite_IsSuperAdmin DEFAULT (0);
            END;

            -- Auto-create TblSubscriptionPlan table if it doesn't exist
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblSubscriptionPlan' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblSubscriptionPlan (
                    PlanId          INT IDENTITY(1,1) NOT NULL,
                    TargetRoleCode  NVARCHAR(50) NOT NULL,
                    PlanName        NVARCHAR(150) NOT NULL,
                    BillingInterval NVARCHAR(20) NOT NULL CONSTRAINT DF_TblSubPlan_BillingInterval DEFAULT ('Monthly'),
                    DurationDays    INT NOT NULL CONSTRAINT DF_TblSubPlan_DurationDays DEFAULT (30),
                    PriceAmount     DECIMAL(18,2) NOT NULL CONSTRAINT DF_TblSubPlan_PriceAmount DEFAULT (0),
                    LinkDropCost    BIGINT NOT NULL CONSTRAINT DF_TblSubPlan_LinkDropCost DEFAULT (0),
                    PerksJson       NVARCHAR(MAX) NULL,
                    IsActive        BIT NOT NULL CONSTRAINT DF_TblSubPlan_IsActive DEFAULT (1),
                    CreatedAt       DATETIME2(7) NOT NULL CONSTRAINT DF_TblSubPlan_CreatedAt DEFAULT (SYSUTCDATETIME()),
                    UpdatedAt       DATETIME2(7) NULL,
                    CONSTRAINT PK_TblSubscriptionPlan PRIMARY KEY CLUSTERED (PlanId ASC)
                );
            END;

            -- Auto-create TblUserSubscription table if it doesn't exist
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblUserSubscription' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblUserSubscription (
                    SubscriptionId INT IDENTITY(1,1) NOT NULL,
                    UserId         INT NOT NULL,
                    PlanId         INT NOT NULL,
                    RoleId         INT NOT NULL,
                    Status         NVARCHAR(30) NOT NULL CONSTRAINT DF_TblUserSub_Status DEFAULT ('Active'),
                    PaymentMethod  NVARCHAR(50) NOT NULL CONSTRAINT DF_TblUserSub_PaymentMethod DEFAULT ('LinkDropPoints'),
                    StartDateUtc   DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserSub_StartDateUtc DEFAULT (SYSUTCDATETIME()),
                    ExpiresAtUtc   DATETIME2(7) NOT NULL,
                    CreatedAtUtc   DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserSub_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT PK_TblUserSubscription PRIMARY KEY CLUSTERED (SubscriptionId ASC),
                    CONSTRAINT FK_TblUserSubscription_User FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId),
                    CONSTRAINT FK_TblUserSubscription_Plan FOREIGN KEY (PlanId) REFERENCES dbo.TblSubscriptionPlan (PlanId),
                    CONSTRAINT FK_TblUserSubscription_Role FOREIGN KEY (RoleId) REFERENCES dbo.TblRole (RoleId)
                );
            END;

            -- Auto-create TblIdentityVerification table if it doesn't exist
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblIdentityVerification' AND schema_id = SCHEMA_ID('dbo'))
            BEGIN
                CREATE TABLE dbo.TblIdentityVerification (
                    VerificationId          INT IDENTITY(1,1) NOT NULL,
                    UserId                  INT NOT NULL,
                    PlanId                  INT NOT NULL,
                    TargetRoleCode          NVARCHAR(50) NOT NULL,
                    FullLegalName           NVARCHAR(200) NOT NULL,
                    WorkEmail               NVARCHAR(256) NULL,
                    ProfessionalUrl         NVARCHAR(500) NULL,
                    IdCardFrontUrl          NVARCHAR(1000) NOT NULL,
                    IdCardBackUrl           NVARCHAR(1000) NULL,
                    PaymentMethod           NVARCHAR(50) NOT NULL CONSTRAINT DF_TblIdVerif_PaymentMethod DEFAULT ('LinkDropPoints'),
                    LinkDropPointsDeducted  BIGINT NOT NULL CONSTRAINT DF_TblIdVerif_PointsDeducted DEFAULT (0),
                    Status                  NVARCHAR(30) NOT NULL CONSTRAINT DF_TblIdVerif_Status DEFAULT ('PendingReview'),
                    ReviewNotes             NVARCHAR(1000) NULL,
                    ReviewedByAdminId       INT NULL,
                    ReviewedAtUtc           DATETIME2(7) NULL,
                    Skills                  NVARCHAR(1000) NULL,
                    Location                NVARCHAR(200) NULL,
                    CertificateUrls         NVARCHAR(MAX) NULL,
                    CreatedAtUtc            DATETIME2(7) NOT NULL CONSTRAINT DF_TblIdVerif_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT PK_TblIdentityVerification PRIMARY KEY CLUSTERED (VerificationId ASC),
                    CONSTRAINT FK_TblIdentityVerification_User FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId),
                    CONSTRAINT FK_TblIdentityVerification_Plan FOREIGN KEY (PlanId) REFERENCES dbo.TblSubscriptionPlan (PlanId),
                    CONSTRAINT FK_TblIdentityVerification_Admin FOREIGN KEY (ReviewedByAdminId) REFERENCES dbo.TblAdmin (AdminId)
                );
            END;

            IF OBJECT_ID(N'dbo.TblIdentityVerification', N'U') IS NOT NULL
            BEGIN
                IF COL_LENGTH('dbo.TblIdentityVerification', 'Skills') IS NULL
                    ALTER TABLE dbo.TblIdentityVerification ADD Skills NVARCHAR(1000) NULL;
                IF COL_LENGTH('dbo.TblIdentityVerification', 'Location') IS NULL
                    ALTER TABLE dbo.TblIdentityVerification ADD Location NVARCHAR(200) NULL;
                IF COL_LENGTH('dbo.TblIdentityVerification', 'CertificateUrls') IS NULL
                    ALTER TABLE dbo.TblIdentityVerification ADD CertificateUrls NVARCHAR(MAX) NULL;
            END;");

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

            // Auto-create/alter RoleType on TblRole and Type on TblPermission
            await db.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblRole') AND name = 'RoleType')
                BEGIN
                    ALTER TABLE dbo.TblRole ADD RoleType INT NOT NULL CONSTRAINT DF_TblRole_RoleType DEFAULT (0);
                END;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPermission') AND name = 'Type')
                BEGIN
                    ALTER TABLE dbo.TblPermission ADD [Type] INT NOT NULL CONSTRAINT DF_TblPermission_Type DEFAULT (0);
                END;
            ");

            // Ensure IDENTITY current values match or exceed existing data to prevent duplicate key violations (e.g. PK (1))
            await db.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID(N'dbo.TblPermission', N'U') IS NOT NULL
                BEGIN
                    DECLARE @MaxPermId INT = ISNULL((SELECT MAX(PermissionId) FROM dbo.TblPermission), 0);
                    IF @MaxPermId > 0
                        DBCC CHECKIDENT (N'dbo.TblPermission', RESEED, @MaxPermId);
                END;

                IF OBJECT_ID(N'dbo.TblRole', N'U') IS NOT NULL
                BEGIN
                    DECLARE @MaxRoleId INT = ISNULL((SELECT MAX(RoleId) FROM dbo.TblRole), 0);
                    IF @MaxRoleId > 0
                        DBCC CHECKIDENT (N'dbo.TblRole', RESEED, @MaxRoleId);
                END;

                IF OBJECT_ID(N'dbo.TblRolePermission', N'U') IS NOT NULL
                BEGIN
                    DECLARE @MaxRolePermId INT = ISNULL((SELECT MAX(RolePermissionId) FROM dbo.TblRolePermission), 0);
                    IF @MaxRolePermId > 0
                        DBCC CHECKIDENT (N'dbo.TblRolePermission', RESEED, @MaxRolePermId);
                END;
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
                    Type = def.Type,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Type = def.Type;
                existing.PermissionName = def.Name;
                existing.Module = def.Module;
            }
        }
        await db.SaveChangesAsync();

        // 2. Seed Roles
        // User Domain Roles (RoleType = 0): DEFAULT_USER (System Role, Undeletable Default Fallback), DOMAIN_PROFESSIONAL, PUBLIC_FIGURE
        // Admin Domain Roles (RoleType = 1): SYSTEM_ADMIN (System Role, Undeletable), ADMIN (System Role, Undeletable Fallback)
        var rolesToSeed = new (string Code, string Name, string Description, bool IsSystem, int RoleType)[]
        {
            ("SYSTEM_ADMIN", "System Administrator", "Full system administrator with unchangeable permissions", true, 1),
            ("ADMIN", "Administrator", "Default administrator account with customizable permissions", true, 1),
            ("DEFAULT_USER", "Default User", "Default standard user role assigned to all regular users", true, 0),
            ("DOMAIN_PROFESSIONAL", "Domain Professional", "Domain Professional premium creator account", false, 0),
            ("PUBLIC_FIGURE", "Public Figure", "Notable community creator or public figure", false, 0)
        };

        foreach (var (code, name, desc, isSystem, roleType) in rolesToSeed)
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
                    RoleType = roleType,
                    CreatedAt = DateTime.UtcNow
                };
                db.TblRoles.Add(role);
                await db.SaveChangesAsync();
            }
            else
            {
                role.RoleType = roleType;
                role.IsSystemRole = isSystem;
                role.RoleName = name;
                if (!string.IsNullOrEmpty(desc)) role.Description = desc;
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
                else if (mapping.IsDeleted && (code == "SYSTEM_ADMIN" || code == "ADMIN" || defaultCodes.Contains(perm.PermissionCode)))
                {
                    mapping.IsDeleted = false;
                    mapping.UpdatedAt = DateTime.UtcNow;
                }
            }

            // Sync/remove any role permissions that should not belong to the system roles by default
            if (code == "ADMIN")
            {
                var adminDisallowed = await db.TblRolePermissions
                    .Include(rp => rp.Permission)
                    .Where(rp => rp.RoleId == role.RoleId && !rp.IsDeleted &&
                                 (rp.Permission.PermissionCode == PermissionCatalog.AdminRbacManage ||
                                  rp.Permission.PermissionCode == PermissionCatalog.AdminUserManage ||
                                  rp.Permission.Type != 1))
                    .ToListAsync();
                foreach (var dis in adminDisallowed)
                {
                    dis.IsDeleted = true;
                    dis.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (code == "DEFAULT_USER")
            {
                var userDisallowed = await db.TblRolePermissions
                    .Include(rp => rp.Permission)
                    .Where(rp => rp.RoleId == role.RoleId && !rp.IsDeleted &&
                                 (!defaultCodes.Contains(rp.Permission.PermissionCode) || rp.Permission.Type != 0))
                    .ToListAsync();
                foreach (var dis in userDisallowed)
                {
                    dis.IsDeleted = true;
                    dis.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        await db.SaveChangesAsync();

        if (db.Database.IsRelational())
        {
            // Reassign any leftover legacy roles to DEFAULT_USER or ADMIN, then clean them up
            await db.Database.ExecuteSqlRawAsync(@"
                DECLARE @DefaultUserId INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'DEFAULT_USER');
                DECLARE @AdminId INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'ADMIN');

                IF @DefaultUserId IS NOT NULL
                BEGIN
                    UPDATE dbo.TblUserRole 
                    SET RoleId = @DefaultUserId 
                    WHERE RoleId IN (SELECT RoleId FROM dbo.TblRole WHERE RoleCode NOT IN ('SYSTEM_ADMIN', 'ADMIN', 'DEFAULT_USER', 'DOMAIN_PROFESSIONAL', 'PUBLIC_FIGURE'));
                END;

                IF @AdminId IS NOT NULL
                BEGIN
                    UPDATE dbo.TblAdminRole 
                    SET RoleId = @AdminId 
                    WHERE RoleId IN (SELECT RoleId FROM dbo.TblRole WHERE RoleCode NOT IN ('SYSTEM_ADMIN', 'ADMIN', 'DEFAULT_USER', 'DOMAIN_PROFESSIONAL', 'PUBLIC_FIGURE'));
                END;

                DELETE FROM dbo.TblRolePermission 
                WHERE RoleId IN (SELECT RoleId FROM dbo.TblRole WHERE RoleCode NOT IN ('SYSTEM_ADMIN', 'ADMIN', 'DEFAULT_USER', 'DOMAIN_PROFESSIONAL', 'PUBLIC_FIGURE'));

                DELETE FROM dbo.TblRole 
                WHERE RoleCode NOT IN ('SYSTEM_ADMIN', 'ADMIN', 'DEFAULT_USER', 'DOMAIN_PROFESSIONAL', 'PUBLIC_FIGURE');

                -- Sync IsSystemRole in the database so strictly SYSTEM_ADMIN, ADMIN, and DEFAULT_USER are 1
                UPDATE dbo.TblRole
                SET IsSystemRole = CASE 
                    WHEN UPPER(RoleCode) IN ('SYSTEM_ADMIN', 'ADMIN', 'DEFAULT_USER') THEN 1 
                    ELSE 0 
                END,
                RoleType = CASE
                    WHEN UPPER(RoleCode) IN ('SYSTEM_ADMIN', 'ADMIN', 'SYSTEMADMIN') THEN 1
                    ELSE 0
                END;

                -- Sync Permission Type: exactly 11 Admin Domain permissions have Type = 1, all others have Type = 0
                UPDATE dbo.TblPermission
                SET [Type] = 1, [Module] = 'Administration'
                WHERE UPPER(PermissionCode) IN (
                    'ADMIN.RBAC.MANAGE', 'ADMIN.USER.MANAGE', 'ADMIN.USER.VIEW', 'SYSTEMAUDIT.VIEW',
                    'COMMUNITY.MODERATE', 'COMMUNITY.CREATE', 'COMMUNITY.MANAGE',
                    'GROUP.MANAGEMENT', 'GROUP.MANAGE', 'POST.MANAGEMENT', 'REPORT.VIEW'
                );

                UPDATE dbo.TblPermission
                SET [Type] = 0
                WHERE UPPER(PermissionCode) NOT IN (
                    'ADMIN.RBAC.MANAGE', 'ADMIN.USER.MANAGE', 'ADMIN.USER.VIEW', 'SYSTEMAUDIT.VIEW',
                    'COMMUNITY.MODERATE', 'COMMUNITY.CREATE', 'COMMUNITY.MANAGE',
                    'GROUP.MANAGEMENT', 'GROUP.MANAGE', 'POST.MANAGEMENT', 'REPORT.VIEW'
                );

                -- Ensure REPORT.VIEW is assigned to Admin roles and revoked from non-admin roles
                DELETE FROM dbo.TblRolePermission 
                WHERE PermissionId IN (SELECT PermissionId FROM dbo.TblPermission WHERE UPPER(PermissionCode) = 'REPORT.VIEW')
                AND RoleId NOT IN (SELECT RoleId FROM dbo.TblRole WHERE UPPER(RoleCode) IN ('SYSTEM_ADMIN', 'ADMIN', 'SYSTEMADMIN', 'SUPERADMIN'));

                DECLARE @ReportPermId INT = (SELECT PermissionId FROM dbo.TblPermission WHERE UPPER(PermissionCode) = 'REPORT.VIEW');
                IF @ReportPermId IS NOT NULL AND @AdminId IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM dbo.TblRolePermission WHERE RoleId = @AdminId AND PermissionId = @ReportPermId)
                    BEGIN
                        INSERT INTO dbo.TblRolePermission (RoleId, PermissionId, IsDeleted, CreatedAt)
                        VALUES (@AdminId, @ReportPermId, 0, SYSUTCDATETIME());
                    END
                    ELSE
                    BEGIN
                        UPDATE dbo.TblRolePermission SET IsDeleted = 0 WHERE RoleId = @AdminId AND PermissionId = @ReportPermId;
                    END
                END;");
        }

        // 3. Seed Default Admin & Demo Users
        if (!await db.TblUsers.AnyAsync())
        {
            var adminRole = await db.TblRoles.FirstAsync(r => r.RoleCode == "ADMIN");
            var defaultUserRole = await db.TblRoles.FirstAsync(r => r.RoleCode == "DEFAULT_USER");

            var adminUser = new TblUser
            {
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                DisplayName = "Administrator",
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

            db.TblUserRoles.Add(new TblUserRole { UserId = demoMember.UserId, RoleId = defaultUserRole.RoleId, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        // 4. Seed Master Admin in TblAdmin (admin@communitylink.local / 123456789)
        var masterAdminEmail = "admin@communitylink.local";
        var normalizedMasterEmail = masterAdminEmail.ToUpperInvariant();
        var existingAdmin = await db.TblAdmins.FirstOrDefaultAsync(a => a.NormalizedEmail == normalizedMasterEmail);
        var systemAdminRoleEntity = await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "SYSTEM_ADMIN")
                                    ?? await db.TblRoles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN");

        if (existingAdmin is null)
        {
            var masterAdmin = new TblAdmin
            {
                FullName = "Master System Admin",
                Email = masterAdminEmail,
                NormalizedEmail = normalizedMasterEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789", 12),
                IsSuperAdmin = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            db.TblAdmins.Add(masterAdmin);
            await db.SaveChangesAsync();

            if (systemAdminRoleEntity != null)
            {
                db.TblAdminRoles.Add(new TblAdminRole
                {
                    AdminId = masterAdmin.AdminId,
                    RoleId = systemAdminRoleEntity.RoleId,
                    CreatedAt = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }
        else
        {
            // Ensure password is set to 123456789 and role is mapped
            existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456789", 12);
            existingAdmin.IsActive = true;
            existingAdmin.IsDeleted = false;
            await db.SaveChangesAsync();

            if (systemAdminRoleEntity != null)
            {
                var existingRoleMapping = await db.TblAdminRoles.FirstOrDefaultAsync(ar => ar.AdminId == existingAdmin.AdminId && !ar.IsDeleted);
                if (existingRoleMapping == null)
                {
                    db.TblAdminRoles.Add(new TblAdminRole
                    {
                        AdminId = existingAdmin.AdminId,
                        RoleId = systemAdminRoleEntity.RoleId,
                        CreatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
                else
                {
                    existingRoleMapping.RoleId = systemAdminRoleEntity.RoleId;
                    await db.SaveChangesAsync();
                }
            }
        }

        // 5. Seed Initial Subscription Plans if empty
        if (!await db.TblSubscriptionPlans.AnyAsync())
        {
            db.TblSubscriptionPlans.AddRange(
                new TblSubscriptionPlan
                {
                    TargetRoleCode = "DOMAIN_PRO",
                    PlanName = "Domain Professional (Monthly)",
                    BillingInterval = "Monthly",
                    DurationDays = 30,
                    PriceAmount = 39.00m,
                    LinkDropCost = 390,
                    PerksJson = "[\"1-on-1 Paid Advisory Engine (set custom hourly rate)\",\"Public Peer Rating & Review Card\",\"Priority Sub-Community Ownership (up to 5)\",\"Domain Competency Endorsement\",\"Credential Verification Badging\"]",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TblSubscriptionPlan
                {
                    TargetRoleCode = "DOMAIN_PRO",
                    PlanName = "Domain Professional (Annual)",
                    BillingInterval = "Annual",
                    DurationDays = 365,
                    PriceAmount = 390.00m,
                    LinkDropCost = 3900,
                    PerksJson = "[\"All Monthly Perks included\",\"2 Months Free Discount\",\"Expedited 24h Verification Audit\",\"Featured in Domain Pro Directory Shelf\"]",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TblSubscriptionPlan
                {
                    TargetRoleCode = "PUBLIC_FIGURE",
                    PlanName = "Public Figure VIP (Monthly)",
                    BillingInterval = "Monthly",
                    DurationDays = 30,
                    PriceAmount = 119.00m,
                    LinkDropCost = 1190,
                    PerksJson = "[\"Discovery shelf Priority Guaranteed top placement\",\"Unlimited Sovereign Communities\",\"0% Platform Fees on Advisory (first $10,000/yr)\",\"Dedicated Admin Concierge Direct Slack/Signal channel\",\"Government ID & Identity Card Verified\"]",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new TblSubscriptionPlan
                {
                    TargetRoleCode = "PUBLIC_FIGURE",
                    PlanName = "Public Figure VIP (Annual)",
                    BillingInterval = "Annual",
                    DurationDays = 365,
                    PriceAmount = 1190.00m,
                    LinkDropCost = 11900,
                    PerksJson = "[\"All Public Figure VIP Monthly Perks\",\"Save $238 (2 Months Free)\",\"Priority Expedited SecOps Review\",\"VIP Platinum Profile Crest\"]",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();
        }

        // 6. Seed Premium Demo Users (DOMAIN_PROFESSIONAL & PUBLIC_FIGURE)
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

        // 7. Seed companion TblAdmin rows for admins that live in TblUser.
        await SeedAdminCompanionsAsync(db);
    }

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

        await db.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('dbo.TblAdmin') WITH NO_INFOMSGS;");
    }
}