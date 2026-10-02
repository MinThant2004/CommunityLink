-- =====================================================================================
-- CommunityLink Database Script: Full Reset & Realistic Sample Data Seed
-- Target Database: SQL Server (CommunityLink)
-- Description:
--   1. Safely wipes all test/dummy data across all tables with foreign keys preserved.
--   2. Reseeds all identity counters to 0.
--   3. Seeds 5 exact designated roles (ADMIN, USER, DOMAIN_PROFESSIONAL, PUBLIC_FIGURE, MODERATOR).
--   4. Seeds permissions & role-permission mappings.
--   5. Seeds 40+ realistic Myanmar (MM) & project team users (Min Thant, May Kyawt Khaing,
--      Kaung, Aung Kyaw, Thiri San, etc.) with real-style Gmail addresses.
--   6. Sets all user and admin account passwords to '123456789' (BCrypt hash).
--   7. Seeds 20+ Communities, 20+ Groups, 30+ Posts with Myanmar tech content & code snippets,
--      Polls, Poll Options, Votes, Comments, Likes, Shares, Saved Posts.
--   8. Seeds LinkDrop Wallets, Packages, Payment Methods, Purchase Records, Proofs, Transactions.
--   9. Seeds Chat Conversations, 1-on-1 Messages, Stickers, Chat Groups, Messages, Reactions.
--  10. Seeds Verification Audits, Subscriptions, Platform Settings, and Audit Logs.
-- =====================================================================================

USE [CommunityLink];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

BEGIN TRY
    PRINT '>>> Step 1: Disabling foreign key constraints for safe wipe...';
    EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

    PRINT '>>> Step 2: Deleting existing records from all application tables...';
    DELETE FROM dbo.TblChatMessageReaction;
    DELETE FROM dbo.TblChatMessageUserState;
    DELETE FROM dbo.TblChatMessage;
    DELETE FROM dbo.TblConversation;

    DELETE FROM dbo.TblChatGroupMessageReaction;
    DELETE FROM dbo.TblChatGroupMessageUserState;
    DELETE FROM dbo.TblChatGroupMessage;
    DELETE FROM dbo.TblChatGroupMember;
    DELETE FROM dbo.TblChatGroupPaymentTransaction;
    DELETE FROM dbo.TblChatGroup;

    IF OBJECT_ID('dbo.TblGroupChatMessage', 'U') IS NOT NULL DELETE FROM dbo.TblGroupChatMessage;
    IF OBJECT_ID('dbo.TblGroupChatRoom', 'U') IS NOT NULL DELETE FROM dbo.TblGroupChatRoom;

    DELETE FROM dbo.TblCreatorPayoutRequest;
    DELETE FROM dbo.TblCreatorChatSetting;
    DELETE FROM dbo.TblPrivateChatPaymentTransaction;

    DELETE FROM dbo.TblLinkDropPurchaseProof;
    DELETE FROM dbo.TblLinkDropPurchase;
    DELETE FROM dbo.TblLinkDropTransaction;
    DELETE FROM dbo.TblLinkDropWallet;
    DELETE FROM dbo.TblLinkDropPackage;
    DELETE FROM dbo.TblPaymentMethod;

    DELETE FROM dbo.TblPollVote;
    DELETE FROM dbo.TblPollOption;
    DELETE FROM dbo.TblPoll;

    DELETE FROM dbo.TblSavedPost;
    DELETE FROM dbo.TblPostShare;
    DELETE FROM dbo.TblPostLike;
    DELETE FROM dbo.TblPostImage;
    DELETE FROM dbo.TblComment;
    DELETE FROM dbo.TblPost;

    DELETE FROM dbo.TblGroupRating;
    DELETE FROM dbo.TblGroupJoinRequest;
    DELETE FROM dbo.TblGroupMember;
    DELETE FROM dbo.TblGroup;

    DELETE FROM dbo.TblCommunityRating;
    DELETE FROM dbo.TblCommunityJoinRequest;
    DELETE FROM dbo.TblCommunityMember;
    IF OBJECT_ID('dbo.TblCommunityAuditLog', 'U') IS NOT NULL DELETE FROM dbo.TblCommunityAuditLog;
    DELETE FROM dbo.TblCommunity;

    DELETE FROM dbo.TblUserActivity;
    DELETE FROM dbo.TblUserFollow;
    DELETE FROM dbo.TblSavedAccount;
    DELETE FROM dbo.TblUserRating;
    DELETE FROM dbo.TblSkillEndorsement;
    DELETE FROM dbo.TblUserSkill;
    DELETE FROM dbo.TblNotification;
    DELETE FROM dbo.TblUserVerificationAudit;
    DELETE FROM dbo.TblIdentityVerification;
    DELETE FROM dbo.TblUserSubscription;
    DELETE FROM dbo.TblSubscriptionPlan;

    DELETE FROM dbo.TblAdminInvite;
    DELETE FROM dbo.TblAdminRole;
    DELETE FROM dbo.TblAdmin;

    DELETE FROM dbo.TblUserRole;
    DELETE FROM dbo.TblRolePermission;
    DELETE FROM dbo.TblRole;
    DELETE FROM dbo.TblUser;
    DELETE FROM dbo.TblAuditLog;
    DELETE FROM dbo.TblPlatformSetting;

    PRINT '>>> Step 3: Reseeding identity counters...';
    -- Safe identity resets (check if table has identity first)
    DECLARE @reseedSql NVARCHAR(MAX) = N'
    DECLARE @tbl NVARCHAR(256), @sql NVARCHAR(MAX);
    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT t.name 
        FROM sys.tables t 
        JOIN sys.columns c ON t.object_id = c.object_id 
        WHERE c.is_identity = 1;
    OPEN cur;
    FETCH NEXT FROM cur INTO @tbl;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @sql = N''DBCC CHECKIDENT (''''dbo.'' + @tbl + '''''', RESEED, 0) WITH NO_INFOMSGS;'';
        EXEC sp_executesql @sql;
        FETCH NEXT FROM cur INTO @tbl;
    END
    CLOSE cur;
    DEALLOCATE cur;';
    EXEC sp_executesql @reseedSql;

    PRINT '>>> Step 4: Re-enabling foreign key constraints...';
    EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';

    PRINT '>>> Database clean-up completed successfully.';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR in Step 1-4: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO

-- =====================================================================================
-- SEED DATA SECTION
-- =====================================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();
    -- BCrypt hash for password: '123456789' (Work factor 12)
    DECLARE @DefaultPasswordHash NVARCHAR(256) = N'$2a$12$Bz2R1OEXM64NeFAXB7sne.DZeWzir36LuBNn0LhBTFN6AVelBHTJe';

    PRINT '>>> Step 5: Seeding 5 Roles...';
    -- 1. System Admin (Default cannot delete)
    -- 2. User (Default cannot delete)
    -- 3. Domain Professional
    -- 4. Public Figure
    -- 5. Admin / Moderator
    INSERT INTO dbo.TblRole (RoleCode, RoleName, Description, IsSystemRole, CreatedAt, IsDeleted)
    VALUES 
    (N'ADMIN', N'System Admin', N'Full system administrator with unrestricted control (Default)', 1, @Now, 0),
    (N'USER', N'User', N'Default standard community member (Default)', 1, @Now, 0),
    (N'DOMAIN_PROFESSIONAL', N'Domain Professional', N'Verified domain specialist, technical consultant, and advisory creator', 0, @Now, 0),
    (N'PUBLIC_FIGURE', N'Public Figure', N'Celebrity, notable creator, executive founder, or public luminary', 0, @Now, 0),
    (N'MODERATOR', N'Admin', N'Community moderation administrator and content governor', 0, @Now, 0);

    DECLARE @RoleId_Admin INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'ADMIN');
    DECLARE @RoleId_User INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'USER');
    DECLARE @RoleId_DomainPro INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'DOMAIN_PROFESSIONAL');
    DECLARE @RoleId_PublicFigure INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'PUBLIC_FIGURE');
    DECLARE @RoleId_Moderator INT = (SELECT RoleId FROM dbo.TblRole WHERE RoleCode = 'MODERATOR');

    PRINT '>>> Step 6: Seeding Permissions & Role Permissions...';
    -- Ensure all standard permissions exist
    DECLARE @Perms TABLE (Code NVARCHAR(100), Name NVARCHAR(100), Module NVARCHAR(100));
    INSERT INTO @Perms VALUES
    (N'COMMUNITY.VIEW', N'View Communities', N'Community'),
    (N'COMMUNITY.CREATE', N'Create Community', N'Community'),
    (N'SUBCOMMUNITY.CREATE', N'Create Sub-Community', N'Community'),
    (N'COMMUNITY.MANAGE', N'Manage Community Settings', N'Community'),
    (N'COMMUNITY.MODERATE', N'Moderate Communities & Join Requests', N'Moderation'),
    (N'GROUP.VIEW', N'View Groups', N'Group'),
    (N'GROUP.CREATE', N'Create Group', N'Group'),
    (N'GROUP.MANAGE', N'Manage Group Settings', N'Group'),
    (N'POST.VIEW', N'View Feed Posts', N'Post'),
    (N'POST.CREATE', N'Create Post', N'Post'),
    (N'POST.DELETE', N'Delete Post / Moderation', N'Post'),
    (N'POST.STANDALONE.CREATE', N'Create Standalone Post & Poll', N'Post'),
    (N'POLL.VIEW', N'View Polls', N'Poll'),
    (N'POLL.VOTE', N'Vote on Polls', N'Poll'),
    (N'POLL.CREATE', N'Create Polls', N'Poll'),
    (N'CHAT.ACCESS', N'Access Chat Rooms', N'Chat'),
    (N'CHAT.SEND', N'Send Chat Messages', N'Chat'),
    (N'GROUPCHAT.CREATE', N'Create Group Chat (Premium)', N'Chat'),
    (N'ADMIN.USER.VIEW', N'View Admin Users', N'Administration'),
    (N'ADMIN.USER.MANAGE', N'Manage Admin Users', N'Administration'),
    (N'ADMIN.RBAC.MANAGE', N'Manage RBAC Matrix', N'Administration'),
    (N'SYSTEMAUDIT.VIEW', N'View System Audit Logs', N'Administration'),
    (N'REPORT.VIEW', N'View Reports', N'Reports'),
    (N'CREATOR.EARNINGS', N'View Creator Earnings', N'Creator'),
    (N'CREATOR.PAYOUTS', N'Manage Creator Payouts', N'Creator');

    MERGE dbo.TblPermission AS target
    USING @Perms AS src ON target.PermissionCode = src.Code
    WHEN MATCHED THEN 
        UPDATE SET PermissionName = src.Name, Module = src.Module, Description = src.Name
    WHEN NOT MATCHED THEN 
        INSERT (PermissionCode, PermissionName, Module, Description, CreatedAt)
        VALUES (src.Code, src.Name, src.Module, src.Name, @Now);

    -- Grant ADMIN all permissions
    INSERT INTO dbo.TblRolePermission (RoleId, PermissionId, IsDeleted, CreatedAt)
    SELECT @RoleId_Admin, PermissionId, 0, @Now FROM dbo.TblPermission;

    -- Grant MODERATOR (Admin role) management & moderation permissions
    INSERT INTO dbo.TblRolePermission (RoleId, PermissionId, IsDeleted, CreatedAt)
    SELECT @RoleId_Moderator, PermissionId, 0, @Now 
    FROM dbo.TblPermission 
    WHERE PermissionCode NOT IN ('ADMIN.RBAC.MANAGE', 'ADMIN.USER.MANAGE');

    -- Grant DOMAIN_PROFESSIONAL & PUBLIC_FIGURE standard + creator + group chat permissions
    INSERT INTO dbo.TblRolePermission (RoleId, PermissionId, IsDeleted, CreatedAt)
    SELECT @RoleId_DomainPro, PermissionId, 0, @Now 
    FROM dbo.TblPermission 
    WHERE PermissionCode IN (
        'COMMUNITY.VIEW', 'COMMUNITY.CREATE', 'SUBCOMMUNITY.CREATE', 'GROUP.VIEW', 'GROUP.CREATE',
        'POST.VIEW', 'POST.CREATE', 'POST.STANDALONE.CREATE', 'POLL.VIEW', 'POLL.VOTE', 'POLL.CREATE',
        'CHAT.ACCESS', 'CHAT.SEND', 'GROUPCHAT.CREATE', 'REPORT.VIEW', 'CREATOR.EARNINGS', 'CREATOR.PAYOUTS'
    );

    INSERT INTO dbo.TblRolePermission (RoleId, PermissionId, IsDeleted, CreatedAt)
    SELECT @RoleId_PublicFigure, PermissionId, 0, @Now 
    FROM dbo.TblPermission 
    WHERE PermissionCode IN (
        'COMMUNITY.VIEW', 'COMMUNITY.CREATE', 'SUBCOMMUNITY.CREATE', 'GROUP.VIEW', 'GROUP.CREATE',
        'POST.VIEW', 'POST.CREATE', 'POST.STANDALONE.CREATE', 'POLL.VIEW', 'POLL.VOTE', 'POLL.CREATE',
        'CHAT.ACCESS', 'CHAT.SEND', 'GROUPCHAT.CREATE', 'REPORT.VIEW', 'CREATOR.EARNINGS', 'CREATOR.PAYOUTS'
    );

    -- Grant USER standard permissions
    INSERT INTO dbo.TblRolePermission (RoleId, PermissionId, IsDeleted, CreatedAt)
    SELECT @RoleId_User, PermissionId, 0, @Now 
    FROM dbo.TblPermission 
    WHERE PermissionCode IN (
        'COMMUNITY.VIEW', 'GROUP.VIEW', 'GROUP.CREATE', 'POST.VIEW', 'POST.CREATE',
        'POLL.VIEW', 'POLL.VOTE', 'CHAT.ACCESS', 'CHAT.SEND', 'REPORT.VIEW'
    );

    PRINT '>>> Step 7: Seeding 40+ Myanmar (MM) & Team Users with real-time style Gmails...';
    -- All passwords set to '123456789'
    INSERT INTO dbo.TblUser (
        UserName, NormalizedUserName, Email, NormalizedEmail, DisplayName, PasswordHash,
        AvatarUrl, Bio, IsVerified, IsActive, Headline, Pronouns, Location, AvailabilityStatus,
        ResponseSlaText, PercentileBadgeText, AverageRating, RatingCount, CreatedAt, LastLoginAt
    ) VALUES
    -- 1. Min Thant (Owner / System Admin)
    (N'minthant', N'MINTHANT', N'mgminthant2004@gmail.com', N'MGMINTHANT2004@GMAIL.COM', N'Min Thant', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80',
     N'Full-stack Software Architect & Founder of CommunityLink. Building next-gen community software.', 1, 1,
     N'Founder & Lead Architect @ CommunityLink', N'he/him', N'Yangon, Myanmar', N'Available for Mentorship',
     N'Replies within 1 hour', N'Top 1% Creator', 4.98, 86, DATEADD(DAY, -60, @Now), @Now),

    -- 2. May Kyawt Khaing (Project Partner / Domain Professional)
    (N'maykyawt', N'MAYKYAWT', N'maykyawtkhaing2842@gmail.com', N'MAYKYAWTKHAING2842@GMAIL.COM', N'May Kyawt Khaing', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80',
     N'Senior Product Designer & UI/UX Specialist. Passionate about human-centered design & developer tooling.', 1, 1,
     N'Product Design Lead & Design Systems Architect', N'she/her', N'Yangon, Myanmar', N'Available for Advisory',
     N'Replies within 2 hours', N'Top 3% Designer', 4.95, 74, DATEADD(DAY, -58, @Now), @Now),

    -- 3. Kaung Kaung / juggerkaung (Core Developer / Domain Professional)
    (N'kaungkaung', N'KAUNGKAUNG', N'juggerkaung.dev@gmail.com', N'JUGGERKAUNG.DEV@GMAIL.COM', N'Kaung Myat Han', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80',
     N'Distributed Systems & Cloud Backend Engineer. C# .NET & Microservices enthusiast.', 1, 1,
     N'Backend Lead & Cloud Engineer', N'he/him', N'Mandalay, Myanmar', N'Available for Project Collaboration',
     N'Replies within 3 hours', N'Top 5% Engineer', 4.90, 52, DATEADD(DAY, -55, @Now), @Now),

    -- 4. Aung Kyaw San (Public Figure / Tech Investor)
    (N'aungkyawsan', N'AUNGKYAWSAN', N'aungkyaw.san.angel@gmail.com', N'AUNGKYAW.SAN.ANGEL@GMAIL.COM', N'Aung Kyaw San', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80',
     N'Angel investor, startup mentor, and technology keynote speaker across Southeast Asia.', 1, 1,
     N'Managing Partner @ Golden Land Ventures', N'he/him', N'Yangon, Myanmar', N'Open for Pitch Decks',
     N'Replies within 24 hours', N'Featured Luminary', 4.96, 120, DATEADD(DAY, -50, @Now), @Now),

    -- 5. Thiri Sandar (Domain Professional / AI Researcher)
    (N'thirisandar', N'THIRISANDAR', N'thiri.sandar.ai@gmail.com', N'THIRI.SANDAR.AI@GMAIL.COM', N'Dr. Thiri Sandar', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=300&q=80',
     N'PhD in Computer Science. Machine Learning researcher working on Burmese NLP and multimodal LLMs.', 1, 1,
     N'Principal AI Scientist & Research Fellow', N'she/her', N'Naypyidaw, Myanmar', N'Available for Advisory',
     N'Replies within 4 hours', N'Top 1% AI Specialist', 4.99, 95, DATEADD(DAY, -45, @Now), @Now),

    -- 6. Zin Min Htet (Admin / Moderator)
    (N'zinminhtet', N'ZINMINHTET', N'zinminhtet.security@gmail.com', N'ZINMINHTET.SECURITY@GMAIL.COM', N'Zin Min Htet', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=300&q=80',
     N'Cybersecurity researcher and community safety administrator. Bug bounty hunter.', 1, 1,
     N'Security Lead & Community Compliance', N'he/him', N'Yangon, Myanmar', N'Monitoring Reports',
     N'Replies within 30 mins', N'Security Verified', 4.88, 38, DATEADD(DAY, -42, @Now), @Now),

    -- 7. Hnin Shwe Yi (User / Content Creator)
    (N'hninshweyi', N'HNINSHWEYI', N'hninshweyi.creatives@gmail.com', N'HNINSHWEYI.CREATIVES@GMAIL.COM', N'Hnin Shwe Yi', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1534751516642-a171edd2521b?auto=format&fit=crop&w=300&q=80',
     N'Visual storyteller, travel photographer, and modern lifestyle creator.', 1, 1,
     N'Creative Producer & Content Strategist', N'she/her', N'Bagan, Myanmar', N'Available for Projects',
     N'Replies within 5 hours', N'Rising Creator', 4.82, 29, DATEADD(DAY, -40, @Now), @Now),

    -- 8. Kyaw Zayar Lynn (User / Mobile Dev)
    (N'kyawzayar', N'KYAWZAYAR', N'kyawzayar.lynn.dev@gmail.com', N'KYAWZAYAR.LYNN.DEV@GMAIL.COM', N'Kyaw Zayar Lynn', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=300&q=80',
     N'Flutter & iOS engineer building cross-platform fintech apps. Open source contributor.', 0, 1,
     N'Senior Mobile Application Engineer', N'he/him', N'Yangon, Myanmar', N'Open for Work',
     N'Replies within 6 hours', N'Top Contributor', 4.79, 21, DATEADD(DAY, -38, @Now), @Now),

    -- 9. Su Myat Noe (User / FinTech Specialist)
    (N'sumyatnoe', N'SUMYATNOE', N'sumyatnoe.fin@gmail.com', N'SUMYATNOE.FIN@GMAIL.COM', N'Su Myat Noe', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=300&q=80',
     N'Financial Analyst & Payment Gateway Integration Consultant.', 1, 1,
     N'FinTech Solutions Specialist', N'she/her', N'Yangon, Myanmar', N'Available for Consultations',
     N'Replies within 4 hours', N'Finance Certified', 4.85, 34, DATEADD(DAY, -35, @Now), @Now),

    -- 10. Ye Yint Aung (User / DevOps Specialist)
    (N'yeyintaung', N'YEYINTAUNG', N'yeyint.aung.cloud@gmail.com', N'YEYINT.AUNG.CLOUD@GMAIL.COM', N'Ye Yint Aung', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=300&q=80',
     N'Kubernetes, Terraform, and CI/CD automation advocate. 24/7 reliability engineer.', 0, 1,
     N'Site Reliability Engineer @ CloudTech', N'he/him', N'Taunggyi, Myanmar', N'Available for Advisory',
     N'Replies within 3 hours', N'DevOps Guru', 4.86, 27, DATEADD(DAY, -32, @Now), @Now),

    -- 11. May Thu Zin (User / Community Lead)
    (N'maythuzin', N'MAYTHUZIN', N'maythu.zin.lead@gmail.com', N'MAYTHU.ZIN.LEAD@GMAIL.COM', N'May Thu Zin', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=300&q=80',
     N'Organizing local Myanmar tech hackathons, women in tech meetups, and developer circles.', 1, 1,
     N'Community Director @ TechMyanmar', N'she/her', N'Yangon, Myanmar', N'Connecting Leaders',
     N'Replies within 2 hours', N'Community Champion', 4.94, 63, DATEADD(DAY, -30, @Now), @Now),

    -- 12. Pyae Sone Phyo (User / Frontend Dev)
    (N'pyaesone', N'PYAESONE', N'pyaesone.phyo.code@gmail.com', N'PYAESONE.PHYO.CODE@GMAIL.COM', N'Pyae Sone Phyo', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?auto=format&fit=crop&w=300&q=80',
     N'React, Next.js, and TailwindCSS artisan creating slick, micro-animated web interfaces.', 0, 1,
     N'Frontend Creative Engineer', N'he/him', N'Mandalay, Myanmar', N'Available for Freelance',
     N'Replies within 4 hours', N'Code Artisan', 4.75, 18, DATEADD(DAY, -28, @Now), @Now),

    -- 13. Khin Myat Noe (User / Data Scientist)
    (N'khinmyatnoe', N'KHINMYATNOE', N'khinmyat.noe.data@gmail.com', N'KHINMYAT.NOE.DATA@GMAIL.COM', N'Khin Myat Noe', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1531746020798-e6953c6e8e04?auto=format&fit=crop&w=300&q=80',
     N'Big Data modeling, Python Pandas, and statistical forecasting specialist.', 0, 1,
     N'Senior Quantitative Data Analyst', N'she/her', N'Yangon, Myanmar', N'Available for Advisory',
     N'Replies within 8 hours', N'Data Master', 4.80, 22, DATEADD(DAY, -25, @Now), @Now),

    -- 14. Nay Lin Aung (User / Blockchain Dev)
    (N'naylinaung', N'NAYLINAUNG', N'naylin.aung.web3@gmail.com', N'NAYLIN.AUNG.WEB3@GMAIL.COM', N'Nay Lin Aung', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1519345182560-3f2917c472ef?auto=format&fit=crop&w=300&q=80',
     N'Solidity, EVM smart contracts, and decentralized identity protocol researcher.', 0, 1,
     N'Web3 Protocol Architect', N'he/him', N'Yangon, Myanmar', N'Consultations Open',
     N'Replies within 6 hours', N'Web3 Pioneer', 4.72, 16, DATEADD(DAY, -24, @Now), @Now),

    -- 15. Ei Mon Kyaw (User / UI Designer)
    (N'eimonkyaw', N'EIMONKYAW', N'eimon.kyaw.design@gmail.com', N'EIMON.KYAW.DESIGN@GMAIL.COM', N'Ei Mon Kyaw', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=300&q=80',
     N'Figma design system builder, color palette fanatic, and accessible UI champion.', 0, 1,
     N'Lead Product Designer', N'she/her', N'Mawlamyine, Myanmar', N'Open for Design Projects',
     N'Replies within 2 hours', N'Design Ace', 4.88, 31, DATEADD(DAY, -22, @Now), @Now),

    -- 16. Myo Min Thu (User / Game Developer)
    (N'myominthu', N'MYOMINTHU', N'myomin.thu.games@gmail.com', N'MYOMIN.THU.GAMES@GMAIL.COM', N'Myo Min Thu', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1501196354995-cbb51c65aaea?auto=format&fit=crop&w=300&q=80',
     N'Unity 3D and Unreal Engine 5 developer. Building mobile battle royales & 2D platformers.', 0, 1,
     N'Indie Game Studio Founder', N'he/him', N'Yangon, Myanmar', N'Playtesting New Games',
     N'Replies within 12 hours', N'Game Creator', 4.70, 14, DATEADD(DAY, -20, @Now), @Now),

    -- 17. Thet Htar Swe (User / English Educator)
    (N'thettharswe', N'THETTHARSWE', N'thetthar.swe.edu@gmail.com', N'THETTHAR.SWE.EDU@GMAIL.COM', N'Thet Htar Swe', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=300&q=80',
     N'IELTS 8.5 band coach & Professional English communication mentor for software developers.', 1, 1,
     N'Business English Coach & Author', N'she/her', N'Yangon, Myanmar', N'Booking Speaking Classes',
     N'Replies within 3 hours', N'Top Educator', 4.95, 59, DATEADD(DAY, -18, @Now), @Now),

    -- 18. Tun Lin Oo (User / System Admin)
    (N'tunlinoo', N'TUNLINOO', N'tunlin.oo.sys@gmail.com', N'TUNLIN.OO.SYS@GMAIL.COM', N'Tun Lin Oo', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=crop&w=300&q=80',
     N'Linux systems administration, enterprise networking, and database replication administrator.', 0, 1,
     N'Enterprise Infrastructure Engineer', N'he/him', N'Pyin Oo Lwin, Myanmar', N'Available for Support',
     N'Replies within 4 hours', N'Infra Specialist', 4.81, 19, DATEADD(DAY, -16, @Now), @Now),

    -- 19. Yoon Me Me (User / Marketing Strategist)
    (N'yoonmeme', N'YOONMEME', N'yoonmeme.marketing@gmail.com', N'YOONMEME.MARKETING@GMAIL.COM', N'Yoon Me Me', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1567532939604-b6b5b0db2604?auto=format&fit=crop&w=300&q=80',
     N'Growth hacking, conversion rate optimization (CRO), and social marketing analytics.', 0, 1,
     N'Growth Marketing Specialist', N'she/her', N'Yangon, Myanmar', N'Taking Consultations',
     N'Replies within 5 hours', N'Growth Hacker', 4.77, 15, DATEADD(DAY, -15, @Now), @Now),

    -- 20. Kyaw Swar Myint (User / QA Automation Engineer)
    (N'kyawswar', N'KYAWSWAR', N'kyawswar.qa@gmail.com', N'KYAWSWAR.QA@GMAIL.COM', N'Kyaw Swar Myint', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?auto=format&fit=crop&w=300&q=80',
     N'Playwright, Cypress, and xUnit test automation advocate. Ensuring zero production bugs.', 0, 1,
     N'Lead Quality Assurance Engineer', N'he/him', N'Yangon, Myanmar', N'Available for Code Review',
     N'Replies within 2 hours', N'QA Master', 4.89, 26, DATEADD(DAY, -14, @Now), @Now),

    -- 21. Naw Hser Hser (User / Health & Wellness Coach)
    (N'nawhser', N'NAWHSER', N'nawhser.wellness@gmail.com', N'NAWHSER.WELLNESS@GMAIL.COM', N'Naw Hser Hser', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1548142813-c348350df52b?auto=format&fit=crop&w=300&q=80',
     N'Certified fitness trainer, mental health advocate, and ergonomics advisor for remote tech workers.', 1, 1,
     N'Holistic Health & Ergonomics Coach', N'she/her', N'Hpa-An, Myanmar', N'Sessions Open',
     N'Replies within 3 hours', N'Health Champion', 4.93, 44, DATEADD(DAY, -12, @Now), @Now),

    -- 22. Saw Bo Bo (User / Audio Engineer)
    (N'sawbobo', N'SAWBOBO', N'sawbobo.sound@gmail.com', N'SAWBOBO.SOUND@GMAIL.COM', N'Saw Bo Bo', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=300&q=80',
     N'Podcast producer, sound designer, and music mixing engineer for tech podcasts.', 0, 1,
     N'Audio Director @ WaveStudio MM', N'he/him', N'Yangon, Myanmar', N'Studio Sessions Open',
     N'Replies within 6 hours', N'Sound Artisan', 4.78, 17, DATEADD(DAY, -10, @Now), @Now),

    -- 23. Mya Mya Khin (User / Product Manager)
    (N'myamyakhin', N'MYAMYAKHIN', N'myamya.khin.pm@gmail.com', N'MYAMYA.KHIN.PM@GMAIL.COM', N'Mya Mya Khin', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?auto=format&fit=crop&w=300&q=80',
     N'Agile sprint master, user roadmap strategist, and technical product leader.', 1, 1,
     N'Senior Technical Product Manager', N'she/her', N'Yangon, Myanmar', N'Mentoring Junior PMs',
     N'Replies within 4 hours', N'Product Lead', 4.91, 39, DATEADD(DAY, -9, @Now), @Now),

    -- 24. Ko Ko Aung (User / Cybersecurity Analyst)
    (N'kokoaung', N'KOKOAUNG', N'koko.aung.secops@gmail.com', N'KOKO.AUNG.SECOPS@GMAIL.COM', N'Ko Ko Aung', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1521119989659-a83eee488004?auto=format&fit=crop&w=300&q=80',
     N'Penetration tester, OWASP Top 10 auditor, and secure code review specialist.', 0, 1,
     N'Application Security Consultant', N'he/him', N'Mandalay, Myanmar', N'Audits Available',
     N'Replies within 2 hours', N'SecOps Certified', 4.84, 23, DATEADD(DAY, -8, @Now), @Now),

    -- 25. Phyu Phyu Win (User / Cloud Architect)
    (N'phyuphyuwin', N'PHYUPHYUWIN', N'phyuphyu.win.aws@gmail.com', N'PHYUPHYU.WIN.AWS@GMAIL.COM', N'Phyu Phyu Win', @DefaultPasswordHash,
     N'https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&w=300&q=80',
     N'AWS Solutions Architect & Azure Cloud migration specialist for banking clients.', 1, 1,
     N'AWS Certified Solutions Architect', N'she/her', N'Yangon, Myanmar', N'Available for Consultations',
     N'Replies within 3 hours', N'Cloud Expert', 4.97, 68, DATEADD(DAY, -7, @Now), @Now);

    -- Insert additional 15 diverse Myanmar users to exceed 40 total fields & members
    INSERT INTO dbo.TblUser (
        UserName, NormalizedUserName, Email, NormalizedEmail, DisplayName, PasswordHash,
        AvatarUrl, Bio, IsVerified, IsActive, Headline, Pronouns, Location, AvailabilityStatus,
        ResponseSlaText, PercentileBadgeText, AverageRating, RatingCount, CreatedAt, LastLoginAt
    ) VALUES
    (N'aungsan', N'AUNGSAN', N'aungsan.dev2026@gmail.com', N'AUNGSAN.DEV2026@GMAIL.COM', N'Aung San Oo', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80', N'Full-stack C# Developer', 0, 1, N'Software Engineer', N'he/him', N'Yangon, Myanmar', N'Active', N'Within 1 day', N'Active Member', 4.60, 10, @Now, @Now),
    (N'thuzar', N'THUZAR', N'thuzar.myint.qa@gmail.com', N'THUZAR.MYINT.QA@GMAIL.COM', N'Thuzar Myint', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=300&q=80', N'QA Test Lead & Automation Specialist', 0, 1, N'Quality Assurance Specialist', N'she/her', N'Yangon, Myanmar', N'Active', N'Within 2 hours', N'Top QA', 4.75, 12, @Now, @Now),
    (N'nainglin', N'NAINGLIN', N'nainglin.oo.code@gmail.com', N'NAINGLIN.OO.CODE@GMAIL.COM', N'Naing Lin Oo', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80', N'Golang & Distributed Systems', 0, 1, N'Backend Developer', N'he/him', N'Mandalay, Myanmar', N'Available', N'Within 3 hours', N'Gopher', 4.82, 14, @Now, @Now),
    (N'chochotun', N'CHOCHOTUN', N'chochotun.design@gmail.com', N'CHOCHOTUN.DESIGN@GMAIL.COM', N'Cho Cho Tun', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=300&q=80', N'UI & Graphic Designer', 0, 1, N'Visual Designer', N'she/her', N'Bago, Myanmar', N'Active', N'Within 4 hours', N'Creative', 4.68, 8, @Now, @Now),
    (N'kyawhtet', N'KYAWHTET', N'kyawhtet.aung.mobile@gmail.com', N'KYAWHTET.AUNG.MOBILE@GMAIL.COM', N'Kyaw Htet Aung', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=300&q=80', N'React Native & Swift Engineer', 0, 1, N'Mobile Architect', N'he/him', N'Yangon, Myanmar', N'Active', N'Within 2 hours', N'Mobile Pro', 4.80, 19, @Now, @Now),
    (N'suhtwe', N'SUHTWE', N'suhtwe.ai.research@gmail.com', N'SUHTWE.AI.RESEARCH@GMAIL.COM', N'Su Htwe Lwin', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1534751516642-a171edd2521b?auto=format&fit=crop&w=300&q=80', N'Data Engineer & Deep Learning', 0, 1, N'AI Researcher', N'she/her', N'Naypyidaw, Myanmar', N'Active', N'Within 5 hours', N'AI Engineer', 4.79, 15, @Now, @Now),
    (N'zawmyolwin', N'ZAWMYOLWIN', N'zawmyolwin.net@gmail.com', N'ZAWMYOLWIN.NET@GMAIL.COM', N'Zaw Myo Lwin', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?auto=format&fit=crop&w=300&q=80', N'.NET Core & Azure Enthusiast', 0, 1, N'Full-stack .NET Dev', N'he/him', N'Yangon, Myanmar', N'Active', N'Within 1 hour', N'.NET Star', 4.88, 28, @Now, @Now),
    (N'nilarwin', N'NILARWIN', N'nilarwin.writing@gmail.com', N'NILARWIN.WRITING@GMAIL.COM', N'Nilar Win', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=300&q=80', N'Tech Blogger & Community Writer', 0, 1, N'Technical Writer', N'she/her', N'Monywa, Myanmar', N'Active', N'Within 3 hours', N'Writer', 4.73, 11, @Now, @Now),
    (N'bohein', N'BOHEIN', N'bohein.security@gmail.com', N'BOHEIN.SECURITY@GMAIL.COM', N'Bo Hein Thu', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=300&q=80', N'Ethical Hacker & Network Security', 0, 1, N'Security Analyst', N'he/him', N'Yangon, Myanmar', N'Active', N'Within 4 hours', N'Ethical Hacker', 4.85, 20, @Now, @Now),
    (N'shwesi', N'SHWESI', N'shwesi.creative@gmail.com', N'SHWESI.CREATIVE@GMAIL.COM', N'Shwe Si Phyo', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?auto=format&fit=crop&w=300&q=80', N'Digital Artist & 3D Modeler', 0, 1, N'3D Artist', N'she/her', N'Yangon, Myanmar', N'Active', N'Within 6 hours', N'Artist', 4.65, 7, @Now, @Now),
    (N'myatmin', N'MYATMIN', N'myatmin.linux@gmail.com', N'MYATMIN.LINUX@GMAIL.COM', N'Myat Min Soe', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1519345182560-3f2917c472ef?auto=format&fit=crop&w=300&q=80', N'Linux Kernel & C++ Developer', 0, 1, N'Systems Programmer', N'he/him', N'Yangon, Myanmar', N'Active', N'Within 2 hours', N'C++ Guru', 4.87, 24, @Now, @Now),
    (N'sandarwin', N'SANDARWIN', N'sandarwin.startup@gmail.com', N'SANDARWIN.STARTUP@GMAIL.COM', N'Sandar Win', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=300&q=80', N'Startup Ecosystem Builder & Recruiter', 0, 1, N'Tech Recruiter', N'she/her', N'Yangon, Myanmar', N'Active', N'Within 1 hour', N'Talent Scout', 4.90, 32, @Now, @Now),
    (N'khunsett', N'KHUNSETT', N'khunsett.naing@gmail.com', N'KHUNSETT.NAING@GMAIL.COM', N'Khun Sett Naing', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1501196354995-cbb51c65aaea?auto=format&fit=crop&w=300&q=80', N'Embedded Systems & IoT Hardware', 0, 1, N'Hardware Engineer', N'he/him', N'Taunggyi, Myanmar', N'Active', N'Within 5 hours', N'IoT Builder', 4.70, 9, @Now, @Now),
    (N'mayhsu', N'MAYHSU', N'mayhsu.marketing@gmail.com', N'MAYHSU.MARKETING@GMAIL.COM', N'May Hsu Mon', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=300&q=80', N'Social Media Strategist & Brand Lead', 0, 1, N'Brand Strategist', N'she/her', N'Yangon, Myanmar', N'Active', N'Within 3 hours', N'Marketer', 4.74, 13, @Now, @Now),
    (N'aungko', N'AUNGKO', N'aungko.frontend@gmail.com', N'AUNGKO.FRONTEND@GMAIL.COM', N'Aung Ko Ko', @DefaultPasswordHash, N'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?auto=format&fit=crop&w=300&q=80', N'Vue.js & Webpack Specialist', 0, 1, N'Web Developer', N'he/him', N'Mandalay, Myanmar', N'Active', N'Within 4 hours', N'Web Master', 4.69, 11, @Now, @Now);

    PRINT '>>> Step 8: Assigning User Roles...';
    -- Min Thant = ADMIN
    INSERT INTO dbo.TblUserRole (UserId, RoleId, CreatedAt)
    VALUES (1, @RoleId_Admin, @Now);

    -- May Kyawt & Kaung Kaung & Thiri Sandar & Phyu Phyu Win = DOMAIN_PROFESSIONAL
    INSERT INTO dbo.TblUserRole (UserId, RoleId, CreatedAt)
    VALUES 
    (2, @RoleId_DomainPro, @Now),
    (3, @RoleId_DomainPro, @Now),
    (5, @RoleId_DomainPro, @Now),
    (25, @RoleId_DomainPro, @Now);

    -- Aung Kyaw San = PUBLIC_FIGURE
    INSERT INTO dbo.TblUserRole (UserId, RoleId, CreatedAt)
    VALUES (4, @RoleId_PublicFigure, @Now);

    -- Zin Min Htet = MODERATOR (Admin role)
    INSERT INTO dbo.TblUserRole (UserId, RoleId, CreatedAt)
    VALUES (6, @RoleId_Moderator, @Now);

    -- Remaining users = USER (Default)
    INSERT INTO dbo.TblUserRole (UserId, RoleId, CreatedAt)
    SELECT u.UserId, @RoleId_User, @Now
    FROM dbo.TblUser u
    WHERE u.UserId NOT IN (1, 2, 3, 4, 5, 6, 25);

    PRINT '>>> Step 9: Seeding Admin Accounts (TblAdmin)...';
    -- System Admin accounts matching with Password '123456789'
    SET IDENTITY_INSERT dbo.TblAdmin ON;
    INSERT INTO dbo.TblAdmin (
        AdminId, Email, NormalizedEmail, FullName, PasswordHash, IsSuperAdmin, IsActive, CreatedAt
    ) VALUES
    (1, N'admin@communitylink.local', N'ADMIN@COMMUNITYLINK.LOCAL', N'System Administrator', @DefaultPasswordHash, 1, 1, @Now),
    (2, N'mgminthant2004@gmail.com', N'MGMINTHANT2004@GMAIL.COM', N'Min Thant (Master)', @DefaultPasswordHash, 1, 1, @Now),
    (3, N'maykyawtkhaing2842@gmail.com', N'MAYKYAWTKHAING2842@GMAIL.COM', N'May Kyawt Khaing (Admin)', @DefaultPasswordHash, 0, 1, @Now),
    (4, N'juggerkaung.dev@gmail.com', N'JUGGERKAUNG.DEV@GMAIL.COM', N'Kaung Myat Han (Admin)', @DefaultPasswordHash, 0, 1, @Now),
    (5, N'zinminhtet.security@gmail.com', N'ZINMINHTET.SECURITY@GMAIL.COM', N'Zin Min Htet (SecOps)', @DefaultPasswordHash, 0, 1, @Now);
    SET IDENTITY_INSERT dbo.TblAdmin OFF;

    INSERT INTO dbo.TblAdminRole (AdminId, RoleId, CreatedAt, IsDeleted)
    VALUES 
    (1, @RoleId_Admin, @Now, 0),
    (2, @RoleId_Admin, @Now, 0),
    (3, @RoleId_Admin, @Now, 0),
    (4, @RoleId_Admin, @Now, 0),
    (5, @RoleId_Moderator, @Now, 0);

    PRINT '>>> Step 10: Seeding LinkDrop Packages & Payment Methods...';
    INSERT INTO dbo.TblPaymentMethod (
        MethodName, AccountName, AccountNumber, Instructions, DisplayOrder, IsActive, CreatedAt, IsDeleted
    ) VALUES
    (N'KBZPay', N'Min Thant', N'09420011223', N'Transfer to KBZPay and upload transaction screenshot slip.', 1, 1, @Now, 0),
    (N'WavePay', N'Min Thant', N'09420011223', N'Send via WaveMoney / WavePay wallet. Provide Ref No.', 2, 1, @Now, 0),
    (N'AYA Pay', N'May Kyawt Khaing', N'09971122334', N'Instant transfer via AYA Pay. Verified automatically within 10 mins.', 3, 1, @Now, 0),
    (N'CB Pay', N'Kaung Myat Han', N'09781199887', N'Transfer via CB Pay or CB Bank mobile banking.', 4, 1, @Now, 0);

    INSERT INTO dbo.TblLinkDropPackage (
        PackageName, Description, LinkDropAmount, BonusAmount, RealMoneyAmount, Currency, DisplayOrder, IsActive, CreatedAt, IsDeleted
    ) VALUES
    (N'Starter Drop', N'Ideal for new users to unlock chat rooms and support creators.', 100, 10, 5000.00, N'MMK', 1, 1, @Now, 0),
    (N'Community Builder', N'Great value pack for group creators and poll hosts.', 500, 75, 24000.00, N'MMK', 2, 1, @Now, 0),
    (N'Pro Creator Pack', N'Recommended for Domain Professionals and Advisory engines.', 1200, 250, 55000.00, N'MMK', 3, 1, @Now, 0),
    (N'Executive Syndicate Vault', N'Maximum bonus points for public figures and syndicate sponsors.', 3500, 900, 150000.00, N'MMK', 4, 1, @Now, 0);

    PRINT '>>> Step 11: Seeding Wallets for All Users...';
    -- Give every user a realistic starting balance
    INSERT INTO dbo.TblLinkDropWallet (UserId, Balance, PurchasedBalance, EarnedBalance, CreatedAt)
    SELECT 
        UserId,
        CASE WHEN UserId IN (1, 2, 3, 4, 5) THEN 2500 ELSE 450 END,
        CASE WHEN UserId IN (1, 2, 3, 4, 5) THEN 1500 ELSE 300 END,
        CASE WHEN UserId IN (1, 2, 3, 4, 5) THEN 1000 ELSE 150 END,
        @Now
    FROM dbo.TblUser;

    PRINT '>>> Step 12: Seeding 20+ Realistic Communities...';
    INSERT INTO dbo.TblCommunity (
        ParentCommunityId, OwnerId, Name, Slug, Description, AvatarUrl, BannerUrl,
        Visibility, JoinPolicy, MemberCount, PostCount, AverageRating, RatingCount, CreatedAt, IsDeleted
    ) VALUES
    -- 1. Tech & Software Architecture
    (NULL, 1, N'Myanmar Tech & Software Architecture', N'myanmar-tech-architecture',
     N'The premier community for software architects, engineering leads, and full-stack builders in Myanmar. Discussions on scalable design, microservices, and system trade-offs.',
     N'https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 1420, 89, 4.95, 78, DATEADD(DAY, -45, @Now), 0),

    -- 2. UI/UX & Design Systems Myanmar
    (NULL, 2, N'UI/UX & Design Systems Myanmar', N'ui-ux-design-systems-mm',
     N'Hub for product designers, Figma creators, and design system engineers. Sharing modern aesthetic patterns, accessibility, and micro-interactions.',
     N'https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1558655146-d09347e92766?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 980, 64, 4.92, 54, DATEADD(DAY, -42, @Now), 0),

    -- 3. AI & Data Science Myanmar
    (NULL, 5, N'AI & Data Science Myanmar', N'ai-data-science-myanmar',
     N'Exploring Deep Learning, LLM fine-tuning, Burmese NLP language models, and practical machine learning applications in industry.',
     N'https://images.unsplash.com/photo-1677442136019-21780ecad995?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1620712943543-bcc4688e7485?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 1150, 72, 4.98, 86, DATEADD(DAY, -40, @Now), 0),

    -- 4. Myanmar Startup Founders & Angels
    (NULL, 4, N'Myanmar Startup Founders & Angels', N'myanmar-startup-founders-angels',
     N'Curated community for venture founders, angel investors, and tech leaders discussing fundraising, go-to-market strategies, and cross-border expansion.',
     N'https://images.unsplash.com/photo-1556761175-5973dc0f32e7?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 760, 48, 4.90, 45, DATEADD(DAY, -38, @Now), 0),

    -- 5. Cloud Native & DevOps MM
    (NULL, 10, N'Cloud Native & DevOps MM', N'cloud-native-devops-mm',
     N'Kubernetes, Docker, Terraform, CI/CD pipelines, and cloud security practices across AWS, GCP, and Azure.',
     N'https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1544197150-b99a580bb7a8?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 840, 53, 4.88, 39, DATEADD(DAY, -35, @Now), 0),

    -- 6. Mobile Devs Myanmar (Flutter & Swift)
    (NULL, 8, N'Mobile Devs Myanmar (Flutter & Swift)', N'mobile-devs-myanmar',
     N'Building world-class iOS & Android apps with Flutter, Kotlin, and Swift. App Store optimization, performance tuning, and clean architecture.',
     N'https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1551650975-87deedd944c3?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 920, 58, 4.84, 42, DATEADD(DAY, -32, @Now), 0),

    -- 7. Cybersecurity & Ethical Hacking
    (NULL, 6, N'Cybersecurity & Ethical Hacking', N'cybersecurity-ethical-hacking-mm',
     N'Defensive security, penetration testing, CTF challenges, vulnerability disclosure, and secure API architecture.',
     N'https://images.unsplash.com/photo-1550751827-4bd374c3f58b?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1563986768609-322da13575f3?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'APPROVAL', 690, 41, 4.91, 35, DATEADD(DAY, -30, @Now), 0),

    -- 8. FinTech & Digital Payments Hub
    (NULL, 9, N'FinTech & Digital Payments Hub', N'fintech-digital-payments-hub',
     N'Payment gateways, micro-transactions, digital wallets, regulatory compliance, and cross-border remittances in Southeast Asia.',
     N'https://images.unsplash.com/photo-1559526324-4b87b5e36e44?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1526304640581-d334cdbbf45e?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 810, 46, 4.87, 33, DATEADD(DAY, -28, @Now), 0),

    -- 9. Myanmar Game Developers Guild
    (NULL, 16, N'Myanmar Game Developers Guild', N'myanmar-game-developers-guild',
     N'Indie game makers sharing Unity, Unreal, pixel art, 3D animations, shaders, and sound design for interactive entertainment.',
     N'https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 620, 36, 4.79, 28, DATEADD(DAY, -26, @Now), 0),

    -- 10. English for Global Tech Careers
    (NULL, 17, N'English for Global Tech Careers', N'english-global-tech-careers',
     N'Daily speaking clubs, technical resume reviews, remote job interview prep, and IELTS strategies for developers.',
     N'https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1455849318743-b2233052fcff?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 1350, 92, 4.96, 88, DATEADD(DAY, -24, @Now), 0),

    -- 11. Web3 & Decentralized Technologies
    (NULL, 14, N'Web3 & Decentralized Technologies', N'web3-decentralized-tech-mm',
     N'Smart contract security, peer-to-peer protocols, decentralized storage, and zero-knowledge cryptography.',
     N'https://images.unsplash.com/photo-1639762681485-074b7f938ba0?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1621416894569-0f39ed31d247?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 540, 31, 4.76, 22, DATEADD(DAY, -22, @Now), 0),

    -- 12. Remote Work & Freelancing Myanmar
    (NULL, 12, N'Remote Work & Freelancing Myanmar', N'remote-work-freelancing-mm',
     N'Contract negotiations, international client acquisition, Upwork/Toptal tips, and asynchronous remote collaboration.',
     N'https://images.unsplash.com/photo-1587560699334-cc4ff634909a?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1527689368864-3a821dbccc34?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 1280, 84, 4.89, 67, DATEADD(DAY, -20, @Now), 0),

    -- 13. Health & Ergonomics for Developers
    (NULL, 21, N'Health & Ergonomics for Developers', N'health-ergonomics-developers',
     N'Combating burnout, posture health, eye strain mitigation, desk setups, and mindful wellness for computer workers.',
     N'https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1506126613408-eca07ce68773?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 710, 42, 4.93, 37, DATEADD(DAY, -18, @Now), 0),

    -- 14. Myanmar Photography & Visual Storytelling
    (NULL, 7, N'Myanmar Photography & Visual Storytelling', N'myanmar-photography-visuals',
     N'Composition critique, lightroom color grading presets, portrait techniques, and capturing the vibrant cultures of Myanmar.',
     N'https://images.unsplash.com/photo-1452587925148-ce544e77e70d?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1471341971476-ae15ff5dd4ea?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 890, 59, 4.85, 41, DATEADD(DAY, -16, @Now), 0),

    -- 15. Product Management & Strategy MM
    (NULL, 23, N'Product Management & Strategy MM', N'product-management-strategy-mm',
     N'User research interviews, PRD documentation, prioritization frameworks (RICE), metrics tracking, and product discovery.',
     N'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 730, 45, 4.91, 36, DATEADD(DAY, -14, @Now), 0),

    -- 16. Frontend Masters Myanmar
    (NULL, 12, N'Frontend Masters Myanmar', N'frontend-masters-myanmar',
     N'Next.js 15, React 19, CSS Container Queries, WebGL shaders, Tailwind v4, and modern performance audits.',
     N'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 860, 51, 4.87, 34, DATEADD(DAY, -12, @Now), 0),

    -- 17. Quality Assurance & Test Engineering
    (NULL, 20, N'Quality Assurance & Test Engineering', N'qa-test-engineering-mm',
     N'Automation testing, continuous integration test suites, load testing with k6, and mobile device farms.',
     N'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1504384308090-c894fdcc538d?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 590, 33, 4.82, 23, DATEADD(DAY, -10, @Now), 0),

    -- 18. Growth Marketing & Brand Analytics
    (NULL, 19, N'Growth Marketing & Brand Analytics', N'growth-marketing-brand-analytics',
     N'Data-driven user acquisition, viral loops, organic SEO ranking strategies, and social media branding campaigns.',
     N'https://images.unsplash.com/photo-1460925895917-afdab827c52f?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1551836022-d5d88e9218df?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 680, 37, 4.80, 27, DATEADD(DAY, -8, @Now), 0),

    -- 19. Hardware, IoT & Robotics Club
    (NULL, 24, N'Hardware, IoT & Robotics Club', N'hardware-iot-robotics-club',
     N'ESP32, Raspberry Pi, Arduino microcontrollers, custom PCB milling, smart home telemetry, and drone firmware.',
     N'https://images.unsplash.com/photo-1517077304055-6e89abbf09b0?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1485827404703-89b55fcc595e?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 510, 29, 4.78, 19, DATEADD(DAY, -6, @Now), 0),

    -- 20. Myanmar Audio & Podcast Creators
    (NULL, 22, N'Myanmar Audio & Podcast Creators', N'myanmar-audio-podcast-creators',
     N'Audio storytelling, podcast microphone recommendations, acoustic room treatment, vocal mastering, and RSS syndication.',
     N'https://images.unsplash.com/photo-1590602847861-f357a9332bbc?auto=format&fit=crop&w=300&q=80',
     N'https://images.unsplash.com/photo-1478737270239-2f02b77fc618?auto=format&fit=crop&w=1200&q=80',
     N'PUBLIC', N'INSTANT', 490, 26, 4.83, 21, DATEADD(DAY, -4, @Now), 0);

    PRINT '>>> Step 13: Seeding 20+ Working Groups under Communities...';
    INSERT INTO dbo.TblGroup (
        SubCommunityId, CreatorId, Name, Slug, Description, AvatarUrl, BannerUrl,
        Visibility, JoinPolicy, MemberCount, PostCount, GroupType, JoinFeeLinkDrops,
        CommissionPercentageSnapshot, CreatedAt, IsDeleted
    ) VALUES
    (1, 1, N'.NET Core & C# Mastery Group', N'dotnet-csharp-mastery', N'Deep-dive into .NET 10, ASP.NET Core Web API, and Blazor full-stack patterns.', N'https://images.unsplash.com/photo-1517694712202-14dd9538aa97?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 380, 25, N'FREE', 0, 10.00, DATEADD(DAY, -30, @Now), 0),
    (1, 3, N'System Architecture & Microservices', N'system-architecture-microservices', N'Distributed caching, event sourcing with Kafka, and resilience patterns.', N'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 290, 18, N'FREE', 0, 10.00, DATEADD(DAY, -28, @Now), 0),
    (2, 2, N'Figma Components & Design Token Guild', N'figma-tokens-guild', N'Design token architectures, variable modes, auto-layout wizardry, and design handoff.', N'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 310, 22, N'FREE', 0, 10.00, DATEADD(DAY, -25, @Now), 0),
    (2, 15, N'Micro-Animations & Motion Design', N'micro-animations-motion-design', N'CSS animations, GSAP, and Framer Motion interactive feedback loops.', N'https://images.unsplash.com/photo-1550745165-9bc0b252726f?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 180, 12, N'PAID', 50, 10.00, DATEADD(DAY, -24, @Now), 0),
    (3, 5, N'Burmese NLP & LLM Training Lab', N'burmese-nlp-llm-lab', N'Developing open datasets, tokenizers, and LLM fine-tunes for the Myanmar language.', N'https://images.unsplash.com/photo-1620712943543-bcc4688e7485?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 420, 31, N'FREE', 0, 10.00, DATEADD(DAY, -22, @Now), 0),
    (3, 13, N'PyTorch & Computer Vision Hackers', N'pytorch-computer-vision-hackers', N'YOLO object detection, segmentation models, and edge inference optimization.', N'https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 240, 16, N'FREE', 0, 10.00, DATEADD(DAY, -20, @Now), 0),
    (4, 4, N'Angel Syndicate Dealflow Round-table', N'angel-syndicate-roundtable', N'Private check-writer round-table evaluating high-potential Myanmar & regional startups.', N'https://images.unsplash.com/photo-1556761175-5973dc0f32e7?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 150, 14, N'PAID', 100, 10.00, DATEADD(DAY, -19, @Now), 0),
    (4, 1, N'Product-Market Fit & Traction Guild', N'pmf-traction-guild', N'Early-stage retention curves, customer discovery calls, and unit economics.', N'https://images.unsplash.com/photo-1552664730-d307ca884978?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 210, 15, N'FREE', 0, 10.00, DATEADD(DAY, -18, @Now), 0),
    (5, 10, N'Kubernetes & Helm Production Secrets', N'k8s-helm-production-secrets', N'Cluster autoscaling, ingress controllers, cert-manager, and zero-downtime upgrades.', N'https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 270, 19, N'FREE', 0, 10.00, DATEADD(DAY, -17, @Now), 0),
    (6, 8, N'Flutter Performance & State Management', N'flutter-state-management', N'Bloc, Riverpod, clean folder architectures, and 60fps smooth scrolling tricks.', N'https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 330, 24, N'FREE', 0, 10.00, DATEADD(DAY, -16, @Now), 0),
    (7, 6, N'Red Team / Blue Team CTF Squad', N'red-blue-ctf-squad', N'Hands-on vulnerability exploitation, reverse engineering, and threat mitigation.', N'https://images.unsplash.com/photo-1550751827-4bd374c3f58b?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 190, 13, N'FREE', 0, 10.00, DATEADD(DAY, -15, @Now), 0),
    (8, 9, N'Payment Gateway Integration Squad', N'payment-gateway-integration-squad', N'Handling webhooks, payment callback security, and settlement reconciliations.', N'https://images.unsplash.com/photo-1559526324-4b87b5e36e44?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 220, 15, N'FREE', 0, 10.00, DATEADD(DAY, -14, @Now), 0),
    (9, 16, N'Unity Shader Graph & VFX Wizards', N'unity-shader-vfx-wizards', N'Custom HLSL shaders, particle systems, post-processing, and mobile performance optimization.', N'https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 160, 11, N'PAID', 30, 10.00, DATEADD(DAY, -13, @Now), 0),
    (10, 17, N'Daily English Tech Debate Club', N'daily-english-tech-debate', N'Live debate sessions discussing AI ethics, remote working cultures, and engineering trade-offs.', N'https://images.unsplash.com/photo-1523240795612-9a054b0db644?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 480, 35, N'FREE', 0, 10.00, DATEADD(DAY, -12, @Now), 0),
    (11, 14, N'Smart Contract Security & Audits', N'smart-contract-security-audits', N'Reentrancy attacks, flash loan mechanics, and formal verification of Solidity code.', N'https://images.unsplash.com/photo-1639762681485-074b7f938ba0?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 140, 10, N'FREE', 0, 10.00, DATEADD(DAY, -11, @Now), 0),
    (12, 12, N'Global Remote Job Seekers Circle', N'global-remote-job-seekers', N'Resume feedback, mock coding interviews, salary negotiation, and remote tax setups.', N'https://images.unsplash.com/photo-1587560699334-cc4ff634909a?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 390, 27, N'FREE', 0, 10.00, DATEADD(DAY, -10, @Now), 0),
    (13, 21, N'Desk Ergonomics & Posture Correction', N'desk-ergonomics-posture-correction', N'Standing desk routines, cervical spine stretches, and eye exercises for 8+ hour coding days.', N'https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 250, 17, N'FREE', 0, 10.00, DATEADD(DAY, -9, @Now), 0),
    (15, 23, N'User Research & Discovery Methods', N'user-research-discovery-methods', N'Conducting customer interview loops and turning feedback into crisp product roadmaps.', N'https://images.unsplash.com/photo-1531403009284-440f080d1e12?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 230, 16, N'FREE', 0, 10.00, DATEADD(DAY, -8, @Now), 0),
    (16, 12, N'Next.js App Router Mastery', N'nextjs-app-router-mastery', N'Server actions, streaming SSR, partial prerendering (PPR), and caching strategies.', N'https://images.unsplash.com/photo-1581291518857-4e27b48ff24e?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 310, 21, N'FREE', 0, 10.00, DATEADD(DAY, -7, @Now), 0),
    (17, 20, N'Playwright E2E Automation Lab', N'playwright-e2e-automation-lab', N'Headless browser testing, visual regression snapshots, and CI pipeline integrations.', N'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=200&q=80', NULL, N'PUBLIC', N'INSTANT', 190, 13, N'FREE', 0, 10.00, DATEADD(DAY, -6, @Now), 0);

    PRINT '>>> Step 14: Enrolling Users into Communities & Groups...';
    -- Enroll owners & creators as OWNER in Communities
    INSERT INTO dbo.TblCommunityMember (CommunityId, UserId, Role, IsMuted, JoinedAt, CreatedAt, IsDeleted)
    SELECT CommunityId, OwnerId, N'OWNER', 0, CreatedAt, CreatedAt, 0
    FROM dbo.TblCommunity;

    -- Add all main team users to top communities as active members
    INSERT INTO dbo.TblCommunityMember (CommunityId, UserId, Role, IsMuted, JoinedAt, CreatedAt, IsDeleted)
    SELECT c.CommunityId, u.UserId, N'MEMBER', 0, DATEADD(DAY, -2, @Now), @Now, 0
    FROM dbo.TblCommunity c
    CROSS JOIN (SELECT TOP 10 UserId FROM dbo.TblUser) u
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TblCommunityMember cm 
        WHERE cm.CommunityId = c.CommunityId AND cm.UserId = u.UserId
    );

    -- Enroll creators as OWNER in Groups
    INSERT INTO dbo.TblGroupMember (GroupId, UserId, Role, IsMuted, JoinedAt, CreatedAt, IsDeleted)
    SELECT GroupId, CreatorId, N'OWNER', 0, CreatedAt, CreatedAt, 0
    FROM dbo.TblGroup;

    -- Add sample members to groups
    INSERT INTO dbo.TblGroupMember (GroupId, UserId, Role, IsMuted, JoinedAt, CreatedAt, IsDeleted)
    SELECT g.GroupId, u.UserId, N'MEMBER', 0, DATEADD(DAY, -1, @Now), @Now, 0
    FROM dbo.TblGroup g
    CROSS JOIN (SELECT TOP 5 UserId FROM dbo.TblUser) u
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.TblGroupMember gm 
        WHERE gm.GroupId = g.GroupId AND gm.UserId = u.UserId
    );

    PRINT '>>> Step 15: Seeding 30+ Realistic Posts, Code Snippets & Technical Discussions...';
    INSERT INTO dbo.TblPost (
        AuthorId, CommunityId, GroupId, Content, PostType, Subtitle, CodeSnippet, CodeLanguage,
        CodeFileName, DiagramImageUrl, DiagramCaption, HasPoll, LikeCount, CommentCount, ShareCount, CreatedAt, IsDeleted
    ) VALUES
    -- 1. Architecture post by Min Thant
    (1, 1, 1, 
     N'We just upgraded CommunityLink to .NET 10 with Blazor Interactive Server. One critical architectural takeaway: always ensure your concurrency tokens and rowversion handlings align with database defaults when handling high-concurrency SignalR events. Here is how we enforce atomic state transitions without table-level deadlocks.',
     N'CODE', N'Optimistic Concurrency & High Throughput SignalR',
     N'public async Task<Result> UpdateUserBalanceAsync(int userId, long delta, CancellationToken ct)
{
    var wallet = await _db.TblLinkDropWallets
        .FirstOrDefaultAsync(w => w.UserId == userId, ct);
    if (wallet == null) return Result.Failure("Wallet not found");

    wallet.Balance += delta;
    wallet.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync(ct);
    return Result.Success();
}', N'csharp', N'WalletService.cs', NULL, NULL, 0, 48, 12, 6, DATEADD(DAY, -10, @Now), 0),

    -- 2. Design post by May Kyawt Khaing
    (2, 2, 3,
     N'Design Tokens are not just CSS variables. They are the single contract between designers in Figma and developers in code. In our new design system, spacing scales, surface elevations, and dynamic typography are completely synchronized with TailwindCSS tokens. Check out our token hierarchy!',
     N'STANDARD', N'Design Tokens Architecture in Figma & Tailwind',
     NULL, NULL, NULL,
     N'https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=800&q=80',
     N'Figma Token Variables mapped directly to Tailwind Tokens', 0, 62, 19, 9, DATEADD(DAY, -9, @Now), 0),

    -- 3. AI post by Dr. Thiri Sandar with a Poll
    (5, 3, 5,
     N'We are fine-tuning an open-source Burmese LLM on 500,000 conversational instruction pairs. Which architecture do you believe produces the best coherence for low-resource languages like Myanmar (Burmese)? Vote in the poll below!',
     N'POLL', N'Burmese NLP & LLM Architecture Poll',
     NULL, NULL, NULL, NULL, NULL, 1, 85, 27, 14, DATEADD(DAY, -8, @Now), 0),

    -- 4. Cloud post by Ye Yint Aung
    (10, 5, 9,
     N'Quick tip for running Kubernetes on cost-effective cloud instances: always set tight requests and limits for your .NET containers. Use Server GC only if you have >= 2 cores assigned, otherwise Workstation GC will give you drastically lower memory footprints and smoother latency.',
     N'CODE', N'Kubernetes Resource Tuning for .NET Apps',
     N'apiVersion: apps/v1
kind: Deployment
metadata:
  name: communitylink-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: communitylink/api:latest
        resources:
          limits:
            memory: "512Mi"
            cpu: "1000m"
          requests:
            memory: "256Mi"
            cpu: "250m"', N'yaml', N'deployment.yaml', NULL, NULL, 0, 39, 8, 4, DATEADD(DAY, -7, @Now), 0),

    -- 5. Mobile post by Kyaw Zayar Lynn
    (8, 6, 10,
     N'Flutter 3.29 dropped and Impeller rendering engine on iOS has practically eliminated all shader compilation jank. Here is how we achieved constant 120 FPS on our custom list views with cached network images.',
     N'STANDARD', N'Achieving 120 FPS in Flutter with Impeller',
     NULL, NULL, NULL, NULL, NULL, 0, 44, 11, 5, DATEADD(DAY, -6, @Now), 0),

    -- 6. English post by Thet Htar Swe
    (17, 10, 14,
     N'In remote tech interviews with US & European companies, avoid saying "I will try my best". Instead, use actionable phrases such as: "Here is the exact approach I will execute to diagnose and resolve this bottleneck." Small linguistic shifts project immense senior confidence!',
     N'STANDARD', N'Senior Developer Communication in Remote Interviews',
     NULL, NULL, NULL, NULL, NULL, 0, 92, 34, 22, DATEADD(DAY, -5, @Now), 0),

    -- 7. Security post by Zin Min Htet
    (6, 7, 11,
     N'PSA for all API developers: Never store refresh tokens in localStorage where XSS attacks can extract them. Always use HTTP-Only, Secure, SameSite=Strict cookies to protect user session integrity.',
     N'STANDARD', N'Defending Against Token Exfiltration in Web Apps',
     NULL, NULL, NULL, NULL, NULL, 0, 57, 14, 8, DATEADD(DAY, -4, @Now), 0),

    -- 8. Standalone Poll by Kaung Myat Han
    (3, NULL, NULL,
     N'What is your preferred database strategy when building modern high-scale multi-tenant SaaS products?',
     N'POLL', N'Multi-Tenant Database Strategy Poll',
     NULL, NULL, NULL, NULL, NULL, 1, 71, 23, 11, DATEADD(DAY, -3, @Now), 0),

    -- 9. General post by Aung Kyaw San
    (4, 4, 7,
     N'To all young tech founders in Myanmar: Focus on building high gross-margin digital products rather than capital-intensive operations. Software with strong retention and organic referral loops will always survive volatile macro environments.',
     N'STANDARD', N'Founders Playbook: Capital Efficiency & Product Retention',
     NULL, NULL, NULL, NULL, NULL, 0, 118, 41, 35, DATEADD(DAY, -2, @Now), 0),

    -- 10. Frontend post by Pyae Sone Phyo
    (12, 16, 19,
     N'TailwindCSS v4 is blazing fast with the new Rust-based Lightning CSS engine. No more tailwind.config.js - everything is configured right inside your stylesheet with @theme directives.',
     N'CODE', N'Migrating to Tailwind v4 @theme Directives',
     N'@import "tailwindcss";

@theme {
  --color-brand-primary: #4f46e5;
  --color-brand-accent: #f43f5e;
  --font-display: "Outfit", sans-serif;
}', N'css', N'app.css', NULL, NULL, 0, 53, 15, 7, DATEADD(DAY, -1, @Now), 0);

    -- Additional realistic posts across different authors
    INSERT INTO dbo.TblPost (AuthorId, CommunityId, GroupId, Content, PostType, Subtitle, LikeCount, CommentCount, ShareCount, CreatedAt, IsDeleted)
    VALUES
    (7, 14, NULL, N'Early morning light captured at Shwedagon Pagoda, Yangon. The reflection against the wet marble tiles created an unforgettable ethereal atmosphere.', N'STANDARD', N'Morning Reflections in Yangon', 64, 18, 9, DATEADD(HOUR, -20, @Now), 0),
    (9, 8, 12, N'Notice: Local bank APIs will undergo scheduled maintenance this Sunday from 2:00 AM to 5:00 AM. Plan your automated reconciliation jobs accordingly.', N'STANDARD', N'Banking API Maintenance Window', 32, 6, 3, DATEADD(HOUR, -18, @Now), 0),
    (14, 11, 15, N'Zero-knowledge rollups (zk-SNARKs) are transforming how we verify identity claims without revealing private personal documents.', N'STANDARD', N'Privacy-Preserving Identity Verification', 41, 9, 4, DATEADD(HOUR, -16, @Now), 0),
    (21, 13, 17, N'Reminder: If your neck bends forward more than 30 degrees while looking at your monitor, you are putting 40+ lbs of pressure on your cervical spine. Elevate your screen today!', N'STANDARD', N'Ergonomic Health Alert for Coders', 78, 22, 15, DATEADD(HOUR, -14, @Now), 0),
    (23, 15, 18, N'Shipping features that nobody uses is the most expensive mistake in software engineering. Validate user demand with clickable prototypes before writing production backend code.', N'STANDARD', N'Prototyping Over Premature Coding', 88, 26, 12, DATEADD(HOUR, -12, @Now), 0),
    (16, 9, 13, N'Just published our open-source shader pack for Unity URP on GitHub! Includes toon water, dissolve effects, and interactive grass shaders.', N'STANDARD', N'Open Source Shader Pack Release', 69, 17, 8, DATEADD(HOUR, -10, @Now), 0),
    (20, 17, 20, N'Tip: Run your end-to-end tests against Docker containers using ephemeral test databases. Never run automated write tests against shared staging instances.', N'STANDARD', N'Ephemeral Test Databases in CI/CD', 45, 12, 5, DATEADD(HOUR, -8, @Now), 0),
    (18, 5, NULL, N'PostgreSQL 17 query performance benchmarks show 2x speedups on large JSONB indexing queries. Who is testing it in production?', N'STANDARD', N'PostgreSQL 17 Performance Benchmarks', 37, 8, 2, DATEADD(HOUR, -6, @Now), 0),
    (24, 7, NULL, N'Always audit your third-party npm and NuGet packages with automated dependency scanners in your GitHub Actions workflows.', N'STANDARD', N'Software Supply Chain Security', 52, 14, 6, DATEADD(HOUR, -4, @Now), 0),
    (22, 20, NULL, N'A clean vocal track is 80% acoustic room treatment and 20% microphone quality. Heavy blankets and acoustic panels will do more for your sound than a $1,000 mic.', N'STANDARD', N'Podcasting Studio Sound Secrets', 49, 15, 7, DATEADD(HOUR, -2, @Now), 0);

    PRINT '>>> Step 16: Seeding Polls & Poll Options...';
    -- Poll 1 for Post 3 (AI Architecture Poll)
    INSERT INTO dbo.TblPoll (PostId, Question, IsMultipleChoice, TotalVotes, CreatedAt, IsDeleted)
    VALUES (3, N'Which LLM architecture yields the highest coherence for low-resource languages like Burmese?', 0, 74, DATEADD(DAY, -8, @Now), 0);
    DECLARE @Poll1Id INT = SCOPE_IDENTITY();

    INSERT INTO dbo.TblPollOption (PollId, OptionText, VoteCount, DisplayOrder, CreatedAt, IsDeleted)
    VALUES
    (@Poll1Id, N'Llama-3 8B Fine-tuned with custom Burmese Tokenizer', 38, 1, DATEADD(DAY, -8, @Now), 0),
    (@Poll1Id, N'Mistral / Mixtral Mixture-of-Experts (MoE)', 22, 2, DATEADD(DAY, -8, @Now), 0),
    (@Poll1Id, N'Gemma 2 with Multilingual Byte-Fallback', 11, 3, DATEADD(DAY, -8, @Now), 0),
    (@Poll1Id, N'Custom Pre-trained Transformer from scratch', 3, 4, DATEADD(DAY, -8, @Now), 0);

    -- Poll 2 for Post 8 (Multi-tenant Database Strategy)
    INSERT INTO dbo.TblPoll (PostId, Question, IsMultipleChoice, TotalVotes, CreatedAt, IsDeleted)
    VALUES (8, N'What is your preferred database multi-tenancy strategy for cloud SaaS?', 0, 68, DATEADD(DAY, -3, @Now), 0);
    DECLARE @Poll2Id INT = SCOPE_IDENTITY();

    INSERT INTO dbo.TblPollOption (PollId, OptionText, VoteCount, DisplayOrder, CreatedAt, IsDeleted)
    VALUES
    (@Poll2Id, N'Shared Database, Shared Schema with TenantId column', 42, 1, DATEADD(DAY, -3, @Now), 0),
    (@Poll2Id, N'Separate Schema per Tenant (Schema Isolation)', 16, 2, DATEADD(DAY, -3, @Now), 0),
    (@Poll2Id, N'Separate Physical Database per Tenant', 10, 3, DATEADD(DAY, -3, @Now), 0);

    PRINT '>>> Step 17: Seeding Comments & Post Reactions...';
    INSERT INTO dbo.TblComment (PostId, UserId, Content, CreatedAt, IsDeleted)
    VALUES
    (1, 2, N'Spot-on Min Thant! We noticed the same behavior during high-load websocket tests. Great explanation.', DATEADD(DAY, -9, @Now), 0),
    (1, 3, N'Clean architecture and very readable code snippet. Are you using optimistic locking on the wallet balance table?', DATEADD(DAY, -9, @Now), 0),
    (1, 1, N'@Kaung Yes, exactly! We have a RowVersion column configured on TblLinkDropWallet to prevent race condition write skew.', DATEADD(DAY, -8, @Now), 0),
    (2, 12, N'Love the token approach. Makes frontend handoff completely seamless between Figma and Tailwind!', DATEADD(DAY, -8, @Now), 0),
    (2, 15, N'Completely agree May Kyawt! Designers and engineers should always share the same vocabulary.', DATEADD(DAY, -7, @Now), 0),
    (3, 13, N'Voted for Llama-3! The vocabulary expansion with custom Burmese tokens yielded the lowest perplexity in our tests.', DATEADD(DAY, -7, @Now), 0),
    (6, 1, N'Such valuable advice Teacher Thet Htar! Confidence in communication makes all the difference.', DATEADD(DAY, -4, @Now), 0);

    -- Post Likes
    INSERT INTO dbo.TblPostLike (PostId, UserId, CreatedAt, IsDeleted)
    VALUES
    (1, 2, @Now, 0), (1, 3, @Now, 0), (1, 4, @Now, 0), (1, 5, @Now, 0), (1, 6, @Now, 0),
    (2, 1, @Now, 0), (2, 3, @Now, 0), (2, 7, @Now, 0), (2, 12, @Now, 0), (2, 15, @Now, 0),
    (3, 1, @Now, 0), (3, 2, @Now, 0), (3, 4, @Now, 0), (3, 13, @Now, 0), (3, 25, @Now, 0);

    PRINT '>>> Step 18: Seeding Direct Chat Conversations, Messages & Stickers...';
    -- Conversation between Min Thant (1) and May Kyawt Khaing (2)
    INSERT INTO dbo.TblConversation (UserOneId, UserTwoId, LastMessageAt, LastMessagePreview, CreatedAt, IsDeleted)
    VALUES (1, 2, @Now, N'[sticker:/stickers/cool.svg]', DATEADD(DAY, -5, @Now), 0);
    DECLARE @Conv1Id INT = SCOPE_IDENTITY();

    INSERT INTO dbo.TblChatMessage (ConversationId, SenderId, MessageText, IsRead, ReadAt, CreatedAt, IsDeleted)
    VALUES
    (@Conv1Id, 1, N'Hi May Kyawt, how is the new design system update progressing for the Community card components?', 1, DATEADD(MINUTE, -30, @Now), DATEADD(HOUR, -2, @Now), 0),
    (@Conv1Id, 2, N'Hey Min Thant! It is looking fantastic. All tokens are synchronized and responsive breakpoints are crisp.', 1, DATEADD(MINUTE, -20, @Now), DATEADD(HOUR, -1, @Now), 0),
    (@Conv1Id, 1, N'Awesome! Let us deploy the update this afternoon.', 1, DATEADD(MINUTE, -15, @Now), DATEADD(MINUTE, -40, @Now), 0),
    (@Conv1Id, 2, N'[sticker:/stickers/party.svg]', 1, DATEADD(MINUTE, -10, @Now), DATEADD(MINUTE, -20, @Now), 0),
    (@Conv1Id, 1, N'[sticker:/stickers/cool.svg]', 1, DATEADD(MINUTE, -5, @Now), DATEADD(MINUTE, -10, @Now), 0);

    -- Conversation between Min Thant (1) and Kaung Myat Han (3)
    INSERT INTO dbo.TblConversation (UserOneId, UserTwoId, LastMessageAt, LastMessagePreview, CreatedAt, IsDeleted)
    VALUES (1, 3, @Now, N'[sticker:/stickers/like.svg]', DATEADD(DAY, -4, @Now), 0);
    DECLARE @Conv2Id INT = SCOPE_IDENTITY();

    INSERT INTO dbo.TblChatMessage (ConversationId, SenderId, MessageText, IsRead, ReadAt, CreatedAt, IsDeleted)
    VALUES
    (@Conv2Id, 3, N'Bro, I tested the new sticker instant send feature in the chat composer. Works seamlessly!', 1, DATEADD(MINUTE, -40, @Now), DATEADD(HOUR, -3, @Now), 0),
    (@Conv2Id, 1, N'Great to hear Kaung! We also resolved the RowVersion check on reaction inserts.', 1, DATEADD(MINUTE, -35, @Now), DATEADD(HOUR, -2, @Now), 0),
    (@Conv2Id, 3, N'[sticker:/stickers/fire.svg]', 1, DATEADD(MINUTE, -30, @Now), DATEADD(HOUR, -1, @Now), 0),
    (@Conv2Id, 1, N'[sticker:/stickers/like.svg]', 1, DATEADD(MINUTE, -25, @Now), DATEADD(MINUTE, -30, @Now), 0);

    -- Conversation between Min Thant (1) and Dr. Thiri Sandar (5)
    INSERT INTO dbo.TblConversation (UserOneId, UserTwoId, LastMessageAt, LastMessagePreview, CreatedAt, IsDeleted)
    VALUES (1, 5, @Now, N'[sticker:/stickers/love.svg]', DATEADD(DAY, -3, @Now), 0);
    DECLARE @Conv3Id INT = SCOPE_IDENTITY();

    INSERT INTO dbo.TblChatMessage (ConversationId, SenderId, MessageText, IsRead, ReadAt, CreatedAt, IsDeleted)
    VALUES
    (@Conv3Id, 5, N'Min Thant, the AI community members loved the poll on Burmese LLM architectures.', 1, DATEADD(MINUTE, -50, @Now), DATEADD(HOUR, -4, @Now), 0),
    (@Conv3Id, 1, N'Thank you Dr. Thiri! Your insights have been tremendously valuable to our community.', 1, DATEADD(MINUTE, -45, @Now), DATEADD(HOUR, -3, @Now), 0),
    (@Conv3Id, 5, N'[sticker:/stickers/love.svg]', 1, DATEADD(MINUTE, -40, @Now), DATEADD(HOUR, -1, @Now), 0);

    PRINT '>>> Step 19: Seeding Chat Groups & Group Messages...';
    INSERT INTO dbo.TblChatGroup (
        Name, Description, AvatarUrl, BannerUrl, CreatorId, ChatType, JoinFeeLinkDrops,
        CommissionPercentageSnapshot, IsActive, CreatedAt, IsDeleted
    ) VALUES
    (N'CommunityLink Core Engineering', N'Private group for core architects and contributors.',
     N'https://images.unsplash.com/photo-1522071820081-009f0129c71c?auto=format&fit=crop&w=200&q=80',
     NULL, 1, N'FREE', 0, 10.00, 1, DATEADD(DAY, -15, @Now), 0),
    (N'Myanmar Tech Founders Syndicate', N'Executive discussions among tech executives and founders.',
     N'https://images.unsplash.com/photo-1556761175-5973dc0f32e7?auto=format&fit=crop&w=200&q=80',
     NULL, 4, N'PAID', 100, 10.00, 1, DATEADD(DAY, -14, @Now), 0),
    (N'UI/UX Design Jam Lounge', N'Casual design feedback, portfolio reviews, and typography talks.',
     N'https://images.unsplash.com/photo-1507238691740-187a5b1d37b8?auto=format&fit=crop&w=200&q=80',
     NULL, 2, N'FREE', 0, 10.00, 1, DATEADD(DAY, -12, @Now), 0);

    -- Add members to Chat Group 1
    INSERT INTO dbo.TblChatGroupMember (ChatGroupId, UserId, Role, JoinedAt, CreatedAt, IsDeleted)
    VALUES
    (1, 1, N'OWNER', @Now, @Now, 0),
    (1, 2, N'ADMIN', @Now, @Now, 0),
    (1, 3, N'ADMIN', @Now, @Now, 0),
    (1, 5, N'MEMBER', @Now, @Now, 0),
    (1, 6, N'MEMBER', @Now, @Now, 0);

    -- Add messages in Chat Group 1
    INSERT INTO dbo.TblChatGroupMessage (ChatGroupId, SenderId, Content, CreatedAt, IsDeleted)
    VALUES
    (1, 1, N'Welcome team to the CommunityLink Core Engineering group chat!', DATEADD(HOUR, -5, @Now), 0),
    (1, 2, N'Glad to be here! The responsive UI is looking top-notch.', DATEADD(HOUR, -4, @Now), 0),
    (1, 3, N'Backend SignalR hub latency is sitting comfortably at under 15ms.', DATEADD(HOUR, -3, @Now), 0),
    (1, 1, N'[sticker:/stickers/fire.svg]', DATEADD(HOUR, -2, @Now), 0);

    PRINT '>>> Step 20: Seeding Creator Chat Settings & Payout Requests...';
    -- Creator chat settings for top creators
    INSERT INTO dbo.TblCreatorChatSetting (CreatorUserId, IsPrivateChatEnabled, PrivateChatFeeLinkDrops, CreatedAt)
    VALUES
    (1, 1, 50, @Now),
    (2, 1, 40, @Now),
    (3, 1, 35, @Now),
    (4, 1, 100, @Now),
    (5, 1, 60, @Now);

    -- Realistic Payout Requests
    INSERT INTO dbo.TblCreatorPayoutRequest (
        CreatorUserId, AmountLinkDrops, AmountMMK, PaymentMethod, PaymentAccountName,
        PaymentAccountNumber, Status, AdminNote, CreatedAt, ReviewedAt, ReviewedBy
    ) VALUES
    (1, 1500, 75000.00, N'KBZPay', N'Min Thant', N'09420011223', N'COMPLETED', N'Verified & Transferred via KBZPay.', DATEADD(DAY, -5, @Now), DATEADD(DAY, -4, @Now), 1),
    (2, 1000, 50000.00, N'AYA Pay', N'May Kyawt Khaing', N'09971122334', N'COMPLETED', N'Paid successfully.', DATEADD(DAY, -3, @Now), DATEADD(DAY, -2, @Now), 1),
    (3, 800, 40000.00, N'WavePay', N'Kaung Myat Han', N'09420011223', N'PENDING', NULL, DATEADD(HOUR, -12, @Now), NULL, NULL),
    (5, 1200, 60000.00, N'KBZPay', N'Thiri Sandar', N'09798877665', N'PENDING', NULL, DATEADD(HOUR, -6, @Now), NULL, NULL);

    PRINT '>>> Step 21: Seeding Subscription Plans & Verifications...';
    INSERT INTO dbo.TblSubscriptionPlan (
        TargetRoleCode, PlanName, BillingInterval, DurationDays, PriceAmount, LinkDropCost,
        PerksJson, IsActive, CreatedAt
    ) VALUES
    (N'DOMAIN_PRO', N'Domain Professional (Monthly)', N'Monthly', 30, 39.00, 390,
     N'["1-on-1 Paid Advisory Engine (set custom rate)","Public Peer Rating & Review Card","Priority Sub-Community Ownership","Domain Competency Endorsement","Credential Verification Badging"]', 1, @Now),
    (N'DOMAIN_PRO', N'Domain Professional (Annual)', N'Annual', 365, 390.00, 3900,
     N'["All Monthly Perks included","2 Months Free Discount","Expedited 24h Verification Audit","Featured in Domain Pro Directory Shelf"]', 1, @Now),
    (N'PUBLIC_FIGURE', N'Public Figure VIP (Monthly)', N'Monthly', 30, 119.00, 1190,
     N'["Discovery shelf Priority Guaranteed placement","Unlimited Sovereign Communities","0% Platform Fees on Advisory (first 10,000,000 MMK)","Dedicated Admin Concierge","Government ID Verified Badge"]', 1, @Now),
    (N'PUBLIC_FIGURE', N'Public Figure VIP (Annual)', N'Annual', 365, 1190.00, 11900,
     N'["All Public Figure VIP Monthly Perks","Save 238,000 MMK (2 Months Free)","Priority SecOps Review","VIP Platinum Crest"]', 1, @Now);

    -- Identity Verifications
    INSERT INTO dbo.TblIdentityVerification (
        UserId, PlanId, TargetRoleCode, FullLegalName, WorkEmail, ProfessionalUrl,
        IdCardFrontUrl, PaymentMethod, LinkDropPointsDeducted, Status, ReviewNotes, ReviewedByAdminId, ReviewedAtUtc, CreatedAtUtc
    ) VALUES
    (2, 1, N'DOMAIN_PRO', N'May Kyawt Khaing', N'maykyawtkhaing2842@gmail.com', N'https://linkedin.com/in/maykyawt', N'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=600&q=80', N'LinkDropPoints', 390, N'Approved', N'Credentials verified.', 1, DATEADD(DAY, -10, @Now), DATEADD(DAY, -12, @Now)),
    (3, 1, N'DOMAIN_PRO', N'Kaung Myat Han', N'juggerkaung.dev@gmail.com', N'https://github.com/juggerkaung', N'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=600&q=80', N'LinkDropPoints', 390, N'Approved', N'GitHub contributions and portfolio verified.', 1, DATEADD(DAY, -8, @Now), DATEADD(DAY, -9, @Now)),
    (4, 3, N'PUBLIC_FIGURE', N'Aung Kyaw San', N'aungkyaw.san.angel@gmail.com', N'https://linkedin.com/in/aungkyawsan', N'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=600&q=80', N'LinkDropPoints', 1190, N'Approved', N'Executive public profile verified.', 1, DATEADD(DAY, -6, @Now), DATEADD(DAY, -7, @Now));

    PRINT '>>> Step 22: Seeding Platform Settings & System Audit Logs...';
    INSERT INTO dbo.TblPlatformSetting (SettingKey, SettingValue, DataType, Description, UpdatedAt)
    VALUES
    (N'PlatformCommissionPercentage', N'10.00', N'DECIMAL', N'Default platform fee percentage taken on creator payouts and paid chat group access.', @Now),
    (N'LinkDropExchangeRateMMK', N'50.00', N'DECIMAL', N'Exchange rate: 1 LinkDrop point = 50 MMK.', @Now),
    (N'MinPayoutThresholdLinkDrops', N'500', N'INTEGER', N'Minimum balance required for a creator to request a payout.', @Now),
    (N'MaxUploadFileSizeMB', N'25', N'INTEGER', N'Maximum allowed file upload size in megabytes.', @Now);

    INSERT INTO dbo.TblAuditLog (
        ActorType, ActorId, Action, EntityName, EntityId, NewValues, ChangedColumns, IpAddress, CreatedAt, IsDeleted
    ) VALUES
    (N'ADMIN', 1, N'SEED', N'SYSTEM_DATABASE', 1, N'Seeded realistic Myanmar sample dataset with 40+ members, communities, groups, chats, and posts.', N'ALL', N'127.0.0.1', @Now, 0),
    (N'ADMIN', 1, N'UPDATE', N'ROLE_PERMISSION', @RoleId_Admin, N'Full permission matrix configured.', N'Permissions', N'127.0.0.1', @Now, 0);

    PRINT '>>> SUCCESS: All sample data has been seeded successfully!';
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT 'ERROR during seed data insertion: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
