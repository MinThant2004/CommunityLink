using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class HomeStep4LocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static readonly string[] Step4KeyPrefixes =
    {
        "Home_People_",
        "Home_Groups_",
        "Home_Treasury_",
        "Home_Quorum_",
        "Home_Charter_",
        "Home_ScrollToTop",
        "Home_Poll_"
    };

    [Fact]
    public void HomeStep4Keys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step4KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step4KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.Equal(95, enKeys.Count);
    }

    [Fact]
    public void AllHomeStep4Keys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        Assert.NotNull(enSet);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step4KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English Culture
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            foreach (var key in keys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Resource '{key}' not found in English");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Resource '{key}' is empty in English");
            }

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            foreach (var key in keys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Resource '{key}' not found in Myanmar");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Resource '{key}' is empty in Myanmar");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void PlaceholdersMatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var placeholderRegex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step4KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        foreach (var key in keys)
        {
            var enVal = enSet.GetString(key) ?? string.Empty;
            var myVal = mySet.GetString(key) ?? string.Empty;

            var enPlaceholders = placeholderRegex.Matches(enVal).Select(m => m.Value).OrderBy(x => x).ToList();
            var myPlaceholders = placeholderRegex.Matches(myVal).Select(m => m.Value).OrderBy(x => x).ToList();

            Assert.Equal(enPlaceholders, myPlaceholders);
        }
    }

    [Fact]
    public void MyanmarTranslations_DoNotContainZawgyiPUACharacters()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);
        Assert.NotNull(mySet);

        var zawgyiRegex = new Regex(@"[\u105A-\u105E\u1060-\u1097\uAA60-\uAA7F]", RegexOptions.Compiled);

        var keys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step4KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        foreach (var key in keys)
        {
            var val = mySet.GetString(key) ?? string.Empty;
            Assert.False(zawgyiRegex.IsMatch(val), $"Key '{key}' contains Zawgyi PUA characters: {val}");
        }
    }

    [Fact]
    public void VerifySampleKeyTranslations()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Myanmar Culture checks
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("အဖွဲ့ဝင်များကို ရှာဖွေပါ...", localizer["Home_People_SearchPlaceholder"].Value);
            Assert.Equal("အားလုံး (10)", localizer["Home_People_Filter_All", 10].Value);
            Assert.Equal("ပူးပေါင်းဆောင်ရွက်မှု အဖွဲ့များနှင့် ဆပ်ကွန်မြူနတီများ", localizer["Home_Groups_Title"].Value);
            Assert.Equal("ထိပ်တန်း ဦးဆောင်အဖွဲ့", localizer["Home_Groups_PremierGroup"].Value);
            Assert.Equal("ကျွန်ုပ်၏ ရပ်တည်ချက်", localizer["Home_Treasury_MyStanding"].Value);
            Assert.Equal("ထောက်ပံ့ကြေး ဘဏ္ဍာရေး", localizer["Home_Treasury_GrantTreasury"].Value);
            Assert.Equal("လာမည့် အစည်းအဝေးများ", localizer["Home_Quorum_UpcomingTitle"].Value);
            Assert.Equal("အသိုက်အဝန်း ပဋိညာဉ်", localizer["Home_Charter_Title"].Value);
            Assert.Equal("အများပြည်သူဆိုင်ရာ စစ်တမ်း ဖန်တီးရန်", localizer["Home_Poll_ModalTitle"].Value);
            Assert.Equal("စစ်တမ်း ဖန်တီးရန်", localizer["Home_Poll_CreateBtn"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
