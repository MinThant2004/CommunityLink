using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class GroupDetailLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void GroupDetailKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("GroupDetail_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("GroupDetail_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 220, $"Expected at least 220 GroupDetail keys, but found {enKeys.Count}");
    }

    [Fact]
    public void AllGroupDetailKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        Assert.NotNull(enSet);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("GroupDetail_", StringComparison.Ordinal))
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
    public void AllMyanmarGroupDetailStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);
        Assert.NotNull(mySet);

        var zawgyiPuaRegex = new Regex(@"[\uE000-\uF8FF]", RegexOptions.Compiled);

        var keys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("GroupDetail_", StringComparison.Ordinal))
            .ToList();

        var violations = new List<string>();

        foreach (var entry in keys)
        {
            var key = entry.Key.ToString()!;
            var value = entry.Value?.ToString() ?? "";

            if (zawgyiPuaRegex.IsMatch(value))
            {
                violations.Add($"{key}: contains Zawgyi/PUA codepoints -> {value}");
            }
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void GroupDetailFormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var tokenRegex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        var enDict = enSet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("GroupDetail_", StringComparison.Ordinal))
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "");

        var myDict = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("GroupDetail_", StringComparison.Ordinal))
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "");

        var mismatches = new List<string>();

        foreach (var (key, enVal) in enDict)
        {
            if (!myDict.TryGetValue(key, out var myVal)) continue;

            var enTokens = tokenRegex.Matches(enVal).Select(m => m.Value).OrderBy(t => t).ToList();
            var myTokens = tokenRegex.Matches(myVal).Select(m => m.Value).OrderBy(t => t).ToList();

            if (!enTokens.SequenceEqual(myTokens))
            {
                mismatches.Add($"{key}: English [{string.Join(",", enTokens)}] vs Myanmar [{string.Join(",", myTokens)}]");
            }
        }

        Assert.Empty(mismatches);
    }

    [Fact]
    public void GroupDetailRoleAndTabLabels_ResolveCorrectly()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English Culture
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            Assert.Equal("Owner", localizer["GroupDetail_Role_Owner"].Value);
            Assert.Equal("Admin", localizer["GroupDetail_Role_Admin"].Value);
            Assert.Equal("Member", localizer["GroupDetail_Role_Member"].Value);
            Assert.Equal("Feed ({0})", localizer["GroupDetail_Tab_Feed"].Value);
            Assert.Equal("Polls ({0})", localizer["GroupDetail_Tab_Polls"].Value);
            Assert.Equal("Members ({0})", localizer["GroupDetail_Tab_Members"].Value);
            Assert.Equal("Ratings ({0})", localizer["GroupDetail_Tab_Ratings"].Value);
            Assert.Equal("Join Group", localizer["GroupDetail_Action_JoinGroup"].Value);
            Assert.Equal("Leave Group", localizer["GroupDetail_Action_LeaveGroup"].Value);
            Assert.Equal("Request to Join", localizer["GroupDetail_Action_RequestToJoin"].Value);
            Assert.Equal("{0} members", localizer["GroupDetail_Stats_Members"].Value);
            Assert.Equal("{0} posts", localizer["GroupDetail_Stats_Posts"].Value);
            Assert.Equal("No ratings", localizer["GroupDetail_Stats_NoRatings"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("ပိုင်ရှင်", localizer["GroupDetail_Role_Owner"].Value);
            Assert.Equal("စီမံခန့်ခွဲသူ", localizer["GroupDetail_Role_Admin"].Value);
            Assert.Equal("အဖွဲ့ဝင်", localizer["GroupDetail_Role_Member"].Value);
            Assert.Equal("ဖိဒ် ({0})", localizer["GroupDetail_Tab_Feed"].Value);
            Assert.Equal("စစ်တမ်းများ ({0})", localizer["GroupDetail_Tab_Polls"].Value);
            Assert.Equal("အဖွဲ့ဝင်များ ({0})", localizer["GroupDetail_Tab_Members"].Value);
            Assert.Equal("အဆင့်သတ်မှတ်ချက်များ ({0})", localizer["GroupDetail_Tab_Ratings"].Value);
            Assert.Equal("အဖွဲ့သို့ ဝင်မည်", localizer["GroupDetail_Action_JoinGroup"].Value);
            Assert.Equal("အဖွဲ့မှ ထွက်ရန်", localizer["GroupDetail_Action_LeaveGroup"].Value);
            Assert.Equal("ဝင်ရောက်ရန် တောင်းဆိုမည်", localizer["GroupDetail_Action_RequestToJoin"].Value);
            Assert.Equal("အဖွဲ့ဝင် {0} ဦး", localizer["GroupDetail_Stats_Members"].Value);
            Assert.Equal("ပို့စ် {0} ခု", localizer["GroupDetail_Stats_Posts"].Value);
            Assert.Equal("အဆင့်သတ်မှတ်ချက် မရှိသေးပါ", localizer["GroupDetail_Stats_NoRatings"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
