using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class AdminPartBLocalizationTests
{
    private static readonly string[] VerificationsKeys =
    [
        "Admin_Verif_ApproveIssueBtn",
        "Admin_Verif_AuditActionHeading",
        "Admin_Verif_AuditRecord",
        "Admin_Verif_BackFace",
        "Admin_Verif_BtnInspect",
        "Admin_Verif_CardCredentialsHeading",
        "Admin_Verif_ColActions",
        "Admin_Verif_ColApplicant",
        "Admin_Verif_ColLegalName",
        "Admin_Verif_ColPayment",
        "Admin_Verif_ColStatus",
        "Admin_Verif_ColSubmittedAt",
        "Admin_Verif_ColTargetTier",
        "Admin_Verif_EmptyDesc",
        "Admin_Verif_EmptyTitle",
        "Admin_Verif_EscrowNoticeDesc",
        "Admin_Verif_EscrowNoticePrefix",
        "Admin_Verif_EscrowPaidLabel",
        "Admin_Verif_FrontFace",
        "Admin_Verif_Heading",
        "Admin_Verif_LightboxClose",
        "Admin_Verif_Loading",
        "Admin_Verif_ModalSubtitle",
        "Admin_Verif_ModalTitle",
        "Admin_Verif_NoBackImage",
        "Admin_Verif_NoneSpecified",
        "Admin_Verif_NotProvided",
        "Admin_Verif_NotesLabel",
        "Admin_Verif_NotesPlaceholder",
        "Admin_Verif_OpenFull",
        "Admin_Verif_OptionalNotesLabel",
        "Admin_Verif_PageTitle",
        "Admin_Verif_PaymentDirect",
        "Admin_Verif_PointsSuffix",
        "Admin_Verif_ProcessedOn",
        "Admin_Verif_ProfileUrlLabel",
        "Admin_Verif_RefreshList",
        "Admin_Verif_RejectRefundBtn",
        "Admin_Verif_ReviewStatusLabel",
        "Admin_Verif_StatusApproved",
        "Admin_Verif_StatusPending",
        "Admin_Verif_StatusRejected",
        "Admin_Verif_SubmissionDateLabel",
        "Admin_Verif_Subtitle",
        "Admin_Verif_TabAll",
        "Admin_Verif_TabApproved",
        "Admin_Verif_TabPending",
        "Admin_Verif_TabRejected",
        "Admin_Verif_TierDomainPro",
        "Admin_Verif_TierVIP",
        "Admin_Verif_Toast_ApproveFailed",
        "Admin_Verif_Toast_ApproveSuccess",
        "Admin_Verif_Toast_FetchFailed",
        "Admin_Verif_Toast_GenericError",
        "Admin_Verif_Toast_RejectFailed",
        "Admin_Verif_Toast_RejectNotesRequired",
        "Admin_Verif_Toast_RejectSuccess",
        "Admin_Verif_UsernameLabel",
        "Admin_Verif_WorkEmailLabel",
    ];

    private static readonly string[] PlansKeys =
    [
        "Admin_Plans_ActiveCheckboxLabel",
        "Admin_Plans_BadgeDomainPro",
        "Admin_Plans_BadgePublicFigure",
        "Admin_Plans_BillingIntervalLabel",
        "Admin_Plans_BtnActivate",
        "Admin_Plans_BtnCreatePlan",
        "Admin_Plans_BtnDeactivate",
        "Admin_Plans_BtnEdit",
        "Admin_Plans_BtnSaveChanges",
        "Admin_Plans_BtnSaving",
        "Admin_Plans_CreateNewPlan",
        "Admin_Plans_CreatePlanNow",
        "Admin_Plans_DropsPerInterval",
        "Admin_Plans_DurationDaysLabel",
        "Admin_Plans_EmptyDesc",
        "Admin_Plans_EmptyTitle",
        "Admin_Plans_Heading",
        "Admin_Plans_IntervalInfo",
        "Admin_Plans_LinkDropCostHint",
        "Admin_Plans_LinkDropCostLabel",
        "Admin_Plans_Loading",
        "Admin_Plans_ModalCreateTitle",
        "Admin_Plans_ModalEditTitle",
        "Admin_Plans_NoPerks",
        "Admin_Plans_OptAnnual",
        "Admin_Plans_OptCustom",
        "Admin_Plans_OptMonthly",
        "Admin_Plans_OptQuarterly",
        "Admin_Plans_OptSemiAnnual",
        "Admin_Plans_PageTitle",
        "Admin_Plans_PerksHeading",
        "Admin_Plans_PerksInputLabel",
        "Admin_Plans_PerksInputPlaceholder",
        "Admin_Plans_PlanNameLabel",
        "Admin_Plans_PlanNamePlaceholder",
        "Admin_Plans_ReferenceFiatHint",
        "Admin_Plans_ReferenceFiatLabel",
        "Admin_Plans_ReferenceMmk",
        "Admin_Plans_RoleDomainProDesc",
        "Admin_Plans_RoleDomainProTitle",
        "Admin_Plans_RolePublicFigureDesc",
        "Admin_Plans_RolePublicFigureTitle",
        "Admin_Plans_StatusActive",
        "Admin_Plans_StatusInactive",
        "Admin_Plans_Subtitle",
        "Admin_Plans_TabAll",
        "Admin_Plans_TabDomainPro",
        "Admin_Plans_TabPublicFigure",
        "Admin_Plans_TargetTierLabel",
        "Admin_Plans_Toast_CreateFailed",
        "Admin_Plans_Toast_CreateSuccess",
        "Admin_Plans_Toast_DeleteFailed",
        "Admin_Plans_Toast_DeleteSuccess",
        "Admin_Plans_Toast_LoadFailed",
        "Admin_Plans_Toast_StatusFailed",
        "Admin_Plans_Toast_StatusNow",
        "Admin_Plans_Toast_UpdateFailed",
        "Admin_Plans_Toast_UpdateSuccess",
        "Admin_Plans_TooltipDelete",
        "Admin_Plans_TooltipEdit",
        "Admin_Plans_Val_CostPositive",
        "Admin_Plans_Val_NameRequired",
    ];

    private static readonly string[] LinkDropsKeys =
    [
        "Admin_LinkDrops_AccountHolderLabel",
        "Admin_LinkDrops_AccountHolderPlaceholder",
        "Admin_LinkDrops_AccountNumberLabel",
        "Admin_LinkDrops_AccountNumberPlaceholder",
        "Admin_LinkDrops_ActiveCheckboxLabel",
        "Admin_LinkDrops_AddMethodHeading",
        "Admin_LinkDrops_AllReviewed",
        "Admin_LinkDrops_ApproveModalDesc",
        "Admin_LinkDrops_ApproveModalTitle",
        "Admin_LinkDrops_ApproveNotesPlaceholder",
        "Admin_LinkDrops_BadgeAdminPortal",
        "Admin_LinkDrops_BonusDropsLabel",
        "Admin_LinkDrops_BonusDropsPlaceholder",
        "Admin_LinkDrops_BtnAddChannel",
        "Admin_LinkDrops_BtnAddPackage",
        "Admin_LinkDrops_BtnApprove",
        "Admin_LinkDrops_BtnConfirmCredit",
        "Admin_LinkDrops_BtnConfirmReject",
        "Admin_LinkDrops_BtnDisable",
        "Admin_LinkDrops_BtnEnable",
        "Admin_LinkDrops_BtnReject",
        "Admin_LinkDrops_BtnSaveChannel",
        "Admin_LinkDrops_BtnSavePackage",
        "Admin_LinkDrops_BtnSaveRates",
        "Admin_LinkDrops_BtnViewQr",
        "Admin_LinkDrops_BtnViewReceipt",
        "Admin_LinkDrops_ChannelNameLabel",
        "Admin_LinkDrops_ChannelNamePlaceholder",
        "Admin_LinkDrops_ColAccountHolder",
        "Admin_LinkDrops_ColAccountNumber",
        "Admin_LinkDrops_ColActions",
        "Admin_LinkDrops_ColBonus",
        "Admin_LinkDrops_ColBuyer",
        "Admin_LinkDrops_ColChannelLogo",
        "Admin_LinkDrops_ColChannelRef",
        "Admin_LinkDrops_ColDrops",
        "Admin_LinkDrops_ColDropsGranted",
        "Admin_LinkDrops_ColEdit",
        "Admin_LinkDrops_ColPackage",
        "Admin_LinkDrops_ColPackageType",
        "Admin_LinkDrops_ColPrice",
        "Admin_LinkDrops_ColProofReceipt",
        "Admin_LinkDrops_ColPurchaseNo",
        "Admin_LinkDrops_ColQrCode",
        "Admin_LinkDrops_ColRealMoney",
        "Admin_LinkDrops_ColStatus",
        "Admin_LinkDrops_ColStatusAction",
        "Admin_LinkDrops_CountPendingReview",
        "Admin_LinkDrops_CreatePkgHeading",
        "Admin_LinkDrops_CurrentLogo",
        "Admin_LinkDrops_CurrentQr",
        "Admin_LinkDrops_CustomPurchase",
        "Admin_LinkDrops_DescriptionLabel",
        "Admin_LinkDrops_DescriptionPlaceholder",
        "Admin_LinkDrops_DisplayOrderLabel",
        "Admin_LinkDrops_DropsAmountLabel",
        "Admin_LinkDrops_DropsAmountPlaceholder",
        "Admin_LinkDrops_EditChannelTitle",
        "Admin_LinkDrops_EditPkgTitle",
        "Admin_LinkDrops_ExampleCny",
        "Admin_LinkDrops_ExampleMmk",
        "Admin_LinkDrops_ExampleUsd",
        "Admin_LinkDrops_Heading",
        "Admin_LinkDrops_InstructionsLabel",
        "Admin_LinkDrops_InstructionsPlaceholder",
        "Admin_LinkDrops_International",
        "Admin_LinkDrops_LoadingPending",
        "Admin_LinkDrops_LoadingRates",
        "Admin_LinkDrops_LogoLabel",
        "Admin_LinkDrops_NoImage",
        "Admin_LinkDrops_None",
        "Admin_LinkDrops_PageTitle",
        "Admin_LinkDrops_PkgNameLabel",
        "Admin_LinkDrops_PkgNamePlaceholder",
        "Admin_LinkDrops_PriceMmkLabel",
        "Admin_LinkDrops_PriceMmkPlaceholder",
        "Admin_LinkDrops_PrimaryCurrency",
        "Admin_LinkDrops_ProofModalTitle",
        "Admin_LinkDrops_QrCodeLabel",
        "Admin_LinkDrops_QueueHeading",
        "Admin_LinkDrops_RateCnyLabel",
        "Admin_LinkDrops_RateMmkLabel",
        "Admin_LinkDrops_RateUsdLabel",
        "Admin_LinkDrops_RatesDesc",
        "Admin_LinkDrops_RatesHeading",
        "Admin_LinkDrops_RatesInstantNotice",
        "Admin_LinkDrops_RefPrefix",
        "Admin_LinkDrops_Regional",
        "Admin_LinkDrops_RejectModalDesc",
        "Admin_LinkDrops_RejectModalTitle",
        "Admin_LinkDrops_RejectReasonPlaceholder",
        "Admin_LinkDrops_StatusActive",
        "Admin_LinkDrops_StatusDisabled",
        "Admin_LinkDrops_Subtitle",
        "Admin_LinkDrops_TabMethods",
        "Admin_LinkDrops_TabPackages",
        "Admin_LinkDrops_TabPending",
        "Admin_LinkDrops_TabRates",
        "Admin_LinkDrops_Toast_ApproveFailed",
        "Admin_LinkDrops_Toast_ApproveSuccess",
        "Admin_LinkDrops_Toast_MethodCreateFailed",
        "Admin_LinkDrops_Toast_MethodCreated",
        "Admin_LinkDrops_Toast_MethodStatusUpdated",
        "Admin_LinkDrops_Toast_MethodUpdateFailed",
        "Admin_LinkDrops_Toast_MethodUpdated",
        "Admin_LinkDrops_Toast_MmkPositive",
        "Admin_LinkDrops_Toast_PkgCreated",
        "Admin_LinkDrops_Toast_PkgStatusUpdated",
        "Admin_LinkDrops_Toast_PkgUpdateFailed",
        "Admin_LinkDrops_Toast_PkgUpdated",
        "Admin_LinkDrops_Toast_RatesFailed",
        "Admin_LinkDrops_Toast_RatesSuccess",
        "Admin_LinkDrops_Toast_RejectFailed",
        "Admin_LinkDrops_Toast_RejectReasonRequired",
        "Admin_LinkDrops_Toast_RejectSuccess",
        "Admin_LinkDrops_TooltipRefresh",
    ];

    private static readonly string[] PayoutsBonusKeys =
    [
        "Admin_Payouts_Toast_UnexpectedError",
    ];

    private static ServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AdminPartB_EnglishCulture_ResolvesAllKeys()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            var allKeys = VerificationsKeys
                .Concat(PlansKeys)
                .Concat(LinkDropsKeys)
                .Concat(PayoutsBonusKeys);

            foreach (var key in allKeys)
            {
                var localizedString = localizer[key];
                Assert.False(localizedString.ResourceNotFound, $"Key '{key}' was not found in English resources.");
                Assert.False(string.IsNullOrWhiteSpace(localizedString.Value), $"Key '{key}' had empty value in English resources.");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminPartB_MyanmarCulture_ResolvesAllKeys()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = VerificationsKeys
                .Concat(PlansKeys)
                .Concat(LinkDropsKeys)
                .Concat(PayoutsBonusKeys);

            foreach (var key in allKeys)
            {
                var localizedString = localizer[key];
                Assert.False(localizedString.ResourceNotFound, $"Key '{key}' was not found in Myanmar resources.");
                Assert.False(string.IsNullOrWhiteSpace(localizedString.Value), $"Key '{key}' had empty value in Myanmar resources.");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminPartB_PlaceholderSymmetry_MatchesBetweenCultures()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var allKeys = VerificationsKeys
                .Concat(PlansKeys)
                .Concat(LinkDropsKeys)
                .Concat(PayoutsBonusKeys);

            foreach (var key in allKeys)
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                var enValue = localizer[key].Value;

                CultureInfo.CurrentCulture = new CultureInfo("my");
                CultureInfo.CurrentUICulture = new CultureInfo("my");
                var myValue = localizer[key].Value;

                for (int i = 0; i < 5; i++)
                {
                    var token = "{" + i + "}";
                    bool enHas = enValue.Contains(token);
                    bool myHas = myValue.Contains(token);
                    Assert.True(enHas == myHas,
                        $"Placeholder mismatch for '{key}': token '{token}' in EN: {enHas}, in MY: {myHas}");
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminPartB_FormatWithParameters_InterpolatesCorrectly()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var enVerifTab = localizer["Admin_Verif_TabPending", 5].Value;
            Assert.Contains("5", enVerifTab);

            var enPlansTab = localizer["Admin_Plans_TabDomainPro", 12].Value;
            Assert.Contains("12", enPlansTab);

            var enLinkDrops = localizer["Admin_LinkDrops_TabPending", 7].Value;
            Assert.Contains("7", enLinkDrops);

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myVerifTab = localizer["Admin_Verif_TabPending", 5].Value;
            Assert.Contains("5", myVerifTab);

            var myPlansTab = localizer["Admin_Plans_TabDomainPro", 12].Value;
            Assert.Contains("12", myPlansTab);

            var myLinkDrops = localizer["Admin_LinkDrops_TabPending", 7].Value;
            Assert.Contains("7", myLinkDrops);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminPartB_MyanmarTranslations_ContainNoZawgyiPuaCharacters()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = VerificationsKeys
                .Concat(PlansKeys)
                .Concat(LinkDropsKeys)
                .Concat(PayoutsBonusKeys);

            foreach (var key in allKeys)
            {
                var value = localizer[key].Value;
                foreach (var ch in value)
                {
                    int codePoint = ch;
                    Assert.False(codePoint >= 0xE000 && codePoint <= 0xF8FF,
                        $"Key '{key}' contains a Zawgyi PUA character: 0x{codePoint:X4} in value '{value}'");
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
