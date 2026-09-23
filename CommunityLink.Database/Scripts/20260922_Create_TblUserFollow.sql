-- ===================================================================
-- CommunityLink Database Script
-- Table: dbo.TblUserFollow
-- Description: Stores user follow relationships (Follower -> Followee)
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
-- ===================================================================
USE [CommunityLink];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblUserFollow' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblUserFollow (
        FollowId            INT IDENTITY(1,1) NOT NULL,
        FollowerId          INT NOT NULL,                     -- User who initiated following
        FolloweeId          INT NOT NULL,                     -- User who is followed
        CreatedAt           DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserFollow_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy           INT NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_TblUserFollow_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2(7) NULL,
        DeletedBy           INT NULL,
        RowVersion          ROWVERSION NOT NULL,
        
        CONSTRAINT PK_TblUserFollow PRIMARY KEY CLUSTERED (FollowId ASC),
        CONSTRAINT FK_TblUserFollow_Follower FOREIGN KEY (FollowerId) 
            REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblUserFollow_Followee FOREIGN KEY (FolloweeId) 
            REFERENCES dbo.TblUser (UserId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_TblUserFollow_Follower_Followee 
        ON dbo.TblUserFollow (FollowerId ASC, FolloweeId ASC)
        WHERE IsDeleted = 0;

    CREATE NONCLUSTERED INDEX IX_TblUserFollow_FolloweeId 
        ON dbo.TblUserFollow (FolloweeId ASC, CreatedAt DESC)
        WHERE IsDeleted = 0;

    PRINT 'SUCCESS: Table dbo.TblUserFollow created successfully with indexes and foreign keys.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblUserFollow already exists. No actions performed.';
END
GO
