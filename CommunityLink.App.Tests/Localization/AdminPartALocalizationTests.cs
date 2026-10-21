using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class AdminPartALocalizationTests
{
    private static readonly string[] ContentModerationKeys =
    [
        "Admin_Mod_ActionApprove",
        "Admin_Mod_ActionBan",
        "Admin_Mod_ActionGeneric",
        "Admin_Mod_ActionLabel",
        "Admin_Mod_ActionMakeActive",
        "Admin_Mod_ActionMakeInactive",
        "Admin_Mod_ActionNoticeDesc",
        "Admin_Mod_ActionUnban",
        "Admin_Mod_ActivateGroup",
        "Admin_Mod_AndSearch",
        "Admin_Mod_ApplyModerationBtn",
        "Admin_Mod_AttachedFile",
        "Admin_Mod_AttachedFileCode",
        "Admin_Mod_AttachedImagesCount",
        "Admin_Mod_AuthorLabel",
        "Admin_Mod_BadgePrivateMode",
        "Admin_Mod_BanContent",
        "Admin_Mod_BanContentType",
        "Admin_Mod_BanReasonLabel",
        "Admin_Mod_ConfirmModalTitle",
        "Admin_Mod_ContentLabel",
        "Admin_Mod_ContentTypeDetails",
        "Admin_Mod_CountBanned",
        "Admin_Mod_CountInactive",
        "Admin_Mod_CreatorLabel",
        "Admin_Mod_CustomReasonPlaceholder",
        "Admin_Mod_CustomSetupNoteLabel",
        "Admin_Mod_DeactivateGroup",
        "Admin_Mod_DeactivationNoticeLabel",
        "Admin_Mod_DismissReport",
        "Admin_Mod_FilterActive",
        "Admin_Mod_FilterAll",
        "Admin_Mod_FilterBanned",
        "Admin_Mod_FilterInactive",
        "Admin_Mod_FilterStatusLabel",
        "Admin_Mod_GroupLabel",
        "Admin_Mod_HeadingGroups",
        "Admin_Mod_HeadingPosts",
        "Admin_Mod_HubLabel",
        "Admin_Mod_ImageCountPlural",
        "Admin_Mod_ImageCountSingular",
        "Admin_Mod_InSubCommunity",
        "Admin_Mod_LoadingQueue",
        "Admin_Mod_ModerationNoticeLabel",
        "Admin_Mod_NoContentFound",
        "Admin_Mod_NoGroupsFound",
        "Admin_Mod_PageTitleGroup",
        "Admin_Mod_PageTitlePost",
        "Admin_Mod_PollQuestionLabel",
        "Admin_Mod_PostCount",
        "Admin_Mod_PostedAt",
        "Admin_Mod_QueueClearDescGroup",
        "Admin_Mod_QueueClearDescPost",
        "Admin_Mod_QueueClearTitle",
        "Admin_Mod_ReasonHarassment",
        "Admin_Mod_ReasonMisinformation",
        "Admin_Mod_ReasonMisleading",
        "Admin_Mod_ReasonSpam",
        "Admin_Mod_RefreshFeed",
        "Admin_Mod_RemoveContent",
        "Admin_Mod_ReportCountPlural",
        "Admin_Mod_ReportCountSingular",
        "Admin_Mod_ReportedBy",
        "Admin_Mod_RestoreContent",
        "Admin_Mod_SearchGroupsPlaceholder",
        "Admin_Mod_SearchPostsPlaceholder",
        "Admin_Mod_SelectPresetReasonLabel",
        "Admin_Mod_StatusActive",
        "Admin_Mod_StatusBanned",
        "Admin_Mod_StatusInactivePostingLocked",
        "Admin_Mod_SubtitleGroups",
        "Admin_Mod_SubtitlePosts",
        "Admin_Mod_TabGroupsList",
        "Admin_Mod_TabPendingReports",
        "Admin_Mod_TabPostsPollsList",
        "Admin_Mod_Toast_ActionApplied",
        "Admin_Mod_Toast_ActionFailed",
        "Admin_Mod_Toast_ActionUpdated",
        "Admin_Mod_Toast_DismissFailed",
        "Admin_Mod_Toast_GenericError",
        "Admin_Mod_Toast_LoadQueueError",
        "Admin_Mod_Toast_ModerationFailed",
        "Admin_Mod_Toast_ReportDismissed",
        "Admin_Mod_TooltipBanPost",
        "Admin_Mod_TooltipBanPostOnlyAuthorAdmin",
        "Admin_Mod_TooltipMakeGroupInactive",
        "Admin_Mod_TooltipReactivateGroup",
        "Admin_Mod_TooltipSetGroupInactive",
        "Admin_Mod_TooltipUnbanPost",
        "Admin_Mod_UnbanContentType",
        "Admin_Mod_ViewGroupBtn",
        "Admin_Mod_ViewPoll",
        "Admin_Mod_ViewPost",
    ];

    private static readonly string[] FinanceKeys =
    [
        "Admin_Finance_AccessDenied",
        "Admin_Finance_AccessDeniedDesc",
        "Admin_Finance_ActiveGroupFilter",
        "Admin_Finance_ActivePeriodFilter",
        "Admin_Finance_AllStatuses",
        "Admin_Finance_AllTxTypes",
        "Admin_Finance_ApplyFilter",
        "Admin_Finance_BadgeFiltered",
        "Admin_Finance_ClearFilter",
        "Admin_Finance_ClearFilters",
        "Admin_Finance_ClearGroupFilterBtn",
        "Admin_Finance_ClearStatus",
        "Admin_Finance_ColChatGroup",
        "Admin_Finance_ColCommission",
        "Admin_Finance_ColCreator",
        "Admin_Finance_ColCreatorNet",
        "Admin_Finance_ColDate",
        "Admin_Finance_ColGross",
        "Admin_Finance_ColGrossRevenue",
        "Admin_Finance_ColNet",
        "Admin_Finance_ColPaidJoins",
        "Admin_Finance_ColStatus",
        "Admin_Finance_ColType",
        "Admin_Finance_ColUnlockedChats",
        "Admin_Finance_ColUserCreator",
        "Admin_Finance_CompletedCashOuts",
        "Admin_Finance_CreatorEarnings",
        "Admin_Finance_CreatorPayouts",
        "Admin_Finance_CreditedToCreators",
        "Admin_Finance_DateFilterHeading",
        "Admin_Finance_ErrorTitle",
        "Admin_Finance_ExportCsv",
        "Admin_Finance_ExportCsvFailed",
        "Admin_Finance_Exporting",
        "Admin_Finance_FirstPage",
        "Admin_Finance_FromDate",
        "Admin_Finance_GroupJoins",
        "Admin_Finance_Heading",
        "Admin_Finance_LastPage",
        "Admin_Finance_LoadingSummary",
        "Admin_Finance_MobileCommissionLabel",
        "Admin_Finance_MobileGrossLabel",
        "Admin_Finance_MobileGroupLabel",
        "Admin_Finance_MobileNetLabel",
        "Admin_Finance_NetPlatformEarnings",
        "Admin_Finance_NextPage",
        "Admin_Finance_NoGroupData",
        "Admin_Finance_NoPendingPayouts",
        "Admin_Finance_NoPrivateChatData",
        "Admin_Finance_NoRecords",
        "Admin_Finance_NoTxFound",
        "Admin_Finance_NoTxFoundDesc",
        "Admin_Finance_OptCreatorPayout",
        "Admin_Finance_OptGroupJoin",
        "Admin_Finance_OptPrivateChat",
        "Admin_Finance_PageNumber",
        "Admin_Finance_PageTitle",
        "Admin_Finance_PendingReview",
        "Admin_Finance_PlatformCut",
        "Admin_Finance_PrevPage",
        "Admin_Finance_PrivateChats",
        "Admin_Finance_RefreshData",
        "Admin_Finance_Reset",
        "Admin_Finance_RowsPerPage",
        "Admin_Finance_ShowingAllTime",
        "Admin_Finance_ShowingRange",
        "Admin_Finance_StatusCompleted",
        "Admin_Finance_StatusPending",
        "Admin_Finance_StatusRejected",
        "Admin_Finance_StreamBreakdownDesc",
        "Admin_Finance_StreamBreakdownTitle",
        "Admin_Finance_Subtitle",
        "Admin_Finance_TabChatGroups",
        "Admin_Finance_TabPrivateChats",
        "Admin_Finance_ToDate",
        "Admin_Finance_Toast_ExportFailed",
        "Admin_Finance_Toast_ExportNoData",
        "Admin_Finance_Toast_ExportSuccess",
        "Admin_Finance_TotalRevenue",
        "Admin_Finance_TxHistorySubtitle",
        "Admin_Finance_TxHistoryTitle",
        "Admin_Finance_TypeCreatorPayout",
        "Admin_Finance_TypeGroupJoin",
        "Admin_Finance_TypePrivateChat",
    ];

    private static readonly string[] AccountsKeys =
    [
        "Admin_Accounts_Activate",
        "Admin_Accounts_Active",
        "Admin_Accounts_AdminCount",
        "Admin_Accounts_Close",
        "Admin_Accounts_ColActions",
        "Admin_Accounts_ColAdminType",
        "Admin_Accounts_ColAdministrator",
        "Admin_Accounts_ColEmail",
        "Admin_Accounts_ColId",
        "Admin_Accounts_ColLastLogin",
        "Admin_Accounts_ColProvisionedDate",
        "Admin_Accounts_ColStatus",
        "Admin_Accounts_Copied",
        "Admin_Accounts_CopyLink",
        "Admin_Accounts_CreateAccountBtn",
        "Admin_Accounts_Deactivate",
        "Admin_Accounts_Deactivated",
        "Admin_Accounts_DispatchingLink",
        "Admin_Accounts_Heading",
        "Admin_Accounts_LastLoginNever",
        "Admin_Accounts_LinkDispatched",
        "Admin_Accounts_LinkNotice",
        "Admin_Accounts_Loading",
        "Admin_Accounts_ModalEmailLabel",
        "Admin_Accounts_ModalEmailPlaceholder",
        "Admin_Accounts_ModalSubtitle",
        "Admin_Accounts_ModalSuperTitle",
        "Admin_Accounts_ModalTitle",
        "Admin_Accounts_ModalTypeLabel",
        "Admin_Accounts_NoAdmins",
        "Admin_Accounts_NoticeDesc",
        "Admin_Accounts_NoticeTitle",
        "Admin_Accounts_PageTitle",
        "Admin_Accounts_Protected",
        "Admin_Accounts_ReadOnly",
        "Admin_Accounts_ReadOnlyTitle",
        "Admin_Accounts_RoleAdmin",
        "Admin_Accounts_RoleSuperAdmin",
        "Admin_Accounts_SearchPlaceholder",
        "Admin_Accounts_SendSetupLink",
        "Admin_Accounts_Subtitle",
        "Admin_Accounts_Toast_InviteCreated",
        "Admin_Accounts_Toast_LinkCopied",
        "Admin_Accounts_TypeAdminDesc",
        "Admin_Accounts_TypeSuperAdminDesc",
        "Admin_Accounts_Val_EmailRequired",
    ];

    private static readonly string[] AuditLogsKeys =
    [
        "Admin_AuditLogs_ActorTypeUser",
        "Admin_AuditLogs_ColAction",
        "Admin_AuditLogs_ColChangedColumns",
        "Admin_AuditLogs_ColModifiedBy",
        "Admin_AuditLogs_ColNewValue",
        "Admin_AuditLogs_ColNo",
        "Admin_AuditLogs_ColOldValue",
        "Admin_AuditLogs_ColType",
        "Admin_AuditLogs_ColUpdatedAt",
        "Admin_AuditLogs_Heading",
        "Admin_AuditLogs_Loading",
        "Admin_AuditLogs_Next",
        "Admin_AuditLogs_NoLogs",
        "Admin_AuditLogs_PageNumber",
        "Admin_AuditLogs_PageStats",
        "Admin_AuditLogs_PageTitle",
        "Admin_AuditLogs_Prev",
        "Admin_AuditLogs_SearchPlaceholder",
        "Admin_AuditLogs_Subtitle",
        "Admin_AuditLogs_SystemUser",
    ];

    private static readonly string[] UsersKeys =
    [
        "Admin_Users_AccountEmail",
        "Admin_Users_ActivateBtn",
        "Admin_Users_ActivateConfirmPrompt",
        "Admin_Users_ActivateModalTitle",
        "Admin_Users_ActivateTitle",
        "Admin_Users_ColActions",
        "Admin_Users_ColAssignedRole",
        "Admin_Users_ColDisplayName",
        "Admin_Users_ColEmail",
        "Admin_Users_ColNo",
        "Admin_Users_ColRegisteredDate",
        "Admin_Users_ColStatus",
        "Admin_Users_ColUsername",
        "Admin_Users_ConfirmActivateBtn",
        "Admin_Users_ConfirmDeactivateBtn",
        "Admin_Users_ConfirmRoleBtn",
        "Admin_Users_ConfirmRoleDesc",
        "Admin_Users_ConfirmRoleTitle",
        "Admin_Users_CurrentRole",
        "Admin_Users_CurrentStatus",
        "Admin_Users_DeactivateBtn",
        "Admin_Users_DeactivateConfirmPrompt",
        "Admin_Users_DeactivateModalTitle",
        "Admin_Users_DeactivateTitle",
        "Admin_Users_Heading",
        "Admin_Users_LoadingRecords",
        "Admin_Users_NewRole",
        "Admin_Users_NoRecords",
        "Admin_Users_NoUsersFound",
        "Admin_Users_PageTitle",
        "Admin_Users_PaginationFirst",
        "Admin_Users_PaginationLast",
        "Admin_Users_PaginationNext",
        "Admin_Users_PaginationPageOf",
        "Admin_Users_PaginationPrev",
        "Admin_Users_RoleAdmin",
        "Admin_Users_RoleAll",
        "Admin_Users_RoleCreator",
        "Admin_Users_RoleDefault",
        "Admin_Users_RoleUpdatedSuccess",
        "Admin_Users_RoleUser",
        "Admin_Users_RowsPerPageLabel",
        "Admin_Users_SearchPlaceholder",
        "Admin_Users_ShowingRange",
        "Admin_Users_StatusActive",
        "Admin_Users_StatusActiveBadge",
        "Admin_Users_StatusAll",
        "Admin_Users_StatusDeactivated",
        "Admin_Users_StatusDeactivatedBadge",
        "Admin_Users_StatusInactiveBadge",
        "Admin_Users_Subtitle",
        "Admin_Users_TargetStatus",
        "Admin_Users_Updating",
    ];

    private static ServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AdminContentModeration_AllKeys_ExistInBothCultures_AndAreNonEmpty()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in ContentModerationKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Content Moderation key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Content Moderation key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in ContentModerationKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Content Moderation key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Content Moderation key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminFinance_AllKeys_ExistInBothCultures_AndAreNonEmpty()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in FinanceKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Finance key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Finance key '{key}' is empty in English AppResources.resx");
            }

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in FinanceKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Finance key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Finance key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminAccounts_AllKeys_ExistInBothCultures_AndAreNonEmpty()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in AccountsKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Accounts key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Accounts key '{key}' is empty in English AppResources.resx");
            }

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in AccountsKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Accounts key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Accounts key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminAuditLogs_And_AdminUsers_AllKeys_ExistInBothCultures_AndAreNonEmpty()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var keys = AuditLogsKeys.Concat(UsersKeys);

            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in keys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"AuditLogs/Users key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"AuditLogs/Users key '{key}' is empty in English AppResources.resx");
            }

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in keys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"AuditLogs/Users key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"AuditLogs/Users key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminPartA_WithPlaceholders_FormatSymmetrically()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enCount = localizer["Admin_Mod_ReportCountSingular", 1].Value;
            Assert.Contains("1", enCount);

            var enToast = localizer["Admin_Mod_Toast_ActionApplied", "BAN"].Value;
            Assert.Contains("BAN", enToast);

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myCount = localizer["Admin_Mod_ReportCountSingular", 1].Value;
            Assert.Contains("1", myCount);

            var myToast = localizer["Admin_Mod_Toast_ActionApplied", "BAN"].Value;
            Assert.Contains("BAN", myToast);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AdminPartA_MyanmarTranslations_ContainNoZawgyiPuaCharacters()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = ContentModerationKeys
                .Concat(FinanceKeys)
                .Concat(AccountsKeys)
                .Concat(AuditLogsKeys)
                .Concat(UsersKeys);

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
