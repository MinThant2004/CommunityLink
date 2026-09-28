-- ===================================================================
-- CommunityLink Database Script
-- File: 20260925_Community_Card_Enhancements.sql
-- Description: 
--   1. Ensures BannerUrl & AvatarUrl columns exist on TblCommunity & TblGroup
--   2. Ensures DOMAIN_PRO & PUBLIC_FIGURE roles exist in TblRole
--   3. Updates existing communities with executive high-res fallback banners
--   4. Seeds sample Sub-Community and child Working Groups matching the mockup
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
-- ===================================================================
USE [CommunityLink];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- 1. Ensure BannerUrl and AvatarUrl exist on TblCommunity
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TblCommunity') AND name = 'BannerUrl')
    BEGIN
        ALTER TABLE dbo.TblCommunity ADD BannerUrl NVARCHAR(1000) NULL;
        PRINT '[OK] Added BannerUrl to dbo.TblCommunity';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TblCommunity') AND name = 'AvatarUrl')
    BEGIN
        ALTER TABLE dbo.TblCommunity ADD AvatarUrl NVARCHAR(1000) NULL;
        PRINT '[OK] Added AvatarUrl to dbo.TblCommunity';
    END

    -- 2. Ensure BannerUrl and AvatarUrl exist on TblGroup
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TblGroup') AND name = 'BannerUrl')
    BEGIN
        ALTER TABLE dbo.TblGroup ADD BannerUrl NVARCHAR(1000) NULL;
        PRINT '[OK] Added BannerUrl to dbo.TblGroup';
    END

    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.TblGroup') AND name = 'AvatarUrl')
    BEGIN
        ALTER TABLE dbo.TblGroup ADD AvatarUrl NVARCHAR(1000) NULL;
        PRINT '[OK] Added AvatarUrl to dbo.TblGroup';
    END

    -- 3. Ensure DOMAIN_PRO and PUBLIC_FIGURE system roles exist
    IF NOT EXISTS (SELECT 1 FROM dbo.TblRole WHERE RoleCode = 'DOMAIN_PRO')
    BEGIN
        INSERT INTO dbo.TblRole (RoleCode, RoleName, Description, IsSystemRole, CreatedAt, IsDeleted)
        VALUES ('DOMAIN_PRO', 'Domain Professional', 'Industry specialist / domain authority with verified credentials', 1, SYSUTCDATETIME(), 0);
        PRINT '[OK] Created DOMAIN_PRO role';
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.TblRole WHERE RoleCode = 'PUBLIC_FIGURE')
    BEGIN
        INSERT INTO dbo.TblRole (RoleCode, RoleName, Description, IsSystemRole, CreatedAt, IsDeleted)
        VALUES ('PUBLIC_FIGURE', 'Public Figure', 'Recognized executive, founder, or public luminary', 1, SYSUTCDATETIME(), 0);
        PRINT '[OK] Created PUBLIC_FIGURE role';
    END

    -- 4. Provide high-res fallback executive banners for existing communities and groups without images
    UPDATE dbo.TblCommunity
    SET BannerUrl = 'https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1200&q=80'
    WHERE BannerUrl IS NULL OR BannerUrl = '';

    UPDATE dbo.TblGroup
    SET BannerUrl = COALESCE(AvatarUrl, 'https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1200&q=80')
    WHERE BannerUrl IS NULL OR BannerUrl = '';

    UPDATE dbo.TblGroup
    SET AvatarUrl = BannerUrl
    WHERE AvatarUrl IS NULL OR AvatarUrl = '';

    -- 5. Seed realistic sample parent community, sub-community, and working groups if needed
    DECLARE @DefaultOwnerId INT = (SELECT TOP 1 UserId FROM dbo.TblUser WHERE IsDeleted = 0 ORDER BY UserId ASC);

    IF @DefaultOwnerId IS NOT NULL
    BEGIN
        -- Parent Community: Capital & Syndicate Hub
        DECLARE @ParentCommunityId INT;
        SELECT @ParentCommunityId = CommunityId 
        FROM dbo.TblCommunity 
        WHERE Slug = 'capital-syndicate-hub' AND IsDeleted = 0;

        IF @ParentCommunityId IS NULL
        BEGIN
            INSERT INTO dbo.TblCommunity (
                Name, Slug, Description, BannerUrl, AvatarUrl, Visibility, JoinPolicy, 
                MemberCount, PostCount, AverageRating, RatingCount, OwnerId, CreatedAt, IsDeleted
            )
            VALUES (
                'Capital & Syndicate Ecosystem',
                'capital-syndicate-hub',
                'Curated executive network of verified fund managers, venture scouts, and institutional asset deployers.',
                'https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=1200&q=80',
                'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=200&q=80',
                'PUBLIC', 'INSTANT', 3420, 142, 4.95, 88, @DefaultOwnerId, SYSUTCDATETIME(), 0
            );
            SET @ParentCommunityId = SCOPE_IDENTITY();
            PRINT '[OK] Seeded Parent Community: Capital & Syndicate Ecosystem';
        END

        -- Sub-Community: AI Founders & Angels
        DECLARE @SubCommunityId INT;
        SELECT @SubCommunityId = CommunityId 
        FROM dbo.TblCommunity 
        WHERE Slug = 'ai-founders-and-angels' AND IsDeleted = 0;

        IF @SubCommunityId IS NULL
        BEGIN
            INSERT INTO dbo.TblCommunity (
                ParentCommunityId, Name, Slug, Description, BannerUrl, AvatarUrl, Visibility, JoinPolicy, 
                MemberCount, PostCount, AverageRating, RatingCount, OwnerId, CreatedAt, IsDeleted
            )
            VALUES (
                @ParentCommunityId,
                'AI Founders & Angels',
                'ai-founders-and-angels',
                'Curated ecosystem of seed to Series B founders and active check-writers. Focuses on founder dilution governance, cap-table defense against hyper-dilutive compute rounds, and institutional syndication.',
                'https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1200&q=80',
                'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=200&q=80',
                'PUBLIC', 'INSTANT', 1280, 76, 4.92, 42, @DefaultOwnerId, SYSUTCDATETIME(), 0
            );
            SET @SubCommunityId = SCOPE_IDENTITY();
            PRINT '[OK] Seeded Sub-Community: AI Founders & Angels';
        END

        -- Ensure Owner is joined as member of both
        IF NOT EXISTS (SELECT 1 FROM dbo.TblCommunityMember WHERE CommunityId = @ParentCommunityId AND UserId = @DefaultOwnerId)
        BEGIN
            INSERT INTO dbo.TblCommunityMember (CommunityId, UserId, Role, IsMuted, JoinedAt, CreatedAt, IsDeleted)
            VALUES (@ParentCommunityId, @DefaultOwnerId, 'OWNER', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
        END

        IF NOT EXISTS (SELECT 1 FROM dbo.TblCommunityMember WHERE CommunityId = @SubCommunityId AND UserId = @DefaultOwnerId)
        BEGIN
            INSERT INTO dbo.TblCommunityMember (CommunityId, UserId, Role, IsMuted, JoinedAt, CreatedAt, IsDeleted)
            VALUES (@SubCommunityId, @DefaultOwnerId, 'OWNER', 0, SYSUTCDATETIME(), SYSUTCDATETIME(), 0);
        END

        -- Seed the 3 Working Groups under the Sub-Community
        -- Group 1: Series A Pitch Decks Tear-downs
        IF NOT EXISTS (SELECT 1 FROM dbo.TblGroup WHERE Slug = 'series-a-pitch-decks-tear-downs' AND SubCommunityId = @SubCommunityId AND IsDeleted = 0)
        BEGIN
            INSERT INTO dbo.TblGroup (
                SubCommunityId, CreatorId, Name, Slug, Description, BannerUrl,
                Visibility, JoinPolicy, MemberCount, PostCount, CreatedAt, IsDeleted, GroupType, JoinFeeLinkDrops
            )
            VALUES (
                @SubCommunityId, @DefaultOwnerId, 'Series A Pitch Decks Tear-downs', 'series-a-pitch-decks-tear-downs',
                'Bi-weekly live tear-downs and investor-readiness scorecards for enterprise AI seed ventures.',
                'https://images.unsplash.com/photo-1460925895917-afdab827c52f?auto=format&fit=crop&w=800&q=80',
                'PUBLIC', 'INSTANT', 840, 34, SYSUTCDATETIME(), 0, 'FREE', 0
            );
            PRINT '[OK] Seeded Group: Series A Pitch Decks Tear-downs';
        END

        -- Group 2: Compute Barter Guild
        IF NOT EXISTS (SELECT 1 FROM dbo.TblGroup WHERE Slug = 'compute-barter-guild' AND SubCommunityId = @SubCommunityId AND IsDeleted = 0)
        BEGIN
            INSERT INTO dbo.TblGroup (
                SubCommunityId, CreatorId, Name, Slug, Description, BannerUrl,
                Visibility, JoinPolicy, MemberCount, PostCount, CreatedAt, IsDeleted, GroupType, JoinFeeLinkDrops
            )
            VALUES (
                @SubCommunityId, @DefaultOwnerId, 'Compute Barter Guild', 'compute-barter-guild',
                'Decentralized liquidity pool for H100/A100 cluster reservations, credits trading, and spot swaps.',
                'https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=800&q=80',
                'PUBLIC', 'INSTANT', 620, 28, SYSUTCDATETIME(), 0, 'FREE', 0
            );
            PRINT '[OK] Seeded Group: Compute Barter Guild';
        END

        -- Group 3: YC & Thiel Fellows Syndicate
        IF NOT EXISTS (SELECT 1 FROM dbo.TblGroup WHERE Slug = 'yc-thiel-fellows-syndicate' AND SubCommunityId = @SubCommunityId AND IsDeleted = 0)
        BEGIN
            INSERT INTO dbo.TblGroup (
                SubCommunityId, CreatorId, Name, Slug, Description, BannerUrl,
                Visibility, JoinPolicy, MemberCount, PostCount, CreatedAt, IsDeleted, GroupType, JoinFeeLinkDrops
            )
            VALUES (
                @SubCommunityId, @DefaultOwnerId, 'YC & Thiel Fellows Syndicate', 'yc-thiel-fellows-syndicate',
                'Private allocations, secondary liquidity rights, and co-investment syndicates reserved for alumni.',
                'https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=800&q=80',
                'PUBLIC', 'INSTANT', 490, 19, SYSUTCDATETIME(), 0, 'FREE', 0
            );
            PRINT '[OK] Seeded Group: YC & Thiel Fellows Syndicate';
        END
    END

    COMMIT TRANSACTION;
    PRINT '=========================================================';
    PRINT '[SUCCESS] Community card enhancements script applied successfully!';
    PRINT '=========================================================';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '[ERROR] Error encountered while running script:';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH;
GO
