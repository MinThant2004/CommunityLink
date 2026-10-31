/* ==============================================================================
   CommunityLink - Seed Creator Earnings for Dr. Thant Myint-U
   Target User: tmyintu@heritage.org (UserId = 102)
   ==============================================================================
   This script seeds:
   1. 3 Paid Chat Groups created by Dr. Thant Myint-U:
      - Southeast Asian Geopolitics Roundtable (250 LD)
      - Myanmar History & Heritage Fellowship (100 LD)
      - Manuscripts & Cultural Preservation Circle (50 LD)
   2. 18 Paid Group Joins across 8 different members
   3. Private Chat Setting (100 LD) + 7 Private Chat Unlocks
   4. Group Memberships for Creator and Buyers
   5. Direct Conversations between Dr. Thant Myint-U and Unlocked Buyers
   6. Wallet balances and LinkDrop transaction ledgers
   7. Real-world 40-day chronological timeline
   ============================================================================== */

USE [CommunityLink];
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

PRINT '=== SEEDING CREATOR EARNINGS FOR DR. THANT MYINT-U (USER 102) ===';

DECLARE @creatorId INT = 102;
DECLARE @now DATETIME2 = SYSUTCDATETIME();

-- Verify User Exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[TblUser] WHERE UserId = @creatorId)
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('User 102 (tmyintu@heritage.org) not found.', 16, 1);
    RETURN;
END

-- ==============================================================================
-- CLEANUP PREVIOUS SEEDED EARNINGS DATA FOR USER 102 (IDEMPOTENCY)
-- ==============================================================================
-- Delete previous private chat payment transactions for user 102
DELETE FROM [dbo].[TblPrivateChatPaymentTransaction] WHERE CreatorUserId = @creatorId;

-- Delete previous group payment transactions for user 102
DELETE FROM [dbo].[TblChatGroupPaymentTransaction] WHERE CreatorUserId = @creatorId;

-- Find previous chat groups created by user 102 with these demo names
DECLARE @OldGroupIds TABLE (GroupId INT);
INSERT INTO @OldGroupIds
SELECT ChatGroupId FROM [dbo].[TblChatGroup]
WHERE CreatorId = @creatorId
  AND Name IN (
      N'Myanmar History & Heritage Fellowship',
      N'Southeast Asian Geopolitics Roundtable',
      N'Manuscripts & Cultural Preservation Circle'
  );

DELETE FROM [dbo].[TblChatGroupMember] WHERE ChatGroupId IN (SELECT GroupId FROM @OldGroupIds);
DELETE FROM [dbo].[TblChatGroup] WHERE ChatGroupId IN (SELECT GroupId FROM @OldGroupIds);

-- Ensure Creator Chat Settings exist
IF EXISTS (SELECT 1 FROM [dbo].[TblCreatorChatSetting] WHERE CreatorUserId = @creatorId)
BEGIN
    UPDATE [dbo].[TblCreatorChatSetting]
    SET IsPrivateChatEnabled = 1,
        PrivateChatFeeLinkDrops = 100,
        UpdatedAt = @now
    WHERE CreatorUserId = @creatorId;
END
ELSE
BEGIN
    INSERT INTO [dbo].[TblCreatorChatSetting] (CreatorUserId, IsPrivateChatEnabled, PrivateChatFeeLinkDrops, CreatedAt)
    VALUES (@creatorId, 1, 100, DATEADD(DAY, -45, @now));
END

-- ==============================================================================
-- 1. CREATE 3 PAID CHAT GROUPS
-- ==============================================================================
DECLARE @g1 INT, @g2 INT, @g3 INT;

-- Group 1: High-Tier Geopolitics Roundtable (250 LD)
INSERT INTO [dbo].[TblChatGroup] (
    Name, Description, CreatorId, ChatType, AccessMode, JoinFeeLinkDrops,
    CommissionPercentageSnapshot, IsActive, CreatedAt, CreatedBy, IsDeleted
)
VALUES (
    N'Southeast Asian Geopolitics Roundtable',
    N'Strategic policy analysis, diplomatic case studies, and historic treaty frameworks across ASEAN and Asian superpowers.',
    @creatorId, 'PAID', 'PRIVATE', 250, 10.00, 1, DATEADD(DAY, -42, @now), @creatorId, 0
);
SET @g1 = CAST(SCOPE_IDENTITY() AS INT);

-- Group 2: Mid-Tier Heritage Fellowship (100 LD)
INSERT INTO [dbo].[TblChatGroup] (
    Name, Description, CreatorId, ChatType, AccessMode, JoinFeeLinkDrops,
    CommissionPercentageSnapshot, IsActive, CreatedAt, CreatedBy, IsDeleted
)
VALUES (
    N'Myanmar History & Heritage Fellowship',
    N'Exclusive deep-dives into pre-colonial chronicles, royal architectural heritage, and historical research methodology.',
    @creatorId, 'PAID', 'PUBLIC', 100, 10.00, 1, DATEADD(DAY, -40, @now), @creatorId, 0
);
SET @g2 = CAST(SCOPE_IDENTITY() AS INT);

-- Group 3: Community Manuscripts Circle (50 LD)
INSERT INTO [dbo].[TblChatGroup] (
    Name, Description, CreatorId, ChatType, AccessMode, JoinFeeLinkDrops,
    CommissionPercentageSnapshot, IsActive, CreatedAt, CreatedBy, IsDeleted
)
VALUES (
    N'Manuscripts & Cultural Preservation Circle',
    N'Community collaboration on digitizing antique palm-leaf manuscripts (Parabaik) and preserving oral histories.',
    @creatorId, 'PAID', 'PUBLIC', 50, 10.00, 1, DATEADD(DAY, -38, @now), @creatorId, 0
);
SET @g3 = CAST(SCOPE_IDENTITY() AS INT);

-- Add Creator as OWNER of all 3 groups
INSERT INTO [dbo].[TblChatGroupMember] (ChatGroupId, UserId, Role, JoinedAt, CreatedAt, CreatedBy, IsDeleted, IsMuted, CanDeleteMessages, CanRemoveMembers, CanBanMembers, CanManageInviteLinks, CanPinMessages)
VALUES 
    (@g1, @creatorId, 'OWNER', DATEADD(DAY, -42, @now), DATEADD(DAY, -42, @now), @creatorId, 0, 0, 1, 1, 1, 1, 1),
    (@g2, @creatorId, 'OWNER', DATEADD(DAY, -40, @now), DATEADD(DAY, -40, @now), @creatorId, 0, 0, 1, 1, 1, 1, 1),
    (@g3, @creatorId, 'OWNER', DATEADD(DAY, -38, @now), DATEADD(DAY, -38, @now), @creatorId, 0, 0, 1, 1, 1, 1, 1);

PRINT 'Created 3 Paid Chat Groups for Dr. Thant Myint-U.';

-- ==============================================================================
-- 2. ENSURE BUYER WALLETS ARE AMPLY FUNDED
-- ==============================================================================
-- Buyers: 121, 122, 123, 124, 125 (Normal Members), 114 (Aik Htun), 111 (Zaw Zaw), 112 (Aung Ko Win), 113 (Serge Pun), 115 (Dr. Sai Sam Htun), 101 (Aung La), 104 (Paing Takhon)
UPDATE [dbo].[TblLinkDropWallet]
SET PurchasedBalance = CASE WHEN PurchasedBalance < 2000 THEN 2000 ELSE PurchasedBalance END,
    Balance = CASE WHEN PurchasedBalance < 2000 THEN 2000 + EarnedBalance ELSE Balance END,
    UpdatedAt = @now
WHERE UserId IN (101, 104, 111, 112, 113, 114, 115, 121, 122, 123, 124, 125);

-- ==============================================================================
-- 3. SEED 18 PAID GROUP JOINS
-- ==============================================================================
DECLARE @GroupJoins TABLE (
    BuyerId INT,
    GroupId INT,
    Fee BIGINT,
    DaysAgo INT,
    HoursAgo INT
);

-- Geopolitics Roundtable (250 LD each: 5 VIP buyers)
INSERT INTO @GroupJoins (BuyerId, GroupId, Fee, DaysAgo, HoursAgo) VALUES
    (112, @g1, 250, 36, 14), -- Aung Ko Win
    (113, @g1, 250, 31, 9),  -- Serge Pun
    (114, @g1, 250, 24, 16), -- Aik Htun
    (115, @g1, 250, 17, 11), -- Dr. Sai Sam Htun
    (104, @g1, 250,  6, 18); -- Paing Takhon

-- History & Heritage Fellowship (100 LD each: 8 diverse buyers)
INSERT INTO @GroupJoins (BuyerId, GroupId, Fee, DaysAgo, HoursAgo) VALUES
    (111, @g2, 100, 38, 10), -- Zaw Zaw
    (101, @g2, 100, 33, 15), -- Aung La N Sang
    (114, @g2, 100, 28, 8),  -- Aik Htun
    (121, @g2, 100, 22, 14), -- Aung Kyaw Zin
    (122, @g2, 100, 18, 17), -- Thazin Moe
    (123, @g2, 100, 14, 11), -- Hein Htet Zaw
    (124, @g2, 100,  8, 13), -- Khin Yadanar Oo
    (125, @g2, 100,  3, 16); -- Min Khant Kyaw

-- Manuscripts Preservation Circle (50 LD each: 5 enthusiastic members)
INSERT INTO @GroupJoins (BuyerId, GroupId, Fee, DaysAgo, HoursAgo) VALUES
    (121, @g3, 50, 35, 12), -- Aung Kyaw Zin
    (122, @g3, 50, 29, 14), -- Thazin Moe
    (123, @g3, 50, 20, 10), -- Hein Htet Zaw
    (124, @g3, 50, 12, 15), -- Khin Yadanar Oo
    (125, @g3, 50,  2, 9);  -- Min Khant Kyaw

-- Insert Group Payment Transactions
DECLARE @SeededGroupTx TABLE (
    TxId BIGINT,
    GroupId INT,
    BuyerId INT,
    Gross BIGINT,
    Commission BIGINT,
    Net BIGINT,
    TxDate DATETIME2
);

INSERT INTO [dbo].[TblChatGroupPaymentTransaction] (
    ChatGroupId, UserId, CreatorUserId, GrossAmount, CommissionPercentage,
    CommissionAmount, NetAmount, PurchasedAmountDeducted, EarnedAmountDeducted,
    Status, CreatedAt
)
OUTPUT 
    inserted.PaymentTransactionId,
    inserted.ChatGroupId,
    inserted.UserId,
    inserted.GrossAmount,
    inserted.CommissionAmount,
    inserted.NetAmount,
    inserted.CreatedAt
INTO @SeededGroupTx (TxId, GroupId, BuyerId, Gross, Commission, Net, TxDate)
SELECT 
    j.GroupId,
    j.BuyerId,
    @creatorId,
    j.Fee,
    10.00,
    CAST(j.Fee * 10 / 100 AS BIGINT),
    CAST(j.Fee * 90 / 100 AS BIGINT),
    j.Fee,
    0,
    'COMPLETED',
    DATEADD(HOUR, -j.HoursAgo, DATEADD(DAY, -j.DaysAgo, @now))
FROM @GroupJoins j;

-- Add buyers as members of the chat groups they joined
INSERT INTO [dbo].[TblChatGroupMember] (
    ChatGroupId, UserId, Role, JoinedAt, CreatedAt, CreatedBy,
    IsDeleted, IsMuted, CanDeleteMessages, CanRemoveMembers, CanBanMembers, CanManageInviteLinks, CanPinMessages
)
SELECT DISTINCT 
    gtx.GroupId,
    gtx.BuyerId,
    'MEMBER',
    gtx.TxDate,
    gtx.TxDate,
    gtx.BuyerId,
    0, 0, 0, 0, 0, 0, 0
FROM @SeededGroupTx gtx
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[TblChatGroupMember] m 
    WHERE m.ChatGroupId = gtx.GroupId AND m.UserId = gtx.BuyerId AND m.IsDeleted = 0
);

PRINT 'Seeded 18 Chat Group Payment Transactions and Memberships.';

-- ==============================================================================
-- 4. SEED 7 PRIVATE CHAT UNLOCKS
-- ==============================================================================
DECLARE @PrivateUnlocks TABLE (
    BuyerId INT,
    DaysAgo INT,
    HoursAgo INT,
    Fee BIGINT
);

INSERT INTO @PrivateUnlocks (BuyerId, DaysAgo, HoursAgo, Fee) VALUES
    (114, 37, 11, 100), -- Aik Htun
    (111, 30, 16, 100), -- Zaw Zaw
    (101, 25,  9, 100), -- Aung La N Sang
    (112, 19, 15, 100), -- Aung Ko Win
    (121, 15, 13, 100), -- Aung Kyaw Zin
    (123,  9, 17, 100), -- Hein Htet Zaw
    (124,  4, 10, 100); -- Khin Yadanar Oo

-- Ensure conversation exists for each buyer
DECLARE @BuyerConversations TABLE (
    BuyerId INT PRIMARY KEY,
    ConversationId INT
);

DECLARE @curBuyer INT, @cDays INT, @cHours INT, @cFee BIGINT;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
SELECT BuyerId, DaysAgo, HoursAgo, Fee FROM @PrivateUnlocks;

OPEN cur;
FETCH NEXT FROM cur INTO @curBuyer, @cDays, @cHours, @cFee;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @u1 INT = CASE WHEN @curBuyer < @creatorId THEN @curBuyer ELSE @creatorId END;
    DECLARE @u2 INT = CASE WHEN @curBuyer < @creatorId THEN @creatorId ELSE @curBuyer END;
    DECLARE @convId INT = NULL;

    SELECT TOP 1 @convId = ConversationId 
    FROM [dbo].[TblConversation]
    WHERE UserOneId = @u1 AND UserTwoId = @u2 AND IsDeleted = 0;

    IF @convId IS NULL
    BEGIN
        INSERT INTO [dbo].[TblConversation] (
            UserOneId, UserTwoId, LastMessageAt, LastMessagePreview,
            CreatedAt, CreatedBy, IsDeleted
        )
        VALUES (
            @u1, @u2, 
            DATEADD(HOUR, -@cHours, DATEADD(DAY, -@cDays, @now)),
            N'Private consultation on heritage and research initiated.',
            DATEADD(HOUR, -@cHours, DATEADD(DAY, -@cDays, @now)),
            @curBuyer, 0
        );
        SET @convId = CAST(SCOPE_IDENTITY() AS INT);
    END

    INSERT INTO @BuyerConversations (BuyerId, ConversationId)
    VALUES (@curBuyer, @convId);

    FETCH NEXT FROM cur INTO @curBuyer, @cDays, @cHours, @cFee;
END

CLOSE cur;
DEALLOCATE cur;

-- Insert Private Chat Payment Transactions
DECLARE @SeededPrivateTx TABLE (
    TxId BIGINT,
    ConversationId INT,
    BuyerId INT,
    Gross BIGINT,
    Commission BIGINT,
    Net BIGINT,
    TxDate DATETIME2
);

INSERT INTO [dbo].[TblPrivateChatPaymentTransaction] (
    ConversationId, BuyerUserId, CreatorUserId, GrossAmountLinkDrops,
    CommissionAmount, CreatorAmount, Status, CreatedAt
)
OUTPUT 
    inserted.PrivateChatPaymentTransactionId,
    inserted.ConversationId,
    inserted.BuyerUserId,
    inserted.GrossAmountLinkDrops,
    inserted.CommissionAmount,
    inserted.CreatorAmount,
    inserted.CreatedAt
INTO @SeededPrivateTx (TxId, ConversationId, BuyerId, Gross, Commission, Net, TxDate)
SELECT 
    bc.ConversationId,
    pu.BuyerId,
    @creatorId,
    pu.Fee,
    CAST(pu.Fee * 10 / 100 AS BIGINT),
    CAST(pu.Fee * 90 / 100 AS BIGINT),
    'COMPLETED',
    DATEADD(HOUR, -pu.HoursAgo, DATEADD(DAY, -pu.DaysAgo, @now))
FROM @PrivateUnlocks pu
JOIN @BuyerConversations bc ON bc.BuyerId = pu.BuyerId;

PRINT 'Seeded 7 Private Chat Payment Transactions.';

-- ==============================================================================
-- 5. LEDGER ENTRIES: TblLinkDropTransaction
-- ==============================================================================
-- Record Buyer Spend Transactions
INSERT INTO [dbo].[TblLinkDropTransaction] (
    WalletId, UserId, TransactionType, Amount, BalanceBefore, BalanceAfter,
    ReferenceType, ReferenceId, Notes, CreatedAt,
    PurchasedAmountDeducted, EarnedAmountDeducted, RelatedUserId, RelatedGroupId,
    PurchasedBalanceBefore, PurchasedBalanceAfter, EarnedBalanceBefore, EarnedBalanceAfter
)
SELECT 
    w.WalletId,
    gt.BuyerId,
    'CHAT_GROUP_JOIN',
    gt.Gross,
    w.Balance,
    w.Balance - gt.Gross,
    'TblChatGroupPaymentTransaction',
    CAST(gt.TxId AS INT),
    CONCAT(N'Paid ', gt.Gross, N' LinkDrops to join Chat Group #', gt.GroupId),
    gt.TxDate,
    gt.Gross,
    0,
    @creatorId,
    gt.GroupId,
    w.PurchasedBalance,
    w.PurchasedBalance - gt.Gross,
    w.EarnedBalance,
    w.EarnedBalance
FROM @SeededGroupTx gt
JOIN [dbo].[TblLinkDropWallet] w ON w.UserId = gt.BuyerId;

INSERT INTO [dbo].[TblLinkDropTransaction] (
    WalletId, UserId, TransactionType, Amount, BalanceBefore, BalanceAfter,
    ReferenceType, ReferenceId, Notes, CreatedAt,
    PurchasedAmountDeducted, EarnedAmountDeducted, RelatedUserId, RelatedGroupId,
    PurchasedBalanceBefore, PurchasedBalanceAfter, EarnedBalanceBefore, EarnedBalanceAfter
)
SELECT 
    w.WalletId,
    pt.BuyerId,
    'PRIVATE_CHAT_UNLOCK',
    pt.Gross,
    w.Balance,
    w.Balance - pt.Gross,
    'CONVERSATION',
    pt.ConversationId,
    N'Unlocked 1-on-1 private chat with Dr. Thant Myint-U',
    pt.TxDate,
    pt.Gross,
    0,
    @creatorId,
    NULL,
    w.PurchasedBalance,
    w.PurchasedBalance - pt.Gross,
    w.EarnedBalance,
    w.EarnedBalance
FROM @SeededPrivateTx pt
JOIN [dbo].[TblLinkDropWallet] w ON w.UserId = pt.BuyerId;

-- Record Creator Earning Transactions
DECLARE @CreatorWalletId INT = (SELECT WalletId FROM [dbo].[TblLinkDropWallet] WHERE UserId = @creatorId);

INSERT INTO [dbo].[TblLinkDropTransaction] (
    WalletId, UserId, TransactionType, Amount, BalanceBefore, BalanceAfter,
    ReferenceType, ReferenceId, Notes, CreatedAt,
    PurchasedAmountDeducted, EarnedAmountDeducted, RelatedUserId, RelatedGroupId,
    PurchasedBalanceBefore, PurchasedBalanceAfter, EarnedBalanceBefore, EarnedBalanceAfter
)
SELECT 
    @CreatorWalletId,
    @creatorId,
    'CHAT_GROUP_EARNING',
    gt.Net,
    13500,
    13500 + gt.Net,
    'TblChatGroupPaymentTransaction',
    CAST(gt.TxId AS INT),
    CONCAT(N'Earned ', gt.Net, N' LinkDrops from paid group join by User #', gt.BuyerId),
    gt.TxDate,
    0, 0,
    gt.BuyerId,
    gt.GroupId,
    8500, 8500,
    5000, 5000 + gt.Net
FROM @SeededGroupTx gt;

INSERT INTO [dbo].[TblLinkDropTransaction] (
    WalletId, UserId, TransactionType, Amount, BalanceBefore, BalanceAfter,
    ReferenceType, ReferenceId, Notes, CreatedAt,
    PurchasedAmountDeducted, EarnedAmountDeducted, RelatedUserId, RelatedGroupId,
    PurchasedBalanceBefore, PurchasedBalanceAfter, EarnedBalanceBefore, EarnedBalanceAfter
)
SELECT 
    @CreatorWalletId,
    @creatorId,
    'PRIVATE_CHAT_EARNING',
    pt.Net,
    13500,
    13500 + pt.Net,
    'CONVERSATION',
    pt.ConversationId,
    CONCAT(N'Earned ', pt.Net, N' LinkDrops from private chat unlock by User #', pt.BuyerId),
    pt.TxDate,
    0, 0,
    pt.BuyerId,
    NULL,
    8500, 8500,
    5000, 5000 + pt.Net
FROM @SeededPrivateTx pt;

PRINT 'Recorded LinkDrop transaction ledger entries.';

-- ==============================================================================
-- 6. UPDATE CREATOR WALLET BALANCE ACCORDINGLY
-- ==============================================================================
-- Total Net Earned = 2,070 (from 18 group joins) + 630 (from 7 private unlocks) = 2,700 LinkDrops
DECLARE @totalNetEarned BIGINT = (SELECT SUM(Net) FROM @SeededGroupTx) + (SELECT SUM(Net) FROM @SeededPrivateTx);

UPDATE [dbo].[TblLinkDropWallet]
SET EarnedBalance = @totalNetEarned,
    Balance = PurchasedBalance + @totalNetEarned,
    UpdatedAt = @now
WHERE UserId = @creatorId;

PRINT 'Updated Creator Wallet Earned Balance to: ' + CAST(@totalNetEarned AS VARCHAR(20));

COMMIT TRANSACTION;

PRINT '=== CREATOR EARNINGS SEEDING COMPLETED SUCCESSFULLY ===';
