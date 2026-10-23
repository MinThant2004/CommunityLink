using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class PostUploadLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void PostUploadKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("PostUpload_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("PostUpload_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 90, $"Expected at least 90 PostUpload keys, but found {enKeys.Count}");
    }

    [Fact]
    public void AllPostUploadKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        Assert.NotNull(enSet);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("PostUpload_", StringComparison.Ordinal))
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
    public void AllMyanmarPostUploadStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);
        Assert.NotNull(mySet);

        var zawgyiPuaRegex = new Regex(@"[\uE000-\uF8FF]", RegexOptions.Compiled);

        var keys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("PostUpload_", StringComparison.Ordinal))
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
    public void PostUploadFormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var tokenRegex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        var enDict = enSet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("PostUpload_", StringComparison.Ordinal))
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "");

        var myDict = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("PostUpload_", StringComparison.Ordinal))
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
    public void PostUploadKeyLabels_ResolveCorrectly()
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

            Assert.Equal("Post Upload & Feed", localizer["PostUpload_PageTitle"].Value);
            Assert.Equal("Post Sharing & Upload", localizer["PostUpload_Header_Title"].Value);
            Assert.Equal("Upload Post", localizer["PostUpload_Header_UploadBtn"].Value);
            Assert.Equal("Private", localizer["PostUpload_Post_BadgePrivate"].Value);
            Assert.Equal("Like", localizer["PostUpload_Post_LikeBtn"].Value);
            Assert.Equal("Comment", localizer["PostUpload_Post_CommentBtn"].Value);
            Assert.Equal("Share", localizer["PostUpload_Post_ShareBtn"].Value);
            Assert.Equal("Create New Post", localizer["PostUpload_Create_Title"].Value);
            Assert.Equal("Publish", localizer["PostUpload_Create_PublishBtn"].Value);
            Assert.Equal("Edit Your Post", localizer["PostUpload_Edit_Title"].Value);
            Assert.Equal("Save Changes", localizer["PostUpload_Edit_SaveChangesBtn"].Value);
            Assert.Equal("Share Post", localizer["PostUpload_Share_Title"].Value);
            Assert.Equal("Move Post to Recycle Bin?", localizer["PostUpload_Recycle_Title"].Value);
            Assert.Equal("Move to Recycle Bin", localizer["PostUpload_Recycle_ConfirmBtn"].Value);
            Assert.Equal("Your post got banned", localizer["PostUpload_Banned_Title"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("ပို့စ်မျှဝေခြင်းနှင့် ဖိဒ်", localizer["PostUpload_PageTitle"].Value);
            Assert.Equal("ပို့စ်မျှဝေခြင်းနှင့် တင်ခြင်း", localizer["PostUpload_Header_Title"].Value);
            Assert.Equal("ပို့စ်တင်မည်", localizer["PostUpload_Header_UploadBtn"].Value);
            Assert.Equal("သီးသန့်", localizer["PostUpload_Post_BadgePrivate"].Value);
            Assert.Equal("သဘောကျသည်", localizer["PostUpload_Post_LikeBtn"].Value);
            Assert.Equal("မှတ်ချက်", localizer["PostUpload_Post_CommentBtn"].Value);
            Assert.Equal("မျှဝေမည်", localizer["PostUpload_Post_ShareBtn"].Value);
            Assert.Equal("ပို့စ်အသစ် ဖန်တီးမည်", localizer["PostUpload_Create_Title"].Value);
            Assert.Equal("ထုတ်ဝေမည်", localizer["PostUpload_Create_PublishBtn"].Value);
            Assert.Equal("သင့်ပို့စ်အား တည်းဖြတ်မည်", localizer["PostUpload_Edit_Title"].Value);
            Assert.Equal("အပြောင်းအလဲများကို သိမ်းမည်", localizer["PostUpload_Edit_SaveChangesBtn"].Value);
            Assert.Equal("ပို့စ် မျှဝေမည်", localizer["PostUpload_Share_Title"].Value);
            Assert.Equal("ပို့စ်အား အမှိုက်ပုံးသို့ ရွှေ့မလား?", localizer["PostUpload_Recycle_Title"].Value);
            Assert.Equal("အမှိုက်ပုံးသို့ ရွှေ့မည်", localizer["PostUpload_Recycle_ConfirmBtn"].Value);
            Assert.Equal("သင့်ပို့စ်အား ပိတ်ပင်ထားပါသည်", localizer["PostUpload_Banned_Title"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
