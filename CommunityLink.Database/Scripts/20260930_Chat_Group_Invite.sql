-- ===================================================================
-- CommunityLink Database Script
-- Table: dbo.TblChatGroupInvite
-- Description: Backs owner invites into a PAID Chat Group. The invitee
--              must pay the join fee before a membership row is written.
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
--
-- Notes
--   * An invite is an entitlement offer, NOT access. While an invite is
--     PENDING the invitee has no TblChatGroupMember row, so the history,
--     preview, notification fan-out and realtime room gates all treat
--     them as a non-member. Only the paid-join path creates membership.
--   * Resolved invites are RETAINED (Status moves to ACCEPTED / REVOKED /
--     DECLINED) so the creator can see whether an invite converted to
--     revenue. That is why the unique index is filtered on Status rather
--     than on IsDeleted: a user who declined or was revoked can be
--     invited again, and only the open PENDING row blocks that.
--   * FeeAtInviteLinkDrops snapshots the quoted price. A later fee change
--     applies to new invites only, so invitees are not repriced after
--     the fact.
--
--   * The unique index below is FILTERED, and SQL Server requires
--     SET QUOTED_IDENTIFIER ON for any INSERT/UPDATE against a table
--     that has one. The application is fine: Microsoft.Data.SqlClient
--     issues that SET on every connection it opens. Ad-hoc tooling does
--     not - sqlcmd defaults it OFF and will fail with "SET options have
--     incorrect settings". SSMS is ON by default. If you script inserts
--     against this table, start the session with:
--         SET QUOTED_IDENTIFIER ON;
--
--   This is the same DDL the application applies at startup via
--   ChatGroupInviteDatabaseSeeder. Every object is guarded on its own, so
--   applying it by hand on an already-updated database is a no-op, and a
--   partially-applied run heals on the next application.
-- ===================================================================
USE [CommunityLink];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupInvite' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblChatGroupInvite (
        ChatGroupInviteId       INT IDENTITY(1,1) NOT NULL,
        ChatGroupId             INT NOT NULL,                -- Group the invite is scoped to
        UserId                  INT NOT NULL,                -- Invitee; no membership row while PENDING
        InvitedByUserId         INT NOT NULL,                -- Owner who issued the invite
        Status                  VARCHAR(20) NOT NULL,        -- PENDING | ACCEPTED | REVOKED | DECLINED
        FeeAtInviteLinkDrops    BIGINT NOT NULL CONSTRAINT DF_TblChatGroupInvite_FeeAtInviteLinkDrops DEFAULT (0),
        InvitedAt               DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupInvite_InvitedAt DEFAULT (getutcdate()),
        RespondedAt             DATETIME2 NULL,              -- Set when the invite reaches a terminal state
        IsDeleted               BIT NOT NULL CONSTRAINT DF_TblChatGroupInvite_IsDeleted DEFAULT (0),
        CreatedAt               DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupInvite_CreatedAt DEFAULT (getutcdate()),
        CreatedBy               INT NOT NULL,
        UpdatedAt               DATETIME2 NULL,
        UpdatedBy               INT NULL,
        DeletedAt               DATETIME2 NULL,
        DeletedBy               INT NULL,
        RowVersion              ROWVERSION NOT NULL,
        CONSTRAINT PK_TblChatGroupInvite PRIMARY KEY CLUSTERED (ChatGroupInviteId ASC),
        CONSTRAINT CK_TblChatGroupInvite_Status CHECK (Status IN ('PENDING','ACCEPTED','REVOKED','DECLINED')),
        CONSTRAINT FK_TblChatGroupInvite_TblChatGroup FOREIGN KEY (ChatGroupId) REFERENCES dbo.TblChatGroup (ChatGroupId),
        CONSTRAINT FK_TblChatGroupInvite_TblUser_User FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblChatGroupInvite_TblUser_InvitedBy FOREIGN KEY (InvitedByUserId) REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblChatGroupInvite_TblUser_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES dbo.TblUser (UserId)
    );

    PRINT 'INFO: TblChatGroupInvite created.';
END
ELSE
BEGIN
    PRINT 'INFO: TblChatGroupInvite already exists - table left alone.';
END
GO

-- Each index is guarded on its own rather than sharing the table check above.
-- A failure part way through the table block would otherwise leave the table
-- present but an index missing, and the table-level guard would then treat
-- the schema as complete and never retry it. This makes a partial run heal on
-- the next application instead of needing a manual drop.

-- At most one open invite per (group, user). Filtering on Status rather
-- than IsDeleted is what permits re-inviting a user whose previous
-- invite was declined or revoked, while still blocking a duplicate
-- open invite. This index also serves the pending-invite probe on the
-- direct-add and paid-join hot paths.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_TblChatGroupInvite_Group_User' AND object_id = OBJECT_ID(N'dbo.TblChatGroupInvite'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TblChatGroupInvite_Group_User
        ON dbo.TblChatGroupInvite (ChatGroupId, UserId)
        WHERE Status = 'PENDING';
    PRINT 'INFO: UQ_TblChatGroupInvite_Group_User created.';
END
ELSE
BEGIN
    PRINT 'INFO: UQ_TblChatGroupInvite_Group_User already exists.';
END
GO

-- Covers the "my invitations" list, which filters on invitee + status
-- and orders by invite time. InvitedAt must not also appear in INCLUDE:
-- it is already a key column, and SQL Server rejects a duplicate column
-- name in the same index definition.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupInvite_User_Status' AND object_id = OBJECT_ID(N'dbo.TblChatGroupInvite'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TblChatGroupInvite_User_Status
        ON dbo.TblChatGroupInvite (UserId, Status, InvitedAt DESC)
        INCLUDE (ChatGroupId, FeeAtInviteLinkDrops, InvitedByUserId);
    PRINT 'INFO: IX_TblChatGroupInvite_User_Status created.';
END
ELSE
BEGIN
    PRINT 'INFO: IX_TblChatGroupInvite_User_Status already exists.';
END
GO

PRINT 'SUCCESS: Chat Group invite schema is in place (TblChatGroupInvite).';
GO
