using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.RoleAndPermission;

public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

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