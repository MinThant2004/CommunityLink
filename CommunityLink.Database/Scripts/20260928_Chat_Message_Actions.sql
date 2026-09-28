-- ===================================================================
-- CommunityLink Database Script
-- Feature: Per-message actions (reply, delete for myself,
--           delete for everyone, emoji reactions)
-- Applies to: dbo.TblChatMessage (1:1 stack)
--             dbo.TblChatGroupMessage (group stack)
-- Adds:       Reply FK columns + 4 new tables
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
--
-- NOTE: The API also runs this DDL on startup through
--       ChatMessageFeaturesDatabaseSeeder, which uses the same
--       statements without the GO batch separators. Applying this
--       file by hand is optional but keeps the database in step with
--       a reviewed change set.
-- ===================================================================
USE [CommunityLink];
GO

-- ------------------------------------------------------------------
-- 1. Reply links
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatMessage') AND name = 'ReplyToMessageId')
BEGIN
    ALTER TABLE dbo.TblChatMessage ADD ReplyToMessageId INT NULL;
    PRINT 'SUCCESS: Column dbo.TblChatMessage.ReplyToMessageId added.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TblChatMessage_ReplyTo')
BEGIN
    ALTER TABLE dbo.TblChatMessage ADD CONSTRAINT FK_TblChatMessage_ReplyTo
        FOREIGN KEY (ReplyToMessageId) REFERENCES dbo.TblChatMessage (ChatMessageId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatMessage_ReplyToMessageId' AND object_id = OBJECT_ID('dbo.TblChatMessage'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TblChatMessage_ReplyToMessageId
        ON dbo.TblChatMessage (ReplyToMessageId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'ReplyToChatGroupMessageId')
BEGIN
    ALTER TABLE dbo.TblChatGroupMessage ADD ReplyToChatGroupMessageId INT NULL;
    PRINT 'SUCCESS: Column dbo.TblChatGroupMessage.ReplyToChatGroupMessageId added.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TblChatGroupMessage_ReplyTo')
BEGIN
    ALTER TABLE dbo.TblChatGroupMessage ADD CONSTRAINT FK_TblChatGroupMessage_ReplyTo
        FOREIGN KEY (ReplyToChatGroupMessageId) REFERENCES dbo.TblChatGroupMessage (ChatGroupMessageId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupMessage_ReplyToChatGroupMessageId' AND object_id = OBJECT_ID('dbo.TblChatGroupMessage'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TblChatGroupMessage_ReplyToChatGroupMessageId
        ON dbo.TblChatGroupMessage (ReplyToChatGroupMessageId);
END
GO

-- ------------------------------------------------------------------
-- 2. Per-user hide state
--    One row per (message, viewer). IsHidden = 1 means that viewer has
--    chosen "Delete for myself"; the other participant still sees the
--    message because the message row itself is untouched.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatMessageUserState' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblChatMessageUserState (
        ChatMessageUserStateId INT IDENTITY(1,1) NOT NULL,
        ChatMessageId         INT NOT NULL,             -- FK to dbo.TblChatMessage
        UserId                INT NOT NULL,             -- FK to dbo.TblUser
        IsHidden              BIT NOT NULL CONSTRAINT DF_TblChatMessageUserState_IsHidden DEFAULT (0),
        CreatedAt             DATETIME2 NOT NULL CONSTRAINT DF_TblChatMessageUserState_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt             DATETIME2 NULL,
        RowVersion            VARBINARY(8) NOT NULL CONSTRAINT DF_TblChatMessageUserState_RowVersion DEFAULT (CONVERT(VARBINARY(8), NEWID())),

        CONSTRAINT PK_TblChatMessageUserState PRIMARY KEY CLUSTERED (ChatMessageUserStateId ASC),
        CONSTRAINT FK_TblChatMessageUserState_TblChatMessage FOREIGN KEY (ChatMessageId)
            REFERENCES dbo.TblChatMessage (ChatMessageId),
        CONSTRAINT FK_TblChatMessageUserState_TblUser FOREIGN KEY (UserId)
            REFERENCES dbo.TblUser (UserId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UQ_TblChatMessageUserState_Message_User
        ON dbo.TblChatMessageUserState (ChatMessageId, UserId);

    PRINT 'SUCCESS: Table dbo.TblChatMessageUserState created.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblChatMessageUserState already exists. No actions performed.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupMessageUserState' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblChatGroupMessageUserState (
        ChatGroupMessageUserStateId INT IDENTITY(1,1) NOT NULL,
        ChatGroupMessageId         INT NOT NULL,         -- FK to dbo.TblChatGroupMessage
        UserId                     INT NOT NULL,         -- FK to dbo.TblUser
        IsHidden                   BIT NOT NULL CONSTRAINT DF_TblChatGroupMessageUserState_IsHidden DEFAULT (0),
        CreatedAt                  DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupMessageUserState_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt                  DATETIME2 NULL,
        RowVersion                 VARBINARY(8) NOT NULL CONSTRAINT DF_TblChatGroupMessageUserState_RowVersion DEFAULT (CONVERT(VARBINARY(8), NEWID())),

        CONSTRAINT PK_TblChatGroupMessageUserState PRIMARY KEY CLUSTERED (ChatGroupMessageUserStateId ASC),
        CONSTRAINT FK_TblChatGroupMessageUserState_TblChatGroupMessage FOREIGN KEY (ChatGroupMessageId)
            REFERENCES dbo.TblChatGroupMessage (ChatGroupMessageId),
        CONSTRAINT FK_TblChatGroupMessageUserState_TblUser FOREIGN KEY (UserId)
            REFERENCES dbo.TblUser (UserId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UQ_TblChatGroupMessageUserState_Message_User
        ON dbo.TblChatGroupMessageUserState (ChatGroupMessageId, UserId);

    PRINT 'SUCCESS: Table dbo.TblChatGroupMessageUserState created.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblChatGroupMessageUserState already exists. No actions performed.';
END
GO

-- ------------------------------------------------------------------
-- 3. Emoji reactions
--    The unique (message, user) index is what enforces "one reaction
--    per person per message". Choosing a different emoji updates the
--    existing row; choosing the same emoji again removes it.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatMessageReaction' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblChatMessageReaction (
        ChatMessageReactionId INT IDENTITY(1,1) NOT NULL,
        ChatMessageId         INT NOT NULL,             -- FK to dbo.TblChatMessage
        UserId                INT NOT NULL,             -- FK to dbo.TblUser
        Emoji                 NVARCHAR(16) NOT NULL,
        CreatedAt             DATETIME2 NOT NULL CONSTRAINT DF_TblChatMessageReaction_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt             DATETIME2 NULL,
        RowVersion            VARBINARY(8) NOT NULL CONSTRAINT DF_TblChatMessageReaction_RowVersion DEFAULT (CONVERT(VARBINARY(8), NEWID())),

        CONSTRAINT PK_TblChatMessageReaction PRIMARY KEY CLUSTERED (ChatMessageReactionId ASC),
        CONSTRAINT FK_TblChatMessageReaction_TblChatMessage FOREIGN KEY (ChatMessageId)
            REFERENCES dbo.TblChatMessage (ChatMessageId),
        CONSTRAINT FK_TblChatMessageReaction_TblUser FOREIGN KEY (UserId)
            REFERENCES dbo.TblUser (UserId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UQ_TblChatMessageReaction_Message_User
        ON dbo.TblChatMessageReaction (ChatMessageId, UserId);

    PRINT 'SUCCESS: Table dbo.TblChatMessageReaction created.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblChatMessageReaction already exists. No actions performed.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupMessageReaction' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblChatGroupMessageReaction (
        ChatGroupMessageReactionId INT IDENTITY(1,1) NOT NULL,
        ChatGroupMessageId         INT NOT NULL,         -- FK to dbo.TblChatGroupMessage
        UserId                     INT NOT NULL,         -- FK to dbo.TblUser
        Emoji                      NVARCHAR(16) NOT NULL,
        CreatedAt                  DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupMessageReaction_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt                  DATETIME2 NULL,
        RowVersion                 VARBINARY(8) NOT NULL CONSTRAINT DF_TblChatGroupMessageReaction_RowVersion DEFAULT (CONVERT(VARBINARY(8), NEWID())),

        CONSTRAINT PK_TblChatGroupMessageReaction PRIMARY KEY CLUSTERED (ChatGroupMessageReactionId ASC),
        CONSTRAINT FK_TblChatGroupMessageReaction_TblChatGroupMessage FOREIGN KEY (ChatGroupMessageId)
            REFERENCES dbo.TblChatGroupMessage (ChatGroupMessageId),
        CONSTRAINT FK_TblChatGroupMessageReaction_TblUser FOREIGN KEY (UserId)
            REFERENCES dbo.TblUser (UserId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX UQ_TblChatGroupMessageReaction_Message_User
        ON dbo.TblChatGroupMessageReaction (ChatGroupMessageId, UserId);

    PRINT 'SUCCESS: Table dbo.TblChatGroupMessageReaction created.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblChatGroupMessageReaction already exists. No actions performed.';
END
GO
