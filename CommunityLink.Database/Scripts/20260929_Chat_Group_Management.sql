-- ===================================================================
-- CommunityLink Database Script
-- Table: dbo.TblChatGroupBan
-- Description: Backs Chat Group Creator/Admin management - per-group bans.
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
--
-- Notes
--   * Scoped to a single group: a user banned from one group may still
--     take part in every other group.
--   * A ban also removes the membership, so this row is the durable
--     record of why that membership is gone.
--   * Unban is a soft delete (IsDeleted = 1) so the ban history is
--     retained for audit. The unique index is therefore filtered on
--     IsDeleted = 0, which lets the same user be banned again later
--     without colliding with the revoked row.
--
--   This is the same DDL the application applies at startup via
--   ChatGroupManagementDatabaseSeeder. Every statement is guarded, so
--   applying it by hand on an already-updated database is a no-op.
-- ===================================================================
USE [CommunityLink];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupBan' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblChatGroupBan (
        ChatGroupBanId     INT IDENTITY(1,1) NOT NULL,
        ChatGroupId        INT NOT NULL,                    -- Group the ban is scoped to
        UserId             INT NOT NULL,                    -- Banned user
        BannedByUserId     INT NOT NULL,                    -- Owner/Admin who imposed the ban
        IsDeleted          BIT NOT NULL CONSTRAINT DF_TblChatGroupBan_IsDeleted DEFAULT (0),
        BannedAt           DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupBan_BannedAt DEFAULT (getutcdate()),
        CreatedAt          DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupBan_CreatedAt DEFAULT (getutcdate()),
        CreatedBy          INT NOT NULL,
        RevokedAt          DATETIME2 NULL,                  -- When the ban was lifted
        RevokedByUserId    INT NULL,                        -- Who lifted it
        UpdatedAt          DATETIME2 NULL,
        UpdatedBy          INT NULL,
        RowVersion         ROWVERSION NOT NULL,

        CONSTRAINT PK_TblChatGroupBan PRIMARY KEY CLUSTERED (ChatGroupBanId ASC),
        CONSTRAINT FK_TblChatGroupBan_ChatGroup FOREIGN KEY (ChatGroupId)
            REFERENCES dbo.TblChatGroup (ChatGroupId),
        CONSTRAINT FK_TblChatGroupBan_User FOREIGN KEY (UserId)
            REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblChatGroupBan_BannedByUser FOREIGN KEY (BannedByUserId)
            REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblChatGroupBan_RevokedByUser FOREIGN KEY (RevokedByUserId)
            REFERENCES dbo.TblUser (UserId)
    );

    -- One ACTIVE ban per (group, user). The filter is what permits a re-ban
    -- after an unban: the revoked row keeps its history while the new row
    -- claims the pair.
    --
    -- This index also serves the banned-user probe on the free-join,
    -- paid-join and owner direct-add hot paths, so a second non-unique
    -- index on the same columns would be redundant.
    CREATE UNIQUE NONCLUSTERED INDEX UQ_TblChatGroupBan_Group_User
        ON dbo.TblChatGroupBan (ChatGroupId ASC, UserId ASC)
        WHERE IsDeleted = 0;

    PRINT 'SUCCESS: Table dbo.TblChatGroupBan created successfully with indexes and foreign keys.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblChatGroupBan already exists. No actions performed.';
END
GO
