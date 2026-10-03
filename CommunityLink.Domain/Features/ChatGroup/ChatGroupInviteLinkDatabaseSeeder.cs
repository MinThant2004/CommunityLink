using CommunityLink.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.ChatGroup;

/// <summary>
/// Applies the DDL for shareable invite links. Database-first, same pattern as
/// <see cref="ChatGroupInviteDatabaseSeeder"/>.
/// </summary>
public static class ChatGroupInviteLinkDatabaseSeeder
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
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;

-- ------------------------------------------------------------------
-- Chat Group shareable invite links.
--
-- Unlike TblChatGroupInvite (user-targeted invitations), these are
-- anonymous, URL-sharable tokens. Any authenticated user who holds the
-- token can join the group, subject to ban checks and optional
-- usage/expiry limits.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupInviteLink' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[TblChatGroupInviteLink] (
        [ChatGroupInviteLinkId] INT IDENTITY(1,1) NOT NULL,
        [ChatGroupId] INT NOT NULL,
        [Token] VARCHAR(32) NOT NULL,
        [CreatedByUserId] INT NOT NULL,
        [Name] NVARCHAR(100) NULL,
        [IsPrimary] BIT NOT NULL CONSTRAINT [DF_TblChatGroupInviteLink_IsPrimary] DEFAULT ((0)),
        [ExpiresAt] DATETIME2 NULL,
        [MaxUses] INT NULL,
        [UseCount] INT NOT NULL CONSTRAINT [DF_TblChatGroupInviteLink_UseCount] DEFAULT ((0)),
        [IsRevoked] BIT NOT NULL CONSTRAINT [DF_TblChatGroupInviteLink_IsRevoked] DEFAULT ((0)),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupInviteLink_CreatedAt] DEFAULT (getutcdate()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_TblChatGroupInviteLink_IsDeleted] DEFAULT ((0)),
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_TblChatGroupInviteLink] PRIMARY KEY CLUSTERED ([ChatGroupInviteLinkId] ASC),
        CONSTRAINT [FK_TblChatGroupInviteLink_TblChatGroup] FOREIGN KEY ([ChatGroupId]) REFERENCES [dbo].[TblChatGroup] ([ChatGroupId]),
        CONSTRAINT [FK_TblChatGroupInviteLink_TblUser_CreatedBy] FOREIGN KEY ([CreatedByUserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );
    PRINT 'INFO: TblChatGroupInviteLink created.';
END
ELSE
BEGIN
    PRINT 'INFO: TblChatGroupInviteLink already exists - table left alone.';
END;

-- Unique token index.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_TblChatGroupInviteLink_Token' AND object_id = OBJECT_ID(N'[dbo].[TblChatGroupInviteLink]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatGroupInviteLink_Token]
        ON [dbo].[TblChatGroupInviteLink] ([Token]);
    PRINT 'INFO: UQ_TblChatGroupInviteLink_Token created.';
END
ELSE
BEGIN
    PRINT 'INFO: UQ_TblChatGroupInviteLink_Token already exists.';
END;

-- At most one primary link per active group.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_TblChatGroupInviteLink_Primary' AND object_id = OBJECT_ID(N'[dbo].[TblChatGroupInviteLink]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatGroupInviteLink_Primary]
        ON [dbo].[TblChatGroupInviteLink] ([ChatGroupId])
        WHERE [IsPrimary] = 1 AND [IsDeleted] = 0 AND [IsRevoked] = 0;
    PRINT 'INFO: UQ_TblChatGroupInviteLink_Primary created.';
END
ELSE
BEGIN
    PRINT 'INFO: UQ_TblChatGroupInviteLink_Primary already exists.';
END;

PRINT 'SUCCESS: Chat Group invite link schema is in place (TblChatGroupInviteLink).';
";
}
