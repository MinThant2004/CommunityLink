/* ==============================================================================
   CommunityLink - Master Presentation Seed Data & DB Clean Reset Script
   ==============================================================================
   - Resets and cleans all application activity and transactional data while preserving Admin accounts & System RBAC roles.
   - Sets up 10 Communities, 50 Sub-Communities, and 150 Groups.
   - Sets up 20 Verified Premium Leaders (10 Domain Professionals, 10 Public Figures) with skills, ratings, and LinkDrops (> 5000).
   - Sets up 10 Normal Real Burmese Users with authentic activities, ratings, and comments.
   - Profile Avatars assigned to all users pointing to existing verified files in /uploads/.
   - Response SLA Text matches exact dropdown options from Upgrade.razor.
   - Populates User Follow network (TblUserFollow).
   - Populates group posts across all 150 groups so groups are active and vibrant.
   - Each premium leader has at least 2 Group Posts and 2 Standalone Posts (including Polls) with real user comments.
   - Populates Content Moderation data with distinct statuses (PENDING, RESOLVED, DISMISSED, ACTIONED).
   - Uniform Password for all generated accounts: Ace@1234
   ============================================================================== */

USE [CommunityLink];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

BEGIN TRANSACTION;

PRINT '1. Cleaning existing data and resetting transactional tables...';

-- Safe cleaning: delete from tables only if they exist in the database
IF OBJECT_ID('dbo.TblContentReport', 'U') IS NOT NULL DELETE FROM [dbo].[TblContentReport];

IF OBJECT_ID('dbo.TblChatGroupMessageReaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMessageReaction];
IF OBJECT_ID('dbo.TblChatGroupMessageUserState', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMessageUserState];
IF OBJECT_ID('dbo.TblChatGroupMessage', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMessage];
IF OBJECT_ID('dbo.TblChatGroupInviteLink', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupInviteLink];
IF OBJECT_ID('dbo.TblChatGroupInvite', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupInvite];
IF OBJECT_ID('dbo.TblChatGroupJoinRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupJoinRequest];
IF OBJECT_ID('dbo.TblChatGroupBan', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupBan];
IF OBJECT_ID('dbo.TblChatGroupMember', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupMember];
IF OBJECT_ID('dbo.TblChatGroupPaymentTransaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroupPaymentTransaction];
IF OBJECT_ID('dbo.TblChatGroup', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatGroup];

IF OBJECT_ID('dbo.TblChatMessageReaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatMessageReaction];
IF OBJECT_ID('dbo.TblChatMessageUserState', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatMessageUserState];
IF OBJECT_ID('dbo.TblChatMessage', 'U') IS NOT NULL DELETE FROM [dbo].[TblChatMessage];
IF OBJECT_ID('dbo.TblConversation', 'U') IS NOT NULL DELETE FROM [dbo].[TblConversation];

IF OBJECT_ID('dbo.TblNotification', 'U') IS NOT NULL DELETE FROM [dbo].[TblNotification];
IF OBJECT_ID('dbo.TblUserActivity', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserActivity];
IF OBJECT_ID('dbo.TblUserFollow', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserFollow];
IF OBJECT_ID('dbo.TblUserBlock', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserBlock];
IF OBJECT_ID('dbo.TblUserRating', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserRating];
IF OBJECT_ID('dbo.TblSkillEndorsement', 'U') IS NOT NULL DELETE FROM [dbo].[TblSkillEndorsement];
IF OBJECT_ID('dbo.TblUserSkill', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserSkill];
IF OBJECT_ID('dbo.TblIdentityVerification', 'U') IS NOT NULL DELETE FROM [dbo].[TblIdentityVerification];
IF OBJECT_ID('dbo.TblUserVerificationAudit', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserVerificationAudit];
IF OBJECT_ID('dbo.TblUserSubscription', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserSubscription];
IF OBJECT_ID('dbo.TblLinkDropTransaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropTransaction];
IF OBJECT_ID('dbo.TblLinkDropPurchaseProof', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropPurchaseProof];
IF OBJECT_ID('dbo.TblLinkDropPurchase', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropPurchase];
IF OBJECT_ID('dbo.TblLinkDropWallet', 'U') IS NOT NULL DELETE FROM [dbo].[TblLinkDropWallet];
IF OBJECT_ID('dbo.TblCreatorPayoutRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblCreatorPayoutRequest];
IF OBJECT_ID('dbo.TblCreatorChatSetting', 'U') IS NOT NULL DELETE FROM [dbo].[TblCreatorChatSetting];
IF OBJECT_ID('dbo.TblPrivateChatPaymentTransaction', 'U') IS NOT NULL DELETE FROM [dbo].[TblPrivateChatPaymentTransaction];

IF OBJECT_ID('dbo.TblPollVote', 'U') IS NOT NULL DELETE FROM [dbo].[TblPollVote];
IF OBJECT_ID('dbo.TblPollOption', 'U') IS NOT NULL DELETE FROM [dbo].[TblPollOption];
IF OBJECT_ID('dbo.TblPoll', 'U') IS NOT NULL DELETE FROM [dbo].[TblPoll];
IF OBJECT_ID('dbo.TblComment', 'U') IS NOT NULL DELETE FROM [dbo].[TblComment];
IF OBJECT_ID('dbo.TblPostLike', 'U') IS NOT NULL DELETE FROM [dbo].[TblPostLike];
IF OBJECT_ID('dbo.TblPostShare', 'U') IS NOT NULL DELETE FROM [dbo].[TblPostShare];
IF OBJECT_ID('dbo.TblPostImage', 'U') IS NOT NULL DELETE FROM [dbo].[TblPostImage];
IF OBJECT_ID('dbo.TblSavedPost', 'U') IS NOT NULL DELETE FROM [dbo].[TblSavedPost];
IF OBJECT_ID('dbo.TblSavedAccount', 'U') IS NOT NULL DELETE FROM [dbo].[TblSavedAccount];
IF OBJECT_ID('dbo.TblPost', 'U') IS NOT NULL DELETE FROM [dbo].[TblPost];

IF OBJECT_ID('dbo.TblGroupRating', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroupRating];
IF OBJECT_ID('dbo.TblGroupJoinRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroupJoinRequest];
IF OBJECT_ID('dbo.TblGroupMember', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroupMember];
IF OBJECT_ID('dbo.TblGroup', 'U') IS NOT NULL DELETE FROM [dbo].[TblGroup];

IF OBJECT_ID('dbo.TblCommunityRating', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityRating];
IF OBJECT_ID('dbo.TblCommunityJoinRequest', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityJoinRequest];
IF OBJECT_ID('dbo.TblCommunityMember', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityMember];
IF OBJECT_ID('dbo.TblCommunityAuditLog', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunityAuditLog];
IF OBJECT_ID('dbo.TblCommunity', 'U') IS NOT NULL DELETE FROM [dbo].[TblCommunity];

IF OBJECT_ID('dbo.TblUserRole', 'U') IS NOT NULL DELETE FROM [dbo].[TblUserRole] WHERE [UserId] IN (SELECT [UserId] FROM [dbo].[TblUser] WHERE [UserName] != 'admin' AND [Email] NOT LIKE '%admin%');
IF OBJECT_ID('dbo.TblUser', 'U') IS NOT NULL DELETE FROM [dbo].[TblUser] WHERE [UserName] != 'admin' AND [Email] NOT LIKE '%admin%';

-- Ensure TblContentReport exists
IF OBJECT_ID('dbo.TblContentReport', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TblContentReport] (
        [ContentReportId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ContentType] NVARCHAR(20) NOT NULL,
        [ContentId] INT NOT NULL,
        [ReporterUserId] INT NOT NULL,
        [ReasonCategory] NVARCHAR(100) NOT NULL,
        [Details] NVARCHAR(1000) NULL,
        [Status] NVARCHAR(20) NOT NULL CONSTRAINT DF_TblContentReport_Status DEFAULT ('PENDING'),
        [HandledByAdminId] INT NULL,
        [AdminNote] NVARCHAR(500) NULL,
        [HandledAt] DATETIME2(7) NULL,
        [CreatedAt] DATETIME2(7) NOT NULL CONSTRAINT DF_TblContentReport_CreatedAt DEFAULT (SYSUTCDATETIME())
    );
END;

DELETE FROM [dbo].[TblRole] WHERE [RoleCode] IN ('DOMAIN_PRO', 'MEMBER');

PRINT '2. Inserting Roles verification...';
-- Ensure Target Roles Exist
IF NOT EXISTS (SELECT 1 FROM [dbo].[TblRole] WHERE [RoleCode] = 'DOMAIN_PROFESSIONAL')
BEGIN
    INSERT INTO [dbo].[TblRole] ([RoleCode], [RoleName], [Description], [IsSystemRole], [RoleType], [CreatedAt], [IsDeleted])
    VALUES ('DOMAIN_PROFESSIONAL', 'Domain Professional', 'Verified Domain Professional & Corporate Leader', 0, 0, SYSUTCDATETIME(), 0);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[TblRole] WHERE [RoleCode] = 'PUBLIC_FIGURE')
BEGIN
    INSERT INTO [dbo].[TblRole] ([RoleCode], [RoleName], [Description], [IsSystemRole], [RoleType], [CreatedAt], [IsDeleted])
    VALUES ('PUBLIC_FIGURE', 'Public Figure', 'Verified Public Figure, Cultural Icon & Influencer', 0, 0, SYSUTCDATETIME(), 0);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[TblRole] WHERE [RoleCode] = 'DEFAULT_USER')
BEGIN
    INSERT INTO [dbo].[TblRole] ([RoleCode], [RoleName], [Description], [IsSystemRole], [RoleType], [CreatedAt], [IsDeleted])
    VALUES ('DEFAULT_USER', 'Default User', 'Standard Active Network Member', 1, 0, SYSUTCDATETIME(), 0);
END;

DECLARE @RoleDomainProId INT = (SELECT TOP 1 [RoleId] FROM [dbo].[TblRole] WHERE [RoleCode] = 'DOMAIN_PROFESSIONAL');
DECLARE @RolePublicFigureId INT = (SELECT TOP 1 [RoleId] FROM [dbo].[TblRole] WHERE [RoleCode] = 'PUBLIC_FIGURE');
DECLARE @RoleMemberId INT = (SELECT TOP 1 [RoleId] FROM [dbo].[TblRole] WHERE [RoleCode] = 'DEFAULT_USER');

PRINT '3. Inserting 30 Real Myanmar Users (10 Domain Pros, 10 Public Figures, 10 Members)...';
SET IDENTITY_INSERT [dbo].[TblUser] ON;
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (101, 'aunglansang', 'AUNGLANSANG', 'aungla@onefc.com', 'AUNGLA@ONEFC.COM', 'Aung La N Sang', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'The Burmese Python | Former 2-Division ONE Championship World Champion (Middleweight & Light Heavyweight). Dedicated to martial arts mastery, youth athletic conditioning, and mental grit.', 'https://upload.wikimedia.org/wikipedia/commons/8/87/Aung_La_N_Sang.jpg', 1, 1, 5.0, 5, DATEADD(day, -180, SYSUTCDATETIME()), DATEADD(hour, -3, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Myanmar', 'MMA Elite Coaching');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (102, 'thantmyintu', 'THANTMYINTU', 'tmyintu@heritage.org', 'TMYINTU@HERITAGE.ORG', 'Dr. Thant Myint-U', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Historian, former United Nations official, Cambridge scholar, and author of ''The River of Lost Footsteps'' and ''The Hidden History of Burma''. Advocating cultural heritage preservation and diplomacy.', 'https://assets.frontiermyanmar.net/2020/02/thant-myint-u-everything-is-interlinked-1582113030-1024x682.jpg', 1, 1, 4.8, 5, DATEADD(day, -175, SYSUTCDATETIME()), DATEADD(hour, -5, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Myanmar', 'Southeast Asian History');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (103, 'drcynthiamaung', 'DRCYNTHIAMAUNG', 'cynthia@maetao.org', 'CYNTHIA@MAETAO.ORG', 'Dr. Cynthia Maung', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Founder of Mae Tao Clinic. Ramon Magsaysay Award, Sydney Peace Prize, and Jonathan Mann Award recipient; pioneering border healthcare, maternal medicine, and child health systems.', 'https://upload.wikimedia.org/wikipedia/commons/8/84/Dr._Cynthia_Maung.jpg', 1, 1, 5.0, 5, DATEADD(day, -170, SYSUTCDATETIME()), DATEADD(hour, -8, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Myanmar', 'Humanitarian Medicine');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (104, 'paingtakhon', 'PAINGTAKHON', 'paing@takhonmedia.com', 'PAING@TAKHONMEDIA.COM', 'Paing Takhon', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'International Model, Actor, and Animal Welfare Advocate. Ranked #1 in TC Candler''s 100 Most Handsome Faces; passionate about visual storytelling, cinema arts, and community pet shelters.', 'https://upload.wikimedia.org/wikipedia/commons/9/9f/%D0%9F%D0%B0%D0%B8%D0%BD%D0%B3_%D0%A2%D0%B0%D0%BA%D1%85%D0%BE%D0%BD.png', 1, 1, 4.8, 5, DATEADD(day, -165, SYSUTCDATETIME()), DATEADD(hour, -12, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Myanmar', 'Fashion & Brand Ambassadorship');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (105, 'utejaniya', 'UTEJANIYA', 'tejaniya@shweooomin.org', 'TEJANIYA@SHWEOOOMIN.ORG', 'Sayadaw U Tejaniya', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Theravada Buddhist Meditation Master. Guiding international practitioners in daily life mindfulness, awareness of the observing mind, and practical Dhamma wisdom.', 'https://upload.wikimedia.org/wikipedia/commons/6/65/Sayadaw_U_Tejaniya_and_Bhikku_Kumara_CROPPED.jpg', 1, 1, 5.0, 5, DATEADD(day, -160, SYSUTCDATETIME()), DATEADD(hour, -15, SYSUTCDATETIME()), 0, 'Available', 'Replies within 36 hours', 'Myanmar', 'Vipassana Mindfulness');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (106, 'zarganar', 'ZARGANAR', 'zarganar@freeart.org', 'ZARGANAR@FREEART.ORG', 'Zarganar (Maung Thura)', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Acclaimed satirist, filmmaker, and social activist. Recipient of the Prince Claus Award and PEN/Barbara Goldsmith Freedom to Write Award; mentor in performance art and social reform.', 'https://upload.wikimedia.org/wikipedia/commons/c/c2/U-thura.jpg', 1, 1, 4.8, 4, DATEADD(day, -155, SYSUTCDATETIME()), DATEADD(hour, -18, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Myanmar', 'Satirical Writing');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (107, 'minwaeaung', 'MINWAEAUNG', 'minwaeaung@oldhanoi.art', 'MINWAEAUNG@OLDHANOI.ART', 'Min Wae Aung', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Internationally celebrated fine artist. Renowned for meditative monk-procession canvases exhibited across London, New York, Hong Kong, and Paris galleries.', 'https://thukhuma.org/wp-content/uploads/2016/06/min-wae-aung.jpg', 1, 1, 4.8, 4, DATEADD(day, -150, SYSUTCDATETIME()), DATEADD(hour, -22, SYSUTCDATETIME()), 0, 'Available', 'Replies within 36 hours', 'Myanmar', 'Contemporary Oil Painting');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (108, 'waiwainu', 'WAIWAINU', 'waiwainu@peaceadvocacy.org', 'WAIWAINU@PEACEADVOCACY.ORG', 'Wai Wai Nu', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Founder of Women''s Peace Network. Time Magazine Next Generation Leader, recipient of the US State Department International Women of Courage Award; advocate for youth empowerment and peace.', 'https://upload.wikimedia.org/wikipedia/commons/4/41/Wai_Wai_Nu.jpg', 1, 1, 4.8, 4, DATEADD(day, -145, SYSUTCDATETIME()), DATEADD(hour, -26, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Myanmar', 'Peacebuilding & Dialogue');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (109, 'drsaikhayguan', 'DRSAIKHAYGUAN', 'dr.saikhayguan@medresearch.org', 'DR.SAIKHAYGUAN@MEDRESEARCH.ORG', 'Dr. Sai Khay Guan', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Consultant Medical Oncologist & Clinical Researcher. Leading international clinician specializing in oncology therapeutics, cancer genetics, and medical academic research.', 'https://www.nuhs.edu.sg/sites/default/files/2023-11/prof-yeoh-khay-guan.jpg', 1, 1, 4.8, 4, DATEADD(day, -140, SYSUTCDATETIME()), DATEADD(hour, -30, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Myanmar', 'Clinical Oncology');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (110, 'mawnaing', 'MAWNAING', 'mawnaing@cineburma.com', 'MAWNAING@CINEBURMA.COM', 'Maw Naing', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Award-winning independent film director & visual poet. Creator of ''The Monk'' and ''MA - Cry of Silence'', premiered at Karlovy Vary, Busan IFF, and international film festivals.', 'https://asianwiki.com/images/5/5d/The_Maw_Naing-p1.jpg', 1, 1, 4.8, 4, DATEADD(day, -135, SYSUTCDATETIME()), DATEADD(hour, -34, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Myanmar', 'Independent Cinema Direction');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (111, 'zawzaw', 'ZAWZAW', 'zawzaw@maxmyanmar.com', 'ZAWZAW@MAXMYANMAR.COM', 'Zaw Zaw', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Chairman of Max Myanmar Group & AYA Financial Group. Passionate about commercial banking innovation, national infrastructure development, sports academies, and philanthropic healthcare.', 'https://upload.wikimedia.org/wikipedia/commons/b/b1/U_Zaw_Zaw_donates_10000_footballs_%28cropped%29.jpg', 1, 1, 4.8, 5, DATEADD(day, -182, SYSUTCDATETIME()), DATEADD(hour, -4, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Yangon, Myanmar', 'Corporate Strategy');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (112, 'aungkowin', 'AUNGKOWIN', 'aungkowin@kbzgroup.com', 'AUNGKOWIN@KBZGROUP.COM', 'Aung Ko Win', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Chairman of KBZ Group of Companies. Spearheading national financial inclusion via KBZ Bank, digital fintech ecosystems with KBZPay, and domestic aviation services.', 'https://www.irrawaddy.com/wp-content/uploads/2023/08/Junta-Crony-April-18-feat.jpg', 1, 1, 5.0, 5, DATEADD(day, -178, SYSUTCDATETIME()), DATEADD(hour, -7, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Yangon, Myanmar', 'Digital Banking');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (113, 'sergepun', 'SERGEPUN', 'sergepun@yoma.com.mm', 'SERGEPUN@YOMA.COM.MM', 'Serge Pun (Theim Wai)', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Executive Chairman of Yoma Strategic Holdings & First Myanmar Investment (FMI). Pioneer of international real estate master-developments, automotive hubs, and capital market ventures.', 'https://upload.wikimedia.org/wikipedia/commons/2/29/Serge_Pun%2C_Chairman%2C_Serge_Pun_%26_Associates%2C_Myanmar_%E2%80%93_Myanmar_is_good_news._%2831234690092%29.jpg', 1, 1, 4.8, 5, DATEADD(day, -174, SYSUTCDATETIME()), DATEADD(hour, -9, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Yangon, Myanmar', 'Real Estate Development');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (114, 'aikhtun', 'AIKHTUN', 'aikhtun@shwetaung.com', 'AIKHTUN@SHWETAUNG.COM', 'Aik Htun', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Chairman of Shwe Taung Group. Building sustainable modern infrastructure, prime retail mixed-use hubs (Junction City), certified green construction, and hydropower transmission.', 'https://www.shwetaunggroup.com/wp-content/uploads/2017/03/management1.jpg', 1, 1, 4.8, 4, DATEADD(day, -169, SYSUTCDATETIME()), DATEADD(hour, -13, SYSUTCDATETIME()), 0, 'Available', 'Replies within 36 hours', 'Yangon, Myanmar', 'Mega-Project Engineering');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (115, 'drsaisamhtun', 'DRSAISAMHTUN', 'saisamhtun@loihein.com', 'SAISAMHTUN@LOIHEIN.COM', 'Dr. Sai Sam Htun', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Founder & Chairman of Loi Hein Group. Pioneering Myanmar''s leading FMCG manufacturing, iconic beverage brands (Alpine), automated packaging plants, and nationwide distribution chains.', 'https://rubyhillmicrofinance.com/images/dr-sai-sam-htun.jpg', 1, 1, 4.8, 5, DATEADD(day, -164, SYSUTCDATETIME()), DATEADD(hour, -16, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Yangon, Myanmar', 'FMCG Manufacturing');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (116, 'tayza', 'TAYZA', 'tayza@htoogroup.com', 'TAYZA@HTOOGROUP.COM', 'Tay Za', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Chairman of Htoo Group of Companies. Renowned leader in international trade logistics, boutique eco-resorts & luxury hospitality, regional aviation networks, and timber processing.', 'https://www.irrawaddy.com/wp-content/uploads/2022/12/TayZa-1.jpg', 1, 1, 4.5, 4, DATEADD(day, -159, SYSUTCDATETIME()), DATEADD(hour, -19, SYSUTCDATETIME()), 0, 'Available', 'Replies within 48 hours', 'Yangon, Myanmar', 'Luxury Hospitality');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (117, 'michaelmoemyint', 'MICHAELMOEMYINT', 'mmoemyint@mprlexp.com', 'MMOEMYINT@MPRLEXP.COM', 'Michael Moe Myint', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Founder & CEO of Myint & Associates and MPRL E&P Group. Offshore deepwater energy exploration, industrial HSE safety benchmarks, and independent upstream petroleum engineering.', 'https://www.peninsula-press.com/wp-content/uploads/2015/02/UmoeMynt-2.jpg', 1, 1, 4.8, 4, DATEADD(day, -154, SYSUTCDATETIME()), DATEADD(hour, -23, SYSUTCDATETIME()), 0, 'Available', 'Replies within 36 hours', 'Yangon, Myanmar', 'Offshore Exploration');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (118, 'waiminhtin', 'WAIMINHTIN', 'mikehtin@fortune.com.mm', 'MIKEHTIN@FORTUNE.COM.MM', 'Wai Min Htin (Mike)', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Managing Director of Fortune International Group. Telecom cellular towers, fiber optic enterprise backbones, IT infrastructure deployment, and consumer tech logistics.', 'https://fortunemm.com/wp-content/uploads/2020/09/Chairman-Dr.-Mya-Han.jpg', 1, 1, 4.8, 4, DATEADD(day, -149, SYSUTCDATETIME()), DATEADD(hour, -27, SYSUTCDATETIME()), 0, 'Available', 'Replies within 24 hours', 'Yangon, Myanmar', 'Telecommunications');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (119, 'drthaunghan', 'DRTHAUNGHAN', 'thaunghan@cbbank.com.mm', 'THAUNGHAN@CBBANK.COM.MM', 'Dr. Thaung Han', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Managing Director & CEO of CB Bank. Leader in modern commercial and trade banking, core digital enterprise modernization, and open API financial architectures.', 'https://myanmariod.com/wp-content/uploads/2024/09/dr_thaung_han_img.png', 1, 1, 4.8, 4, DATEADD(day, -144, SYSUTCDATETIME()), DATEADD(hour, -31, SYSUTCDATETIME()), 0, 'Available', 'Replies within 12 hours', 'Yangon, Myanmar', 'Commercial Banking');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (120, 'winaung', 'WINAUNG', 'winaung@dagongroup.com', 'WINAUNG@DAGONGROUP.COM', 'Win Aung', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Chairman of Dagon Group of Companies & Former President of UMFCCI. Deep-sea port terminals, regional agricultural export value chains, and modern industrial zone master-planning.', 'https://dagon-group.com/wp-content/uploads/2013/09/chairman-message1.jpg', 1, 1, 4.5, 4, DATEADD(day, -139, SYSUTCDATETIME()), DATEADD(hour, -35, SYSUTCDATETIME()), 0, 'Available', 'Replies within 36 hours', 'Yangon, Myanmar', 'Port Logistics Management');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (121, 'aungkyawzin', 'AUNGKYAWZIN', 'aungkyawzin@gmail.com', 'AUNGKYAWZIN@GMAIL.COM', 'Aung Kyaw Zin', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Junior Backend Developer | .NET & C# Learner', '/uploads/posts/202610/15983265f7f54c2faa62419918ed85e8_639264352658469432.jpg', 0, 1, 5.0, 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(hour, -2, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Kamayut, Yangon', 'Junior Backend Developer | .NET & C# Learner');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (122, 'thazinmoe', 'THAZINMOE', 'thazinmoe.mkt@gmail.com', 'THAZINMOE.MKT@GMAIL.COM', 'Thazin Moe', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Digital Marketing Freelancer & Content Creator', '/uploads/posts/202610/29de9a9b8f4d4b7c948e966747d0a9c0_639264352665934534.jpg', 0, 1, 5.0, 0, DATEADD(day, -105, SYSUTCDATETIME()), DATEADD(hour, -6, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Sanchaung, Yangon', 'Digital Marketing Freelancer & Content Creator');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (123, 'heinhtetzaw', 'HEINHTETZAW', 'heinhtet.uit@gmail.com', 'HEINHTET.UIT@GMAIL.COM', 'Hein Htet Zaw', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Final Year CS Student @ UIT | Mobile App & Flutter Explorer', '/uploads/posts/202610/2a965a7821d34c1393ff992c8618bb8e_639264352654733706.jpg', 0, 1, 5.0, 0, DATEADD(day, -98, SYSUTCDATETIME()), DATEADD(hour, -9, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Hlaing, Yangon', 'Final Year CS Student @ UIT | Mobile App & Flutter Explorer');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (124, 'khinyadanar', 'KHINYADANAR', 'khinyadanar.design@gmail.com', 'KHINYADANAR.DESIGN@GMAIL.COM', 'Khin Yadanar Oo', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'UI/UX Design Learner | Figma & Web Design', '/uploads/posts/202610/38676668edbe4fc8b2e29de42690de89_639283387193117772.jpg', 0, 1, 5.0, 0, DATEADD(day, -90, SYSUTCDATETIME()), DATEADD(hour, -13, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Chanayethazan, Mandalay', 'UI/UX Design Learner | Figma & Web Design');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (125, 'minkhantkyaw', 'MINKHANTKYAW', 'minkhant.mdy@gmail.com', 'MINKHANT.MDY@GMAIL.COM', 'Min Khant Kyaw', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Small business owner @ Mandalay Tea & Snacks', '/uploads/posts/202610/455134739cf04f1ba65ce74f8b17018f_639289328536512425.jpg', 0, 1, 5.0, 0, DATEADD(day, -85, SYSUTCDATETIME()), DATEADD(hour, -16, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Maha Aungmye, Mandalay', 'Small business owner @ Mandalay Tea & Snacks');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (126, 'sumyatnoe', 'SUMYATNOE', 'sumyatnoe.edu@gmail.com', 'SUMYATNOE.EDU@GMAIL.COM', 'Su Myat Noe', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'High school English tutor & Language Enthusiast', '/uploads/posts/202610/4689cc5bb67a46f8a9e890610841461a_639279804033667591.jpg', 0, 1, 5.0, 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(hour, -20, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Taunggyi, Shan State', 'High school English tutor & Language Enthusiast');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (127, 'zayarlin', 'ZAYARLIN', 'zayarlin.eng@gmail.com', 'ZAYARLIN.ENG@GMAIL.COM', 'Zayar Lin', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Civil Engineering Graduate | Urban Architecture & Photography', '/uploads/posts/202610/64caf7e8a2d041c8a0a65824cb88c0a2_639283359358458377.jpg', 0, 1, 5.0, 0, DATEADD(day, -75, SYSUTCDATETIME()), DATEADD(hour, -24, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Pyin Oo Lwin', 'Civil Engineering Graduate | Urban Architecture & Photography');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (128, 'eiphyusin', 'EIPHYUSIN', 'eiphyusin.hr@gmail.com', 'EIPHYUSIN.HR@GMAIL.COM', 'Ei Phyu Sin', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'HR Executive | Workplace culture & professional networking', '/uploads/posts/202610/8850f2ebf9ca4edda9dbf489c8893c7f_639273729741980335.jpg', 0, 1, 5.0, 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(hour, -29, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Bahan, Yangon', 'HR Executive | Workplace culture & professional networking');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (129, 'kaungmyatthu', 'KAUNGMYATTHU', 'kaungmyat.photo@gmail.com', 'KAUNGMYAT.PHOTO@GMAIL.COM', 'Kaung Myat Thu', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Amateur Landscape Photographer & Travel Blogger', '/uploads/posts/202610/9172be0a5573462eb03ecdde84f8b1d6_639264352661925222.jpg', 0, 1, 5.0, 0, DATEADD(day, -65, SYSUTCDATETIME()), DATEADD(hour, -33, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Mawlamyine, Mon State', 'Amateur Landscape Photographer & Travel Blogger');
INSERT INTO [dbo].[TblUser] ([UserId], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [DisplayName], [PasswordHash], [Bio], [AvatarUrl], [IsVerified], [IsActive], [AverageRating], [RatingCount], [CreatedAt], [LastLoginAt], [IsDeleted], [AvailabilityStatus], [ResponseSlaText], [Location], [Headline])
VALUES (130, 'phyuphyuthin', 'PHYUPHYUTHIN', 'phyuphyuthin.fin@gmail.com', 'PHYUPHYUTHIN.FIN@GMAIL.COM', 'Phyu Phyu Thin', '$2b$12$o7OZvR0CauClATYL3fbbBOvYfy27ec8GNlwdcwXfikfoEC8TmrIhG', 'Accounting & Finance Officer | Lifelong learner', '/uploads/posts/202610/945491f8224e44468d240f8cc3561e4a_639283657961211862.jpg', 0, 1, 5.0, 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(hour, -37, SYSUTCDATETIME()), 0, 'Active', 'Replies within 24 hours', 'Bago', 'Accounting & Finance Officer | Lifelong learner');
SET IDENTITY_INSERT [dbo].[TblUser] OFF;

PRINT '4. Assigning User Roles & Premium Subscriptions...';
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (101, @RoleDomainProId, DATEADD(day, -180, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (102, @RoleDomainProId, DATEADD(day, -175, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (103, @RoleDomainProId, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (104, @RoleDomainProId, DATEADD(day, -165, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (105, @RoleDomainProId, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (106, @RoleDomainProId, DATEADD(day, -155, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (107, @RoleDomainProId, DATEADD(day, -150, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (108, @RoleDomainProId, DATEADD(day, -145, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (109, @RoleDomainProId, DATEADD(day, -140, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (110, @RoleDomainProId, DATEADD(day, -135, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (111, @RolePublicFigureId, DATEADD(day, -182, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (112, @RolePublicFigureId, DATEADD(day, -178, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (113, @RolePublicFigureId, DATEADD(day, -174, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (114, @RolePublicFigureId, DATEADD(day, -169, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (115, @RolePublicFigureId, DATEADD(day, -164, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (116, @RolePublicFigureId, DATEADD(day, -159, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (117, @RolePublicFigureId, DATEADD(day, -154, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (118, @RolePublicFigureId, DATEADD(day, -149, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (119, @RolePublicFigureId, DATEADD(day, -144, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (120, @RolePublicFigureId, DATEADD(day, -139, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (121, @RoleMemberId, DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (122, @RoleMemberId, DATEADD(day, -105, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (123, @RoleMemberId, DATEADD(day, -98, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (124, @RoleMemberId, DATEADD(day, -90, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (125, @RoleMemberId, DATEADD(day, -85, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (126, @RoleMemberId, DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (127, @RoleMemberId, DATEADD(day, -75, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (128, @RoleMemberId, DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (129, @RoleMemberId, DATEADD(day, -65, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRole] ([UserId], [RoleId], [CreatedAt], [IsDeleted]) VALUES (130, @RoleMemberId, DATEADD(day, -60, SYSUTCDATETIME()), 0);

PRINT '5. Populating LinkDrop Wallets (> 5000 for each premium leader)...';
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (101, 26500, 21500, 5000, DATEADD(day, -180, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (102, 13500, 8500, 5000, DATEADD(day, -175, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (103, 11200, 6200, 5000, DATEADD(day, -170, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (104, 22400, 17400, 5000, DATEADD(day, -165, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (105, 9800, 4800, 5000, DATEADD(day, -160, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (106, 12500, 7500, 5000, DATEADD(day, -155, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (107, 15400, 10400, 5000, DATEADD(day, -150, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (108, 10500, 5500, 5000, DATEADD(day, -145, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (109, 14800, 9800, 5000, DATEADD(day, -140, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (110, 11900, 6900, 5000, DATEADD(day, -135, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (111, 28500, 25500, 3000, DATEADD(day, -182, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (112, 35000, 32000, 3000, DATEADD(day, -178, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (113, 24000, 21000, 3000, DATEADD(day, -174, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (114, 19500, 16500, 3000, DATEADD(day, -169, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (115, 16800, 13800, 3000, DATEADD(day, -164, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (116, 21000, 18000, 3000, DATEADD(day, -159, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (117, 18200, 15200, 3000, DATEADD(day, -154, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (118, 14500, 11500, 3000, DATEADD(day, -149, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (119, 17400, 14400, 3000, DATEADD(day, -144, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (120, 15900, 12900, 3000, DATEADD(day, -139, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (121, 500, 500, 0, DATEADD(day, -110, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (122, 500, 500, 0, DATEADD(day, -105, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (123, 500, 500, 0, DATEADD(day, -98, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (124, 500, 500, 0, DATEADD(day, -90, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (125, 500, 500, 0, DATEADD(day, -85, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (126, 500, 500, 0, DATEADD(day, -80, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (127, 500, 500, 0, DATEADD(day, -75, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (128, 500, 500, 0, DATEADD(day, -70, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (129, 500, 500, 0, DATEADD(day, -65, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblLinkDropWallet] ([UserId], [Balance], [PurchasedBalance], [EarnedBalance], [CreatedAt]) VALUES (130, 500, 500, 0, DATEADD(day, -60, SYSUTCDATETIME()));

PRINT '6. Populating Skills for Premium Users...';
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (101, 'MMA Elite Coaching', 5, 1, 1, DATEADD(day, -180, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (101, 'Athletic Conditioning', 4, 1, 2, DATEADD(day, -180, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (101, 'Mental Toughness', 3, 1, 3, DATEADD(day, -180, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (101, 'Youth Sports Mentorship', 2, 1, 4, DATEADD(day, -180, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (102, 'Southeast Asian History', 5, 1, 1, DATEADD(day, -175, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (102, 'Diplomatic Affairs', 4, 1, 2, DATEADD(day, -175, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (102, 'Cultural Heritage Policy', 3, 1, 3, DATEADD(day, -175, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (102, 'Non-Fiction Authorship', 2, 1, 4, DATEADD(day, -175, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (103, 'Humanitarian Medicine', 5, 1, 1, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (103, 'Border Health Systems', 4, 1, 2, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (103, 'Maternal Care Policy', 3, 1, 3, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (103, 'Community Health Training', 2, 1, 4, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (104, 'Fashion & Brand Ambassadorship', 5, 1, 1, DATEADD(day, -165, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (104, 'Acting & Visual Arts', 4, 1, 2, DATEADD(day, -165, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (104, 'Animal Welfare Activism', 3, 1, 3, DATEADD(day, -165, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (104, 'Social Media Engagement', 2, 1, 4, DATEADD(day, -165, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (105, 'Vipassana Mindfulness', 5, 1, 1, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (105, 'Mind State Observation', 4, 1, 2, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (105, 'Buddhist Philosophy', 3, 1, 3, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (105, 'Mental Well-Being Mentorship', 2, 1, 4, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (106, 'Satirical Writing', 5, 1, 1, DATEADD(day, -155, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (106, 'Film Directing', 4, 1, 2, DATEADD(day, -155, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (106, 'Performing Arts & Comedy', 3, 1, 3, DATEADD(day, -155, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (106, 'Civic Education', 2, 1, 4, DATEADD(day, -155, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (107, 'Contemporary Oil Painting', 5, 1, 1, DATEADD(day, -150, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (107, 'Exhibition Curation', 4, 1, 2, DATEADD(day, -150, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (107, 'Color Harmony Theory', 3, 1, 3, DATEADD(day, -150, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (107, 'Fine Arts Mentorship', 2, 1, 4, DATEADD(day, -150, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (108, 'Peacebuilding & Dialogue', 5, 1, 1, DATEADD(day, -145, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (108, 'Women Human Rights Law', 4, 1, 2, DATEADD(day, -145, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (108, 'Youth Civic Mobilization', 3, 1, 3, DATEADD(day, -145, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (108, 'International Advocacy', 2, 1, 4, DATEADD(day, -145, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (109, 'Clinical Oncology', 5, 1, 1, DATEADD(day, -140, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (109, 'Evidence-Based Therapeutics', 4, 1, 2, DATEADD(day, -140, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (109, 'Medical Data Analytics', 3, 1, 3, DATEADD(day, -140, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (109, 'Healthcare Mentorship', 2, 1, 4, DATEADD(day, -140, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (110, 'Independent Cinema Direction', 5, 1, 1, DATEADD(day, -135, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (110, 'Screenwriting & Storytelling', 4, 1, 2, DATEADD(day, -135, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (110, 'Cinematography Aesthetics', 3, 1, 3, DATEADD(day, -135, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (110, 'Creative Production', 2, 1, 4, DATEADD(day, -135, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (111, 'Corporate Strategy', 5, 1, 1, DATEADD(day, -182, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (111, 'Banking & Finance', 4, 1, 2, DATEADD(day, -182, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (111, 'Infrastructure Leadership', 3, 1, 3, DATEADD(day, -182, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (111, 'CSR & Sports Development', 2, 1, 4, DATEADD(day, -182, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (112, 'Digital Banking', 5, 1, 1, DATEADD(day, -178, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (112, 'Fintech Ecosystems', 4, 1, 2, DATEADD(day, -178, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (112, 'Financial Inclusion', 3, 1, 3, DATEADD(day, -178, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (112, 'Aviation Management', 2, 1, 4, DATEADD(day, -178, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (113, 'Real Estate Development', 5, 1, 1, DATEADD(day, -174, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (113, 'Cross-Border Investment', 4, 1, 2, DATEADD(day, -174, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (113, 'Capital Markets', 3, 1, 3, DATEADD(day, -174, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (113, 'Corporate Governance', 2, 1, 4, DATEADD(day, -174, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (114, 'Mega-Project Engineering', 5, 1, 1, DATEADD(day, -169, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (114, 'Retail Property Development', 4, 1, 2, DATEADD(day, -169, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (114, 'Sustainable Construction', 3, 1, 3, DATEADD(day, -169, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (114, 'Commercial Strategy', 2, 1, 4, DATEADD(day, -169, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (115, 'FMCG Manufacturing', 5, 1, 1, DATEADD(day, -164, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (115, 'Supply Chain Architecture', 4, 1, 2, DATEADD(day, -164, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (115, 'Brand Building', 3, 1, 3, DATEADD(day, -164, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (115, 'Beverage Industry Innovation', 2, 1, 4, DATEADD(day, -164, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (116, 'Luxury Hospitality', 5, 1, 1, DATEADD(day, -159, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (116, 'Aviation Logistics', 4, 1, 2, DATEADD(day, -159, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (116, 'Commodity Trading', 3, 1, 3, DATEADD(day, -159, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (116, 'Resort Operations', 2, 1, 4, DATEADD(day, -159, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (117, 'Offshore Exploration', 5, 1, 1, DATEADD(day, -154, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (117, 'Petroleum Engineering', 4, 1, 2, DATEADD(day, -154, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (117, 'Energy Security', 3, 1, 3, DATEADD(day, -154, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (117, 'HSE Compliance', 2, 1, 4, DATEADD(day, -154, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (118, 'Telecommunications', 5, 1, 1, DATEADD(day, -149, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (118, 'IT Infrastructure', 4, 1, 2, DATEADD(day, -149, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (118, 'Consumer Tech Logistics', 3, 1, 3, DATEADD(day, -149, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (118, 'Network Modernization', 2, 1, 4, DATEADD(day, -149, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (119, 'Commercial Banking', 5, 1, 1, DATEADD(day, -144, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (119, 'Core Banking Transformation', 4, 1, 2, DATEADD(day, -144, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (119, 'API Banking', 3, 1, 3, DATEADD(day, -144, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (119, 'Risk Governance', 2, 1, 4, DATEADD(day, -144, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (120, 'Port Logistics Management', 5, 1, 1, DATEADD(day, -139, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (120, 'Agro-Export Supply Chains', 4, 1, 2, DATEADD(day, -139, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (120, 'Trade Policy', 3, 1, 3, DATEADD(day, -139, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserSkill] ([UserId], [SkillName], [EndorsementCount], [IsVerified], [DisplayOrder], [CreatedAt], [IsDeleted]) VALUES (120, 'Industrial Zone Development', 2, 1, 4, DATEADD(day, -139, SYSUTCDATETIME()), 0);

PRINT '7. Populating KYC Identity Verification Audit Records...';
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (101, 'KYC-101-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -178, SYSUTCDATETIME()), DATEADD(day, -180, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (102, 'KYC-102-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -173, SYSUTCDATETIME()), DATEADD(day, -175, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (103, 'KYC-103-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -168, SYSUTCDATETIME()), DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (104, 'KYC-104-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -163, SYSUTCDATETIME()), DATEADD(day, -165, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (105, 'KYC-105-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -158, SYSUTCDATETIME()), DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (106, 'KYC-106-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -153, SYSUTCDATETIME()), DATEADD(day, -155, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (107, 'KYC-107-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -148, SYSUTCDATETIME()), DATEADD(day, -150, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (108, 'KYC-108-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -143, SYSUTCDATETIME()), DATEADD(day, -145, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (109, 'KYC-109-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -138, SYSUTCDATETIME()), DATEADD(day, -140, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (110, 'KYC-110-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -133, SYSUTCDATETIME()), DATEADD(day, -135, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (111, 'KYC-111-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -180, SYSUTCDATETIME()), DATEADD(day, -182, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (112, 'KYC-112-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -176, SYSUTCDATETIME()), DATEADD(day, -178, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (113, 'KYC-113-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -172, SYSUTCDATETIME()), DATEADD(day, -174, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (114, 'KYC-114-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -167, SYSUTCDATETIME()), DATEADD(day, -169, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (115, 'KYC-115-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -162, SYSUTCDATETIME()), DATEADD(day, -164, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (116, 'KYC-116-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -157, SYSUTCDATETIME()), DATEADD(day, -159, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (117, 'KYC-117-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -152, SYSUTCDATETIME()), DATEADD(day, -154, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (118, 'KYC-118-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -147, SYSUTCDATETIME()), DATEADD(day, -149, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (119, 'KYC-119-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -142, SYSUTCDATETIME()), DATEADD(day, -144, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserVerificationAudit] ([UserId], [AuditCode], [AuditTitle], [AuditDescription], [Authority], [Status], [AuditedAt], [CreatedAt], [IsDeleted]) VALUES (120, 'KYC-120-MM', 'National Digital ID & Executive Credentials Verified', 'Approved by SecOps Compliance Team with high platform trust score.', 'Myanmar Financial Intelligence / SecOps', 'APPROVED', DATEADD(day, -137, SYSUTCDATETIME()), DATEADD(day, -139, SYSUTCDATETIME()), 0);

PRINT '8. Populating User Ratings (Matching rated real user counts)...';
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 101, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 101, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 101, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 101, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 101, 5, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 102, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 102, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 102, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 102, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 102, 4, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 103, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 103, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 103, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 103, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 103, 5, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 104, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 104, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 104, 4, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 104, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 104, 5, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 105, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 105, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 105, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 105, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 105, 5, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 106, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 106, 4, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 106, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 106, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 107, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 107, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 107, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 107, 4, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 108, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 108, 4, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 108, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 108, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 109, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 109, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 109, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 109, 4, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 110, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 110, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 110, 4, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 110, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 111, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 111, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 111, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 111, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 111, 4, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 112, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 112, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 112, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 112, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 112, 5, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 113, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 113, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 113, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 113, 4, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 113, 5, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (124, 114, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 114, 4, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 114, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 114, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (125, 115, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 115, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 115, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 115, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 115, 4, 'A true role model in both professional discipline and societal impact.', DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (126, 116, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 116, 4, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 116, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 116, 4, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (127, 117, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 117, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 117, 4, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 117, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (128, 118, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 118, 4, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 118, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 118, 5, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (129, 119, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 119, 5, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 119, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 119, 4, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (130, 120, 5, 'Inspirational leadership and transformative vision for Myanmar''s emerging sectors.', DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (121, 120, 4, 'Exceptional mentor with profound practical expertise. Learned tremendously.', DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (122, 120, 5, 'Very responsive and dedicated to advancing community industry benchmarks.', DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserRating] ([RaterUserId], [TargetUserId], [Score], [ReviewText], [CreatedAt], [IsDeleted]) VALUES (123, 120, 4, 'Highly recommended! The real-world insights shared are gold standard.', DATEADD(day, -24, SYSUTCDATETIME()), 0);

PRINT '9. Populating Following Network (TblUserFollow)...';
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 101, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 104, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 107, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 110, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 113, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 116, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (121, 119, DATEADD(day, -105, SYSUTCDATETIME()), 121, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 102, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 105, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 108, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 111, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 114, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 117, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (122, 120, DATEADD(day, -100, SYSUTCDATETIME()), 122, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 103, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 106, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 109, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 112, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 115, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 118, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (123, 101, DATEADD(day, -93, SYSUTCDATETIME()), 123, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 104, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 107, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 110, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 113, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 116, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 119, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (124, 102, DATEADD(day, -85, SYSUTCDATETIME()), 124, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 105, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 108, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 111, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 114, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 117, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 120, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (125, 103, DATEADD(day, -80, SYSUTCDATETIME()), 125, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 106, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 109, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 112, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 115, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 118, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 101, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (126, 104, DATEADD(day, -75, SYSUTCDATETIME()), 126, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 107, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 110, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 113, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 116, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 119, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 102, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (127, 105, DATEADD(day, -70, SYSUTCDATETIME()), 127, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 108, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 111, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 114, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 117, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 120, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 103, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (128, 106, DATEADD(day, -65, SYSUTCDATETIME()), 128, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 109, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 112, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 115, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 118, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 101, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 104, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (129, 107, DATEADD(day, -60, SYSUTCDATETIME()), 129, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 110, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 113, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 116, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 119, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 102, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 105, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (130, 108, DATEADD(day, -55, SYSUTCDATETIME()), 130, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (101, 102, DATEADD(day, -170, SYSUTCDATETIME()), 101, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (101, 103, DATEADD(day, -170, SYSUTCDATETIME()), 101, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (101, 104, DATEADD(day, -170, SYSUTCDATETIME()), 101, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (102, 103, DATEADD(day, -165, SYSUTCDATETIME()), 102, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (102, 104, DATEADD(day, -165, SYSUTCDATETIME()), 102, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (102, 105, DATEADD(day, -165, SYSUTCDATETIME()), 102, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (103, 104, DATEADD(day, -160, SYSUTCDATETIME()), 103, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (103, 105, DATEADD(day, -160, SYSUTCDATETIME()), 103, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (103, 106, DATEADD(day, -160, SYSUTCDATETIME()), 103, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (104, 105, DATEADD(day, -155, SYSUTCDATETIME()), 104, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (104, 106, DATEADD(day, -155, SYSUTCDATETIME()), 104, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (104, 107, DATEADD(day, -155, SYSUTCDATETIME()), 104, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (105, 106, DATEADD(day, -150, SYSUTCDATETIME()), 105, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (105, 107, DATEADD(day, -150, SYSUTCDATETIME()), 105, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (105, 108, DATEADD(day, -150, SYSUTCDATETIME()), 105, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (106, 107, DATEADD(day, -145, SYSUTCDATETIME()), 106, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (106, 108, DATEADD(day, -145, SYSUTCDATETIME()), 106, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (106, 109, DATEADD(day, -145, SYSUTCDATETIME()), 106, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (107, 108, DATEADD(day, -140, SYSUTCDATETIME()), 107, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (107, 109, DATEADD(day, -140, SYSUTCDATETIME()), 107, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (107, 110, DATEADD(day, -140, SYSUTCDATETIME()), 107, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (108, 109, DATEADD(day, -135, SYSUTCDATETIME()), 108, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (108, 110, DATEADD(day, -135, SYSUTCDATETIME()), 108, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (108, 111, DATEADD(day, -135, SYSUTCDATETIME()), 108, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (109, 110, DATEADD(day, -130, SYSUTCDATETIME()), 109, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (109, 111, DATEADD(day, -130, SYSUTCDATETIME()), 109, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (109, 112, DATEADD(day, -130, SYSUTCDATETIME()), 109, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (110, 111, DATEADD(day, -125, SYSUTCDATETIME()), 110, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (110, 112, DATEADD(day, -125, SYSUTCDATETIME()), 110, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (110, 113, DATEADD(day, -125, SYSUTCDATETIME()), 110, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (111, 112, DATEADD(day, -172, SYSUTCDATETIME()), 111, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (111, 113, DATEADD(day, -172, SYSUTCDATETIME()), 111, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (111, 114, DATEADD(day, -172, SYSUTCDATETIME()), 111, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (112, 113, DATEADD(day, -168, SYSUTCDATETIME()), 112, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (112, 114, DATEADD(day, -168, SYSUTCDATETIME()), 112, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (112, 115, DATEADD(day, -168, SYSUTCDATETIME()), 112, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (113, 114, DATEADD(day, -164, SYSUTCDATETIME()), 113, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (113, 115, DATEADD(day, -164, SYSUTCDATETIME()), 113, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (113, 116, DATEADD(day, -164, SYSUTCDATETIME()), 113, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (114, 115, DATEADD(day, -159, SYSUTCDATETIME()), 114, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (114, 116, DATEADD(day, -159, SYSUTCDATETIME()), 114, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (114, 117, DATEADD(day, -159, SYSUTCDATETIME()), 114, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (115, 116, DATEADD(day, -154, SYSUTCDATETIME()), 115, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (115, 117, DATEADD(day, -154, SYSUTCDATETIME()), 115, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (115, 118, DATEADD(day, -154, SYSUTCDATETIME()), 115, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (116, 117, DATEADD(day, -149, SYSUTCDATETIME()), 116, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (116, 118, DATEADD(day, -149, SYSUTCDATETIME()), 116, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (116, 119, DATEADD(day, -149, SYSUTCDATETIME()), 116, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (117, 118, DATEADD(day, -144, SYSUTCDATETIME()), 117, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (117, 119, DATEADD(day, -144, SYSUTCDATETIME()), 117, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (117, 120, DATEADD(day, -144, SYSUTCDATETIME()), 117, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (118, 119, DATEADD(day, -139, SYSUTCDATETIME()), 118, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (118, 120, DATEADD(day, -139, SYSUTCDATETIME()), 118, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (118, 101, DATEADD(day, -139, SYSUTCDATETIME()), 118, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (119, 120, DATEADD(day, -134, SYSUTCDATETIME()), 119, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (119, 101, DATEADD(day, -134, SYSUTCDATETIME()), 119, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (119, 102, DATEADD(day, -134, SYSUTCDATETIME()), 119, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (120, 101, DATEADD(day, -129, SYSUTCDATETIME()), 120, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (120, 102, DATEADD(day, -129, SYSUTCDATETIME()), 120, 0);
INSERT INTO [dbo].[TblUserFollow] ([FollowerId], [FolloweeId], [CreatedAt], [CreatedBy], [IsDeleted]) VALUES (120, 103, DATEADD(day, -129, SYSUTCDATETIME()), 120, 0);

PRINT '10. Inserting 10 Top-Level Communities...';
SET IDENTITY_INSERT [dbo].[TblCommunity] ON;
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (1, NULL, 118, 'Technology & Software Innovation', 'technology-software-innovation', 'Myanmar software engineering, cloud architectures, and digital ecosystem discussions.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (2, NULL, 111, 'Business & Financial Markets', 'business-financial-markets', 'Commercial banking, capital investments, corporate governance, and fiscal policy in Myanmar.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (3, NULL, 113, 'Infrastructure & Real Estate', 'infrastructure-real-estate', 'Mega-developments, smart urbanization, sustainable construction, and architectural feats.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (4, NULL, 115, 'FMCG, Retail & Consumer Goods', 'fmcg-retail-consumer-goods', 'Fast-moving consumer products, national supply chains, and retail store management.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (5, NULL, 117, 'Energy & Natural Resources', 'energy-natural-resources', 'Offshore exploration, green transitions, renewable power grids, and petroleum safety.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (6, NULL, 102, 'Arts, Culture & Literature', 'arts-culture-literature', 'Burmese contemporary paintings, historical heritage preservation, literature, and screenwriting.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (7, NULL, 101, 'Sports, Martial Arts & Health', 'sports-martial-arts-health', 'Combat athletics, youth physical fitness conditioning, wellness, and mental stamina.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (8, NULL, 103, 'Public Health & Social Impact', 'public-health-social-impact', 'Humanitarian border clinics, maternal medicine, peacebuilding, and community welfare.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (9, NULL, 110, 'Telecom, Media & Creative Arts', 'telecom-media-creative-arts', 'Telecommunications infrastructure, cinema direction, satire, and digital entertainment.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (10, NULL, 120, 'Trade, Logistics & Aviation', 'trade-logistics-aviation', 'Deep-sea port logistics, regional agricultural exports, commercial air cargo, and transit corridors.', 'PUBLIC', 'INSTANT', 30, 20, DATEADD(day, -170, SYSUTCDATETIME()), 0);

PRINT '11. Inserting 50 Sub-Communities (5 per Community)...';
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (11, 1, 118, 'Enterprise Cloud & DevOps', 'enterprise-cloud-devops-11', 'Focused discussions and professional case studies in Enterprise Cloud & DevOps.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (12, 1, 118, 'Web & Mobile Engineering', 'web-mobile-engineering-12', 'Focused discussions and professional case studies in Web & Mobile Engineering.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (13, 1, 118, 'AI & Data Analytics', 'ai-data-analytics-13', 'Focused discussions and professional case studies in AI & Data Analytics.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (14, 1, 118, 'Cybersecurity & Governance', 'cybersecurity-governance-14', 'Focused discussions and professional case studies in Cybersecurity & Governance.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (15, 1, 118, 'Emerging Tech & Web3', 'emerging-tech-web3-15', 'Focused discussions and professional case studies in Emerging Tech & Web3.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (16, 2, 111, 'Commercial Banking Systems', 'commercial-banking-systems-16', 'Focused discussions and professional case studies in Commercial Banking Systems.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (17, 2, 111, 'Fintech & Mobile Wallets', 'fintech-mobile-wallets-17', 'Focused discussions and professional case studies in Fintech & Mobile Wallets.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (18, 2, 111, 'Microfinance & Rural Inclusion', 'microfinance-rural-inclusion-18', 'Focused discussions and professional case studies in Microfinance & Rural Inclusion.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (19, 2, 111, 'Venture Capital & Startups', 'venture-capital-startups-19', 'Focused discussions and professional case studies in Venture Capital & Startups.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (20, 2, 111, 'Corporate Accounting & Tax', 'corporate-accounting-tax-20', 'Focused discussions and professional case studies in Corporate Accounting & Tax.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (21, 3, 113, 'Urban Planning & Zoning', 'urban-planning-zoning-21', 'Focused discussions and professional case studies in Urban Planning & Zoning.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (22, 3, 113, 'Green Building Materials', 'green-building-materials-22', 'Focused discussions and professional case studies in Green Building Materials.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (23, 3, 113, 'Smart Transit Networks', 'smart-transit-networks-23', 'Focused discussions and professional case studies in Smart Transit Networks.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (24, 3, 113, 'Commercial Real Estate', 'commercial-real-estate-24', 'Focused discussions and professional case studies in Commercial Real Estate.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (25, 3, 113, 'Residential Architecture', 'residential-architecture-25', 'Focused discussions and professional case studies in Residential Architecture.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (26, 4, 115, 'Food & Beverage Manufacturing', 'food-beverage-manufacturing-26', 'Focused discussions and professional case studies in Food & Beverage Manufacturing.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (27, 4, 115, 'Modern Trade Distribution', 'modern-trade-distribution-27', 'Focused discussions and professional case studies in Modern Trade Distribution.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (28, 4, 115, 'Cold Chain Logistics', 'cold-chain-logistics-28', 'Focused discussions and professional case studies in Cold Chain Logistics.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (29, 4, 115, 'Packaging & Quality Control', 'packaging-quality-control-29', 'Focused discussions and professional case studies in Packaging & Quality Control.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (30, 4, 115, 'Direct-to-Consumer Brands', 'direct-to-consumer-brands-30', 'Focused discussions and professional case studies in Direct-to-Consumer Brands.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (31, 5, 117, 'Offshore Oil & Deepwater', 'offshore-oil-deepwater-31', 'Focused discussions and professional case studies in Offshore Oil & Deepwater.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (32, 5, 117, 'Solar & Mini-Grids', 'solar-mini-grids-32', 'Focused discussions and professional case studies in Solar & Mini-Grids.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (33, 5, 117, 'HSE Industrial Safety', 'hse-industrial-safety-33', 'Focused discussions and professional case studies in HSE Industrial Safety.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (34, 5, 117, 'Power Transmission Engineering', 'power-transmission-engineering-34', 'Focused discussions and professional case studies in Power Transmission Engineering.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (35, 5, 117, 'Petrochemical Refining', 'petrochemical-refining-35', 'Focused discussions and professional case studies in Petrochemical Refining.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (36, 6, 102, 'Colonial Heritage Conservation', 'colonial-heritage-conservation-36', 'Focused discussions and professional case studies in Colonial Heritage Conservation.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (37, 6, 102, 'Contemporary Fine Art', 'contemporary-fine-art-37', 'Focused discussions and professional case studies in Contemporary Fine Art.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (38, 6, 102, 'Burmese Literature & Translation', 'burmese-literature-translation-38', 'Focused discussions and professional case studies in Burmese Literature & Translation.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (39, 6, 102, 'Independent Cinema & Scripting', 'independent-cinema-scripting-39', 'Focused discussions and professional case studies in Independent Cinema & Scripting.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (40, 6, 102, 'Traditional Textile & Design', 'traditional-textile-design-40', 'Focused discussions and professional case studies in Traditional Textile & Design.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (41, 7, 101, 'Lethwei & Mixed Martial Arts', 'lethwei-mixed-martial-arts-41', 'Focused discussions and professional case studies in Lethwei & Mixed Martial Arts.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (42, 7, 101, 'Strength & High-Performance Conditioning', 'strength-high-performance-conditioning-42', 'Focused discussions and professional case studies in Strength & High-Performance Conditioning.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (43, 7, 101, 'Youth Football Academies', 'youth-football-academies-43', 'Focused discussions and professional case studies in Youth Football Academies.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (44, 7, 101, 'Sports Nutrition & Recovery', 'sports-nutrition-recovery-44', 'Focused discussions and professional case studies in Sports Nutrition & Recovery.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (45, 7, 101, 'Mental Focus & Meditation', 'mental-focus-meditation-45', 'Focused discussions and professional case studies in Mental Focus & Meditation.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (46, 8, 103, 'Rural Maternal & Child Healthcare', 'rural-maternal-child-healthcare-46', 'Focused discussions and professional case studies in Rural Maternal & Child Healthcare.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (47, 8, 103, 'Epidemiology & Infectious Control', 'epidemiology-infectious-control-47', 'Focused discussions and professional case studies in Epidemiology & Infectious Control.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (48, 8, 103, 'Humanitarian Relief Logistics', 'humanitarian-relief-logistics-48', 'Focused discussions and professional case studies in Humanitarian Relief Logistics.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (49, 8, 103, 'Community Health Worker Training', 'community-health-worker-training-49', 'Focused discussions and professional case studies in Community Health Worker Training.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (50, 8, 103, 'Mental Well-Being & Counseling', 'mental-well-being-counseling-50', 'Focused discussions and professional case studies in Mental Well-Being & Counseling.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (51, 9, 110, 'Telecom Tower Infrastructure', 'telecom-tower-infrastructure-51', 'Focused discussions and professional case studies in Telecom Tower Infrastructure.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (52, 9, 110, 'Fiber Optic Backbone Networks', 'fiber-optic-backbone-networks-52', 'Focused discussions and professional case studies in Fiber Optic Backbone Networks.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (53, 9, 110, 'Broadcasting & Digital Media', 'broadcasting-digital-media-53', 'Focused discussions and professional case studies in Broadcasting & Digital Media.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (54, 9, 110, 'Satirical Media & Visual Arts', 'satirical-media-visual-arts-54', 'Focused discussions and professional case studies in Satirical Media & Visual Arts.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (55, 9, 110, 'Creator Economy & Digital Marketing', 'creator-economy-digital-marketing-55', 'Focused discussions and professional case studies in Creator Economy & Digital Marketing.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (56, 10, 120, 'Maritime Container Shipping', 'maritime-container-shipping-56', 'Focused discussions and professional case studies in Maritime Container Shipping.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (57, 10, 120, 'Inland Waterway Logistics', 'inland-waterway-logistics-57', 'Focused discussions and professional case studies in Inland Waterway Logistics.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (58, 10, 120, 'Agro-Export Commodity Trading', 'agro-export-commodity-trading-58', 'Focused discussions and professional case studies in Agro-Export Commodity Trading.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (59, 10, 120, 'Aviation Cargo & Ground Handling', 'aviation-cargo-ground-handling-59', 'Focused discussions and professional case studies in Aviation Cargo & Ground Handling.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunity] ([CommunityId], [ParentCommunityId], [OwnerId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [CreatedAt], [IsDeleted])
VALUES (60, 10, 120, 'Cross-Border Customs Facilitation', 'cross-border-customs-facilitation-60', 'Focused discussions and professional case studies in Cross-Border Customs Facilitation.', 'PUBLIC', 'INSTANT', 15, 10, DATEADD(day, -160, SYSUTCDATETIME()), 0);
SET IDENTITY_INSERT [dbo].[TblCommunity] OFF;

PRINT '12. Inserting 150 Groups (3 per Sub-Community)...';
SET IDENTITY_INSERT [dbo].[TblGroup] ON;
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (1, 11, 118, 'Enterprise Cloud & DevOps - Working Circle 1', 'group-11-1-1', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Enterprise Cloud & DevOps.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (2, 11, 119, 'Enterprise Cloud & DevOps - Working Circle 2', 'group-11-2-2', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Enterprise Cloud & DevOps.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (3, 11, 112, 'Enterprise Cloud & DevOps - Working Circle 3', 'group-11-3-3', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Enterprise Cloud & DevOps.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (4, 12, 118, 'Web & Mobile Engineering - Working Circle 1', 'group-12-1-4', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Web & Mobile Engineering.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (5, 12, 119, 'Web & Mobile Engineering - Working Circle 2', 'group-12-2-5', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Web & Mobile Engineering.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (6, 12, 112, 'Web & Mobile Engineering - Working Circle 3', 'group-12-3-6', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Web & Mobile Engineering.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (7, 13, 118, 'AI & Data Analytics - Working Circle 1', 'group-13-1-7', 'Specialized working group focusing on operational insights, peer reviews, and best practices for AI & Data Analytics.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (8, 13, 119, 'AI & Data Analytics - Working Circle 2', 'group-13-2-8', 'Specialized working group focusing on operational insights, peer reviews, and best practices for AI & Data Analytics.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (9, 13, 112, 'AI & Data Analytics - Working Circle 3', 'group-13-3-9', 'Specialized working group focusing on operational insights, peer reviews, and best practices for AI & Data Analytics.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (10, 14, 118, 'Cybersecurity & Governance - Working Circle 1', 'group-14-1-10', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cybersecurity & Governance.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (11, 14, 119, 'Cybersecurity & Governance - Working Circle 2', 'group-14-2-11', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cybersecurity & Governance.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (12, 14, 112, 'Cybersecurity & Governance - Working Circle 3', 'group-14-3-12', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cybersecurity & Governance.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (13, 15, 118, 'Emerging Tech & Web3 - Working Circle 1', 'group-15-1-13', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Emerging Tech & Web3.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (14, 15, 119, 'Emerging Tech & Web3 - Working Circle 2', 'group-15-2-14', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Emerging Tech & Web3.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (15, 15, 112, 'Emerging Tech & Web3 - Working Circle 3', 'group-15-3-15', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Emerging Tech & Web3.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (16, 16, 113, 'Commercial Banking Systems - Working Circle 1', 'group-16-1-16', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Commercial Banking Systems.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (17, 16, 111, 'Commercial Banking Systems - Working Circle 2', 'group-16-2-17', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Commercial Banking Systems.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (18, 16, 112, 'Commercial Banking Systems - Working Circle 3', 'group-16-3-18', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Commercial Banking Systems.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (19, 17, 119, 'Fintech & Mobile Wallets - Working Circle 1', 'group-17-1-19', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Fintech & Mobile Wallets.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (20, 17, 113, 'Fintech & Mobile Wallets - Working Circle 2', 'group-17-2-20', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Fintech & Mobile Wallets.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (21, 17, 111, 'Fintech & Mobile Wallets - Working Circle 3', 'group-17-3-21', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Fintech & Mobile Wallets.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (22, 18, 112, 'Microfinance & Rural Inclusion - Working Circle 1', 'group-18-1-22', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Microfinance & Rural Inclusion.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (23, 18, 119, 'Microfinance & Rural Inclusion - Working Circle 2', 'group-18-2-23', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Microfinance & Rural Inclusion.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (24, 18, 113, 'Microfinance & Rural Inclusion - Working Circle 3', 'group-18-3-24', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Microfinance & Rural Inclusion.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (25, 19, 111, 'Venture Capital & Startups - Working Circle 1', 'group-19-1-25', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Venture Capital & Startups.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (26, 19, 112, 'Venture Capital & Startups - Working Circle 2', 'group-19-2-26', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Venture Capital & Startups.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (27, 19, 119, 'Venture Capital & Startups - Working Circle 3', 'group-19-3-27', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Venture Capital & Startups.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (28, 20, 113, 'Corporate Accounting & Tax - Working Circle 1', 'group-20-1-28', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Corporate Accounting & Tax.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (29, 20, 111, 'Corporate Accounting & Tax - Working Circle 2', 'group-20-2-29', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Corporate Accounting & Tax.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (30, 20, 112, 'Corporate Accounting & Tax - Working Circle 3', 'group-20-3-30', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Corporate Accounting & Tax.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (31, 21, 113, 'Urban Planning & Zoning - Working Circle 1', 'group-21-1-31', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Urban Planning & Zoning.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (32, 21, 114, 'Urban Planning & Zoning - Working Circle 2', 'group-21-2-32', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Urban Planning & Zoning.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (33, 21, 113, 'Urban Planning & Zoning - Working Circle 3', 'group-21-3-33', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Urban Planning & Zoning.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (34, 22, 114, 'Green Building Materials - Working Circle 1', 'group-22-1-34', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Green Building Materials.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (35, 22, 113, 'Green Building Materials - Working Circle 2', 'group-22-2-35', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Green Building Materials.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (36, 22, 114, 'Green Building Materials - Working Circle 3', 'group-22-3-36', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Green Building Materials.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (37, 23, 113, 'Smart Transit Networks - Working Circle 1', 'group-23-1-37', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Smart Transit Networks.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (38, 23, 114, 'Smart Transit Networks - Working Circle 2', 'group-23-2-38', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Smart Transit Networks.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (39, 23, 113, 'Smart Transit Networks - Working Circle 3', 'group-23-3-39', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Smart Transit Networks.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (40, 24, 114, 'Commercial Real Estate - Working Circle 1', 'group-24-1-40', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Commercial Real Estate.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (41, 24, 113, 'Commercial Real Estate - Working Circle 2', 'group-24-2-41', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Commercial Real Estate.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (42, 24, 114, 'Commercial Real Estate - Working Circle 3', 'group-24-3-42', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Commercial Real Estate.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (43, 25, 113, 'Residential Architecture - Working Circle 1', 'group-25-1-43', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Residential Architecture.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (44, 25, 114, 'Residential Architecture - Working Circle 2', 'group-25-2-44', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Residential Architecture.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (45, 25, 113, 'Residential Architecture - Working Circle 3', 'group-25-3-45', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Residential Architecture.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (46, 26, 120, 'Food & Beverage Manufacturing - Working Circle 1', 'group-26-1-46', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Food & Beverage Manufacturing.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (47, 26, 115, 'Food & Beverage Manufacturing - Working Circle 2', 'group-26-2-47', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Food & Beverage Manufacturing.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (48, 26, 120, 'Food & Beverage Manufacturing - Working Circle 3', 'group-26-3-48', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Food & Beverage Manufacturing.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (49, 27, 115, 'Modern Trade Distribution - Working Circle 1', 'group-27-1-49', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Modern Trade Distribution.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (50, 27, 120, 'Modern Trade Distribution - Working Circle 2', 'group-27-2-50', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Modern Trade Distribution.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (51, 27, 115, 'Modern Trade Distribution - Working Circle 3', 'group-27-3-51', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Modern Trade Distribution.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (52, 28, 120, 'Cold Chain Logistics - Working Circle 1', 'group-28-1-52', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cold Chain Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (53, 28, 115, 'Cold Chain Logistics - Working Circle 2', 'group-28-2-53', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cold Chain Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (54, 28, 120, 'Cold Chain Logistics - Working Circle 3', 'group-28-3-54', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cold Chain Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (55, 29, 115, 'Packaging & Quality Control - Working Circle 1', 'group-29-1-55', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Packaging & Quality Control.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (56, 29, 120, 'Packaging & Quality Control - Working Circle 2', 'group-29-2-56', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Packaging & Quality Control.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (57, 29, 115, 'Packaging & Quality Control - Working Circle 3', 'group-29-3-57', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Packaging & Quality Control.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (58, 30, 120, 'Direct-to-Consumer Brands - Working Circle 1', 'group-30-1-58', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Direct-to-Consumer Brands.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (59, 30, 115, 'Direct-to-Consumer Brands - Working Circle 2', 'group-30-2-59', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Direct-to-Consumer Brands.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (60, 30, 120, 'Direct-to-Consumer Brands - Working Circle 3', 'group-30-3-60', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Direct-to-Consumer Brands.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (61, 31, 117, 'Offshore Oil & Deepwater - Working Circle 1', 'group-31-1-61', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Offshore Oil & Deepwater.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (62, 31, 116, 'Offshore Oil & Deepwater - Working Circle 2', 'group-31-2-62', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Offshore Oil & Deepwater.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (63, 31, 117, 'Offshore Oil & Deepwater - Working Circle 3', 'group-31-3-63', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Offshore Oil & Deepwater.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (64, 32, 116, 'Solar & Mini-Grids - Working Circle 1', 'group-32-1-64', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Solar & Mini-Grids.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (65, 32, 117, 'Solar & Mini-Grids - Working Circle 2', 'group-32-2-65', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Solar & Mini-Grids.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (66, 32, 116, 'Solar & Mini-Grids - Working Circle 3', 'group-32-3-66', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Solar & Mini-Grids.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (67, 33, 117, 'HSE Industrial Safety - Working Circle 1', 'group-33-1-67', 'Specialized working group focusing on operational insights, peer reviews, and best practices for HSE Industrial Safety.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (68, 33, 116, 'HSE Industrial Safety - Working Circle 2', 'group-33-2-68', 'Specialized working group focusing on operational insights, peer reviews, and best practices for HSE Industrial Safety.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (69, 33, 117, 'HSE Industrial Safety - Working Circle 3', 'group-33-3-69', 'Specialized working group focusing on operational insights, peer reviews, and best practices for HSE Industrial Safety.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (70, 34, 116, 'Power Transmission Engineering - Working Circle 1', 'group-34-1-70', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Power Transmission Engineering.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (71, 34, 117, 'Power Transmission Engineering - Working Circle 2', 'group-34-2-71', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Power Transmission Engineering.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (72, 34, 116, 'Power Transmission Engineering - Working Circle 3', 'group-34-3-72', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Power Transmission Engineering.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (73, 35, 117, 'Petrochemical Refining - Working Circle 1', 'group-35-1-73', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Petrochemical Refining.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (74, 35, 116, 'Petrochemical Refining - Working Circle 2', 'group-35-2-74', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Petrochemical Refining.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (75, 35, 117, 'Petrochemical Refining - Working Circle 3', 'group-35-3-75', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Petrochemical Refining.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (76, 36, 106, 'Colonial Heritage Conservation - Working Circle 1', 'group-36-1-76', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Colonial Heritage Conservation.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (77, 36, 102, 'Colonial Heritage Conservation - Working Circle 2', 'group-36-2-77', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Colonial Heritage Conservation.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (78, 36, 107, 'Colonial Heritage Conservation - Working Circle 3', 'group-36-3-78', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Colonial Heritage Conservation.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (79, 37, 105, 'Contemporary Fine Art - Working Circle 1', 'group-37-1-79', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Contemporary Fine Art.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (80, 37, 106, 'Contemporary Fine Art - Working Circle 2', 'group-37-2-80', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Contemporary Fine Art.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (81, 37, 102, 'Contemporary Fine Art - Working Circle 3', 'group-37-3-81', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Contemporary Fine Art.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (82, 38, 107, 'Burmese Literature & Translation - Working Circle 1', 'group-38-1-82', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Burmese Literature & Translation.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (83, 38, 105, 'Burmese Literature & Translation - Working Circle 2', 'group-38-2-83', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Burmese Literature & Translation.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (84, 38, 106, 'Burmese Literature & Translation - Working Circle 3', 'group-38-3-84', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Burmese Literature & Translation.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (85, 39, 102, 'Independent Cinema & Scripting - Working Circle 1', 'group-39-1-85', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Independent Cinema & Scripting.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (86, 39, 107, 'Independent Cinema & Scripting - Working Circle 2', 'group-39-2-86', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Independent Cinema & Scripting.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (87, 39, 105, 'Independent Cinema & Scripting - Working Circle 3', 'group-39-3-87', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Independent Cinema & Scripting.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (88, 40, 106, 'Traditional Textile & Design - Working Circle 1', 'group-40-1-88', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Traditional Textile & Design.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (89, 40, 102, 'Traditional Textile & Design - Working Circle 2', 'group-40-2-89', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Traditional Textile & Design.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (90, 40, 107, 'Traditional Textile & Design - Working Circle 3', 'group-40-3-90', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Traditional Textile & Design.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (91, 41, 101, 'Lethwei & Mixed Martial Arts - Working Circle 1', 'group-41-1-91', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Lethwei & Mixed Martial Arts.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (92, 41, 105, 'Lethwei & Mixed Martial Arts - Working Circle 2', 'group-41-2-92', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Lethwei & Mixed Martial Arts.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (93, 41, 111, 'Lethwei & Mixed Martial Arts - Working Circle 3', 'group-41-3-93', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Lethwei & Mixed Martial Arts.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (94, 42, 101, 'Strength & High-Performance Conditioning - Working Circle 1', 'group-42-1-94', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Strength & High-Performance Conditioning.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (95, 42, 105, 'Strength & High-Performance Conditioning - Working Circle 2', 'group-42-2-95', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Strength & High-Performance Conditioning.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (96, 42, 111, 'Strength & High-Performance Conditioning - Working Circle 3', 'group-42-3-96', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Strength & High-Performance Conditioning.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (97, 43, 101, 'Youth Football Academies - Working Circle 1', 'group-43-1-97', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Youth Football Academies.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (98, 43, 105, 'Youth Football Academies - Working Circle 2', 'group-43-2-98', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Youth Football Academies.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (99, 43, 111, 'Youth Football Academies - Working Circle 3', 'group-43-3-99', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Youth Football Academies.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (100, 44, 101, 'Sports Nutrition & Recovery - Working Circle 1', 'group-44-1-100', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Sports Nutrition & Recovery.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (101, 44, 105, 'Sports Nutrition & Recovery - Working Circle 2', 'group-44-2-101', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Sports Nutrition & Recovery.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (102, 44, 111, 'Sports Nutrition & Recovery - Working Circle 3', 'group-44-3-102', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Sports Nutrition & Recovery.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (103, 45, 101, 'Mental Focus & Meditation - Working Circle 1', 'group-45-1-103', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Mental Focus & Meditation.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (104, 45, 105, 'Mental Focus & Meditation - Working Circle 2', 'group-45-2-104', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Mental Focus & Meditation.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (105, 45, 111, 'Mental Focus & Meditation - Working Circle 3', 'group-45-3-105', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Mental Focus & Meditation.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (106, 46, 103, 'Rural Maternal & Child Healthcare - Working Circle 1', 'group-46-1-106', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Rural Maternal & Child Healthcare.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (107, 46, 109, 'Rural Maternal & Child Healthcare - Working Circle 2', 'group-46-2-107', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Rural Maternal & Child Healthcare.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (108, 46, 108, 'Rural Maternal & Child Healthcare - Working Circle 3', 'group-46-3-108', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Rural Maternal & Child Healthcare.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (109, 47, 103, 'Epidemiology & Infectious Control - Working Circle 1', 'group-47-1-109', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Epidemiology & Infectious Control.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (110, 47, 109, 'Epidemiology & Infectious Control - Working Circle 2', 'group-47-2-110', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Epidemiology & Infectious Control.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (111, 47, 108, 'Epidemiology & Infectious Control - Working Circle 3', 'group-47-3-111', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Epidemiology & Infectious Control.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (112, 48, 103, 'Humanitarian Relief Logistics - Working Circle 1', 'group-48-1-112', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Humanitarian Relief Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (113, 48, 109, 'Humanitarian Relief Logistics - Working Circle 2', 'group-48-2-113', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Humanitarian Relief Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (114, 48, 108, 'Humanitarian Relief Logistics - Working Circle 3', 'group-48-3-114', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Humanitarian Relief Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (115, 49, 103, 'Community Health Worker Training - Working Circle 1', 'group-49-1-115', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Community Health Worker Training.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (116, 49, 109, 'Community Health Worker Training - Working Circle 2', 'group-49-2-116', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Community Health Worker Training.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (117, 49, 108, 'Community Health Worker Training - Working Circle 3', 'group-49-3-117', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Community Health Worker Training.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (118, 50, 103, 'Mental Well-Being & Counseling - Working Circle 1', 'group-50-1-118', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Mental Well-Being & Counseling.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (119, 50, 109, 'Mental Well-Being & Counseling - Working Circle 2', 'group-50-2-119', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Mental Well-Being & Counseling.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (120, 50, 108, 'Mental Well-Being & Counseling - Working Circle 3', 'group-50-3-120', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Mental Well-Being & Counseling.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (121, 51, 110, 'Telecom Tower Infrastructure - Working Circle 1', 'group-51-1-121', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Telecom Tower Infrastructure.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (122, 51, 106, 'Telecom Tower Infrastructure - Working Circle 2', 'group-51-2-122', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Telecom Tower Infrastructure.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (123, 51, 104, 'Telecom Tower Infrastructure - Working Circle 3', 'group-51-3-123', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Telecom Tower Infrastructure.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (124, 52, 118, 'Fiber Optic Backbone Networks - Working Circle 1', 'group-52-1-124', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Fiber Optic Backbone Networks.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (125, 52, 110, 'Fiber Optic Backbone Networks - Working Circle 2', 'group-52-2-125', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Fiber Optic Backbone Networks.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (126, 52, 106, 'Fiber Optic Backbone Networks - Working Circle 3', 'group-52-3-126', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Fiber Optic Backbone Networks.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (127, 53, 104, 'Broadcasting & Digital Media - Working Circle 1', 'group-53-1-127', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Broadcasting & Digital Media.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (128, 53, 118, 'Broadcasting & Digital Media - Working Circle 2', 'group-53-2-128', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Broadcasting & Digital Media.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (129, 53, 110, 'Broadcasting & Digital Media - Working Circle 3', 'group-53-3-129', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Broadcasting & Digital Media.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (130, 54, 106, 'Satirical Media & Visual Arts - Working Circle 1', 'group-54-1-130', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Satirical Media & Visual Arts.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (131, 54, 104, 'Satirical Media & Visual Arts - Working Circle 2', 'group-54-2-131', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Satirical Media & Visual Arts.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (132, 54, 118, 'Satirical Media & Visual Arts - Working Circle 3', 'group-54-3-132', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Satirical Media & Visual Arts.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (133, 55, 110, 'Creator Economy & Digital Marketing - Working Circle 1', 'group-55-1-133', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Creator Economy & Digital Marketing.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (134, 55, 106, 'Creator Economy & Digital Marketing - Working Circle 2', 'group-55-2-134', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Creator Economy & Digital Marketing.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (135, 55, 104, 'Creator Economy & Digital Marketing - Working Circle 3', 'group-55-3-135', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Creator Economy & Digital Marketing.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (136, 56, 112, 'Maritime Container Shipping - Working Circle 1', 'group-56-1-136', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Maritime Container Shipping.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (137, 56, 120, 'Maritime Container Shipping - Working Circle 2', 'group-56-2-137', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Maritime Container Shipping.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (138, 56, 116, 'Maritime Container Shipping - Working Circle 3', 'group-56-3-138', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Maritime Container Shipping.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (139, 57, 111, 'Inland Waterway Logistics - Working Circle 1', 'group-57-1-139', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Inland Waterway Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (140, 57, 112, 'Inland Waterway Logistics - Working Circle 2', 'group-57-2-140', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Inland Waterway Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (141, 57, 120, 'Inland Waterway Logistics - Working Circle 3', 'group-57-3-141', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Inland Waterway Logistics.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (142, 58, 116, 'Agro-Export Commodity Trading - Working Circle 1', 'group-58-1-142', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Agro-Export Commodity Trading.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (143, 58, 111, 'Agro-Export Commodity Trading - Working Circle 2', 'group-58-2-143', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Agro-Export Commodity Trading.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (144, 58, 112, 'Agro-Export Commodity Trading - Working Circle 3', 'group-58-3-144', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Agro-Export Commodity Trading.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (145, 59, 120, 'Aviation Cargo & Ground Handling - Working Circle 1', 'group-59-1-145', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Aviation Cargo & Ground Handling.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (146, 59, 116, 'Aviation Cargo & Ground Handling - Working Circle 2', 'group-59-2-146', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Aviation Cargo & Ground Handling.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (147, 59, 111, 'Aviation Cargo & Ground Handling - Working Circle 3', 'group-59-3-147', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Aviation Cargo & Ground Handling.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (148, 60, 112, 'Cross-Border Customs Facilitation - Working Circle 1', 'group-60-1-148', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cross-Border Customs Facilitation.', 'PUBLIC', 'INSTANT', 12, 4, 'OPEN', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (149, 60, 120, 'Cross-Border Customs Facilitation - Working Circle 2', 'group-60-2-149', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cross-Border Customs Facilitation.', 'PUBLIC', 'INSTANT', 12, 4, 'PAID', 50, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroup] ([GroupId], [SubCommunityId], [CreatorId], [Name], [Slug], [Description], [Visibility], [JoinPolicy], [MemberCount], [PostCount], [GroupType], [JoinFeeLinkDrops], [CommissionPercentageSnapshot], [IsActive], [CreatedAt], [IsDeleted])
VALUES (150, 60, 116, 'Cross-Border Customs Facilitation - Working Circle 3', 'group-60-3-150', 'Specialized working group focusing on operational insights, peer reviews, and best practices for Cross-Border Customs Facilitation.', 'PUBLIC', 'INSTANT', 12, 4, 'RESTRICTED', 0, 10.0, 1, DATEADD(day, -120, SYSUTCDATETIME()), 0);
SET IDENTITY_INSERT [dbo].[TblGroup] OFF;

PRINT '13. Populating Community & Group Memberships...';
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 101, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 101, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 101, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 101, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 101, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 101, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 102, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 102, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 102, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 102, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 102, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 102, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 103, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 103, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 103, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 103, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 103, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 103, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 104, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 104, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 104, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 104, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 104, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 104, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 105, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 105, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 105, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 105, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 105, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 105, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 106, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 106, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 106, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 106, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 106, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 106, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 107, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 107, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 107, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 107, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 107, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 107, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 108, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 108, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 108, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 108, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 108, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 108, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 109, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 109, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 109, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 109, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 109, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 109, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 110, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 110, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 110, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 110, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 110, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 110, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 111, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 111, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 111, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 111, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 111, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 111, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 112, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 112, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 112, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 112, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 112, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 112, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 113, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 113, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 113, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 113, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 113, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 113, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 114, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 114, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 114, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 114, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 114, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 114, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 115, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 115, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 115, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 115, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 115, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 115, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 116, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 116, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 116, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 116, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 116, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 116, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 117, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 117, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 117, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 117, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 117, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 117, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 118, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 118, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 118, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 118, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 118, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 118, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 119, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 119, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 119, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 119, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 119, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 119, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 120, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 120, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 120, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 120, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 120, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 120, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 121, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 121, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 121, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 121, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 121, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 121, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 122, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 122, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 122, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 122, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 122, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 122, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 123, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 123, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 123, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 123, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 123, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 123, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 124, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 124, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 124, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 124, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 124, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 124, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 125, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 125, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 125, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 125, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 125, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 125, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 126, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 126, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 126, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 126, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 126, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 126, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 127, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 127, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 127, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 127, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 127, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 127, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 128, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 128, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 128, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 128, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 128, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 128, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 129, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 129, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 129, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 129, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 129, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 129, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 130, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 130, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 130, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 130, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 130, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblCommunityMember] ([CommunityId], [UserId], [Role], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 130, 'Member', DATEADD(day, -50, SYSUTCDATETIME()), DATEADD(day, -50, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (1, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (2, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (3, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (4, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (4, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (4, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (4, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (5, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (5, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (5, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (5, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (6, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (7, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (8, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (9, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (9, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (9, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (9, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (10, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (10, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (10, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (10, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (11, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (11, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (11, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (11, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (12, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (12, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (12, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (12, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (13, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (13, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (13, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (13, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (14, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (14, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (14, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (14, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (15, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (15, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (15, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (15, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (16, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (16, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (16, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (16, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (17, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (17, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (17, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (17, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (18, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (18, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (18, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (18, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (19, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (19, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (19, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (19, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (20, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (20, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (20, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (20, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (21, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (21, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (21, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (21, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (22, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (22, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (22, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (22, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (23, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (23, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (23, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (23, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (24, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (24, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (24, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (24, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (25, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (25, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (25, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (25, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (26, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (26, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (26, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (26, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (27, 119, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (27, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (27, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (27, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (28, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (28, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (28, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (28, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (29, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (29, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (29, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (29, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (30, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (30, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (30, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (30, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (31, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (31, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (31, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (31, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (32, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (32, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (32, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (32, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (33, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (33, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (33, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (33, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (34, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (34, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (34, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (34, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (35, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (35, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (35, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (35, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (36, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (36, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (36, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (36, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (37, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (37, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (37, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (37, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (38, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (38, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (38, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (38, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (39, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (39, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (39, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (39, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (40, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (40, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (40, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (40, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (41, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (41, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (41, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (41, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (42, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (42, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (42, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (42, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (43, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (43, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (43, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (43, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (44, 114, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (44, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (44, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (44, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (45, 113, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (45, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (45, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (45, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (46, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (46, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (46, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (46, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (47, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (47, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (47, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (47, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (48, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (48, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (48, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (48, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (49, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (49, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (49, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (49, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (50, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (50, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (50, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (50, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (51, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (51, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (51, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (51, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (52, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (52, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (52, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (52, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (53, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (53, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (53, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (53, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (54, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (54, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (54, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (54, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (55, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (55, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (55, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (55, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (56, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (56, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (56, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (56, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (57, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (57, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (57, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (57, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (58, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (58, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (58, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (58, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (59, 115, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (59, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (59, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (59, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (60, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (60, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (60, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (60, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (61, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (61, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (61, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (61, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (62, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (62, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (62, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (62, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (63, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (63, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (63, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (63, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (64, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (64, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (64, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (64, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (65, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (65, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (65, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (65, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (66, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (66, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (66, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (66, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (67, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (67, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (67, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (67, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (68, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (68, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (68, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (68, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (69, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (69, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (69, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (69, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (70, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (70, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (70, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (70, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (71, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (71, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (71, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (71, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (72, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (72, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (72, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (72, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (73, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (73, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (73, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (73, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (74, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (74, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (74, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (74, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (75, 117, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (75, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (75, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (75, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (76, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (76, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (76, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (76, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (77, 102, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (77, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (77, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (77, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (78, 107, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (78, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (78, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (78, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (79, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (79, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (79, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (79, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (80, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (80, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (80, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (80, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (81, 102, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (81, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (81, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (81, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (82, 107, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (82, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (82, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (82, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (83, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (83, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (83, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (83, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (84, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (84, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (84, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (84, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (85, 102, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (85, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (85, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (85, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (86, 107, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (86, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (86, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (86, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (87, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (87, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (87, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (87, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (88, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (88, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (88, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (88, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (89, 102, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (89, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (89, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (89, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (90, 107, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (90, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (90, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (90, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (91, 101, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (91, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (91, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (91, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (92, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (92, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (92, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (92, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (93, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (93, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (93, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (93, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (94, 101, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (94, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (94, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (94, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (95, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (95, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (95, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (95, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (96, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (96, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (96, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (96, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (97, 101, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (97, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (97, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (97, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (98, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (98, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (98, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (98, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (99, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (99, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (99, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (99, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (100, 101, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (100, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (100, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (100, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (101, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (101, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (101, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (101, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (102, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (102, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (102, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (102, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (103, 101, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (103, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (103, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (103, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (104, 105, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (104, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (104, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (104, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (105, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (105, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (105, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (105, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (106, 103, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (106, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (106, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (106, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (107, 109, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (107, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (107, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (107, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (108, 108, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (108, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (108, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (108, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (109, 103, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (109, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (109, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (109, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (110, 109, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (110, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (110, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (110, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (111, 108, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (111, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (111, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (111, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (112, 103, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (112, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (112, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (112, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (113, 109, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (113, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (113, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (113, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (114, 108, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (114, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (114, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (114, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (115, 103, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (115, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (115, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (115, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (116, 109, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (116, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (116, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (116, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (117, 108, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (117, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (117, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (117, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (118, 103, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (118, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (118, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (118, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (119, 109, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (119, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (119, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (119, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (120, 108, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (120, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (120, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (120, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (121, 110, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (121, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (121, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (121, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (122, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (122, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (122, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (122, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (123, 104, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (123, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (123, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (123, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (124, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (124, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (124, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (124, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (125, 110, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (125, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (125, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (125, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (126, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (126, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (126, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (126, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (127, 104, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (127, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (127, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (127, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (128, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (128, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (128, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (128, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (129, 110, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (129, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (129, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (129, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (130, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (130, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (130, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (130, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (131, 104, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (131, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (131, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (131, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (132, 118, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (132, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (132, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (132, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (133, 110, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (133, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (133, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (133, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (134, 106, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (134, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (134, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (134, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (135, 104, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (135, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (135, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (135, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (136, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (136, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (136, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (136, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (137, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (137, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (137, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (137, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (138, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (138, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (138, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (138, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (139, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (139, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (139, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (139, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (140, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (140, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (140, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (140, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (141, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (141, 122, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (141, 123, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (141, 124, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (142, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (142, 123, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (142, 124, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (142, 125, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (143, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (143, 124, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (143, 125, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (143, 126, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (144, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (144, 125, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (144, 126, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (144, 127, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (145, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (145, 126, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (145, 127, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (145, 128, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (146, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (146, 127, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (146, 128, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (146, 129, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (147, 111, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (147, 128, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (147, 129, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (147, 130, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (148, 112, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (148, 129, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (148, 130, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (148, 121, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (149, 120, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (149, 130, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (149, 121, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (149, 122, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (150, 116, 'Admin', 0, DATEADD(day, -110, SYSUTCDATETIME()), DATEADD(day, -110, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (150, 121, 'Member', 0, DATEADD(day, -80, SYSUTCDATETIME()), DATEADD(day, -80, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (150, 122, 'Member', 0, DATEADD(day, -70, SYSUTCDATETIME()), DATEADD(day, -70, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblGroupMember] ([GroupId], [UserId], [Role], [IsMuted], [JoinedAt], [CreatedAt], [IsDeleted]) VALUES (150, 123, 'Member', 0, DATEADD(day, -60, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()), 0);

PRINT '14. Inserting Posts & Polls (Standalone + Group Posts across all Groups)...';
SET IDENTITY_INSERT [dbo].[TblPost] ON;
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (1, 101, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (2, 101, NULL, NULL, 'MMA Elite Coaching - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (3, 101, NULL, 91, 'Discussion inside Group #91: Exploring practical deployment challenges in Athletic Conditioning.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (4, 101, NULL, 94, 'Group #94 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (5, 102, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (6, 102, NULL, NULL, 'Southeast Asian History - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (7, 102, NULL, 77, 'Discussion inside Group #77: Exploring practical deployment challenges in Diplomatic Affairs.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (8, 102, NULL, 81, 'Group #81 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (9, 103, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (10, 103, NULL, NULL, 'Humanitarian Medicine - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (11, 103, NULL, 106, 'Discussion inside Group #106: Exploring practical deployment challenges in Border Health Systems.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (12, 103, NULL, 109, 'Group #109 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (13, 104, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (14, 104, NULL, NULL, 'Fashion & Brand Ambassadorship - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (15, 104, NULL, 123, 'Discussion inside Group #123: Exploring practical deployment challenges in Acting & Visual Arts.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (16, 104, NULL, 127, 'Group #127 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (17, 105, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (18, 105, NULL, NULL, 'Vipassana Mindfulness - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (19, 105, NULL, 79, 'Discussion inside Group #79: Exploring practical deployment challenges in Mind State Observation.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (20, 105, NULL, 83, 'Group #83 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (21, 106, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (22, 106, NULL, NULL, 'Satirical Writing - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (23, 106, NULL, 76, 'Discussion inside Group #76: Exploring practical deployment challenges in Film Directing.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (24, 106, NULL, 80, 'Group #80 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (25, 107, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (26, 107, NULL, NULL, 'Contemporary Oil Painting - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (27, 107, NULL, 78, 'Discussion inside Group #78: Exploring practical deployment challenges in Exhibition Curation.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (28, 107, NULL, 82, 'Group #82 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (29, 108, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (30, 108, NULL, NULL, 'Peacebuilding & Dialogue - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (31, 108, NULL, 108, 'Discussion inside Group #108: Exploring practical deployment challenges in Women Human Rights Law.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (32, 108, NULL, 111, 'Group #111 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (33, 109, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (34, 109, NULL, NULL, 'Clinical Oncology - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (35, 109, NULL, 107, 'Discussion inside Group #107: Exploring practical deployment challenges in Evidence-Based Therapeutics.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (36, 109, NULL, 110, 'Group #110 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (37, 110, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (38, 110, NULL, NULL, 'Independent Cinema Direction - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (39, 110, NULL, 121, 'Discussion inside Group #121: Exploring practical deployment challenges in Screenwriting & Storytelling.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (40, 110, NULL, 125, 'Group #125 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (41, 111, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (42, 111, NULL, NULL, 'Corporate Strategy - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (43, 111, NULL, 17, 'Discussion inside Group #17: Exploring practical deployment challenges in Banking & Finance.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (44, 111, NULL, 21, 'Group #21 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (45, 112, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (46, 112, NULL, NULL, 'Digital Banking - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (47, 112, NULL, 3, 'Discussion inside Group #3: Exploring practical deployment challenges in Fintech Ecosystems.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (48, 112, NULL, 6, 'Group #6 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (49, 113, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (50, 113, NULL, NULL, 'Real Estate Development - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (51, 113, NULL, 16, 'Discussion inside Group #16: Exploring practical deployment challenges in Cross-Border Investment.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (52, 113, NULL, 20, 'Group #20 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (53, 114, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (54, 114, NULL, NULL, 'Mega-Project Engineering - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (55, 114, NULL, 32, 'Discussion inside Group #32: Exploring practical deployment challenges in Retail Property Development.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (56, 114, NULL, 34, 'Group #34 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (57, 115, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (58, 115, NULL, NULL, 'FMCG Manufacturing - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (59, 115, NULL, 47, 'Discussion inside Group #47: Exploring practical deployment challenges in Supply Chain Architecture.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (60, 115, NULL, 49, 'Group #49 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (61, 116, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (62, 116, NULL, NULL, 'Luxury Hospitality - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (63, 116, NULL, 62, 'Discussion inside Group #62: Exploring practical deployment challenges in Aviation Logistics.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (64, 116, NULL, 64, 'Group #64 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (65, 117, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (66, 117, NULL, NULL, 'Offshore Exploration - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (67, 117, NULL, 61, 'Discussion inside Group #61: Exploring practical deployment challenges in Petroleum Engineering.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (68, 117, NULL, 63, 'Group #63 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (69, 118, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (70, 118, NULL, NULL, 'Telecommunications - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (71, 118, NULL, 1, 'Discussion inside Group #1: Exploring practical deployment challenges in IT Infrastructure.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (72, 118, NULL, 4, 'Group #4 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (73, 119, NULL, NULL, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (74, 119, NULL, NULL, 'Commercial Banking - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (75, 119, NULL, 2, 'Discussion inside Group #2: Exploring practical deployment challenges in Core Banking Transformation.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (76, 119, NULL, 5, 'Group #5 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (77, 120, NULL, NULL, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 1, 14, 2, 3, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (78, 120, NULL, NULL, 'Port Logistics Management - Strategic Roadmap & Executive Summary: We are establishing multi-tier operational benchmarks for the coming fiscal quarters.', 0, 19, 1, 5, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (79, 120, NULL, 46, 'Discussion inside Group #46: Exploring practical deployment challenges in Agro-Export Supply Chains.', 0, 11, 1, 2, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (80, 120, NULL, 48, 'Group #48 Inquiry: Which methodology should our working circle prioritize this quarter?', 1, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (81, 118, NULL, 7, 'AI & Data Analytics - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (82, 119, NULL, 8, 'AI & Data Analytics - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (83, 112, NULL, 9, 'AI & Data Analytics - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (84, 118, NULL, 10, 'Cybersecurity & Governance - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (85, 119, NULL, 11, 'Cybersecurity & Governance - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (86, 112, NULL, 12, 'Cybersecurity & Governance - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (87, 118, NULL, 13, 'Emerging Tech & Web3 - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (88, 119, NULL, 14, 'Emerging Tech & Web3 - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (89, 112, NULL, 15, 'Emerging Tech & Web3 - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (90, 112, NULL, 18, 'Commercial Banking Systems - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (91, 119, NULL, 19, 'Fintech & Mobile Wallets - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (92, 112, NULL, 22, 'Microfinance & Rural Inclusion - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (93, 119, NULL, 23, 'Microfinance & Rural Inclusion - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (94, 113, NULL, 24, 'Microfinance & Rural Inclusion - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (95, 111, NULL, 25, 'Venture Capital & Startups - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (96, 112, NULL, 26, 'Venture Capital & Startups - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (97, 119, NULL, 27, 'Venture Capital & Startups - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (98, 113, NULL, 28, 'Corporate Accounting & Tax - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (99, 111, NULL, 29, 'Corporate Accounting & Tax - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (100, 112, NULL, 30, 'Corporate Accounting & Tax - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (101, 113, NULL, 31, 'Urban Planning & Zoning - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (102, 113, NULL, 33, 'Urban Planning & Zoning - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (103, 113, NULL, 35, 'Green Building Materials - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (104, 114, NULL, 36, 'Green Building Materials - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (105, 113, NULL, 37, 'Smart Transit Networks - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (106, 114, NULL, 38, 'Smart Transit Networks - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (107, 113, NULL, 39, 'Smart Transit Networks - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (108, 114, NULL, 40, 'Commercial Real Estate - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (109, 113, NULL, 41, 'Commercial Real Estate - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (110, 114, NULL, 42, 'Commercial Real Estate - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (111, 113, NULL, 43, 'Residential Architecture - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (112, 114, NULL, 44, 'Residential Architecture - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (113, 113, NULL, 45, 'Residential Architecture - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (114, 120, NULL, 50, 'Modern Trade Distribution - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (115, 115, NULL, 51, 'Modern Trade Distribution - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (116, 120, NULL, 52, 'Cold Chain Logistics - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (117, 115, NULL, 53, 'Cold Chain Logistics - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (118, 120, NULL, 54, 'Cold Chain Logistics - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (119, 115, NULL, 55, 'Packaging & Quality Control - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (120, 120, NULL, 56, 'Packaging & Quality Control - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (121, 115, NULL, 57, 'Packaging & Quality Control - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (122, 120, NULL, 58, 'Direct-to-Consumer Brands - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (123, 115, NULL, 59, 'Direct-to-Consumer Brands - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (124, 120, NULL, 60, 'Direct-to-Consumer Brands - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (125, 117, NULL, 65, 'Solar & Mini-Grids - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (126, 116, NULL, 66, 'Solar & Mini-Grids - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (127, 117, NULL, 67, 'HSE Industrial Safety - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (128, 116, NULL, 68, 'HSE Industrial Safety - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (129, 117, NULL, 69, 'HSE Industrial Safety - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (130, 116, NULL, 70, 'Power Transmission Engineering - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (131, 117, NULL, 71, 'Power Transmission Engineering - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (132, 116, NULL, 72, 'Power Transmission Engineering - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (133, 117, NULL, 73, 'Petrochemical Refining - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (134, 116, NULL, 74, 'Petrochemical Refining - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (135, 117, NULL, 75, 'Petrochemical Refining - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (136, 106, NULL, 84, 'Burmese Literature & Translation - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (137, 102, NULL, 85, 'Independent Cinema & Scripting - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (138, 107, NULL, 86, 'Independent Cinema & Scripting - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (139, 105, NULL, 87, 'Independent Cinema & Scripting - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (140, 106, NULL, 88, 'Traditional Textile & Design - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (141, 102, NULL, 89, 'Traditional Textile & Design - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (142, 107, NULL, 90, 'Traditional Textile & Design - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (143, 105, NULL, 92, 'Lethwei & Mixed Martial Arts - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (144, 111, NULL, 93, 'Lethwei & Mixed Martial Arts - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (145, 105, NULL, 95, 'Strength & High-Performance Conditioning - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (146, 111, NULL, 96, 'Strength & High-Performance Conditioning - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (147, 101, NULL, 97, 'Youth Football Academies - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (148, 105, NULL, 98, 'Youth Football Academies - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (149, 111, NULL, 99, 'Youth Football Academies - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (150, 101, NULL, 100, 'Sports Nutrition & Recovery - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (151, 105, NULL, 101, 'Sports Nutrition & Recovery - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (152, 111, NULL, 102, 'Sports Nutrition & Recovery - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (153, 101, NULL, 103, 'Mental Focus & Meditation - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (154, 105, NULL, 104, 'Mental Focus & Meditation - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (155, 111, NULL, 105, 'Mental Focus & Meditation - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (156, 103, NULL, 112, 'Humanitarian Relief Logistics - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (157, 109, NULL, 113, 'Humanitarian Relief Logistics - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (158, 108, NULL, 114, 'Humanitarian Relief Logistics - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (159, 103, NULL, 115, 'Community Health Worker Training - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (160, 109, NULL, 116, 'Community Health Worker Training - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (161, 108, NULL, 117, 'Community Health Worker Training - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (162, 103, NULL, 118, 'Mental Well-Being & Counseling - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (163, 109, NULL, 119, 'Mental Well-Being & Counseling - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (164, 108, NULL, 120, 'Mental Well-Being & Counseling - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (165, 106, NULL, 122, 'Telecom Tower Infrastructure - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (166, 118, NULL, 124, 'Fiber Optic Backbone Networks - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (167, 106, NULL, 126, 'Fiber Optic Backbone Networks - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (168, 118, NULL, 128, 'Broadcasting & Digital Media - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (169, 110, NULL, 129, 'Broadcasting & Digital Media - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (170, 106, NULL, 130, 'Satirical Media & Visual Arts - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (171, 104, NULL, 131, 'Satirical Media & Visual Arts - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (172, 118, NULL, 132, 'Satirical Media & Visual Arts - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (173, 110, NULL, 133, 'Creator Economy & Digital Marketing - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (174, 106, NULL, 134, 'Creator Economy & Digital Marketing - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (175, 104, NULL, 135, 'Creator Economy & Digital Marketing - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (176, 112, NULL, 136, 'Maritime Container Shipping - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (177, 120, NULL, 137, 'Maritime Container Shipping - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (178, 116, NULL, 138, 'Maritime Container Shipping - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (179, 111, NULL, 139, 'Inland Waterway Logistics - Working Circle 1: Best practice checklist for project risk management and quality assurance.', 0, 12, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (180, 112, NULL, 140, 'Inland Waterway Logistics - Working Circle 2: Welcome to this working circle! Please share your background and current project focus.', 0, 13, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (181, 120, NULL, 141, 'Inland Waterway Logistics - Working Circle 3: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 14, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (182, 116, NULL, 142, 'Agro-Export Commodity Trading - Working Circle 1: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 15, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (183, 111, NULL, 143, 'Agro-Export Commodity Trading - Working Circle 2: Best practice checklist for project risk management and quality assurance.', 0, 16, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (184, 112, NULL, 144, 'Agro-Export Commodity Trading - Working Circle 3: Welcome to this working circle! Please share your background and current project focus.', 0, 5, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (185, 120, NULL, 145, 'Aviation Cargo & Ground Handling - Working Circle 1: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 6, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (186, 116, NULL, 146, 'Aviation Cargo & Ground Handling - Working Circle 2: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 7, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (187, 111, NULL, 147, 'Aviation Cargo & Ground Handling - Working Circle 3: Best practice checklist for project risk management and quality assurance.', 0, 8, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (188, 112, NULL, 148, 'Cross-Border Customs Facilitation - Working Circle 1: Welcome to this working circle! Please share your background and current project focus.', 0, 9, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (189, 120, NULL, 149, 'Cross-Border Customs Facilitation - Working Circle 2: Monthly discussion thread: What are the primary bottlenecks you have encountered recently?', 0, 10, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPost] ([PostId], [AuthorId], [CommunityId], [GroupId], [Content], [HasPoll], [LikeCount], [CommentCount], [ShareCount], [PostType], [IsActive], [IsPrivate], [CreatedAt], [IsDeleted])
VALUES (190, 116, NULL, 150, 'Cross-Border Customs Facilitation - Working Circle 3: Resource sharing: Key references, research documents, and toolkits for our sector.', 0, 11, 1, 1, 'STANDARD', 1, 0, DATEADD(day, -12, SYSUTCDATETIME()), 0);
SET IDENTITY_INSERT [dbo].[TblPost] OFF;

PRINT '15. Inserting Polls & Options...';
SET IDENTITY_INSERT [dbo].[TblPoll] ON;
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (1, 1, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (2, 4, 'Group #94 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (3, 5, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (4, 8, 'Group #81 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (5, 9, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (6, 12, 'Group #109 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (7, 13, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (8, 16, 'Group #127 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (9, 17, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (10, 20, 'Group #83 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (11, 21, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (12, 24, 'Group #80 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (13, 25, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (14, 28, 'Group #82 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (15, 29, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (16, 32, 'Group #111 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (17, 33, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (18, 36, 'Group #110 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (19, 37, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (20, 40, 'Group #125 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (21, 41, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (22, 44, 'Group #21 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (23, 45, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (24, 48, 'Group #6 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (25, 49, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (26, 52, 'Group #20 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (27, 53, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (28, 56, 'Group #34 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (29, 57, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (30, 60, 'Group #49 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (31, 61, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (32, 64, 'Group #64 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (33, 65, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (34, 68, 'Group #63 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (35, 69, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (36, 72, 'Group #4 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (37, 73, 'National health and community clinical safety: Which public health priority demands urgent collaborative attention?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (38, 76, 'Group #5 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (39, 77, 'Transformative shift in our digital payments infrastructure across Myanmar: What key area needs the fastest adoption?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPoll] ([PollId], [PostId], [Question], [IsMultipleChoice], [TotalVotes], [ExpiresAt], [CreatedAt], [IsDeleted])
VALUES (40, 80, 'Group #48 Inquiry: Which methodology should our working circle prioritize this quarter?', 0, 24, DATEADD(day, 30, SYSUTCDATETIME()), DATEADD(day, -15, SYSUTCDATETIME()), 0);
SET IDENTITY_INSERT [dbo].[TblPoll] OFF;

PRINT '16. Inserting Poll Options & Sample Votes...';
SET IDENTITY_INSERT [dbo].[TblPollOption] ON;
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (1, 1, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (2, 1, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (3, 1, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (4, 1, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (5, 2, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (6, 2, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (7, 2, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (8, 2, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (9, 3, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (10, 3, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (11, 3, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (12, 3, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (13, 4, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (14, 4, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (15, 4, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (16, 4, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (17, 5, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (18, 5, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (19, 5, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (20, 5, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (21, 6, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (22, 6, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (23, 6, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (24, 6, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (25, 7, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (26, 7, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (27, 7, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (28, 7, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (29, 8, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (30, 8, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (31, 8, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (32, 8, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (33, 9, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (34, 9, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (35, 9, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (36, 9, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (37, 10, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (38, 10, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (39, 10, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (40, 10, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (41, 11, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (42, 11, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (43, 11, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (44, 11, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (45, 12, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (46, 12, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (47, 12, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (48, 12, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (49, 13, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (50, 13, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (51, 13, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (52, 13, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (53, 14, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (54, 14, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (55, 14, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (56, 14, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (57, 15, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (58, 15, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (59, 15, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (60, 15, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (61, 16, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (62, 16, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (63, 16, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (64, 16, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (65, 17, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (66, 17, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (67, 17, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (68, 17, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (69, 18, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (70, 18, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (71, 18, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (72, 18, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (73, 19, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (74, 19, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (75, 19, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (76, 19, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (77, 20, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (78, 20, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (79, 20, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (80, 20, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (81, 21, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (82, 21, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (83, 21, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (84, 21, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (85, 22, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (86, 22, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (87, 22, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (88, 22, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (89, 23, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (90, 23, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (91, 23, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (92, 23, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (93, 24, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (94, 24, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (95, 24, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (96, 24, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (97, 25, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (98, 25, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (99, 25, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (100, 25, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (101, 26, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (102, 26, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (103, 26, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (104, 26, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (105, 27, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (106, 27, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (107, 27, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (108, 27, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (109, 28, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (110, 28, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (111, 28, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (112, 28, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (113, 29, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (114, 29, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (115, 29, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (116, 29, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (117, 30, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (118, 30, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (119, 30, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (120, 30, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (121, 31, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (122, 31, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (123, 31, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (124, 31, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (125, 32, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (126, 32, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (127, 32, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (128, 32, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (129, 33, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (130, 33, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (131, 33, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (132, 33, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (133, 34, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (134, 34, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (135, 34, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (136, 34, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (137, 35, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (138, 35, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (139, 35, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (140, 35, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (141, 36, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (142, 36, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (143, 36, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (144, 36, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (145, 37, 'Rural Mobile Clinics', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (146, 37, 'Maternal Nutrition Networks', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (147, 37, 'Clean Water Infrastructure', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (148, 37, 'Healthcare Worker Grants', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (149, 38, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (150, 38, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (151, 38, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (152, 38, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (153, 39, 'Instant Merchant Settlements', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (154, 39, 'Cross-Border Remittances', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (155, 39, 'Offline NFC Wallet Resilience', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (156, 39, 'Zero-Fee Microtransactions', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (157, 40, 'Quarterly Case Study Review', 6, 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (158, 40, 'Live Industry Q&A', 12, 2, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (159, 40, 'Technical Whitepaper Breakdown', 18, 3, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblPollOption] ([PollOptionId], [PollId], [OptionText], [VoteCount], [DisplayOrder], [CreatedAt], [IsDeleted])
VALUES (160, 40, 'Peer Project Critiques', 24, 4, DATEADD(day, -15, SYSUTCDATETIME()), 0);
SET IDENTITY_INSERT [dbo].[TblPollOption] OFF;

PRINT '17. Inserting Real User Comments on Posts...';
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (1, 123, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (1, 124, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (2, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (3, 122, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (4, 125, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (5, 121, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (5, 126, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (6, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (7, 130, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (8, 129, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (9, 129, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (9, 128, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (10, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (11, 128, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (12, 123, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (13, 127, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (13, 130, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (14, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (15, 126, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (16, 127, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (17, 125, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (17, 122, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (18, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (19, 124, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (20, 121, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (21, 123, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (21, 124, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (22, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (23, 122, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (24, 125, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (25, 121, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (25, 126, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (26, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (27, 130, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (28, 129, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (29, 129, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (29, 128, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (30, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (31, 128, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (32, 123, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (33, 127, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (33, 130, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (34, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (35, 126, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (36, 127, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (37, 125, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (37, 122, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (38, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (39, 124, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (40, 121, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (41, 123, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (41, 124, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (42, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (43, 122, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (44, 125, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (45, 121, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (45, 126, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (46, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (47, 130, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (48, 129, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (49, 129, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (49, 128, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (50, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (51, 128, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (52, 123, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (53, 127, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (53, 130, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (54, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (55, 126, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (56, 127, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (57, 125, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (57, 122, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (58, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (59, 124, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (60, 121, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (61, 123, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (61, 124, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (62, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (63, 122, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (64, 125, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (65, 121, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (65, 126, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (66, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (67, 130, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (68, 129, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (69, 129, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (69, 128, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (70, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (71, 128, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (72, 123, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (73, 127, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (73, 130, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (74, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (75, 126, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (76, 127, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (77, 125, 'Incredible insight! Fully agree with your perspective on long-term sustainability.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (77, 122, 'Voted! Looking forward to seeing the aggregated community sentiment on this.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (78, 121, 'Thank you for sharing this real-world case study. The practical takeaways are immense.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (79, 124, 'This is exactly what our local industry needs to focus on right now.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (80, 121, 'Very inspiring read! Bookmarked for our team''s strategic planning session.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (81, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (82, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (83, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (84, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (85, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (86, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (87, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (88, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (89, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (90, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (91, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (92, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (93, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (94, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (95, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (96, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (97, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (98, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (99, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (100, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (101, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (102, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (103, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (104, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (105, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (106, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (107, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (108, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (109, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (110, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (111, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (112, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (113, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (114, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (115, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (116, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (117, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (118, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (119, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (120, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (121, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (122, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (123, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (124, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (125, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (126, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (127, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (128, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (129, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (130, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (131, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (132, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (133, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (134, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (135, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (136, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (137, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (138, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (139, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (140, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (141, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (142, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (143, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (144, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (145, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (146, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (147, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (148, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (149, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (150, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (151, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (152, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (153, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (154, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (155, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (156, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (157, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (158, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (159, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (160, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (161, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (162, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (163, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (164, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (165, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (166, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (167, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (168, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (169, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (170, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (171, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (172, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (173, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (174, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (175, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (176, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (177, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (178, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (179, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (180, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (181, 122, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (182, 123, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (183, 124, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (184, 125, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (185, 126, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (186, 127, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (187, 128, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (188, 129, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (189, 130, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblComment] ([PostId], [UserId], [Content], [CreatedAt], [IsDeleted])
VALUES (190, 121, 'Great initiative! Really glad to be part of this group discussion.', DATEADD(day, -9, SYSUTCDATETIME()), 0);

PRINT '18. Populating Content Moderation Reports (Covering all statuses)...';
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POST', 1, 121, 'MISLEADING', 'Reported claim about transaction latency requires external data verification.', 'PENDING', NULL, NULL, NULL, DATEADD(day, -7, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POST', 2, 122, 'INAPPROPRIATE_CONTENT', 'Flagged due to controversial commercial projection phrasing.', 'RESOLVED', 'Post reviewed by moderator. Clarification note attached. Post approved.', 1, DATEADD(day, -2, SYSUTCDATETIME()), DATEADD(day, -7, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POST', 3, 123, 'SPAM', 'Duplicate discussion thread posted within same working room.', 'DISMISSED', 'Content verified as authentic standalone contribution. Case closed.', 1, DATEADD(day, -2, SYSUTCDATETIME()), DATEADD(day, -7, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POLL', 1, 124, 'MISINFORMATION', 'Option labels alleged to have skewed statistical bias.', 'ACTIONED', 'Poll options reviewed. Content warning appended.', 1, DATEADD(day, -2, SYSUTCDATETIME()), DATEADD(day, -7, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POST', 5, 125, 'COPYRIGHT_VIOLATION', 'Reported excerpt matches proprietary textbook material.', 'PENDING', NULL, NULL, NULL, DATEADD(day, -7, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POST', 7, 126, 'HARASSMENT', 'Review required on heated debate in comment stream.', 'RESOLVED', 'Offending sub-comments removed. Discussion restored to compliance.', 1, DATEADD(day, -2, SYSUTCDATETIME()), DATEADD(day, -7, SYSUTCDATETIME()));
INSERT INTO [dbo].[TblContentReport] ([ContentType], [ContentId], [ReporterUserId], [ReasonCategory], [Details], [Status], [AdminNote], [HandledByAdminId], [HandledAt], [CreatedAt])
VALUES ('POLL', 3, 127, 'OTHER', 'Inquiry regarding eligibility of external corporate voters.', 'DISMISSED', 'Poll criteria compliant with platform charter.', 1, DATEADD(day, -2, SYSUTCDATETIME()), DATEADD(day, -7, SYSUTCDATETIME()));

PRINT '19. Populating User Activities for Telemetry & Report Charts...';
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -30, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -29, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -28, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -27, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -26, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -25, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -24, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -23, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -22, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -21, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -20, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -19, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -18, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -17, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -16, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -15, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -14, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -13, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -12, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -11, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -10, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -9, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -8, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -7, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -6, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -5, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -4, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -3, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -2, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (121, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (122, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (123, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (124, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (125, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (126, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (127, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (128, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (129, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (130, 'JOIN_GROUP', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (101, 'POST_CREATE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (102, 'POLL_VOTE', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (103, 'COMMENT_ADD', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (104, 'LIKE_POST', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);
INSERT INTO [dbo].[TblUserActivity] ([UserId], [ActivityType], [Description], [TargetEntityType], [TargetEntityId], [CreatedAt], [IsDeleted])
VALUES (105, 'JOIN_COMMUNITY', 'User participated in platform activity.', 'POST', 1, DATEADD(day, -1, SYSUTCDATETIME()), 0);

COMMIT TRANSACTION;
PRINT '==============================================================================';
PRINT 'CommunityLink Master Presentation Seed Data generation completed successfully!';
PRINT '==============================================================================';
