-- One-off demo seed: paid group joins + private chat unlocks for creator
-- juggerkaung.dev@gmail.com (UserId = 3, Kaung Myat Han)
-- Idempotent guard: aborts if earnings transactions already exist for this creator.
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @now datetime = GETUTCDATE();

IF EXISTS (SELECT 1 FROM TblChatGroupPaymentTransaction WHERE CreatorUserId = 3)
   OR EXISTS (SELECT 1 FROM TblPrivateChatPaymentTransaction WHERE CreatorUserId = 3)
   OR EXISTS (SELECT 1 FROM TblChatGroup WHERE CreatorId = 3 AND Name IN ('Dev Career Mentoring', 'Myanmar .NET Community', 'Startup Founder Circle'))
BEGIN
    ROLLBACK TRANSACTION;
    RAISERROR('Seed aborted: earnings data already exists for user 3.', 16, 1);
END

-- ============================================================
-- 1. Paid chat groups owned by creator
-- ============================================================
INSERT INTO TblChatGroup (Name, Description, CreatorId, ChatType, AccessMode, JoinFeeLinkDrops, CommissionPercentageSnapshot, IsActive, CreatedAt, CreatedBy)
VALUES ('Dev Career Mentoring', N'One-on-one career guidance, code review and interview prep for Myanmar developers.', 3, 'PAID', 'PUBLIC', 50, 10.00, 1, DATEADD(DAY, -45, @now), 3);
DECLARE @g1 int = CAST(SCOPE_IDENTITY() AS int);

INSERT INTO TblChatGroup (Name, Description, CreatorId, ChatType, AccessMode, JoinFeeLinkDrops, CommissionPercentageSnapshot, IsActive, CreatedAt, CreatedBy)
VALUES ('Myanmar .NET Community', N'Weekly .NET / C# discussions, live Q&A and project showcases.', 3, 'PAID', 'PUBLIC', 100, 10.00, 1, DATEADD(DAY, -44, @now), 3);
DECLARE @g2 int = CAST(SCOPE_IDENTITY() AS int);

INSERT INTO TblChatGroup (Name, Description, CreatorId, ChatType, AccessMode, JoinFeeLinkDrops, CommissionPercentageSnapshot, IsActive, CreatedAt, CreatedBy)
VALUES ('Startup Founder Circle', N'Private circle for founders: fundraising, product and growth strategy.', 3, 'PAID', 'PRIVATE', 200, 10.00, 1, DATEADD(DAY, -43, @now), 3);
DECLARE @g3 int = CAST(SCOPE_IDENTITY() AS int);

-- Creator is OWNER of each group
INSERT INTO TblChatGroupMember (ChatGroupId, UserId, Role, JoinedAt, CreatedAt, CreatedBy)
VALUES (@g1, 3, 'OWNER', DATEADD(DAY, -45, @now), DATEADD(DAY, -45, @now), 3),
       (@g2, 3, 'OWNER', DATEADD(DAY, -44, @now), DATEADD(DAY, -44, @now), 3),
       (@g3, 3, 'OWNER', DATEADD(DAY, -43, @now), DATEADD(DAY, -43, @now), 3);

-- ============================================================
-- 2. Buyer wallets: top up so every seeded payment is affordable
-- ============================================================
DECLARE @buyers TABLE (UserId int PRIMARY KEY);
INSERT INTO @buyers (UserId)
VALUES (7),(8),(9),(10),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23),(24),(26),(28);

UPDATE w
SET PurchasedBalance = 500, EarnedBalance = 150, Balance = 650, UpdatedAt = @now
FROM TblLinkDropWallet w
JOIN @buyers b ON b.UserId = w.UserId;

-- ============================================================
-- 3. Paid group joins
-- ============================================================
DECLARE @joins TABLE (BuyerId int, Gid int, Fee bigint, DaysAgo int);
INSERT INTO @joins (BuyerId, Gid, Fee, DaysAgo) VALUES
 (7,  @g1, 50,  40), (8,  @g1, 50,  37), (9,  @g1, 50,  33), (10, @g1, 50,  29),
 (13, @g1, 50,  24), (14, @g1, 50,  18), (15, @g1, 50,  11), (16, @g1, 50,   4),
 (17, @g2, 100, 35), (18, @g2, 100, 30), (19, @g2, 100, 22), (20, @g2, 100, 15), (21, @g2, 100, 7),
 (22, @g3, 200, 28), (23, @g3, 200, 13), (24, @g3, 200,  2);

DECLARE @gpt TABLE (BuyerId int, Gid int, Fee bigint, Commission bigint, Net bigint, TxId bigint, TxDate datetime);
INSERT INTO TblChatGroupPaymentTransaction
    (ChatGroupId, UserId, CreatorUserId, GrossAmount, CommissionPercentage, CommissionAmount, NetAmount, PurchasedAmountDeducted, EarnedAmountDeducted, Status, CreatedAt)
OUTPUT inserted.UserId, inserted.ChatGroupId, inserted.GrossAmount, inserted.CommissionAmount, inserted.NetAmount, inserted.PaymentTransactionId, inserted.CreatedAt
INTO @gpt
SELECT j.Gid, j.BuyerId, 3, j.Fee, 10.00, j.Fee * 10 / 100, j.Fee * 90 / 100, j.Fee, 0, 'COMPLETED', DATEADD(DAY, -j.DaysAgo, @now)
FROM @joins j;

-- Buyers become members of the groups they paid for
INSERT INTO TblChatGroupMember (ChatGroupId, UserId, Role, JoinedAt, CreatedAt, CreatedBy)
SELECT DISTINCT p.Gid, p.BuyerId, 'MEMBER', p.TxDate, p.TxDate, 3
FROM @gpt p
WHERE NOT EXISTS (SELECT 1 FROM TblChatGroupMember m WHERE m.ChatGroupId = p.Gid AND m.UserId = p.BuyerId AND m.IsDeleted = 0);

-- ============================================================
-- 4. Private chat unlocks (fee aligned with creator chat setting)
-- ============================================================
UPDATE TblCreatorChatSetting
SET PrivateChatFeeLinkDrops = 100, UpdatedAt = @now
WHERE CreatorUserId = 3;

DECLARE @unlocks TABLE (BuyerId int, DaysAgo int);
INSERT INTO @unlocks (BuyerId, DaysAgo) VALUES (8, 31), (14, 26), (19, 19), (24, 12), (26, 8), (28, 3);

DECLARE @conv TABLE (BuyerId int PRIMARY KEY, ConversationId int NULL);

-- Reuse an existing conversation with the creator when present, else create one
INSERT INTO @conv (BuyerId, ConversationId)
SELECT u.BuyerId, c.ConversationId
FROM @unlocks u
OUTER APPLY (
    SELECT TOP 1 v.ConversationId
    FROM TblConversation v
    WHERE v.IsDeleted = 0
      AND ((v.UserOneId = CASE WHEN u.BuyerId < 3 THEN u.BuyerId ELSE 3 END
        AND v.UserTwoId = CASE WHEN u.BuyerId < 3 THEN 3 ELSE u.BuyerId END))
) c;

DECLARE @missing TABLE (BuyerId int PRIMARY KEY);
INSERT INTO @missing (BuyerId) SELECT BuyerId FROM @conv WHERE ConversationId IS NULL;

WHILE EXISTS (SELECT 1 FROM @missing)
BEGIN
    DECLARE @bid int, @one int, @two int, @cid int;
    SELECT TOP 1 @bid = BuyerId FROM @missing;
    SET @one = CASE WHEN @bid < 3 THEN @bid ELSE 3 END;
    SET @two = CASE WHEN @bid < 3 THEN 3 ELSE @bid END;
    INSERT INTO TblConversation (UserOneId, UserTwoId, CreatedAt, CreatedBy)
    VALUES (@one, @two, DATEADD(DAY, -40, @now), @bid);
    SET @cid = CAST(SCOPE_IDENTITY() AS int);
    UPDATE @conv SET ConversationId = @cid WHERE BuyerId = @bid;
    DELETE FROM @missing WHERE BuyerId = @bid;
END

DECLARE @pvt TABLE (BuyerId int, ConversationId int, Gross bigint, Comm bigint, CreatorAmt bigint, TxId bigint, TxDate datetime);
INSERT INTO TblPrivateChatPaymentTransaction
    (ConversationId, BuyerUserId, CreatorUserId, GrossAmountLinkDrops, CommissionAmount, CreatorAmount, Status, CreatedAt)
OUTPUT inserted.BuyerUserId, inserted.ConversationId, inserted.GrossAmountLinkDrops, inserted.CommissionAmount, inserted.CreatorAmount, inserted.PrivateChatPaymentTransactionId, inserted.CreatedAt
INTO @pvt
SELECT c.ConversationId, c.BuyerId, 3, 100, 10, 90, 'COMPLETED', DATEADD(DAY, -u.DaysAgo, @now)
FROM @unlocks u
JOIN @conv c ON c.BuyerId = u.BuyerId;

-- ============================================================
-- 5. Ledger: buyer spend rows (purchased balance chain from 650)
-- ============================================================
DECLARE @allpay TABLE (UserId int, Fee bigint, RefType varchar(50), RefId int, Notes nvarchar(500), TxDate datetime2, Gid int NULL, IsPrivate bit);
INSERT INTO @allpay (UserId, Fee, RefType, RefId, Notes, TxDate, Gid, IsPrivate)
SELECT p.BuyerId, p.Fee, 'TblChatGroupPaymentTransaction', CAST(p.TxId AS int),
       CONCAT(N'Paid ', p.Fee, N' Link Drops to join Chat Group ', (SELECT g.Name FROM TblChatGroup g WHERE g.ChatGroupId = p.Gid)),
       p.TxDate, p.Gid, 0
FROM @gpt p;

INSERT INTO @allpay (UserId, Fee, RefType, RefId, Notes, TxDate, Gid, IsPrivate)
SELECT p.BuyerId, p.Gross, 'CONVERSATION', CAST(p.ConversationId AS int),
       N'Unlocked private chat with Creator #3',
       p.TxDate, NULL, 1
FROM @pvt p;

;WITH ordered AS (
    SELECT a.*,
           ROW_NUMBER() OVER (PARTITION BY a.UserId ORDER BY a.TxDate, a.IsPrivate) AS rn,
           SUM(a.Fee) OVER (PARTITION BY a.UserId ORDER BY a.TxDate, a.IsPrivate ROWS UNBOUNDED PRECEDING) AS cum
    FROM @allpay a
)
INSERT INTO TblLinkDropTransaction
    (WalletId, UserId, TransactionType, Amount, BalanceBefore, BalanceAfter, ReferenceType, ReferenceId, Notes, CreatedAt,
     PurchasedAmountDeducted, EarnedAmountDeducted, RelatedUserId, RelatedGroupId,
     PurchasedBalanceBefore, PurchasedBalanceAfter, EarnedBalanceBefore, EarnedBalanceAfter)
SELECT w.WalletId, o.UserId,
       CASE WHEN o.IsPrivate = 1 THEN 'PRIVATE_CHAT_UNLOCK' ELSE 'CHAT_GROUP_JOIN' END,
       o.Fee, 650 - o.cum, 650 - (o.cum - o.Fee),
       o.RefType, o.RefId, o.Notes, o.TxDate,
       o.Fee, 0, 3, o.Gid,
       500 - o.cum, 500 - (o.cum - o.Fee), 150, 150
FROM ordered o
JOIN TblLinkDropWallet w ON w.UserId = o.UserId;

-- ============================================================
-- 6. Ledger: creator earning rows (earned balance chain from 1000)
-- ============================================================
DECLARE @allern TABLE (Net bigint, RefType varchar(50), RefId int, Notes nvarchar(500), TxDate datetime2, BuyerId int, Gid int NULL, IsPrivate bit);
INSERT INTO @allern (Net, RefType, RefId, Notes, TxDate, BuyerId, Gid, IsPrivate)
SELECT p.Net, 'TblChatGroupPaymentTransaction', CAST(p.TxId AS int),
       CONCAT(N'Earned ', p.Net, N' LinkDrops from paid join by User #', p.BuyerId),
       p.TxDate, p.BuyerId, p.Gid, 0
FROM @gpt p;

INSERT INTO @allern (Net, RefType, RefId, Notes, TxDate, BuyerId, Gid, IsPrivate)
SELECT p.CreatorAmt, 'CONVERSATION', CAST(p.ConversationId AS int),
       CONCAT(N'Earned ', p.CreatorAmt, N' LinkDrops from private chat unlock by User #', p.BuyerId),
       p.TxDate, p.BuyerId, NULL, 1
FROM @pvt p;

;WITH ordered AS (
    SELECT e.*,
           SUM(e.Net) OVER (ORDER BY e.TxDate, e.IsPrivate ROWS UNBOUNDED PRECEDING) AS cum
    FROM @allern e
)
INSERT INTO TblLinkDropTransaction
    (WalletId, UserId, TransactionType, Amount, BalanceBefore, BalanceAfter, ReferenceType, ReferenceId, Notes, CreatedAt,
     PurchasedAmountDeducted, EarnedAmountDeducted, RelatedUserId, RelatedGroupId,
     PurchasedBalanceBefore, PurchasedBalanceAfter, EarnedBalanceBefore, EarnedBalanceAfter)
SELECT w.WalletId, 3,
       CASE WHEN o.IsPrivate = 1 THEN 'PRIVATE_CHAT_EARNING' ELSE 'CHAT_GROUP_EARNING' END,
       o.Net, 2500 + (o.cum - o.Net), 2500 + o.cum,
       o.RefType, o.RefId, o.Notes, o.TxDate,
       0, 0, o.BuyerId, o.Gid,
       1500, 1500, 1000 + (o.cum - o.Net), 1000 + o.cum
FROM ordered o
JOIN TblLinkDropWallet w ON w.UserId = 3;

-- ============================================================
-- 7. Final wallet balances
-- ============================================================
UPDATE w
SET PurchasedBalance = 500 - ISNULL(s.Spend, 0),
    Balance = (500 - ISNULL(s.Spend, 0)) + 150,
    UpdatedAt = @now
FROM TblLinkDropWallet w
JOIN @buyers b ON b.UserId = w.UserId
OUTER APPLY (SELECT SUM(p.Fee) AS Spend FROM @allpay p WHERE p.UserId = w.UserId) s;

DECLARE @creatorNet bigint = (SELECT ISNULL(SUM(Net), 0) FROM @gpt) + (SELECT ISNULL(SUM(CreatorAmt), 0) FROM @pvt);

UPDATE TblLinkDropWallet
SET EarnedBalance = 1000 + @creatorNet,
    Balance = 1500 + 1000 + @creatorNet,
    UpdatedAt = @now
WHERE UserId = 3;

COMMIT TRANSACTION;

-- ============================================================
-- Verification
-- ============================================================
SELECT 'GroupTx' AS What, COUNT(*) AS Cnt FROM TblChatGroupPaymentTransaction WHERE CreatorUserId = 3
UNION ALL SELECT 'PrivateTx', COUNT(*) FROM TblPrivateChatPaymentTransaction WHERE CreatorUserId = 3
UNION ALL SELECT 'CreatorEarnedBalance', EarnedBalance FROM TblLinkDropWallet WHERE UserId = 3
UNION ALL SELECT 'CreatorBalance', Balance FROM TblLinkDropWallet WHERE UserId = 3;

SELECT g.Name, g.JoinFeeLinkDrops, COUNT(t.PaymentTransactionId) AS Joins, SUM(t.NetAmount) AS NetEarned
FROM TblChatGroup g
LEFT JOIN TblChatGroupPaymentTransaction t ON t.ChatGroupId = g.ChatGroupId
WHERE g.CreatorId = 3
GROUP BY g.Name, g.JoinFeeLinkDrops
ORDER BY g.Name;
