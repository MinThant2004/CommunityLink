-- ===================================================================
-- CommunityLink Database Script
-- Table: dbo.TblGroupRating
-- Description: Stores user ratings and review comments for groups.
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
-- ===================================================================
USE [CommunityLink];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblGroupRating' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblGroupRating (
        GroupRatingId       INT IDENTITY(1,1) NOT NULL,
        GroupId             INT NOT NULL,                     -- Group being rated
        UserId              INT NOT NULL,                     -- Member user who submitted the rating
        Score               INT NOT NULL,                     -- Rating score between 1 and 5
        ReviewText          NVARCHAR(1000) NULL,              -- Optional review text / comment
        CreatedAt           DATETIME2(7) NOT NULL CONSTRAINT DF_TblGroupRating_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy           INT NULL,
        UpdatedAt           DATETIME2(7) NULL,
        UpdatedBy           INT NULL,
        IsDeleted           BIT NOT NULL CONSTRAINT DF_TblGroupRating_IsDeleted DEFAULT (0),
        DeletedAt           DATETIME2(7) NULL,
        DeletedBy           INT NULL,
        RowVersion          ROWVERSION NOT NULL,
        
        CONSTRAINT PK_TblGroupRating PRIMARY KEY CLUSTERED (GroupRatingId ASC),
        CONSTRAINT FK_TblGroupRating_Group FOREIGN KEY (GroupId) 
            REFERENCES dbo.TblGroup (GroupId),
        CONSTRAINT FK_TblGroupRating_User FOREIGN KEY (UserId) 
            REFERENCES dbo.TblUser (UserId),
        CONSTRAINT CK_TblGroupRating_Score CHECK (Score >= 1 AND Score <= 5)
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_TblGroupRating_Group_User 
        ON dbo.TblGroupRating (GroupId ASC, UserId ASC)
        WHERE IsDeleted = 0;

    CREATE NONCLUSTERED INDEX IX_TblGroupRating_GroupId_CreatedAt 
        ON dbo.TblGroupRating (GroupId ASC, CreatedAt DESC)
        WHERE IsDeleted = 0;

    PRINT 'SUCCESS: Table dbo.TblGroupRating created successfully with indexes and foreign keys.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblGroupRating already exists. No actions performed.';
END
GO
