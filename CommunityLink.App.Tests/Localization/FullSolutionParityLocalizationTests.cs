using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class FullSolutionParityLocalizationTests
{
    private static readonly string[] ResidualKeys =
    [
        "Common_Dismiss",
        "Common_Uploading",
        "Profile_Modal_SaveBtn",
        "Profile_Modal_AvatarUploading",
        "Profile_Modal_AvatarUploadNew",
        "Profile_Unlock_LockedTitle",
        "Profile_Unlock_DirectFeeDesc",
        "Profile_Unlock_CreatorFeeLabel",
        "Profile_Unlock_AvailableBalanceLabel",
        "Profile_Unlock_InsufficientDropsTitle",
        "Profile_Unlock_TopUpLink",
        "Profile_Share_Sharing",
        "Profile_Share_ShareNow",
        "Profile_BannedModal_Title",
        "Profile_BannedModal_Notice",
        "Profile_BannedModal_DefaultReason",
        "Profile_BannedModal_VisibilityNote",
        "Admin_LinkDrops_DisplayOrderPlaceholder",
        "Admin_LinkDrops_CurrencyMmk",
        "Admin_LinkDrops_CurrencyUsd",
        "Admin_LinkDrops_CurrencyCny",
        "Admin_Rbac_TooltipSelectAll",
        "Guest_Footer_StandardV2",
        "Guest_Footer_BlazorNet10",
        "Guest_Footer_SignalR",
        "Guest_Footer_EnterpriseRbac",
        "Admin_Mod_ReportedAt",
        "Admin_Mod_HubPrefix",
        "Admin_Mod_GroupPrefix",
        "Home_TextFileModal_HeaderMeta",
        "NotFound_PageTitle",
        "NotFound_Heading",
        "NotFound_Message",
        "NotFound_ReturnHome"
    ];

    private static ServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllResidualKeys_ExistInEnglishCulture()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            foreach (var key in ResidualKeys)
            {
                var localizedString = localizer[key];
                Assert.False(localizedString.ResourceNotFound, $"Key '{key}' was not found in English AppResources.");
                Assert.False(string.IsNullOrWhiteSpace(localizedString.Value), $"Key '{key}' has an empty value in English.");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllResidualKeys_ExistInMyanmarCulture()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            foreach (var key in ResidualKeys)
            {
                var localizedString = localizer[key];
                Assert.False(localizedString.ResourceNotFound, $"Key '{key}' was not found in Myanmar AppResources.");
                Assert.False(string.IsNullOrWhiteSpace(localizedString.Value), $"Key '{key}' has an empty value in Myanmar.");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ResidualKeys_MyanmarTranslations_ContainNoZawgyiPuaCharacters()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            foreach (var key in ResidualKeys)
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

    [Fact]
    public void ParameterizedResidualKeys_FormatCorrectlyInBothCultures()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // EN
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            var enFeeDesc = localizer["Profile_Unlock_DirectFeeDesc", "Alice"].Value;
            Assert.Contains("Alice", enFeeDesc);

            var enRepAt = localizer["Admin_Mod_ReportedAt", "Oct 21, 2026"].Value;
            Assert.Contains("Oct 21, 2026", enRepAt);

            var enMeta = localizer["Home_TextFileModal_HeaderMeta", 150].Value;
            Assert.Contains("150", enMeta);

            // MY
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var myFeeDesc = localizer["Profile_Unlock_DirectFeeDesc", "Alice"].Value;
            Assert.Contains("Alice", myFeeDesc);

            var myRepAt = localizer["Admin_Mod_ReportedAt", "Oct 21, 2026"].Value;
            Assert.Contains("Oct 21, 2026", myRepAt);

            var myMeta = localizer["Home_TextFileModal_HeaderMeta", 150].Value;
            Assert.Contains("150", myMeta);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
