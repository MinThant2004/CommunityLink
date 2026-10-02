using CommunityLink.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.ChatGroup;

/// <summary>
/// Applies the DDL backing owner invites into a PAID Chat Group: the invite table the paid-join
/// flow resolves.
/// <para>
/// The project is database-first (scaffolded entities, no EF migrations), so this runs the same
/// idempotent script a DBA can apply by hand from
/// <c>CommunityLink.Database/Scripts/20260930_Chat_Group_Invite.sql</c>. Every statement is
/// guarded by an existence check, so it is safe on an already-updated database.
/// </para>
/// </summary>
public static class ChatGroupInviteDatabaseSeeder
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
-- CREATE INDEX requires ARITHABORT / ANSI_NULLS / QUOTED_IDENTIFIER to be set correctly on the
-- executing session. Microsoft.Data.SqlClient sets these on every connection it opens, but
-- stating them here makes the script safe for any caller regardless of session defaults.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;

-- ------------------------------------------------------------------
-- Chat Group invites.
--
-- An invite is an entitlement offer, not access. While an invite is
-- PENDING the invitee holds no TblChatGroupMember row, so the message
-- history, preview, notification fan-out and realtime room gates all
-- continue to treat them as a non-member. Only the paid-join path
-- writes the membership.
--
-- Resolved invites are retained (Status -> ACCEPTED / REVOKED /
-- DECLINED) so the creator can see whether an invite converted. The
-- unique index is therefore filtered on Status = 'PENDING' rather than
-- IsDeleted = 0, which lets the same user be re-invited after
-- declining or being revoked while still blocking a duplicate open
-- invite.
--
-- Note for ad-hoc inserts: a filtered index requires SET QUOTED_IDENTIFIER ON.
-- The application satisfies this because Microsoft.Data.SqlClient issues the SET
-- on every connection it opens, so it only matters for external tooling - sqlcmd
-- defaults it OFF and fails with ""SET options have incorrect settings"", while
-- SSMS is ON by default.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupInvite' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[TblChatGroupInvite] (
        [ChatGroupInviteId] INT IDENTITY(1,1) NOT NULL,
        [ChatGroupId] INT NOT NULL,
        [UserId] INT NOT NULL,
        [InvitedByUserId] INT NOT NULL,
        [Status] VARCHAR(20) NOT NULL,
        [FeeAtInviteLinkDrops] BIGINT NOT NULL CONSTRAINT [DF_TblChatGroupInvite_FeeAtInviteLinkDrops] DEFAULT ((0)),
        [InvitedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupInvite_InvitedAt] DEFAULT (getutcdate()),
        [RespondedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_TblChatGroupInvite_IsDeleted] DEFAULT ((0)),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblChatGroupInvite_CreatedAt] DEFAULT (getutcdate()),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] INT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        CONSTRAINT [PK_TblChatGroupInvite] PRIMARY KEY CLUSTERED ([ChatGroupInviteId] ASC),
        CONSTRAINT [CK_TblChatGroupInvite_Status] CHECK ([Status] IN ('PENDING','ACCEPTED','REVOKED','DECLINED')),
        CONSTRAINT [FK_TblChatGroupInvite_TblChatGroup] FOREIGN KEY ([ChatGroupId]) REFERENCES [dbo].[TblChatGroup] ([ChatGroupId]),
        CONSTRAINT [FK_TblChatGroupInvite_TblUser_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblChatGroupInvite_TblUser_InvitedBy] FOREIGN KEY ([InvitedByUserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblChatGroupInvite_TblUser_DeletedBy] FOREIGN KEY ([DeletedBy]) REFERENCES [dbo].[TblUser] ([UserId])
    );
    PRINT 'INFO: TblChatGroupInvite created.';
END
ELSE
BEGIN
    PRINT 'INFO: TblChatGroupInvite already exists - table left alone.';
END;

-- Each index is guarded on its own rather than sharing the table check above. A failure part
-- way through the table block would otherwise leave the table present but an index missing, and
-- the table-level guard would then treat the schema as complete and never retry it.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_TblChatGroupInvite_Group_User' AND object_id = OBJECT_ID(N'[dbo].[TblChatGroupInvite]'))
BEGIN
    -- At most one open invite per (group, user). Filtering on Status is what
    -- permits re-inviting a user whose previous invite was declined or
    -- revoked, while still blocking a duplicate open invite.
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblChatGroupInvite_Group_User]
        ON [dbo].[TblChatGroupInvite] ([ChatGroupId], [UserId])
        WHERE [Status] = 'PENDING';
    PRINT 'INFO: UQ_TblChatGroupInvite_Group_User created.';
END
ELSE
BEGIN
    PRINT 'INFO: UQ_TblChatGroupInvite_Group_User already exists.';
END;

-- Covers the ""my invitations"" list, which filters on invitee + status.
-- InvitedAt must not also appear in INCLUDE: it is already a key column, and SQL Server
-- rejects a duplicate column name in the same index definition.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupInvite_User_Status' AND object_id = OBJECT_ID(N'[dbo].[TblChatGroupInvite]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TblChatGroupInvite_User_Status]
        ON [dbo].[TblChatGroupInvite] ([UserId], [Status], [InvitedAt] DESC)
        INCLUDE ([ChatGroupId], [FeeAtInviteLinkDrops], [InvitedByUserId]);
    PRINT 'INFO: IX_TblChatGroupInvite_User_Status created.';
END
ELSE
BEGIN
    PRINT 'INFO: IX_TblChatGroupInvite_User_Status already exists.';
END;

PRINT 'SUCCESS: Chat Group invite schema is in place (TblChatGroupInvite).';
";
}
