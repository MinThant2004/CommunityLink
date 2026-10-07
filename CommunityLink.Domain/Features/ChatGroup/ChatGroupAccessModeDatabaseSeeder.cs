using CommunityLink.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.ChatGroup;

/// <summary>
/// Applies DDL for AccessMode column and TblChatGroupJoinRequest table.
/// Database-first runtime schema updater matching existing project seeders.
/// </summary>
public static class ChatGroupAccessModeDatabaseSeeder
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

-- 1. Add AccessMode column to TblChatGroup if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TblChatGroup]') AND name = 'AccessMode')
BEGIN
    ALTER TABLE [dbo].[TblChatGroup]
        ADD [AccessMode] VARCHAR(20) NOT NULL CONSTRAINT [DF_TblChatGroup_AccessMode] DEFAULT ('PUBLIC');
    PRINT 'INFO: Added AccessMode column to TblChatGroup.';
END
ELSE
BEGIN
    PRINT 'INFO: AccessMode column already exists on TblChatGroup.';
END;

-- 2. Create TblChatGroupJoinRequest table if missing
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupJoinRequest' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[TblChatGroupJoinRequest] (
        [ChatGroupJoinRequestId] INT IDENTITY(1,1) NOT NULL,
        [ChatGroupId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [Status] VARCHAR(30) NOT NULL,
        [RequestNote] NVARCHAR(500) NULL,
        [ReviewedBy] INT NULL,
        [ReviewedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupJoinRequest_CreatedAt] DEFAULT (getutcdate()),
        [CreatedBy] INT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_TblChatGroupJoinRequest_IsDeleted] DEFAULT ((0)),
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_TblChatGroupJoinRequest] PRIMARY KEY CLUSTERED ([ChatGroupJoinRequestId] ASC),
        CONSTRAINT [FK_TblChatGroupJoinRequest_TblChatGroup] FOREIGN KEY ([ChatGroupId]) REFERENCES [dbo].[TblChatGroup] ([ChatGroupId]),
        CONSTRAINT [FK_TblChatGroupJoinRequest_TblUser_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblChatGroupJoinRequest_TblUser_ReviewedBy] FOREIGN KEY ([ReviewedBy]) REFERENCES [dbo].[TblUser] ([UserId])
    );
    PRINT 'INFO: TblChatGroupJoinRequest created.';
END
ELSE
BEGIN
    PRINT 'INFO: TblChatGroupJoinRequest already exists.';
END;

-- 3. Non-clustered index on ChatGroupId + Status
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupJoinRequest_Group_Status' AND object_id = OBJECT_ID(N'[dbo].[TblChatGroupJoinRequest]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TblChatGroupJoinRequest_Group_Status]
        ON [dbo].[TblChatGroupJoinRequest] ([ChatGroupId], [Status]);
    PRINT 'INFO: IX_TblChatGroupJoinRequest_Group_Status created.';
END;

PRINT 'SUCCESS: Chat Group AccessMode and Join Request schema is in place.';
";
}
