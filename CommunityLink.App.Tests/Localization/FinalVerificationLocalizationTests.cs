using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class FinalVerificationLocalizationTests
{
    private static readonly string[] ActivityKeys =
    [
        "Activity_ActiveFiltersApplied",
        "Activity_BtnClearFilters",
        "Activity_BtnDelete",
        "Activity_BtnDeleteRecord",
        "Activity_BtnDeleting",
        "Activity_BtnRefresh",
        "Activity_BtnResetFilters",
        "Activity_DeleteModalDesc",
        "Activity_DeleteModalNote",
        "Activity_DeleteModalTitle",
        "Activity_Heading",
        "Activity_Loading",
        "Activity_NoActivitiesDesc",
        "Activity_NoActivitiesTitle",
        "Activity_NoMatchesDesc",
        "Activity_NoMatchesTitle",
        "Activity_OptAll",
        "Activity_OptComment",
        "Activity_OptGroupCreate",
        "Activity_OptGroupJoinReview",
        "Activity_OptLike",
        "Activity_OptOther",
        "Activity_OptPost",
        "Activity_OptShare",
        "Activity_PageTitle",
        "Activity_SearchPlaceholder",
        "Activity_ShowingCount",
        "Activity_Subtitle",
        "Activity_Toast_DeleteFailed",
        "Activity_Toast_Deleted",
        "Activity_TooltipDelete",
    ];

    private static readonly string[] CommunityReportKeys =
    [
        "Report_Comm_BadgeGroups",
        "Report_Comm_BadgeSubComm",
        "Report_Comm_BtnRefresh",
        "Report_Comm_EmptyDesc",
        "Report_Comm_EmptyTitle",
        "Report_Comm_GroupsCount",
        "Report_Comm_Heading",
        "Report_Comm_Loading",
        "Report_Comm_MemCountShort",
        "Report_Comm_MembersCount",
        "Report_Comm_NoSubComm",
        "Report_Comm_PageTitle",
        "Report_Comm_StatTotalComm",
        "Report_Comm_StatTotalGroups",
        "Report_Comm_StatTotalSubComm",
        "Report_Comm_SubCommHeading",
        "Report_Comm_Subtitle",
    ];

    private static readonly string[] UserReportKeys =
    [
        "Report_User_BadgePublicPost",
        "Report_User_BtnRefresh",
        "Report_User_EmptyUsers",
        "Report_User_FollowersLabel",
        "Report_User_Heading",
        "Report_User_ItemsCount",
        "Report_User_JoinedDate",
        "Report_User_LoadingContent",
        "Report_User_LoadingUsers",
        "Report_User_NoPollsYet",
        "Report_User_NoPostsYet",
        "Report_User_PageTitle",
        "Report_User_PollsCount",
        "Report_User_PrefixCommunity",
        "Report_User_PrefixGroup",
        "Report_User_RankedHeading",
        "Report_User_SelectPromptDesc",
        "Report_User_SelectPromptTitle",
        "Report_User_StatFollowers",
        "Report_User_StatPosts",
        "Report_User_Subtitle",
        "Report_User_TopPollsHeading",
        "Report_User_TopPostsHeading",
        "Report_User_TotalVotes",
        "Report_User_UsersCount",
    ];

    private static readonly string[] RbacKeys =
    [
        "Admin_Rbac_BadgeAllGranted",
        "Admin_Rbac_BadgeFixedPerms",
        "Admin_Rbac_BadgeSystem",
        "Admin_Rbac_BtnAddRole",
        "Admin_Rbac_BtnBack",
        "Admin_Rbac_BtnConfirmCreate",
        "Admin_Rbac_BtnConfirmDelete",
        "Admin_Rbac_BtnContinue",
        "Admin_Rbac_BtnCreating",
        "Admin_Rbac_BtnDeleteRole",
        "Admin_Rbac_BtnDeleting",
        "Admin_Rbac_BtnDeselectAll",
        "Admin_Rbac_BtnSaveChanges",
        "Admin_Rbac_BtnSaving",
        "Admin_Rbac_BtnSelectAll",
        "Admin_Rbac_ColAll",
        "Admin_Rbac_ColModule",
        "Admin_Rbac_ColName",
        "Admin_Rbac_ColNo",
        "Admin_Rbac_ColPermCode",
        "Admin_Rbac_DeleteNotice",
        "Admin_Rbac_GrantedCount",
        "Admin_Rbac_Heading",
        "Admin_Rbac_LabelCode",
        "Admin_Rbac_LabelName",
        "Admin_Rbac_LoadingMatrix",
        "Admin_Rbac_ModalCreateTitle",
        "Admin_Rbac_PageTitle",
        "Admin_Rbac_PermissionsFor",
        "Admin_Rbac_PromptCreateTitle",
        "Admin_Rbac_PromptDeleteTitle",
        "Admin_Rbac_RoleCodeHint",
        "Admin_Rbac_RoleCodeLabel",
        "Admin_Rbac_RoleCodePlaceholder",
        "Admin_Rbac_RoleDescLabel",
        "Admin_Rbac_RoleDescPlaceholder",
        "Admin_Rbac_RoleNameHint",
        "Admin_Rbac_RoleNameLabel",
        "Admin_Rbac_RoleNamePlaceholder",
        "Admin_Rbac_RoleNotice",
        "Admin_Rbac_RoleToRemove",
        "Admin_Rbac_RolesCount",
        "Admin_Rbac_SelectRoleHeading",
        "Admin_Rbac_Subtitle",
        "Admin_Rbac_Toast_AdminPermPermanent",
        "Admin_Rbac_Toast_CreateFailed",
        "Admin_Rbac_Toast_Created",
        "Admin_Rbac_Toast_DeleteFailed",
        "Admin_Rbac_Toast_RoleDeleted",
        "Admin_Rbac_Toast_SaveFailed",
        "Admin_Rbac_Toast_Saved",
        "Admin_Rbac_Toast_SystemRoleCantDelete",
        "Admin_Rbac_TooltipDeleteRole",
        "Admin_Rbac_UserCountBadge",
        "Admin_Rbac_Val_CodeExists",
        "Admin_Rbac_Val_CodeRequired",
        "Admin_Rbac_Val_DescExists",
        "Admin_Rbac_Val_DescRequired",
        "Admin_Rbac_Val_NameExists",
        "Admin_Rbac_Val_NameRequired",
    ];

    private static readonly string[] PolishKeys =
    [
        "Auth_Register_FullNamePlaceholder",
        "Auth_Register_UsernamePlaceholder",
        "Chat_Redirect_Opening",
        "Profile_Error_UnableToLoad",
        "Profile_Loading_ExecutiveDossier",
        "Profile_Loading_Synchronizing",
    ];

    private static ServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Step7_EnglishCulture_ResolvesAllKeys()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            var allKeys = ActivityKeys
                .Concat(CommunityReportKeys)
                .Concat(UserReportKeys)
                .Concat(RbacKeys)
                .Concat(PolishKeys);

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
    public void Step7_MyanmarCulture_ResolvesAllKeys()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = ActivityKeys
                .Concat(CommunityReportKeys)
                .Concat(UserReportKeys)
                .Concat(RbacKeys)
                .Concat(PolishKeys);

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
    public void Step7_PlaceholderSymmetry_MatchesBetweenCultures()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var allKeys = ActivityKeys
                .Concat(CommunityReportKeys)
                .Concat(UserReportKeys)
                .Concat(RbacKeys)
                .Concat(PolishKeys);

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
    public void Step7_FormatWithParameters_InterpolatesCorrectly()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var enActCount = localizer["Activity_ShowingCount", 5, 20].Value;
            Assert.Contains("5", enActCount);
            Assert.Contains("20", enActCount);

            var enCommMem = localizer["Report_Comm_MembersCount", 142].Value;
            Assert.Contains("142", enCommMem);

            var enUserFollow = localizer["Report_User_JoinedDate", "Oct 2026"].Value;
            Assert.Contains("Oct 2026", enUserFollow);

            var enRbacGranted = localizer["Admin_Rbac_GrantedCount", 10, 35].Value;
            Assert.Contains("10", enRbacGranted);

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myActCount = localizer["Activity_ShowingCount", 5, 20].Value;
            Assert.Contains("5", myActCount);
            Assert.Contains("20", myActCount);

            var myCommMem = localizer["Report_Comm_MembersCount", 142].Value;
            Assert.Contains("142", myCommMem);

            var myUserFollow = localizer["Report_User_JoinedDate", "Oct 2026"].Value;
            Assert.Contains("Oct 2026", myUserFollow);

            var myRbacGranted = localizer["Admin_Rbac_GrantedCount", 10, 35].Value;
            Assert.Contains("10", myRbacGranted);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void Step7_MyanmarTranslations_ContainNoZawgyiPuaCharacters()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = ActivityKeys
                .Concat(CommunityReportKeys)
                .Concat(UserReportKeys)
                .Concat(RbacKeys)
                .Concat(PolishKeys);

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
