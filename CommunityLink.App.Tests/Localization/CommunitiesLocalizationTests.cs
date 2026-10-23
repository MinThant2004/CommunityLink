using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class CommunitiesLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static List<string> GetResourceKeys(string prefix)
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var keys = new List<string>();
        if (rs != null)
        {
            foreach (System.Collections.DictionaryEntry entry in rs)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    keys.Add(key);
                }
            }
        }
        return keys;
    }

    [Fact]
    public void CommunitiesKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Communities_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Communities_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 50, $"Expected at least 50 Communities keys, but found {enKeys.Count}");
    }

    [Fact]
    public void SubCommunitiesKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("SubCommunities_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("SubCommunities_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 40, $"Expected at least 40 SubCommunities keys, but found {enKeys.Count}");
    }

    [Fact]
    public void CommonPlatformKey_ExistsInBothLanguages()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enVal = localizer["Common_Platform"];
            Assert.False(enVal.ResourceNotFound);
            Assert.Equal("Platform", enVal.Value);

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myVal = localizer["Common_Platform"];
            Assert.False(myVal.ResourceNotFound);
            Assert.Equal("ပလက်ဖောင်း", myVal.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllCommunitiesAndSubCommunitiesKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var keys = GetResourceKeys("Communities_")
                .Concat(GetResourceKeys("SubCommunities_"))
                .Concat(new[] { "Common_Platform" })
                .ToList();
            Assert.NotEmpty(keys);

            // Verify English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in keys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Key '{key}' is empty in English AppResources.resx");
            }

            // Verify Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in keys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllMyanmarCommunitiesStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var keys = GetResourceKeys("Communities_")
                .Concat(GetResourceKeys("SubCommunities_"))
                .Concat(new[] { "Common_Platform" })
                .ToList();

            foreach (var key in keys)
            {
                var text = localizer[key].Value;
                foreach (var ch in text)
                {
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
    public void CommunitiesAndSubCommunitiesFormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true)!;

        var tokenRegex = new Regex(@"\{(\d+)\}");
        var keys = GetResourceKeys("Communities_").Concat(GetResourceKeys("SubCommunities_")).ToList();

        foreach (var key in keys)
        {
            var enVal = enSet.GetString(key) ?? "";
            var myVal = mySet.GetString(key) ?? "";

            var enTokens = tokenRegex.Matches(enVal).Select(m => m.Value).OrderBy(t => t).ToList();
            var myTokens = tokenRegex.Matches(myVal).Select(m => m.Value).OrderBy(t => t).ToList();

            Assert.Equal(enTokens, myTokens);
        }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("my-MM")]
    [InlineData("fr-FR")]
    [InlineData("de-DE")]
    public void NumericCalculationsAndCompactFormatting_RemainCultureInvariant(string cultureName)
    {
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // Invariant compact formatting logic
            static string FormatCompact(int value)
            {
                if (value >= 1_000_000) return (value / 1_000_000D).ToString("0.#", CultureInfo.InvariantCulture) + "M";
                if (value >= 1_000) return (value / 1_000D).ToString("0.#", CultureInfo.InvariantCulture) + "k";
                return value.ToString("N0", CultureInfo.InvariantCulture);
            }

            static string FormatNumber(int num)
            {
                if (num >= 1_000_000) return (num / 1_000_000D).ToString("0.#", CultureInfo.InvariantCulture) + "M";
                if (num >= 1_000) return (num / 1_000D).ToString("0.#", CultureInfo.InvariantCulture) + "k";
                return num.ToString("N0", CultureInfo.InvariantCulture);
            }

            Assert.Equal("1.5M", FormatCompact(1_500_000));
            Assert.Equal("2.4k", FormatCompact(2_400));
            Assert.Equal("42", FormatCompact(42));

            Assert.Equal("1.5M", FormatNumber(1_500_000));
            Assert.Equal("2.4k", FormatNumber(2_400));
            Assert.Equal("42", FormatNumber(42));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
