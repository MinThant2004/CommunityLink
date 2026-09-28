-- =====================================================================
-- Migration Script: Premium Subscription Plans & User Subscriptions
-- Target Database: CommunityLink Database (SQL Server)
-- =====================================================================

-- 1. Create TblSubscriptionPlan (Admin Configured Membership Plans)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblSubscriptionPlan' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblSubscriptionPlan (
        PlanId          INT IDENTITY(1,1) NOT NULL,
        TargetRoleCode  NVARCHAR(50) NOT NULL,                                                     -- 'DOMAIN_PRO' or 'PUBLIC_FIGURE'
        PlanName        NVARCHAR(150) NOT NULL,                                                    -- e.g. 'Domain Professional (Monthly)'
        BillingInterval NVARCHAR(20) NOT NULL CONSTRAINT DF_TblSubPlan_BillingInterval DEFAULT ('Monthly'), -- 'Monthly', 'Annual', etc.
        DurationDays    INT NOT NULL CONSTRAINT DF_TblSubPlan_DurationDays DEFAULT (30),           -- 30, 365, etc.
        PriceAmount     DECIMAL(18,2) NOT NULL CONSTRAINT DF_TblSubPlan_PriceAmount DEFAULT (0),  -- e.g. 39.00
        LinkDropCost    BIGINT NOT NULL CONSTRAINT DF_TblSubPlan_LinkDropCost DEFAULT (0),         -- Points equivalent (e.g. 390)
        PerksJson       NVARCHAR(MAX) NULL,                                                        -- JSON array of bullet points
        IsActive        BIT NOT NULL CONSTRAINT DF_TblSubPlan_IsActive DEFAULT (1),                -- Active toggle
        CreatedAt       DATETIME2(7) NOT NULL CONSTRAINT DF_TblSubPlan_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2(7) NULL,
        CONSTRAINT PK_TblSubscriptionPlan PRIMARY KEY CLUSTERED (PlanId ASC)
    );
    PRINT 'Created table: dbo.TblSubscriptionPlan';
END
ELSE
BEGIN
    PRINT 'Table dbo.TblSubscriptionPlan already exists.';
END
GO

-- 2. Create TblUserSubscription (Tracks Member Tier Purchases & Expirations)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblUserSubscription' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblUserSubscription (
        SubscriptionId INT IDENTITY(1,1) NOT NULL,
        UserId         INT NOT NULL,
        PlanId         INT NOT NULL,
        RoleId         INT NOT NULL,
        Status         NVARCHAR(30) NOT NULL CONSTRAINT DF_TblUserSub_Status DEFAULT ('Active'),          -- 'PendingReview', 'Active', 'Expired', 'Rejected'
        PaymentMethod  NVARCHAR(50) NOT NULL CONSTRAINT DF_TblUserSub_PaymentMethod DEFAULT ('LinkDropPoints'), -- 'LinkDropPoints', 'ManualPaymentSlip'
        StartDateUtc   DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserSub_StartDateUtc DEFAULT (SYSUTCDATETIME()),
        ExpiresAtUtc   DATETIME2(7) NOT NULL,
        CreatedAtUtc   DATETIME2(7) NOT NULL CONSTRAINT DF_TblUserSub_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_TblUserSubscription PRIMARY KEY CLUSTERED (SubscriptionId ASC),
        CONSTRAINT FK_TblUserSubscription_User FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblUserSubscription_Plan FOREIGN KEY (PlanId) REFERENCES dbo.TblSubscriptionPlan (PlanId),
        CONSTRAINT FK_TblUserSubscription_Role FOREIGN KEY (RoleId) REFERENCES dbo.TblRole (RoleId)
    );
    PRINT 'Created table: dbo.TblUserSubscription';
END
ELSE
BEGIN
    PRINT 'Table dbo.TblUserSubscription already exists.';
END
GO

-- 3. Create TblIdentityVerification (Identity Card & Credential Verification Requests)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TblIdentityVerification' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TblIdentityVerification (
        VerificationId          INT IDENTITY(1,1) NOT NULL,
        UserId                  INT NOT NULL,
        PlanId                  INT NOT NULL,
        TargetRoleCode          NVARCHAR(50) NOT NULL,
        FullLegalName           NVARCHAR(200) NOT NULL,
        WorkEmail               NVARCHAR(256) NULL,
        ProfessionalUrl         NVARCHAR(500) NULL,
        IdCardFrontUrl          NVARCHAR(1000) NOT NULL,
        IdCardBackUrl           NVARCHAR(1000) NULL,
        PaymentMethod           NVARCHAR(50) NOT NULL CONSTRAINT DF_TblIdVerif_PaymentMethod DEFAULT ('LinkDropPoints'),
        LinkDropPointsDeducted  BIGINT NOT NULL CONSTRAINT DF_TblIdVerif_PointsDeducted DEFAULT (0),
        Status                  NVARCHAR(30) NOT NULL CONSTRAINT DF_TblIdVerif_Status DEFAULT ('PendingReview'),
        ReviewNotes             NVARCHAR(1000) NULL,
        ReviewedByAdminId       INT NULL,
        ReviewedAtUtc           DATETIME2(7) NULL,
        CreatedAtUtc            DATETIME2(7) NOT NULL CONSTRAINT DF_TblIdVerif_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_TblIdentityVerification PRIMARY KEY CLUSTERED (VerificationId ASC),
        CONSTRAINT FK_TblIdentityVerification_User FOREIGN KEY (UserId) REFERENCES dbo.TblUser (UserId),
        CONSTRAINT FK_TblIdentityVerification_Plan FOREIGN KEY (PlanId) REFERENCES dbo.TblSubscriptionPlan (PlanId),
        CONSTRAINT FK_TblIdentityVerification_Admin FOREIGN KEY (ReviewedByAdminId) REFERENCES dbo.TblAdmin (AdminId)
    );
    PRINT 'Created table: dbo.TblIdentityVerification';
END
ELSE
BEGIN
    PRINT 'Table dbo.TblIdentityVerification already exists.';
END
GO

-- 3. Seed Default Plans (Matching Design Reference & Specifications)
IF NOT EXISTS (SELECT 1 FROM dbo.TblSubscriptionPlan)
BEGIN
    INSERT INTO dbo.TblSubscriptionPlan 
        (TargetRoleCode, PlanName, BillingInterval, DurationDays, PriceAmount, LinkDropCost, PerksJson, IsActive, CreatedAt)
    VALUES
        (
            'DOMAIN_PRO',
            'Domain Professional (Monthly)',
            'Monthly',
            30,
            39.00,
            390,
            '["1-on-1 Paid Advisory Engine (set custom hourly rate)","Public Peer Rating & Review Card","Priority Sub-Community Ownership (up to 5)","Domain Competency Endorsement","Credential Verification Badging"]',
            1,
            SYSUTCDATETIME()
        ),
        (
            'DOMAIN_PRO',
            'Domain Professional (Annual)',
            'Annual',
            365,
            390.00,
            3900,
            '["All Monthly Perks included","2 Months Free Discount","Expedited 24h Verification Audit","Featured in Domain Pro Directory Shelf"]',
            1,
            SYSUTCDATETIME()
        ),
        (
            'PUBLIC_FIGURE',
            'Public Figure VIP (Monthly)',
            'Monthly',
            30,
            119.00,
            1190,
            '["Discovery shelf Priority Guaranteed top placement","Unlimited Sovereign Communities","0% Platform Fees on Advisory (first $10,000/yr)","Dedicated Admin Concierge Direct Slack/Signal channel","Government ID & Identity Card Verified"]',
            1,
            SYSUTCDATETIME()
        ),
        (
            'PUBLIC_FIGURE',
            'Public Figure VIP (Annual)',
            'Annual',
            365,
            1190.00,
            11900,
            '["All Public Figure VIP Monthly Perks","Save $238 (2 Months Free)","Priority Expedited SecOps Review","VIP Platinum Profile Crest"]',
            1,
            SYSUTCDATETIME()
        );
    PRINT 'Seeded default subscription plans.';
END
ELSE
BEGIN
    PRINT 'Subscription plans already seeded.';
END
GO
