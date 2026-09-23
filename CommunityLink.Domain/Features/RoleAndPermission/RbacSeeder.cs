using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

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

        // 2. Seed Roles (Only ADMIN and USER are System Roles)
        var rolesToSeed = new (string Code, string Name, string Description, bool IsSystem)[]
        {
            ("ADMIN", "Administrator", "Full system administrator", true),
            ("USER", "User", "Default standard user", true),
            ("MODERATOR", "Community Moderator", "Manages community content & moderation", false),
            ("MEMBER", "Community Member", "Standard user account", false),
            ("DOMAIN_PRO", "Domain Professional", "Verified domain expert", false),
            ("PUBLIC_FIGURE", "Public Figure", "Notable community creator or public figure", false)
        };

        // Sync IsSystemRole in the database so only ADMIN and USER are 1, while others are 0
        await db.Database.ExecuteSqlRawAsync(@"
            UPDATE dbo.TblRole
            SET IsSystemRole = CASE 
                WHEN UPPER(RoleCode) IN ('ADMIN', 'USER') THEN 1 
                ELSE 0 
            END;");

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
                else if (code == "ADMIN" && mapping.IsDeleted)
                {
                    mapping.IsDeleted = false;
                    mapping.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        await db.SaveChangesAsync();

        // 3. Seed Default Admin & Demo User
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
    }
}