using System.IO;
using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Security;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Community;

using CommunityLink.Domain.Features.RoleAndPermission;
using CommunityLink.Shared.Security;

namespace CommunityLink.Domain.Features.Community;

public interface ICommunityService
{
    Task<Result<IReadOnlyList<CommunityModel>>> GetCommunitiesAsync(string? search, CancellationToken cancellationToken = default);
    Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default);
    Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<CommunityModel>> UpdateCommunityAsync(int communityId, EditCommunityRequestModel request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommunityAuditModel>>> GetCommunityAuditsAsync(int communityId, CancellationToken cancellationToken = default);
    Task<Result> JoinCommunityAsync(int communityId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommunityModel>>> GetJoinedCommunitiesAsync(int userId, int take = 10, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommunityModel>>> GetRecommendedCommunitiesAsync(int userId, int take = 6, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommunityModel>>> GetCommunityDirectoryAsync(CancellationToken cancellationToken = default);
    Task<Result<string>> UploadBannerAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public sealed class CommunityService(
    AppDbContext dbContext,
    ICurrentUserContext currentUser,
    IPermissionEvaluator permissionEvaluator) : ICommunityService
{
    public async Task<Result<IReadOnlyList<CommunityModel>>> GetCommunitiesAsync(string? search, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseQuery = dbContext.TblCommunities
                .Where(c => !c.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                baseQuery = baseQuery.Where(c => c.Name.Contains(search) || c.Slug.Contains(search));
            }

            var rawList = await baseQuery.Select(c => new
            {
                c.CommunityId,
                c.Name,
                c.Slug,
                c.Description,
                c.AvatarUrl,
                c.BannerUrl,
                c.Visibility,
                c.JoinPolicy,
                MemberCount = c.MemberCount > 0 ? c.MemberCount : c.TblCommunityMembers.Count(m => !m.IsDeleted),
                PostCount = c.PostCount > 0 ? c.PostCount : c.TblPosts.Count(p => !p.IsDeleted),
                AverageRating = (double)(c.AverageRating ?? 5.0m),
                c.OwnerId,
                OwnerName = c.Owner != null ? c.Owner.DisplayName : "Admin",
                c.CreatedAt,
                c.ParentCommunityId,
                ParentCommunityName = c.ParentCommunity != null ? c.ParentCommunity.Name : null,
                SubCommunityCount = c.InverseParentCommunity.Count(sc => !sc.IsDeleted),
                GroupCount = c.TblGroups.Count(g => !g.IsDeleted)
            }).ToListAsync(cancellationToken);

            if (!rawList.Any())
            {
                return Result<IReadOnlyList<CommunityModel>>.Success([]);
            }

        var communityIds = rawList.Select(c => c.CommunityId).ToList();

        HashSet<int> joinedIds = [];
        if (currentUser.UserId.HasValue)
        {
            joinedIds = await dbContext.TblCommunityMembers
                .Where(m => m.UserId == currentUser.UserId.Value && communityIds.Contains(m.CommunityId) && !m.IsDeleted)
                .Select(m => m.CommunityId)
                .Distinct()
                .ToHashSetAsync(cancellationToken);
        }

        var allGroups = await dbContext.TblGroups
            .Where(g => !g.IsDeleted && communityIds.Contains(g.SubCommunityId))
            .Select(g => new { g.GroupId, g.SubCommunityId, g.Name, g.Slug, g.MemberCount })
            .ToListAsync(cancellationToken);

        var topGroupsLookup = allGroups
            .GroupBy(g => g.SubCommunityId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CommunityChildItemSummary>)g.OrderByDescending(x => x.MemberCount).Take(3).Select(x => new CommunityChildItemSummary(x.GroupId, x.Name, x.Slug, x.MemberCount, "Group")).ToList()
            );

        var subCommunitiesLookup = rawList
            .Where(sc => sc.ParentCommunityId.HasValue)
            .GroupBy(sc => sc.ParentCommunityId!.Value)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<CommunityChildItemSummary>)g.OrderByDescending(x => x.MemberCount).Take(3).Select(x => new CommunityChildItemSummary(x.CommunityId, x.Name, x.Slug, x.MemberCount, "Sub-Com")).ToList()
            );

        var memberAvatarsRaw = await dbContext.TblCommunityMembers
            .Where(m => !m.IsDeleted && communityIds.Contains(m.CommunityId) && m.User != null && m.User.AvatarUrl != null && m.User.AvatarUrl != "")
            .OrderByDescending(m => m.JoinedAt)
            .Select(m => new { m.CommunityId, AvatarUrl = m.User!.AvatarUrl! })
            .ToListAsync(cancellationToken);

        var memberAvatarsLookup = memberAvatarsRaw
            .GroupBy(m => m.CommunityId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(x => x.AvatarUrl).Distinct().Take(3).ToList()
            );

        var roleRows = await dbContext.TblCommunityMembers
            .Where(m => !m.IsDeleted && communityIds.Contains(m.CommunityId) && m.User != null && m.User.TblUserRoles.Any(ur => !ur.IsDeleted))
            .Select(m => new
            {
                m.CommunityId,
                IsDomainPro = m.User!.TblUserRoles.Any(ur => !ur.IsDeleted && ur.Role.RoleCode == "DOMAIN_PRO"),
                IsPublicFigure = m.User!.TblUserRoles.Any(ur => !ur.IsDeleted && ur.Role.RoleCode == "PUBLIC_FIGURE")
            })
            .ToListAsync(cancellationToken);

        var domainProLookup = roleRows
            .Where(x => x.IsDomainPro)
            .GroupBy(x => x.CommunityId)
            .ToDictionary(g => g.Key, g => g.Count());

        var publicFigureLookup = roleRows
            .Where(x => x.IsPublicFigure)
            .GroupBy(x => x.CommunityId)
            .ToDictionary(g => g.Key, g => g.Count());

        var list = rawList.Select(c =>
        {
            var isSub = c.ParentCommunityId.HasValue;
            var topItems = isSub
                ? (topGroupsLookup.TryGetValue(c.CommunityId, out var grps) ? grps : [])
                : (subCommunitiesLookup.TryGetValue(c.CommunityId, out var subs) ? subs : []);

            var categoryName = isSub 
                ? (c.ParentCommunityName != null ? c.ParentCommunityName.ToUpperInvariant() : "SUB-COMMUNITY") 
                : "CAPITAL & SYNDICATE";

            var avatars = memberAvatarsLookup.TryGetValue(c.CommunityId, out var avs) ? avs : [];
            var extraAvatars = Math.Max(0, c.MemberCount - avatars.Count);

            var domainProCount = domainProLookup.TryGetValue(c.CommunityId, out var dp) ? dp : 0;
            var publicFigureCount = publicFigureLookup.TryGetValue(c.CommunityId, out var pf) ? pf : 0;

            return new CommunityModel(
                c.CommunityId,
                c.Name,
                c.Slug,
                c.Description,
                c.AvatarUrl,
                c.BannerUrl,
                c.Visibility,
                c.JoinPolicy,
                c.MemberCount,
                c.PostCount,
                c.AverageRating,
                c.OwnerId,
                c.OwnerName,
                c.CreatedAt,
                c.ParentCommunityId,
                c.ParentCommunityName,
                joinedIds.Contains(c.CommunityId),
                c.SubCommunityCount,
                c.GroupCount,
                domainProCount,
                publicFigureCount,
                avatars,
                extraAvatars,
                topItems,
                categoryName);
        }).ToList();

            return Result<IReadOnlyList<CommunityModel>>.Success(list);
        }
        catch (OperationCanceledException)
        {
            return Result<IReadOnlyList<CommunityModel>>.Success([]);
        }
    }

    public async Task<Result<CommunityModel>> GetCommunityByIdAsync(int communityId, CancellationToken cancellationToken = default)
    {
        var raw = await dbContext.TblCommunities
            .Where(c => c.CommunityId == communityId && !c.IsDeleted)
            .Select(c => new
            {
                c.CommunityId,
                c.Name,
                c.Slug,
                c.Description,
                c.AvatarUrl,
                c.BannerUrl,
                c.Visibility,
                c.JoinPolicy,
                MemberCount = c.MemberCount > 0 ? c.MemberCount : c.TblCommunityMembers.Count(m => !m.IsDeleted),
                PostCount = c.PostCount > 0 ? c.PostCount : c.TblPosts.Count(p => !p.IsDeleted),
                AverageRating = (double)(c.AverageRating ?? 5.0m),
                c.OwnerId,
                OwnerName = c.Owner != null ? c.Owner.DisplayName : "Admin",
                c.CreatedAt,
                c.ParentCommunityId,
                ParentCommunityName = c.ParentCommunity != null ? c.ParentCommunity.Name : null,
                SubCommunityCount = c.InverseParentCommunity.Count(sc => !sc.IsDeleted),
                GroupCount = c.TblGroups.Count(g => !g.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (raw is null) return Result<CommunityModel>.Failure("Community not found.", ResultStatus.NotFound);

        var currentUserId = currentUser.UserId;
        var isJoined = currentUserId.HasValue && await dbContext.TblCommunityMembers
            .AnyAsync(m => m.CommunityId == communityId && m.UserId == currentUserId.Value && !m.IsDeleted, cancellationToken);

        var isSub = raw.ParentCommunityId.HasValue;

        IReadOnlyList<CommunityChildItemSummary> topItems;
        if (isSub)
        {
            topItems = await dbContext.TblGroups
                .Where(g => g.SubCommunityId == communityId && !g.IsDeleted)
                .OrderByDescending(g => g.MemberCount)
                .Take(3)
                .Select(g => new CommunityChildItemSummary(g.GroupId, g.Name, g.Slug, g.MemberCount, "Group"))
                .ToListAsync(cancellationToken);
        }
        else
        {
            topItems = await dbContext.TblCommunities
                .Where(sc => sc.ParentCommunityId == communityId && !sc.IsDeleted)
                .OrderByDescending(sc => sc.MemberCount)
                .Take(3)
                .Select(sc => new CommunityChildItemSummary(sc.CommunityId, sc.Name, sc.Slug, sc.MemberCount, "Sub-Com"))
                .ToListAsync(cancellationToken);
        }

        var memberAvatars = await dbContext.TblCommunityMembers
            .Where(m => m.CommunityId == communityId && !m.IsDeleted && m.User != null && m.User.AvatarUrl != null && m.User.AvatarUrl != "")
            .OrderByDescending(m => m.JoinedAt)
            .Select(m => m.User!.AvatarUrl!)
            .Take(3)
            .ToListAsync(cancellationToken);

        var domainProCount = await dbContext.TblCommunityMembers
            .CountAsync(m => m.CommunityId == communityId && !m.IsDeleted && m.User != null && m.User.TblUserRoles.Any(ur => !ur.IsDeleted && ur.Role.RoleCode == "DOMAIN_PRO"), cancellationToken);

        var publicFigureCount = await dbContext.TblCommunityMembers
            .CountAsync(m => m.CommunityId == communityId && !m.IsDeleted && m.User != null && m.User.TblUserRoles.Any(ur => !ur.IsDeleted && ur.Role.RoleCode == "PUBLIC_FIGURE"), cancellationToken);

        var categoryName = isSub 
            ? (raw.ParentCommunityName != null ? raw.ParentCommunityName.ToUpperInvariant() : "SUB-COMMUNITY") 
            : "CAPITAL & SYNDICATE";

        var extraAvatars = Math.Max(0, raw.MemberCount - memberAvatars.Count);

        var model = new CommunityModel(
            raw.CommunityId,
            raw.Name,
            raw.Slug,
            raw.Description,
            raw.AvatarUrl,
            raw.BannerUrl,
            raw.Visibility,
            raw.JoinPolicy,
            raw.MemberCount,
            raw.PostCount,
            raw.AverageRating,
            raw.OwnerId,
            raw.OwnerName,
            raw.CreatedAt,
            raw.ParentCommunityId,
            raw.ParentCommunityName,
            isJoined,
            raw.SubCommunityCount,
            raw.GroupCount,
            domainProCount,
            publicFigureCount,
            memberAvatars,
            extraAvatars,
            topItems,
            categoryName);

        return Result<CommunityModel>.Success(model);
    }

    public async Task<Result<string>> UploadBannerAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
            return Result<string>.Failure("Only JPEG, PNG, and WebP images are allowed.", ResultStatus.ValidationError);

        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "communities", "banners");
        if (!Directory.Exists(wwwrootPath))
        {
            Directory.CreateDirectory(wwwrootPath);
        }

        var uniqueFileName = $"banner_{DateTime.UtcNow.Ticks}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(wwwrootPath, uniqueFileName);

        using (var destStream = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destStream, cancellationToken);
        }

        var bannerUrl = $"/uploads/communities/banners/{uniqueFileName}";
        return Result<string>.Success(bannerUrl, "Banner uploaded successfully.");
    }

    public async Task<Result<CommunityModel>> CreateCommunityAsync(CreateCommunityRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CommunityModel>.Failure("Community name is required.", ResultStatus.ValidationError);
        }

        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToUpper();

        // Check if name already exists anywhere in dbo.TblCommunity (case-insensitive)
        var nameExists = await dbContext.TblCommunities
            .AnyAsync(c => !c.IsDeleted && c.Name.ToUpper() == normalizedName, cancellationToken);

        if (nameExists)
        {
            var errorMessage = request.ParentCommunityId.HasValue
                ? "This sub-community name is already exist!"
                : "This community name is already exist!";
            return Result<CommunityModel>.Failure(errorMessage, ResultStatus.Conflict);
        }

        // Check permissions: COMMUNITY.CREATE for top-level, SUBCOMMUNITY.CREATE for nested
        if (currentUser.UserId.HasValue)
        {
            var requiredPermission = request.ParentCommunityId.HasValue
                ? PermissionCatalog.SubCommunityCreate
                : PermissionCatalog.CommunityCreate;

            var hasPermission = await permissionEvaluator.HasPermissionAsync(requiredPermission, cancellationToken);
            if (!hasPermission && !currentUser.IsAdmin)
            {
                var actionType = request.ParentCommunityId.HasValue ? "sub-communities" : "communities";
                return Result<CommunityModel>.Failure($"You do not have permission to create {actionType}.", ResultStatus.Forbidden);
            }
        }

        // Validate parent community if sub-community
        if (request.ParentCommunityId.HasValue)
        {
            var parentExists = await dbContext.TblCommunities
                .AnyAsync(c => c.CommunityId == request.ParentCommunityId.Value && !c.IsDeleted, cancellationToken);

            if (!parentExists)
            {
                return Result<CommunityModel>.Failure("The selected parent community does not exist.", ResultStatus.NotFound);
            }
        }

        // Resolve owner: use authenticated user or fallback to first available active user (e.g., admin mock data)
        int ownerId;
        if (currentUser.UserId.HasValue)
        {
            ownerId = currentUser.UserId.Value;
        }
        else
        {
            var fallbackUser = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted, cancellationToken);
            if (fallbackUser is null)
            {
                return Result<CommunityModel>.Failure("Default system user not found.", ResultStatus.SystemError);
            }
            ownerId = fallbackUser.UserId;
        }

        var rawSlug = string.IsNullOrWhiteSpace(request.Slug) ? trimmedName.ToLowerInvariant().Replace(" ", "-") : request.Slug.ToLowerInvariant();
        var slug = rawSlug;
        var slugCounter = 1;
        while (await dbContext.TblCommunities.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            slug = $"{rawSlug}-{slugCounter++}";
        }

        var community = new TblCommunity
        {
            Name = trimmedName,
            Slug = slug,
            Description = request.Description,
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl,
            Visibility = string.IsNullOrWhiteSpace(request.Visibility) ? "PUBLIC" : request.Visibility,
            JoinPolicy = string.IsNullOrWhiteSpace(request.JoinPolicy) ? "INSTANT" : request.JoinPolicy,
            ParentCommunityId = request.ParentCommunityId,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TblCommunities.Add(community);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.TblCommunityMembers.Add(new TblCommunityMember
        {
            CommunityId = community.CommunityId,
            UserId = ownerId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetCommunityByIdAsync(community.CommunityId, cancellationToken);
    }

    public async Task<Result> JoinCommunityAsync(int communityId, CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null) return Result.Failure("Unauthorized", ResultStatus.Unauthorized);

        var community = await dbContext.TblCommunities.FindAsync([communityId], cancellationToken);
        if (community is null || community.IsDeleted) return Result.Failure("Community not found.", ResultStatus.NotFound);

        var existing = await dbContext.TblCommunityMembers
            .FirstOrDefaultAsync(m => m.CommunityId == communityId && m.UserId == currentUser.UserId.Value, cancellationToken);

        if (existing is not null)
        {
            if (!existing.IsDeleted) return Result.Failure("Already a member of this community.", ResultStatus.Conflict);
            existing.IsDeleted = false;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            dbContext.TblCommunityMembers.Add(new TblCommunityMember
            {
                CommunityId = communityId,
                UserId = currentUser.UserId.Value,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success("Joined community successfully.");
    }

    public async Task<Result<CommunityModel>> UpdateCommunityAsync(int communityId, EditCommunityRequestModel request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CommunityModel>.Failure("Name cannot be empty.", ResultStatus.ValidationError);
        }

        var community = await dbContext.TblCommunities
            .Include(c => c.Owner)
            .Include(c => c.ParentCommunity)
            .Include(c => c.TblCommunityMembers)
            .Include(c => c.TblPosts)
            .Include(c => c.TblCommunityRatings)
            .FirstOrDefaultAsync(c => c.CommunityId == communityId && !c.IsDeleted, cancellationToken);

        if (community is null)
        {
            return Result<CommunityModel>.Failure("Community not found.", ResultStatus.NotFound);
        }

        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToUpper();

        // Duplicate name verification (excluding current entity)
        var nameConflict = await dbContext.TblCommunities
            .AnyAsync(c => c.CommunityId != communityId && !c.IsDeleted && c.Name.ToUpper() == normalizedName, cancellationToken);

        if (nameConflict)
        {
            var isSub = community.ParentCommunityId.HasValue;
            var msg = isSub
                ? "This sub-community name is already exist!"
                : "This community name is already exist!";
            return Result<CommunityModel>.Failure(msg, ResultStatus.Conflict);
        }

        // Determine editor id
        int editorId;
        if (currentUser.UserId.HasValue)
        {
            editorId = currentUser.UserId.Value;
        }
        else
        {
            var fallbackUser = await dbContext.TblUsers.FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted, cancellationToken);
            editorId = fallbackUser?.UserId ?? community.OwnerId;
        }

        var targetType = community.ParentCommunityId.HasValue ? "SUB_COMMUNITY" : "COMMUNITY";
        var trimmedDesc = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        // Track Name change audit
        if (!string.Equals(community.Name, trimmedName, StringComparison.Ordinal))
        {
            dbContext.TblCommunityAuditLogs.Add(new TblCommunityAuditLog
            {
                CommunityId = communityId,
                TargetType = targetType,
                FieldChanged = "Name",
                OldValue = community.Name,
                NewValue = trimmedName,
                EditorId = editorId,
                CreatedAt = DateTime.UtcNow
            });

            community.Name = trimmedName;

            // Update slug if name changed
            var rawSlug = trimmedName.ToLowerInvariant().Replace(" ", "-");
            var slug = rawSlug;
            var slugCounter = 1;
            while (await dbContext.TblCommunities.AnyAsync(c => c.CommunityId != communityId && c.Slug == slug, cancellationToken))
            {
                slug = $"{rawSlug}-{slugCounter++}";
            }
            community.Slug = slug;
        }

        // Track Description change audit
        var currentDesc = community.Description;
        if (!string.Equals(currentDesc, trimmedDesc, StringComparison.Ordinal))
        {
            dbContext.TblCommunityAuditLogs.Add(new TblCommunityAuditLog
            {
                CommunityId = communityId,
                TargetType = targetType,
                FieldChanged = "Description",
                OldValue = currentDesc,
                NewValue = trimmedDesc,
                EditorId = editorId,
                CreatedAt = DateTime.UtcNow
            });

            community.Description = trimmedDesc;
        }

        community.UpdatedAt = DateTime.UtcNow;
        community.UpdatedBy = editorId;

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedModel = new CommunityModel(
            community.CommunityId,
            community.Name,
            community.Slug,
            community.Description,
            community.AvatarUrl,
            community.BannerUrl,
            community.Visibility,
            community.JoinPolicy,
            community.TblCommunityMembers.Count(m => !m.IsDeleted),
            community.TblPosts.Count(p => !p.IsDeleted),
            community.TblCommunityRatings.Any() ? (double)community.TblCommunityRatings.Average(r => r.Score) : 5.0,
            community.OwnerId,
            community.Owner != null ? community.Owner.DisplayName : "Admin",
            community.CreatedAt,
            community.ParentCommunityId,
            community.ParentCommunity != null ? community.ParentCommunity.Name : null,
            currentUser.UserId.HasValue && community.TblCommunityMembers.Any(m => m.UserId == currentUser.UserId.Value && !m.IsDeleted));

        return Result<CommunityModel>.Success(updatedModel, "Updated successfully.");
    }

    public async Task<Result<IReadOnlyList<CommunityAuditModel>>> GetCommunityAuditsAsync(int communityId, CancellationToken cancellationToken = default)
    {
        var logs = await dbContext.TblCommunityAuditLogs
            .Include(a => a.Editor)
            .Where(a => a.CommunityId == communityId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new CommunityAuditModel(
                a.AuditId,
                a.CommunityId,
                a.TargetType,
                a.FieldChanged,
                a.OldValue,
                a.NewValue,
                a.EditorId,
                a.Editor != null ? a.Editor.DisplayName : "Unknown",
                a.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CommunityAuditModel>>.Success(logs);
    }

    public async Task<Result<IReadOnlyList<CommunityModel>>> GetJoinedCommunitiesAsync(int userId, int take = 10, CancellationToken cancellationToken = default)
    {
        var joinedCommunityIds = await dbContext.TblCommunityMembers
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.CommunityId)
            .ToListAsync(cancellationToken);

        if (!joinedCommunityIds.Any()) return Result<IReadOnlyList<CommunityModel>>.Success([]);

        var list = await dbContext.TblCommunities
            .Where(c => joinedCommunityIds.Contains(c.CommunityId) && !c.IsDeleted)
            .Take(take)
            .Select(c => new CommunityModel(
                c.CommunityId,
                c.Name,
                c.Slug,
                c.Description,
                c.AvatarUrl,
                c.BannerUrl,
                c.Visibility,
                c.JoinPolicy,
                c.TblCommunityMembers.Count(m => !m.IsDeleted),
                c.TblPosts.Count(p => !p.IsDeleted),
                c.TblCommunityRatings.Any() ? (double)c.TblCommunityRatings.Average(r => r.Score) : 5.0,
                c.OwnerId,
                c.Owner != null ? c.Owner.DisplayName : "Admin",
                c.CreatedAt,
                c.ParentCommunityId,
                c.ParentCommunity != null ? c.ParentCommunity.Name : null,
                true))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CommunityModel>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<CommunityModel>>> GetRecommendedCommunitiesAsync(int userId, int take = 6, CancellationToken cancellationToken = default)
    {
        var joinedCommunityIds = await dbContext.TblCommunityMembers
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.CommunityId)
            .ToListAsync(cancellationToken);

        var list = await dbContext.TblCommunities
            .Where(c => !joinedCommunityIds.Contains(c.CommunityId) && c.Visibility == "PUBLIC" && !c.IsDeleted)
            .OrderByDescending(c => c.TblCommunityMembers.Count(m => !m.IsDeleted))
            .Take(take)
            .Select(c => new CommunityModel(
                c.CommunityId,
                c.Name,
                c.Slug,
                c.Description,
                c.AvatarUrl,
                c.BannerUrl,
                c.Visibility,
                c.JoinPolicy,
                c.TblCommunityMembers.Count(m => !m.IsDeleted),
                c.TblPosts.Count(p => !p.IsDeleted),
                c.TblCommunityRatings.Any() ? (double)c.TblCommunityRatings.Average(r => r.Score) : 5.0,
                c.OwnerId,
                c.Owner != null ? c.Owner.DisplayName : "Admin",
                c.CreatedAt,
                c.ParentCommunityId,
                c.ParentCommunity != null ? c.ParentCommunity.Name : null,
                false))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<CommunityModel>>.Success(list);
    }

    /// <summary>
    /// Lightweight listing used by aggregate views (e.g. the user dashboard).
    /// Skips the per-row correlated subqueries that <see cref="GetCommunitiesAsync"/> needs
    /// for the card UI - top children, member avatars and luminary role counts - which keeps
    /// this a single cheap scan instead of N subqueries per community.
    /// </summary>
    public async Task<Result<IReadOnlyList<CommunityModel>>> GetCommunityDirectoryAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.UserId;

        var rawList = await dbContext.TblCommunities
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .Select(c => new
            {
                c.CommunityId,
                c.Name,
                c.Slug,
                c.Description,
                c.AvatarUrl,
                c.BannerUrl,
                c.Visibility,
                c.JoinPolicy,
                MemberCount = c.TblCommunityMembers.Count(m => !m.IsDeleted),
                PostCount = c.TblPosts.Count(p => !p.IsDeleted),
                AverageRating = c.TblCommunityRatings.Any() ? (double)c.TblCommunityRatings.Average(r => r.Score) : 5.0,
                c.OwnerId,
                OwnerName = c.Owner != null ? c.Owner.DisplayName : "Admin",
                c.CreatedAt,
                c.ParentCommunityId,
                ParentCommunityName = c.ParentCommunity != null ? c.ParentCommunity.Name : null,
                SubCommunityCount = c.InverseParentCommunity.Count(sc => !sc.IsDeleted),
                GroupCount = c.TblGroups.Count(g => !g.IsDeleted),
                IsJoined = userId.HasValue && c.TblCommunityMembers.Any(m => !m.IsDeleted && m.UserId == userId.Value),
            })
            .ToListAsync(cancellationToken);

        var list = rawList.Select(c => new CommunityModel(
            c.CommunityId,
            c.Name,
            c.Slug,
            c.Description,
            c.AvatarUrl,
            c.BannerUrl,
            c.Visibility,
            c.JoinPolicy,
            c.MemberCount,
            c.PostCount,
            c.AverageRating,
            c.OwnerId,
            c.OwnerName,
            c.CreatedAt,
            c.ParentCommunityId,
            c.ParentCommunityName,
            c.IsJoined,
            c.SubCommunityCount,
            c.GroupCount,
            CategoryName: c.ParentCommunityId.HasValue
                ? (c.ParentCommunityName != null ? c.ParentCommunityName.ToUpperInvariant() : "SUB-COMMUNITY")
                : "CAPITAL & SYNDICATE"))
            .ToList();

        return Result<IReadOnlyList<CommunityModel>>.Success(list);
    }
}
