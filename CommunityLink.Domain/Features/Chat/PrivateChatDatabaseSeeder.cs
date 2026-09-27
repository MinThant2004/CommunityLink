using Microsoft.EntityFrameworkCore;
using CommunityLink.Database.AppDbContextModels;

namespace CommunityLink.Domain.Features.Chat;

public static class PrivateChatDatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Database.IsRelational())
        {
            var createTablesSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblCreatorChatSetting')
BEGIN
    CREATE TABLE [dbo].[TblCreatorChatSetting] (
        [CreatorChatSettingId] INT IDENTITY(1,1) NOT NULL,
        [CreatorUserId] INT NOT NULL,
        [IsPrivateChatEnabled] BIT NOT NULL CONSTRAINT [DF_TblCreatorChatSetting_IsPrivateChatEnabled] DEFAULT ((0)),
        [PrivateChatFeeLinkDrops] BIGINT NOT NULL CONSTRAINT [DF_TblCreatorChatSetting_Fee] DEFAULT ((0)),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblCreatorChatSetting_CreatedAt] DEFAULT (getutcdate()),
        [UpdatedAt] DATETIME2 NULL,
        [RowVersion] VARBINARY(8) NULL,
        CONSTRAINT [PK_TblCreatorChatSetting] PRIMARY KEY CLUSTERED ([CreatorChatSettingId] ASC),
        CONSTRAINT [UQ_TblCreatorChatSetting_CreatorUserId] UNIQUE ([CreatorUserId]),
        CONSTRAINT [FK_TblCreatorChatSetting_TblUser_Creator] FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblPrivateChatPaymentTransaction')
BEGIN
    CREATE TABLE [dbo].[TblPrivateChatPaymentTransaction] (
        [PrivateChatPaymentTransactionId] BIGINT IDENTITY(1,1) NOT NULL,
        [ConversationId] INT NOT NULL,
        [BuyerUserId] INT NOT NULL,
        [CreatorUserId] INT NOT NULL,
        [GrossAmountLinkDrops] BIGINT NOT NULL,
        [CommissionAmount] BIGINT NOT NULL,
        [CreatorAmount] BIGINT NOT NULL,
        [Status] VARCHAR(20) NOT NULL CONSTRAINT [DF_TblPrivateChatPaymentTransaction_Status] DEFAULT ('COMPLETED'),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_TblPrivateChatPaymentTransaction_CreatedAt] DEFAULT (getutcdate()),
        [RowVersion] VARBINARY(8) NULL,
        CONSTRAINT [PK_TblPrivateChatPaymentTransaction] PRIMARY KEY CLUSTERED ([PrivateChatPaymentTransactionId] ASC),
        CONSTRAINT [FK_TblPrivateChatPaymentTransaction_TblConversation] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[TblConversation] ([ConversationId]),
        CONSTRAINT [FK_TblPrivateChatPaymentTransaction_TblUser_Buyer] FOREIGN KEY ([BuyerUserId]) REFERENCES [dbo].[TblUser] ([UserId]),
        CONSTRAINT [FK_TblPrivateChatPaymentTransaction_TblUser_Creator] FOREIGN KEY ([CreatorUserId]) REFERENCES [dbo].[TblUser] ([UserId])
    );

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblPrivateChatPaymentTransaction_ConversationId')
        CREATE INDEX [IX_TblPrivateChatPaymentTransaction_ConversationId] ON [dbo].[TblPrivateChatPaymentTransaction] ([ConversationId]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblPrivateChatPaymentTransaction_BuyerUserId')
        CREATE INDEX [IX_TblPrivateChatPaymentTransaction_BuyerUserId] ON [dbo].[TblPrivateChatPaymentTransaction] ([BuyerUserId]);

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TblPrivateChatPaymentTransaction_CreatorUserId')
        CREATE INDEX [IX_TblPrivateChatPaymentTransaction_CreatorUserId] ON [dbo].[TblPrivateChatPaymentTransaction] ([CreatorUserId]);
END;
";
            await db.Database.ExecuteSqlRawAsync(createTablesSql);
        }
    }
}
