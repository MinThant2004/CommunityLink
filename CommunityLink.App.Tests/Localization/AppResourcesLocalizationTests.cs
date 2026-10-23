using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class AppResourcesLocalizationTests
{
    private static readonly string[] RequiredKeys =
    [
        "Nav_Home",
        "Nav_Communities",
        "Nav_Groups",
        "Nav_Chat",
        "Nav_Wallet",
        "Nav_Activity",
        "Nav_CreatorEarnings",
        "Nav_Admin",
        "Common_Save",
        "Common_Cancel",
        "Common_Refresh",
        "Common_Loading",
        "Common_Search"
    ];

    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void EnglishResource_Nav_Chat_ResolvesToChat()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var enCulture = new CultureInfo("en");
            CultureInfo.CurrentCulture = enCulture;
            CultureInfo.CurrentUICulture = enCulture;

            var localizedString = localizer["Nav_Chat"];

            Assert.False(localizedString.ResourceNotFound, "Resource key 'Nav_Chat' should be found in AppResources.resx");
            Assert.Equal("Chat", localizedString.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    [Fact]
    public void MyanmarResource_Nav_Chat_ResolvesToBurmeseTranslation()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var myCulture = new CultureInfo("my");
            CultureInfo.CurrentCulture = myCulture;
            CultureInfo.CurrentUICulture = myCulture;

            var localizedString = localizer["Nav_Chat"];

            Assert.False(localizedString.ResourceNotFound, "Resource key 'Nav_Chat' should be found in AppResources.my.resx");
            Assert.Equal("စကားပြောခန်း", localizedString.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    [Fact]
    public void AllThirteenKeys_ExistInEnglishResource()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var enCulture = new CultureInfo("en");
            CultureInfo.CurrentCulture = enCulture;
            CultureInfo.CurrentUICulture = enCulture;

            Assert.Equal(13, RequiredKeys.Length);

            foreach (var key in RequiredKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Key '{key}' was not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Key '{key}' had an empty value in English AppResources.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    [Fact]
    public void AllThirteenKeys_ExistInMyanmarResource()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var myCulture = new CultureInfo("my");
            CultureInfo.CurrentCulture = myCulture;
            CultureInfo.CurrentUICulture = myCulture;

            Assert.Equal(13, RequiredKeys.Length);

            foreach (var key in RequiredKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Key '{key}' was not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Key '{key}' had an empty value in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    [Fact]
    public void CultureFallback_UnsupportedCulture_FallsBackToEnglishDefault()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Culture with no specific resource file falls back to default English resource
            var frCulture = new CultureInfo("fr-FR");
            CultureInfo.CurrentCulture = frCulture;
            CultureInfo.CurrentUICulture = frCulture;

            var localized = localizer["Nav_Chat"];
            Assert.False(localized.ResourceNotFound);
            Assert.Equal("Chat", localized.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    [Fact]
    public void CultureFallback_RegionalMyanmar_FallsBackToNeutralMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Regional culture "my-MM" falls back to neutral culture "my"
            var myMmCulture = new CultureInfo("my-MM");
            CultureInfo.CurrentCulture = myMmCulture;
            CultureInfo.CurrentUICulture = myMmCulture;

            var localized = localizer["Nav_Chat"];
            Assert.False(localized.ResourceNotFound);
            Assert.Equal("စကားပြောခန်း", localized.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUICulture;
        }
    }

    [Fact]
    public void NonExistentKey_ReturnsKeyNameAndSetsResourceNotFoundTrue()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var result = localizer["NonExistent_Key_For_Test"];
        Assert.True(result.ResourceNotFound);
        Assert.Equal("NonExistent_Key_For_Test", result.Value);
    }
}
