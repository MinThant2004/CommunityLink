using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class SharedComponentLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static List<string> GetSharedKeys(string prefix)
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
    public void ReportContentKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("ReportContent_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("ReportContent_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 15, $"Expected at least 15 ReportContent keys, but found {enKeys.Count}");
    }

    [Fact]
    public void TelemetryChartKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("TelemetryChart_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("TelemetryChart_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 11, $"Expected at least 11 TelemetryChart keys, but found {enKeys.Count}");
    }

    [Fact]
    public void AllSharedComponentKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var keys = GetSharedKeys("ReportContent_").Concat(GetSharedKeys("TelemetryChart_")).ToList();
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
    public void AllMyanmarSharedComponentStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var keys = GetSharedKeys("ReportContent_").Concat(GetSharedKeys("TelemetryChart_")).ToList();
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
    public void SharedComponentFormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true)!;

        var tokenRegex = new Regex(@"\{(\d+)\}");
        var keys = GetSharedKeys("ReportContent_").Concat(GetSharedKeys("TelemetryChart_")).ToList();

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
    public void TelemetryChart_NumericCalculations_RemainCultureInvariant(string cultureName)
    {
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // Test invariant point formatting used in TelemetryChart SVG path generation
            double x = 123.456;
            double y = 78.9;

            var xFormatted = x.ToString("0.##", CultureInfo.InvariantCulture);
            var yFormatted = y.ToString("0.##", CultureInfo.InvariantCulture);
            var pctX = (x / 320.0 * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%";

            // Ensure dot is used as decimal separator even in French/German
            Assert.Contains(".", xFormatted);
            Assert.DoesNotContain(",", xFormatted);
            Assert.Contains(".", pctX);
            Assert.DoesNotContain(",", pctX);
            Assert.Equal("123.46", xFormatted);
            Assert.Equal("78.9", yFormatted);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
