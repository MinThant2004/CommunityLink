-- ------------------------------------------------------------------
-- Script: 20261001_Create_TblUserBlock.sql
-- Description: Creates the TblUserBlock table for 1:1 direct chat user blocking
-- ------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblUserBlock')
BEGIN
    CREATE TABLE [dbo].[TblUserBlock] (
        [UserBlockId] INT IDENTITY(1,1) NOT NULL,
        [BlockerUserId] INT NOT NULL,
        [BlockedUserId] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblUserBlock_CreatedAt] DEFAULT (getutcdate()),
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_TblUserBlock_IsDeleted] DEFAULT ((0)),
        [DeletedAt] DATETIME2 NULL,
        CONSTRAINT [PK_TblUserBlock] PRIMARY KEY CLUSTERED ([UserBlockId] ASC),
        CONSTRAINT [FK_TblUserBlock_BlockerUser] FOREIGN KEY ([BlockerUserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblUserBlock_BlockedUser] FOREIGN KEY ([BlockedUserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [UQ_TblUserBlock_Blocker_Blocked]
        ON [dbo].[TblUserBlock] ([BlockerUserId], [BlockedUserId]);
END;
GO
