/* ==============================================================================
   CommunityLink - 50% Seed Data Reduction Script
   ==============================================================================
   - Reduces seeded users, communities, groups, posts, and polls by ~50%.
   - Maintains full foreign key integrity and parent-child hierarchy.
   ============================================================================== */

USE [CommunityLink];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
GO

BEGIN TRANSACTION;

PRINT '=== STARTING 50% SEED DATA REDUCTION ===';

-- 1. Identify Users to Delete (Preserve Admin [1], 5 Domain Pros, 5 Public Figures, 5 Members)
-- Kept Domain Pros: 101, 102, 103, 104, 105
-- Kept Public Figures: 111, 112, 113, 114 (Aik Htun), 115
-- Kept Members: 121, 122, 123, 124, 125
-- Kept Admin: 1
DECLARE @KeptUserIds TABLE (UserId INT PRIMARY KEY);
INSERT INTO @KeptUserIds (UserId)
VALUES (1), (101), (102), (103), (104), (105), (111), (112), (113), (114), (115), (121), (122), (123), (124), (125);

DECLARE @DeletedUserIds TABLE (UserId INT PRIMARY KEY);
INSERT INTO @DeletedUserIds (UserId)
SELECT UserId FROM [dbo].[TblUser]
WHERE UserId NOT IN (SELECT UserId FROM @KeptUserIds);

DECLARE @DelUserCount INT = (SELECT COUNT(*) FROM @DeletedUserIds);
PRINT 'Users identified for deletion: ' + CAST(@DelUserCount AS VARCHAR(10));

-- 2. Identify Communities to Delete (Keep Top-Level 1, 2, 3, 7, 8; Delete 4, 5, 6, 9, 10)
DECLARE @KeptTopCommunityIds TABLE (CommunityId INT PRIMARY KEY);
INSERT INTO @KeptTopCommunityIds (CommunityId)
VALUES (1), (2), (3), (7), (8);

DECLARE @DeletedCommunityIds TABLE (CommunityId INT PRIMARY KEY);

-- Sub-communities of deleted top communities
INSERT INTO @DeletedCommunityIds (CommunityId)
SELECT CommunityId FROM [dbo].[TblCommunity]
WHERE ParentCommunityId IN (4, 5, 6, 9, 10);

-- Deleted top communities themselves
INSERT INTO @DeletedCommunityIds (CommunityId)
VALUES (4), (5), (6), (9), (10);

DECLARE @DelCommCount INT = (SELECT COUNT(*) FROM @DeletedCommunityIds);
PRINT 'Communities identified for deletion: ' + CAST(@DelCommCount AS VARCHAR(10));

-- 3. Identify Groups to Delete
DECLARE @DeletedGroupIds TABLE (GroupId INT PRIMARY KEY);
INSERT INTO @DeletedGroupIds (GroupId)
SELECT GroupId FROM [dbo].[TblGroup]
WHERE SubCommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds)
   OR CreatorId IN (SELECT UserId FROM @DeletedUserIds);

DECLARE @DelGroupCount INT = (SELECT COUNT(*) FROM @DeletedGroupIds);
PRINT 'Groups identified for deletion: ' + CAST(@DelGroupCount AS VARCHAR(10));

-- 4. Identify Posts and Polls to Delete
DECLARE @DeletedPostIds TABLE (PostId INT PRIMARY KEY);
INSERT INTO @DeletedPostIds (PostId)
SELECT PostId FROM [dbo].[TblPost]
WHERE AuthorId IN (SELECT UserId FROM @DeletedUserIds)
   OR GroupId IN (SELECT GroupId FROM @DeletedGroupIds)
   OR CommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds);

DECLARE @DeletedPollIds TABLE (PollId INT PRIMARY KEY);
INSERT INTO @DeletedPollIds (PollId)
SELECT PollId FROM [dbo].[TblPoll]
WHERE PostId IN (SELECT PostId FROM @DeletedPostIds);

DECLARE @DelPostCount INT = (SELECT COUNT(*) FROM @DeletedPostIds);
DECLARE @DelPollCount INT = (SELECT COUNT(*) FROM @DeletedPollIds);
PRINT 'Posts identified for deletion: ' + CAST(@DelPostCount AS VARCHAR(10));
PRINT 'Polls identified for deletion: ' + CAST(@DelPollCount AS VARCHAR(10));

-- ==============================================================================
-- CASCADE DELETIONS IN PROPER DEPENDENCY ORDER
-- ==============================================================================

-- Content Reports
IF OBJECT_ID('dbo.TblContentReport', 'U') IS NOT NULL
BEGIN
    DELETE FROM [dbo].[TblContentReport]
    WHERE ReporterUserId IN (SELECT UserId FROM @DeletedUserIds)
       OR (ContentType = 'POST' AND ContentId IN (SELECT PostId FROM @DeletedPostIds))
       OR (ContentType = 'POLL' AND ContentId IN (SELECT PollId FROM @DeletedPollIds))
       OR (ContentType = 'USER' AND ContentId IN (SELECT UserId FROM @DeletedUserIds))
       OR (ContentType = 'GROUP' AND ContentId IN (SELECT GroupId FROM @DeletedGroupIds))
       OR (ContentType = 'COMMUNITY' AND ContentId IN (SELECT CommunityId FROM @DeletedCommunityIds));
    PRINT 'Cleaned TblContentReport';
END

-- Chat Group tables
IF OBJECT_ID('dbo.TblChatGroupMessageReaction', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupMessageReaction] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupMessageUserState', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupMessageUserState] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupMessage', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupMessage] WHERE SenderId IN (SELECT UserId FROM @DeletedUserIds) OR PinnedByUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupInviteLink', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupInviteLink] WHERE CreatedByUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupInvite', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupInvite] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds) OR InvitedByUserId IN (SELECT UserId FROM @DeletedUserIds) OR DeletedBy IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupJoinRequest', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupJoinRequest] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds) OR ReviewedBy IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupBan', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupBan] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds) OR BannedByUserId IN (SELECT UserId FROM @DeletedUserIds) OR RevokedByUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupMember', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupMember] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroupPaymentTransaction', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroupPaymentTransaction] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds) OR CreatorUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatGroup', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatGroup] WHERE CreatorId IN (SELECT UserId FROM @DeletedUserIds);

-- Direct Chat / Conversation tables
IF OBJECT_ID('dbo.TblChatMessageReaction', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatMessageReaction] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatMessageUserState', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatMessageUserState] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblChatMessage', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblChatMessage] WHERE SenderId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblConversation', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblConversation] WHERE UserOneId IN (SELECT UserId FROM @DeletedUserIds) OR UserTwoId IN (SELECT UserId FROM @DeletedUserIds);

-- User Activity, Social & Audit tables
IF OBJECT_ID('dbo.TblNotification', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblNotification] WHERE RecipientUserId IN (SELECT UserId FROM @DeletedUserIds) OR ActorUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserActivity', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserActivity] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserFollow', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserFollow] WHERE FollowerId IN (SELECT UserId FROM @DeletedUserIds) OR FolloweeId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserBlock', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserBlock] WHERE BlockerUserId IN (SELECT UserId FROM @DeletedUserIds) OR BlockedUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserRating', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserRating] WHERE RaterUserId IN (SELECT UserId FROM @DeletedUserIds) OR TargetUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblSkillEndorsement', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblSkillEndorsement] WHERE EndorserUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserSkill', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserSkill] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblIdentityVerification', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblIdentityVerification] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserVerificationAudit', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserVerificationAudit] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUserSubscription', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserSubscription] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

-- LinkDrop & Payments
IF OBJECT_ID('dbo.TblLinkDropTransaction', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblLinkDropTransaction] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblLinkDropPurchaseProof', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblLinkDropPurchaseProof] WHERE PurchaseId IN (SELECT PurchaseId FROM [dbo].[TblLinkDropPurchase] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds));

IF OBJECT_ID('dbo.TblLinkDropPurchase', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblLinkDropPurchase] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblLinkDropWallet', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblLinkDropWallet] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblCreatorPayoutRequest', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblCreatorPayoutRequest] WHERE CreatorUserId IN (SELECT UserId FROM @DeletedUserIds) OR ReviewedBy IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblCreatorChatSetting', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblCreatorChatSetting] WHERE CreatorUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblPrivateChatPaymentTransaction', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPrivateChatPaymentTransaction] WHERE CreatorUserId IN (SELECT UserId FROM @DeletedUserIds) OR BuyerUserId IN (SELECT UserId FROM @DeletedUserIds);

-- Poll Votes, Options, Polls
IF OBJECT_ID('dbo.TblPollVote', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPollVote] WHERE PollId IN (SELECT PollId FROM @DeletedPollIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblPollOption', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPollOption] WHERE PollId IN (SELECT PollId FROM @DeletedPollIds);

IF OBJECT_ID('dbo.TblPoll', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPoll] WHERE PollId IN (SELECT PollId FROM @DeletedPollIds);

-- Post Interactions & Posts
IF OBJECT_ID('dbo.TblComment', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblComment] WHERE PostId IN (SELECT PostId FROM @DeletedPostIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblPostLike', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPostLike] WHERE PostId IN (SELECT PostId FROM @DeletedPostIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblPostShare', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPostShare] WHERE PostId IN (SELECT PostId FROM @DeletedPostIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds) OR TargetCommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds);

IF OBJECT_ID('dbo.TblPostImage', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPostImage] WHERE PostId IN (SELECT PostId FROM @DeletedPostIds);

IF OBJECT_ID('dbo.TblSavedPost', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblSavedPost] WHERE PostId IN (SELECT PostId FROM @DeletedPostIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblSavedAccount', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblSavedAccount] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds) OR SavedUserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblPost', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblPost] WHERE PostId IN (SELECT PostId FROM @DeletedPostIds);

PRINT 'Cleaned Posts and Polls';

-- Groups
IF OBJECT_ID('dbo.TblGroupRating', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblGroupRating] WHERE GroupId IN (SELECT GroupId FROM @DeletedGroupIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblGroupJoinRequest', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblGroupJoinRequest] WHERE GroupId IN (SELECT GroupId FROM @DeletedGroupIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds) OR ReviewedBy IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblGroupMember', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblGroupMember] WHERE GroupId IN (SELECT GroupId FROM @DeletedGroupIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblGroup', 'U') IS NOT NULL
BEGIN
    DELETE FROM [dbo].[TblGroup] WHERE GroupId IN (SELECT GroupId FROM @DeletedGroupIds);
    
    -- Reassign creator of any remaining groups if previous creator was marked for deletion
    UPDATE [dbo].[TblGroup]
    SET CreatorId = 1
    WHERE CreatorId IN (SELECT UserId FROM @DeletedUserIds);
END

PRINT 'Cleaned Groups';

-- Communities
IF OBJECT_ID('dbo.TblCommunityRating', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblCommunityRating] WHERE CommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblCommunityJoinRequest', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblCommunityJoinRequest] WHERE CommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds) OR ReviewedBy IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblCommunityMember', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblCommunityMember] WHERE CommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds) OR UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblCommunityAuditLog', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblCommunityAuditLog] WHERE CommunityId IN (SELECT CommunityId FROM @DeletedCommunityIds) OR EditorId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblCommunity', 'U') IS NOT NULL
BEGIN
    -- Delete sub-communities first
    DELETE FROM [dbo].[TblCommunity]
    WHERE ParentCommunityId IN (4, 5, 6, 9, 10);

    -- Delete top communities
    DELETE FROM [dbo].[TblCommunity]
    WHERE CommunityId IN (4, 5, 6, 9, 10);

    -- Reassign owner of kept communities if previous owner is being deleted
    UPDATE [dbo].[TblCommunity]
    SET OwnerId = 1
    WHERE OwnerId IN (SELECT UserId FROM @DeletedUserIds);
END

PRINT 'Cleaned Communities';

-- Users & User Roles
IF OBJECT_ID('dbo.TblUserRole', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUserRole] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

IF OBJECT_ID('dbo.TblUser', 'U') IS NOT NULL
    DELETE FROM [dbo].[TblUser] WHERE UserId IN (SELECT UserId FROM @DeletedUserIds);

PRINT 'Cleaned Users and User Roles';

COMMIT TRANSACTION;

PRINT '=== 50% SEED DATA REDUCTION COMPLETED SUCCESSFULLY ===';
