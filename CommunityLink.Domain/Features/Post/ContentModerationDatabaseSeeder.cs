using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;

namespace CommunityLink.Domain.Features.Post;

public static class ContentModerationDatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Database.IsRelational())
        {
            var sql = @"
-- 1. Ensure columns exist on TblPost
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPost') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[TblPost] ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_TblPost_IsActive] DEFAULT (1);
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPost') AND name = 'IsPrivate')
BEGIN
    ALTER TABLE [dbo].[TblPost] ADD [IsPrivate] BIT NOT NULL CONSTRAINT [DF_TblPost_IsPrivate] DEFAULT (0);
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPost') AND name = 'ModeratedBy')
BEGIN
    ALTER TABLE [dbo].[TblPost] ADD [ModeratedBy] INT NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPost') AND name = 'ModeratedAt')
BEGIN
    ALTER TABLE [dbo].[TblPost] ADD [ModeratedAt] DATETIME2 NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPost') AND name = 'ModerationReason')
BEGIN
    ALTER TABLE [dbo].[TblPost] ADD [ModerationReason] NVARCHAR(500) NULL;
END;

-- 2. Ensure columns exist on TblPoll
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPoll') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[TblPoll] ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_TblPoll_IsActive] DEFAULT (1);
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPoll') AND name = 'IsPrivate')
BEGIN
    ALTER TABLE [dbo].[TblPoll] ADD [IsPrivate] BIT NOT NULL CONSTRAINT [DF_TblPoll_IsPrivate] DEFAULT (0);
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPoll') AND name = 'ModeratedBy')
BEGIN
    ALTER TABLE [dbo].[TblPoll] ADD [ModeratedBy] INT NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPoll') AND name = 'ModeratedAt')
BEGIN
    ALTER TABLE [dbo].[TblPoll] ADD [ModeratedAt] DATETIME2 NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblPoll') AND name = 'ModerationReason')
BEGIN
    ALTER TABLE [dbo].[TblPoll] ADD [ModerationReason] NVARCHAR(500) NULL;
END;

-- 3. Ensure TblContentReport table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblContentReport')
BEGIN
    CREATE TABLE [dbo].[TblContentReport] (
        [ContentReportId] INT IDENTITY(1,1) NOT NULL,
        [ContentType] VARCHAR(20) NOT NULL,
        [ContentId] INT NOT NULL,
        [ReporterUserId] INT NOT NULL,
        [ReasonCategory] NVARCHAR(100) NOT NULL,
        [Details] NVARCHAR(1000) NULL,
        [Status] VARCHAR(20) NOT NULL CONSTRAINT [DF_TblContentReport_Status] DEFAULT ('PENDING'),
        [HandledByAdminId] INT NULL,
        [AdminNote] NVARCHAR(500) NULL,
        [HandledAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblContentReport_CreatedAt] DEFAULT (getutcdate()),
        [RowVersion] VARBINARY(8) NOT NULL CONSTRAINT [DF_TblContentReport_RowVersion] DEFAULT (CONVERT(VARBINARY(8), NEWID())),
        CONSTRAINT [PK_TblContentReport] PRIMARY KEY CLUSTERED ([ContentReportId] ASC),
        CONSTRAINT [FK_TblContentReport_Reporter] FOREIGN KEY ([ReporterUserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblContentReport_Content')
        CREATE INDEX [IX_TblContentReport_Content] ON [dbo].[TblContentReport] ([ContentType], [ContentId], [Status]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblContentReport_Reporter')
        CREATE INDEX [IX_TblContentReport_Reporter] ON [dbo].[TblContentReport] ([ReporterUserId]);
END;

-- 4. Ensure moderation columns exist on TblGroup
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblGroup') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[TblGroup] ADD [IsActive] BIT NOT NULL CONSTRAINT [DF_TblGroup_IsActive] DEFAULT (1);
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblGroup') AND name = 'ModeratedBy')
BEGIN
    ALTER TABLE [dbo].[TblGroup] ADD [ModeratedBy] INT NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblGroup') AND name = 'ModeratedAt')
BEGIN
    ALTER TABLE [dbo].[TblGroup] ADD [ModeratedAt] DATETIME2 NULL;
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblGroup') AND name = 'ModerationReason')
BEGIN
    ALTER TABLE [dbo].[TblGroup] ADD [ModerationReason] NVARCHAR(500) NULL;
END;
";
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
