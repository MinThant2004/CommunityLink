using CommunityLink.Database.AppDbContextModels;
using Microsoft.EntityFrameworkCore;

namespace CommunityLink.Domain.Features.ChatGroup;

/// <summary>
/// Applies the DDL for the two member-scoped moderation features that share a release: the
/// per-admin permission matrix on <c>TblChatGroupMember</c> and the pin flag on
/// <c>TblChatGroupMessage</c>.
/// <para>
/// The project is database-first (scaffolded entities, no EF migrations), so this runs the
/// same idempotent script a DBA can apply by hand from
/// <c>CommunityLink.Database/Scripts/20261001_Chat_Group_Moderation.sql</c>. Every statement
/// is guarded by an existence check, so it is safe on an already-updated database.
/// </para>
/// <para>
/// The pin lives on the message row rather than in a side table. A soft-deleted message
/// therefore stops being pinned for free, because every read filters <c>IsDeleted</c>, and there
/// is no orphan to clean up when a moderator retracts a message.
/// </para>
/// </summary>
public static class ChatGroupModerationDatabaseSeeder
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
-- Per-member admin permission matrix.
--
-- Scoped to a single group: a member who can pin in one group can be
-- stripped of that power in another without affecting either. Every
-- column defaults to 0, so an existing ADMIN row created before this
-- feature lands holds no powers until the owner grants them, and a
-- freshly promoted admin is granted them explicitly by the service.
--
-- Never consulted for a plain MEMBER, and ignored for the OWNER, who
-- always holds every permission.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanDeleteMessages')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanDeleteMessages] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanDeleteMessages] DEFAULT ((0));

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanRemoveMembers')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanRemoveMembers] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanRemoveMembers] DEFAULT ((0));

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanBanMembers')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanBanMembers] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanBanMembers] DEFAULT ((0));

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanManageInviteLinks')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanManageInviteLinks] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanManageInviteLinks] DEFAULT ((0));

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanPinMessages')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanPinMessages] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanPinMessages] DEFAULT ((0));

PRINT 'INFO: TblChatGroupMember permission matrix is in place.';

-- ------------------------------------------------------------------
-- Pinned message.
--
-- Telegram-style: at most one message is pinned per group. Uniqueness
-- is enforced in the service, which clears any existing pin in the
-- same transaction; the filtered index below serves only the read of
-- the current pin, ordered most-recent-first, so it stays narrow
-- (IsPinned = 1) rather than scanning the whole message history.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'IsPinned')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [IsPinned] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMessage_IsPinned] DEFAULT ((0));

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'PinnedAt')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [PinnedAt] DATETIME2 NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'PinnedByUserId')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [PinnedByUserId] INT NULL;

-- The FK and the filtered index both reference columns added just above. SQL Server compiles a
-- whole batch before running any of it, and it cannot resolve a column that an ALTER earlier in
-- the same batch will create, so these two statements would fail with an invalid-column-name error even
-- though the column comes to exist. EXEC(N'...') defers their compilation to run time, after the
-- ALTERs have executed, which is the same effect GO has in the hand-run script.
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TblChatGroupMessage_PinnedBy')
    EXEC(N'ALTER TABLE [dbo].[TblChatGroupMessage] ADD CONSTRAINT [FK_TblChatGroupMessage_PinnedBy]
        FOREIGN KEY ([PinnedByUserId]) REFERENCES [dbo].[TblUser] ([UserId]);');

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupMessage_ChatGroupId_PinnedAt' AND object_id = OBJECT_ID('dbo.TblChatGroupMessage'))
    EXEC(N'CREATE NONCLUSTERED INDEX [IX_TblChatGroupMessage_ChatGroupId_PinnedAt]
        ON [dbo].[TblChatGroupMessage] ([ChatGroupId], [PinnedAt] DESC)
        INCLUDE ([ChatGroupMessageId], [SenderId], [Content])
        WHERE [IsPinned] = 1;');

PRINT 'SUCCESS: Chat Group moderation schema is in place (permission matrix, pinned message).';
";
}
