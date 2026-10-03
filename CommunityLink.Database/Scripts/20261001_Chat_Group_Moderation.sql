-- ------------------------------------------------------------------
-- Chat Group moderation: per-admin permission matrix + pinned message.
--
-- Hand-applicable companion to
-- CommunityLink.Domain/Features/ChatGroup/ChatGroupModerationDatabaseSeeder.cs,
-- which runs the same statements at API startup. Every statement is guarded by an
-- existence check, so it is safe to run against an already-updated database.
-- ------------------------------------------------------------------

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
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanRemoveMembers')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanRemoveMembers] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanRemoveMembers] DEFAULT ((0));
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanBanMembers')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanBanMembers] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanBanMembers] DEFAULT ((0));
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanManageInviteLinks')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanManageInviteLinks] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanManageInviteLinks] DEFAULT ((0));
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMember') AND name = 'CanPinMessages')
    ALTER TABLE [dbo].[TblChatGroupMember] ADD [CanPinMessages] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMember_CanPinMessages] DEFAULT ((0));
GO

PRINT 'INFO: TblChatGroupMember permission matrix is in place.';
GO

-- ------------------------------------------------------------------
-- Pinned message.
--
-- The pin lives on the message row rather than in a side table: a
-- soft-deleted message then stops being pinned automatically, because
-- every read filters IsDeleted, and a retracted pin leaves nothing to
-- clean up. At most one message is pinned per group; the service clears
-- any existing pin in the same transaction that sets a new one. The
-- filtered index therefore stays narrow (IsPinned = 1) rather than
-- scanning the whole message history.
-- ------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'IsPinned')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [IsPinned] BIT NOT NULL CONSTRAINT [DF_TblChatGroupMessage_IsPinned] DEFAULT ((0));
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'PinnedAt')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [PinnedAt] DATETIME2 NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TblChatGroupMessage') AND name = 'PinnedByUserId')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD [PinnedByUserId] INT NULL;
GO

-- Foreign key
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TblChatGroupMessage_PinnedBy')
    ALTER TABLE [dbo].[TblChatGroupMessage] ADD CONSTRAINT [FK_TblChatGroupMessage_PinnedBy]
        FOREIGN KEY ([PinnedByUserId]) REFERENCES [dbo].[TblUser] ([UserId]);
GO

-- Index. Guarded separately from the columns so a partial failure above is retried
-- rather than being masked by a "columns already exist" check.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblChatGroupMessage_ChatGroupId_PinnedAt' AND object_id = OBJECT_ID('dbo.TblChatGroupMessage'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TblChatGroupMessage_ChatGroupId_PinnedAt]
        ON [dbo].[TblChatGroupMessage] ([ChatGroupId], [PinnedAt] DESC)
        INCLUDE ([ChatGroupMessageId], [SenderId], [Content])
        WHERE [IsPinned] = 1;
END
GO

PRINT 'SUCCESS: Chat Group moderation schema is in place (permission matrix, pinned message).';
GO
