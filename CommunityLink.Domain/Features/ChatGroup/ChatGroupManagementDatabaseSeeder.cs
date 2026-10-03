using CommunityLink.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.ChatGroup;

/// <summary>
/// Applies the DDL backing Chat Group Creator/Admin management: the per-group ban table that
/// backs the ban/unban feature.
/// <para>
/// The project is database-first (scaffolded entities, no EF migrations), so this runs the same
/// idempotent script a DBA can apply by hand from
/// <c>CommunityLink.Database/Scripts/20260929_Chat_Group_Management.sql</c>. Every statement is
/// guarded by an existence check, so it is safe on an already-updated database.
/// </para>
/// </summary>
public static class ChatGroupManagementDatabaseSeeder
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
-- Chat Group bans.
--
-- Scoped to a single group: a user banned from one group may still
-- take part in every other group. A ban also removes the user from
-- the group, so the ban row is the durable record of why the
-- membership is gone.
--
-- Unban is a soft delete (IsDeleted = 1) so the ban history is
-- retained for audit. The unique index is therefore filtered on
-- IsDeleted = 0, which allows the same user to be banned again
-- later without colliding with the revoked row.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupBan')
BEGIN
    CREATE TABLE [dbo].[TblChatGroupBan] (
        [ChatGroupBanId] INT IDENTITY(1,1) NOT NULL,
        [ChatGroupId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [BannedByUserId] INT NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_TblChatGroupBan_IsDeleted] DEFAULT ((0)),
        [BannedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupBan_BannedAt] DEFAULT (getutcdate()),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupBan_CreatedAt] DEFAULT (getutcdate()),
        [CreatedBy] INT NOT NULL,
        [RevokedAt] DATETIME2 NULL,
        [RevokedByUserId] INT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_TblChatGroupBan] PRIMARY KEY CLUSTERED ([ChatGroupBanId] ASC),
        CONSTRAINT [FK_TblChatGroupBan_TblChatGroup] FOREIGN KEY ([ChatGroupId]) REFERENCES [dbo].[TblChatGroup] ([ChatGroupId]),
        CONSTRAINT [FK_TblChatGroupBan_TblUser_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblChatGroupBan_TblUser_BannedBy] FOREIGN KEY ([BannedByUserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblChatGroupBan_TblUser_RevokedBy] FOREIGN KEY ([RevokedByUserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    PRINT 'INFO: TblChatGroupBan created.';
END
ELSE
BEGIN
    PRINT 'INFO: TblChatGroupBan already exists - table left alone.';
END;

-- Guarded on its own rather than sharing the table check above. A failure part way
-- through the table block would otherwise leave the table present but the index
-- missing, and the table-level guard would then treat the schema as complete and
-- never retry it.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_TblChatGroupBan_Group_User' AND object_id = OBJECT_ID(N'[dbo].[TblChatGroupBan]'))
BEGIN
    -- One active ban per (group, user). The filter is what permits a re-ban after an
    -- unban: the revoked row keeps its history while the new row claims the pair.
    -- This index also serves the banned-user probe on the join and direct-add hot paths,
    -- so a second non-unique index on the same columns would be redundant.
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatGroupBan_Group_User]
        ON [dbo].[TblChatGroupBan] ([ChatGroupId], [UserId])
        WHERE [IsDeleted] = (0);
    PRINT 'INFO: UQ_TblChatGroupBan_Group_User created.';
END
ELSE
BEGIN
    PRINT 'INFO: UQ_TblChatGroupBan_Group_User already exists.';
END;

PRINT 'SUCCESS: Chat Group management schema is in place (TblChatGroupBan).';
";
}
