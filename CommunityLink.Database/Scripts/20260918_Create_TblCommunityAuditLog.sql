-- ===================================================================
-- CommunityLink Database Script
-- Table: dbo.TblCommunityAuditLog
-- Description: Stores field-level audit trails for Community and 
--              Sub-Community edits (Name, Description, Editor info).
-- Target Database: SQL Server 2019 / 2022 / Azure SQL
-- ===================================================================
USE [CommunityLink];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblCommunityAuditLog' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblCommunityAuditLog (
        AuditId             INT IDENTITY(1,1) NOT NULL,
        CommunityId         INT NOT NULL,                     -- Foreign Key to dbo.TblCommunity
        TargetType          VARCHAR(50) NOT NULL,             -- 'COMMUNITY' or 'SUB_COMMUNITY'
        FieldChanged        VARCHAR(50) NOT NULL,             -- 'Name' or 'Description'
        OldValue            NVARCHAR(MAX) NULL,               -- Prior value
        NewValue            NVARCHAR(MAX) NULL,               -- New updated value
        EditorId            INT NOT NULL,                     -- Foreign Key to dbo.TblUser
        IpAddress           VARCHAR(50) NULL,
        CreatedAt           DATETIME2(7) NOT NULL CONSTRAINT DF_TblCommunityAuditLog_CreatedAt DEFAULT (SYSUTCDATETIME()),
        
        CONSTRAINT PK_TblCommunityAuditLog PRIMARY KEY CLUSTERED (AuditId ASC),
        CONSTRAINT FK_TblCommunityAuditLog_Community FOREIGN KEY (CommunityId) 
            REFERENCES dbo.TblCommunity (CommunityId) ON DELETE CASCADE,
        CONSTRAINT FK_TblCommunityAuditLog_User FOREIGN KEY (EditorId) 
            REFERENCES dbo.TblUser (UserId)
    );

    CREATE NONCLUSTERED INDEX IX_TblCommunityAuditLog_CommunityId_CreatedAt 
        ON dbo.TblCommunityAuditLog (CommunityId ASC, CreatedAt DESC);

    PRINT 'SUCCESS: Table dbo.TblCommunityAuditLog created successfully with indexes and foreign keys.';
END
ELSE
BEGIN
    PRINT 'INFO: Table dbo.TblCommunityAuditLog already exists. No actions performed.';
END
GO
