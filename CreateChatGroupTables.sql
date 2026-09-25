SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroup')
BEGIN
    CREATE TABLE TblChatGroup (
        ChatGroupId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        AvatarUrl NVARCHAR(500) NULL,
        BannerUrl NVARCHAR(500) NULL,
        CreatorId INT NOT NULL,
        ChatType VARCHAR(20) NOT NULL CONSTRAINT DF_TblChatGroup_ChatType DEFAULT ('FREE'),
        JoinFeeLinkDrops BIGINT NOT NULL CONSTRAINT DF_TblChatGroup_JoinFeeLinkDrops DEFAULT (0),
        CommissionPercentageSnapshot DECIMAL(5,2) NOT NULL CONSTRAINT DF_TblChatGroup_CommissionPercentageSnapshot DEFAULT (0.00),
        IsActive BIT NOT NULL CONSTRAINT DF_TblChatGroup_IsActive DEFAULT (1),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroup_CreatedAt DEFAULT (GETUTCDATE()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy INT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_TblChatGroup_IsDeleted DEFAULT (0),
        DeletedAt DATETIME2 NULL,
        DeletedBy INT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_TblChatGroup_TblUser_Creator FOREIGN KEY (CreatorId) REFERENCES TblUser(UserId)
    );

    CREATE INDEX IX_TblChatGroup_CreatorId ON TblChatGroup(CreatorId);
    CREATE INDEX IX_TblChatGroup_ChatType ON TblChatGroup(ChatType);
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupMember')
BEGIN
    CREATE TABLE TblChatGroupMember (
        ChatGroupMemberId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ChatGroupId INT NOT NULL,
        UserId INT NOT NULL,
        Role VARCHAR(20) NOT NULL CONSTRAINT DF_TblChatGroupMember_Role DEFAULT ('MEMBER'),
        JoinedAt DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupMember_JoinedAt DEFAULT (GETUTCDATE()),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupMember_CreatedAt DEFAULT (GETUTCDATE()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy INT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_TblChatGroupMember_IsDeleted DEFAULT (0),
        DeletedAt DATETIME2 NULL,
        DeletedBy INT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_TblChatGroupMember_TblChatGroup FOREIGN KEY (ChatGroupId) REFERENCES TblChatGroup(ChatGroupId),
        CONSTRAINT FK_TblChatGroupMember_TblUser FOREIGN KEY (UserId) REFERENCES TblUser(UserId)
    );

    CREATE UNIQUE INDEX UQ_TblChatGroupMember_Group_User ON TblChatGroupMember(ChatGroupId, UserId) WHERE IsDeleted = 0;
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblChatGroupMessage')
BEGIN
    CREATE TABLE TblChatGroupMessage (
        ChatGroupMessageId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ChatGroupId INT NOT NULL,
        SenderId INT NOT NULL,
        Content NVARCHAR(4000) NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_TblChatGroupMessage_CreatedAt DEFAULT (GETUTCDATE()),
        CreatedBy INT NULL,
        UpdatedAt DATETIME2 NULL,
        UpdatedBy INT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_TblChatGroupMessage_IsDeleted DEFAULT (0),
        DeletedAt DATETIME2 NULL,
        DeletedBy INT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT FK_TblChatGroupMessage_TblChatGroup FOREIGN KEY (ChatGroupId) REFERENCES TblChatGroup(ChatGroupId),
        CONSTRAINT FK_TblChatGroupMessage_TblUser_Sender FOREIGN KEY (SenderId) REFERENCES TblUser(UserId)
    );

    CREATE INDEX IX_TblChatGroupMessage_Group_CreatedAt ON TblChatGroupMessage(ChatGroupId, CreatedAt);
    CREATE INDEX IX_TblChatGroupMessage_SenderId ON TblChatGroupMessage(SenderId);
END;
