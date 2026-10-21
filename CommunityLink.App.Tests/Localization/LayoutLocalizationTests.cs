using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class LayoutLocalizationTests
{
    private static readonly string[] NavigationKeys =
    [
        "Nav_Home",
        "Nav_Communities",
        "Nav_Groups",
        "Nav_Chat",
        "Nav_Wallet",
        "Nav_Activity",
        "Nav_CreatorEarnings",
        "Nav_Admin",
        "Nav_Profile",
        "Nav_Premium",
        "Nav_LinkDropWallet",
        "Nav_CreatorCenter",
        "Nav_CreatorPayouts",
        "Nav_Reports",
        "Nav_CommunityReport",
        "Nav_UserReport",
        "Nav_Administration",
        "Nav_AdminDashboard",
        "Nav_AdminHubModeration",
        "Nav_AdminContentModeration",
        "Nav_AdminPost",
        "Nav_AdminGroup",
        "Nav_AdminUserDirectory",
        "Nav_AdminAccountCreation",
        "Nav_AdminRolePermissions",
        "Nav_AdminSystemAuditLogs",
        "Nav_AdminCreatorPayouts",
        "Nav_AdminPlatformFinance",
        "Nav_AdminLinkDropsReview",
        "Nav_AdminPremiumPlans",
        "Nav_AdminIdentityAudits",
        "Nav_AdminPlatformSettings",
        "Nav_Welcome",
        "Nav_Login",
        "Nav_Register",
        "Nav_ViewNode",
        "Nav_PaidChatSettings",
        "Nav_ChangePassword",
        "Nav_DeleteAccount",
        "Nav_LogOut",
        "Nav_CloseSidebar",
        "Nav_VerifiedMember"
    ];

    private static readonly string[] ShellKeys =
    [
        "Shell_SearchPlaceholder",
        "Shell_AdminNode",
        "Shell_Administrator",
        "Shell_AccountMenu",
        "Shell_ThemeToggle_Light",
        "Shell_ThemeToggle_Dark",
        "Shell_ThemeToggle_Aria",
        "Shell_UnhandledError",
        "Common_Reload",
        "Shell_ToggleSidebar",
        "Shell_DeleteAccount_Title",
        "Shell_DeleteAccount_Prompt",
        "Shell_DeleteAccount_WarningTitle",
        "Shell_DeleteAccount_WarningDesc",
        "Shell_DeleteAccount_ConfirmBtn",
        "Role_Administrator",
        "Role_DefaultUser",
        "Role_CommunityMember",
        "Role_CommunityModerator",
        "Role_DomainProfessional",
        "Role_PublicFigure",
        "Role_Guest"
    ];

    private static readonly string[] NotificationKeys =
    [
        "Notifications_Title",
        "Notifications_NewCount",
        "Notifications_MarkAllAsRead",
        "Notifications_Loading",
        "Notifications_EmptyTitle",
        "Notifications_EmptySubtitle",
        "Notifications_SeeMore",
        "Notifications_Time_JustNow",
        "Notifications_Time_MinutesAgo",
        "Notifications_Time_HoursAgo",
        "Notifications_Time_DaysAgo",
        "Notifications_UnreadBadge"
    ];

    private static readonly string[] Step2CommonKeys =
    [
        "Common_CloseDialog",
        "Auth_OtpAriaLabel",
        "Auth_OtpDigitAriaLabel",
        "Common_LanguageSelection",
        "Common_SwitchToEnglish",
        "Common_SwitchToMyanmar",
        "Common_Breadcrumbs"
    ];

    private static readonly string[] LinkDropKeys =
    [
        "LinkDrop_LoadingBalance",
        "LinkDrop_WalletUnavailable",
        "LinkDrop_BalanceTooltip"
    ];

    private static readonly string[] AdminLayoutKeys =
    [
        "Admin_ControlPanel",
        "Admin_SystemAdministration",
        "Admin_UserView",
        "Admin_BackToMainApp",
        "Admin_SwitchToLightMode",
        "Admin_SwitchToDarkMode",
        "Admin_ToggleThemeAria",
        "Admin_ChangeOperatorPassword"
    ];

    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllNavigationKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Test English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in NavigationKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Navigation key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Navigation key '{key}' is empty in English AppResources.resx");
            }

            // Test Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in NavigationKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Navigation key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Navigation key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllShellKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Test English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in ShellKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Shell key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Shell key '{key}' is empty in English AppResources.resx");
            }

            // Test Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in ShellKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Shell key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Shell key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllNotificationKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Test English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in NotificationKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Notification key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Notification key '{key}' is empty in English AppResources.resx");
            }

            // Test Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in NotificationKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Notification key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Notification key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllLinkDropKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Test English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in LinkDropKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"LinkDrop key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"LinkDrop key '{key}' is empty in English AppResources.resx");
            }

            // Test Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in LinkDropKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"LinkDrop key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"LinkDrop key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllAdminLayoutKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Test English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in AdminLayoutKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Admin layout key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Admin layout key '{key}' is empty in English AppResources.resx");
            }

            // Test Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in AdminLayoutKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Admin layout key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Admin layout key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void NotificationNewCount_FormatsCorrectlyInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English format
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enTemplate = localizer["Notifications_NewCount"];
            var enResult = string.Format(CultureInfo.CurrentUICulture, enTemplate.Value, 5);
            Assert.Equal("5 new", enResult);

            // Myanmar format
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myTemplate = localizer["Notifications_NewCount"];
            var myResult = string.Format(CultureInfo.CurrentUICulture, myTemplate.Value, 5);
            Assert.Equal("5 ခု အသစ်", myResult);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void LinkDropBalanceTooltip_FormatsCorrectlyWithCultureInvariantNumbers()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            const decimal balance = 1250500m;
            const decimal purchased = 1000000m;
            const decimal earned = 250500m;

            var bStr = balance.ToString("N0", CultureInfo.InvariantCulture);
            var pStr = purchased.ToString("N0", CultureInfo.InvariantCulture);
            var eStr = earned.ToString("N0", CultureInfo.InvariantCulture);

            // English tooltip
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enTemplate = localizer["LinkDrop_BalanceTooltip"];
            var enFormatted = string.Format(CultureInfo.CurrentUICulture, enTemplate.Value, bStr, pStr, eStr);
            Assert.Equal("Total Link Drops: 1,250,500 (Purchased: 1,000,000 | Earned: 250,500)", enFormatted);

            // Myanmar tooltip
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myTemplate = localizer["LinkDrop_BalanceTooltip"];
            var myFormatted = string.Format(CultureInfo.CurrentUICulture, myTemplate.Value, bStr, pStr, eStr);
            Assert.Equal("စုစုပေါင်း Link Drops: 1,250,500 (ဝယ်ယူထားသော: 1,000,000 | ရရှိထားသော: 250,500)", myFormatted);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllMyanmarPhase4Translations_DoNotContainZawgyiSpecificCodepoints()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = NavigationKeys
                .Concat(ShellKeys)
                .Concat(NotificationKeys)
                .Concat(LinkDropKeys)
                .Concat(AdminLayoutKeys)
                .Concat(Step2CommonKeys);

            foreach (var key in allKeys)
            {
                var text = localizer[key].Value;
                foreach (var ch in text)
                {
                    // Private Use Area (E000-F8FF) used by Zawgyi fonts
                    var isZawgyiPua = ch >= '\uE000' && ch <= '\uF8FF';
                    Assert.False(isZawgyiPua, $"Key '{key}' contains Zawgyi Private Use Area character U+{(int)ch:X4}: '{ch}'");
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
    public void AllStep2CommonKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Test English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in Step2CommonKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Step 2 common key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Step 2 common key '{key}' is empty in English AppResources.resx");
            }

            // Test Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in Step2CommonKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Step 2 common key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Step 2 common key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ShellDeleteAccountPrompt_FormatsCorrectly()
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
            var enPrompt = localizer["Shell_DeleteAccount_Prompt", "Aung Aung"];
            Assert.Equal("Are you sure you want to delete your account (Aung Aung)?", enPrompt.Value);

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myPrompt = localizer["Shell_DeleteAccount_Prompt", "Aung Aung"];
            Assert.Equal("သင့်အကောင့် (Aung Aung) ကို ဖျက်ရန် သေချာပါသလား။", myPrompt.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void NotificationsTimeAgo_FormatsCorrectly()
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
            Assert.Equal("Just now", localizer["Notifications_Time_JustNow"].Value);
            Assert.Equal("5m ago", localizer["Notifications_Time_MinutesAgo", 5].Value);
            Assert.Equal("2h ago", localizer["Notifications_Time_HoursAgo", 2].Value);
            Assert.Equal("3d ago", localizer["Notifications_Time_DaysAgo", 3].Value);

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            Assert.Equal("အခုလေးတင်", localizer["Notifications_Time_JustNow"].Value);
            Assert.Equal("လွန်ခဲ့သော 5 မိနစ်", localizer["Notifications_Time_MinutesAgo", 5].Value);
            Assert.Equal("လွန်ခဲ့သော 2 နာရီ", localizer["Notifications_Time_HoursAgo", 2].Value);
            Assert.Equal("လွန်ခဲ့သော 3 ရက်", localizer["Notifications_Time_DaysAgo", 3].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
