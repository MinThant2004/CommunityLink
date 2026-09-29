using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;

namespace CommunityLink.Domain.Features.Chat;

/// <summary>
/// Applies the DDL for the per-message actions feature: reply links, per-user hide state
/// ("delete for myself") and emoji reactions, for both the 1:1 and group stacks.
/// <para>
/// The project is database-first (scaffolded entities, no EF migrations), so this runs the
/// same idempotent script a DBA can apply by hand from
/// <c>CommunityLink.Database/Scripts/20260928_Chat_Message_Actions.sql</c>. Every statement
/// is guarded by an existence check, so it is safe on an already-updated database.
/// </para>
/// </summary>
public static class ChatMessageFeaturesDatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Database.IsRelational())
        {
            return;
        }

        await db.Database.ExecuteSqlRawAsync(CreateScript);
    }

    private const string CreateScript = @"
-- ------------------------------------------------------------------
-- Alter AvatarUrl and BannerUrl to NVARCHAR(MAX) on TblChatGroup so base64 data URIs fit
-- ------------------------------------------------------------------
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroup') AND name = 'AvatarUrl' AND max_length <> -1)
BEGIN
    ALTER TABLE [dbo].[TblChatGroup] ALTER COLUMN [AvatarUrl] NVARCHAR(MAX) NULL;
END;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroup') AND name = 'BannerUrl' AND max_length <> -1)
BEGIN
    ALTER TABLE [dbo].[TblChatGroup] ALTER COLUMN [BannerUrl] NVARCHAR(MAX) NULL;
END;

-- ------------------------------------------------------------------
-- Reply links: each message may point at the message it answers.
-- The target is a plain FK with no navigation property, so the
-- services resolve the quoted text in bulk instead of per row.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatMessage') AND name = 'ReplyToMessageId')
    ALTER TABLE [dbo].[TblChatMessage] ADD [ReplyToMessageId] INT NULL;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TblChatMessage_ReplyTo')
    ALTER TABLE [dbo].[TblChatMessage] ADD CONSTRAINT [FK_TblChatMessage_ReplyTo]
        FOREIGN KEY ([ReplyToMessageId]) REFERENCES [dbo].[TblChatMessage] ([ChatMessageId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatMessage_ReplyToMessageId' AND object_id = OBJECT_ID('dbo.TblChatMessage'))
    CREATE NONCLUSTERED INDEX [IX_TblChatMessage_ReplyToMessageId] ON [dbo].[TblChatMessage] ([ReplyToMessageId]);

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'ReplyToChatGroupMessageId')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [ReplyToChatGroupMessageId] INT NULL;

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TblChatGroupMessage_ReplyTo')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD CONSTRAINT [FK_TblChatGroupMessage_ReplyTo]
        FOREIGN KEY ([ReplyToChatGroupMessageId]) REFERENCES [dbo].[TblChatGroupMessage] ([ChatGroupMessageId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupMessage_ReplyToChatGroupMessageId' AND object_id = OBJECT_ID('dbo.TblChatGroupMessage'))
    CREATE NONCLUSTERED INDEX [IX_TblChatGroupMessage_ReplyToChatGroupMessageId] ON [dbo].[TblChatGroupMessage] ([ReplyToChatGroupMessageId]);

-- ------------------------------------------------------------------
-- Per-user hide state, so ""Delete for myself"" survives a reload
-- and a second device without hiding the message for the sender.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatMessageUserState')
BEGIN
    CREATE TABLE [dbo].[TblChatMessageUserState] (
        [ChatMessageUserStateId] INT IDENTITY(1,1) NOT NULL,
        [ChatMessageId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [IsHidden] BIT NOT NULL CONSTRAINT [DF_TblChatMessageUserState_IsHidden] DEFAULT ((0)),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatMessageUserState_CreatedAt] DEFAULT (getutcdate()),
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] VARBINARY(8) NOT NULL CONSTRAINT [DF_TblChatMessageUserState_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())),
        CONSTRAINT [PK_TblChatMessageUserState] PRIMARY KEY CLUSTERED ([ChatMessageUserStateId] ASC),
        CONSTRAINT [FK_TblChatMessageUserState_TblChatMessage] FOREIGN KEY ([ChatMessageId]) REFERENCES [dbo].[TblChatMessage] ([ChatMessageId]),
        CONSTRAINT [FK_TblChatMessageUserState_TblUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatMessageUserState_Message_User]
        ON [dbo].[TblChatMessageUserState] ([ChatMessageId], [UserId]);
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupMessageUserState')
BEGIN
    CREATE TABLE [dbo].[TblChatGroupMessageUserState] (
        [ChatGroupMessageUserStateId] INT IDENTITY(1,1) NOT NULL,
        [ChatGroupMessageId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [IsHidden] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMessageUserState_IsHidden] DEFAULT ((0)),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupMessageUserState_CreatedAt] DEFAULT (getutcdate()),
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] VARBINARY(8) NOT NULL CONSTRAINT [DF_TblChatGroupMessageUserState_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())),
        CONSTRAINT [PK_TblChatGroupMessageUserState] PRIMARY KEY CLUSTERED ([ChatGroupMessageUserStateId] ASC),
        CONSTRAINT [FK_TblChatGroupMessageUserState_TblChatGroupMessage] FOREIGN KEY ([ChatGroupMessageId]) REFERENCES [dbo].[TblChatGroupMessage] ([ChatGroupMessageId]),
        CONSTRAINT [FK_TblChatGroupMessageUserState_TblUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatGroupMessageUserState_Message_User]
        ON [dbo].[TblChatGroupMessageUserState] ([ChatGroupMessageId], [UserId]);
END;

-- ------------------------------------------------------------------
-- Reactions. The unique (message, user) index enforces the Telegram
-- rule of one reaction per person per message: picking a different
-- emoji updates the existing row instead of adding a second one.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatMessageReaction')
BEGIN
    CREATE TABLE [dbo].[TblChatMessageReaction] (
        [ChatMessageReactionId] INT IDENTITY(1,1) NOT NULL,
        [ChatMessageId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [Emoji] NVARCHAR(16) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatMessageReaction_CreatedAt] DEFAULT (getutcdate()),
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] VARBINARY(8) NOT NULL CONSTRAINT [DF_TblChatMessageReaction_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())),
        CONSTRAINT [PK_TblChatMessageReaction] PRIMARY KEY CLUSTERED ([ChatMessageReactionId] ASC),
        CONSTRAINT [FK_TblChatMessageReaction_TblChatMessage] FOREIGN KEY ([ChatMessageId]) REFERENCES [dbo].[TblChatMessage] ([ChatMessageId]),
        CONSTRAINT [FK_TblChatMessageReaction_TblUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatMessageReaction_Message_User]
        ON [dbo].[TblChatMessageReaction] ([ChatMessageId], [UserId]);
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupMessageReaction')
BEGIN
    CREATE TABLE [dbo].[TblChatGroupMessageReaction] (
        [ChatGroupMessageReactionId] INT IDENTITY(1,1) NOT NULL,
        [ChatGroupMessageId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [Emoji] NVARCHAR(16) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupMessageReaction_CreatedAt] DEFAULT (getutcdate()),
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] VARBINARY(8) NOT NULL CONSTRAINT [DF_TblChatGroupMessageReaction_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())),
        CONSTRAINT [PK_TblChatGroupMessageReaction] PRIMARY KEY CLUSTERED ([ChatGroupMessageReactionId] ASC),
        CONSTRAINT [FK_TblChatGroupMessageReaction_TblChatGroupMessage] FOREIGN KEY ([ChatGroupMessageId]) REFERENCES [dbo].[TblChatGroupMessage] ([ChatGroupMessageId]),
        CONSTRAINT [FK_TblChatGroupMessageReaction_TblUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatGroupMessageReaction_Message_User]
        ON [dbo].[TblChatGroupMessageReaction] ([ChatGroupMessageId], [UserId]);
END;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatMessageReaction') AND name = 'RowVersion')
   AND NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.TblChatMessageReaction') AND name = 'DF_TblChatMessageReaction_RowVersion')
BEGIN
    ALTER TABLE [dbo].[TblChatMessageReaction] ADD CONSTRAINT [DF_TblChatMessageReaction_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())) FOR [RowVersion];
END;

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessageReaction') AND name = 'RowVersion')
   AND NOT EXISTS (SELECT * FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID('dbo.TblChatGroupMessageReaction') AND name = 'DF_TblChatGroupMessageReaction_RowVersion')
BEGIN
    ALTER TABLE [dbo].[TblChatGroupMessageReaction] ADD CONSTRAINT [DF_TblChatGroupMessageReaction_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())) FOR [RowVersion];
END;

PRINT 'SUCCESS: Chat message actions schema is in place (reply links, per-user state, reactions).';
";
}
