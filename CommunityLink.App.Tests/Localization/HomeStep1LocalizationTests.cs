using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class HomeStep1LocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void HomeStep1Keys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Home_Composer_", StringComparison.Ordinal) || k.StartsWith("Home_Broadcast_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Home_Composer_", StringComparison.Ordinal) || k.StartsWith("Home_Broadcast_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.Equal(25, enKeys.Count);
    }

    [Fact]
    public void AllHomeStep1Keys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        Assert.NotNull(enSet);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Home_Composer_", StringComparison.Ordinal) || k.StartsWith("Home_Broadcast_", StringComparison.Ordinal))
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
    public void AllMyanmarHomeStep1Strings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);
        Assert.NotNull(mySet);

        var zawgyiPuaRegex = new Regex(@"[\uE000-\uF8FF]", RegexOptions.Compiled);

        var keys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("Home_Composer_", StringComparison.Ordinal) || e.Key.ToString()!.StartsWith("Home_Broadcast_", StringComparison.Ordinal))
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
    public void HomeStep1FormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var tokenRegex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        var enDict = enSet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("Home_Composer_", StringComparison.Ordinal) || e.Key.ToString()!.StartsWith("Home_Broadcast_", StringComparison.Ordinal))
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "");

        var myDict = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => e.Key.ToString()!.StartsWith("Home_Composer_", StringComparison.Ordinal) || e.Key.ToString()!.StartsWith("Home_Broadcast_", StringComparison.Ordinal))
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
    public void HomeStep1KeyLabels_ResolveExpectedTranslations()
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

            Assert.Equal("Share an update, technical insight, or question...", localizer["Home_Composer_Prompt"].Value);
            Assert.Equal("Post", localizer["Home_Composer_ActionPost"].Value);
            Assert.Equal("Poll", localizer["Home_Composer_ActionPoll"].Value);
            Assert.Equal("Create", localizer["Home_Composer_ActionCreate"].Value);

            Assert.Equal("Create Public Broadcast", localizer["Home_Broadcast_Title"].Value);
            Assert.Equal("What's on your mind? *", localizer["Home_Broadcast_ContentLabel"].Value);
            Assert.Equal("Share thoughts, discussions, updates, or code with everyone...", localizer["Home_Broadcast_ContentPlaceholder"].Value);
            Assert.Equal("Attach Text Files (.txt) (Multiple allowed)", localizer["Home_Broadcast_AttachTextFilesLabel"].Value);
            Assert.Equal("Clear all files", localizer["Home_Broadcast_ClearAllFiles"].Value);
            Assert.Equal("Scanning text file(s)...", localizer["Home_Broadcast_ScanningFiles"].Value);
            Assert.Equal("Attach .txt files (Choose multiple)", localizer["Home_Broadcast_AttachFilesPrompt"].Value);
            Assert.Equal("Attach Images (Multiple allowed)", localizer["Home_Broadcast_AttachImagesLabel"].Value);
            Assert.Equal("PNG, JPG, WebP", localizer["Home_Broadcast_SupportedFormats"].Value);
            Assert.Equal("Uploading image(s)...", localizer["Home_Broadcast_UploadingImages"].Value);
            Assert.Equal("Upload images (Choose multiple)", localizer["Home_Broadcast_UploadImagesPrompt"].Value);
            Assert.Equal("Broadcast published successfully!", localizer["Home_Broadcast_Toast_Published"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("သတင်းအချက်အလက်၊ နည်းပညာဆိုင်ရာ အမြင် သို့မဟုတ် မေးခွန်းတစ်ခု မျှဝေပါ...", localizer["Home_Composer_Prompt"].Value);
            Assert.Equal("ပို့စ်", localizer["Home_Composer_ActionPost"].Value);
            Assert.Equal("မဲဆန္ဒ", localizer["Home_Composer_ActionPoll"].Value);
            Assert.Equal("ဖန်တီးရန်", localizer["Home_Composer_ActionCreate"].Value);

            Assert.Equal("အများပြည်သူသို့ ထုတ်လွှင့်ချက် ဖန်တီးရန်", localizer["Home_Broadcast_Title"].Value);
            Assert.Equal("သင့်စိတ်ထဲတွင် ဘာရှိနေပါသလဲ? *", localizer["Home_Broadcast_ContentLabel"].Value);
            Assert.Equal("အတွေးအမြင်များ၊ ဆွေးနွေးချက်များ၊ သတင်းများ သို့မဟုတ် ကုဒ်များကို အားလုံးနှင့် မျှဝေပါ...", localizer["Home_Broadcast_ContentPlaceholder"].Value);
            Assert.Equal("စာသားဖိုင်များ (.txt) တွဲတင်ရန် (အများအပြား ခွင့်ပြုသည်)", localizer["Home_Broadcast_AttachTextFilesLabel"].Value);
            Assert.Equal("ဖိုင်အားလုံး ရှင်းထုတ်ရန်", localizer["Home_Broadcast_ClearAllFiles"].Value);
            Assert.Equal("စာသားဖိုင်(များ)ကို စစ်ဆေးနေသည်...", localizer["Home_Broadcast_ScanningFiles"].Value);
            Assert.Equal(".txt ဖိုင်များ တွဲတင်ရန် (အများအပြား ရွေးချယ်နိုင်သည်)", localizer["Home_Broadcast_AttachFilesPrompt"].Value);
            Assert.Equal("ပုံများ တွဲတင်ရန် (အများအပြား ခွင့်ပြုသည်)", localizer["Home_Broadcast_AttachImagesLabel"].Value);
            Assert.Equal("PNG, JPG, WebP", localizer["Home_Broadcast_SupportedFormats"].Value);
            Assert.Equal("ပုံ(များ)ကို တင်နေသည်...", localizer["Home_Broadcast_UploadingImages"].Value);
            Assert.Equal("ပုံများ တင်ရန် (အများအပြား ရွေးချယ်နိုင်သည်)", localizer["Home_Broadcast_UploadImagesPrompt"].Value);
            Assert.Equal("ထုတ်လွှင့်ချက်ကို အောင်မြင်စွာ လွှင့်တင်ပြီးပါပြီ!", localizer["Home_Broadcast_Toast_Published"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ReusedKeys_ForHomeStep1_ExistAndResolveCorrectly()
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

            Assert.Equal("Cancel", localizer["Common_Cancel"].Value);
            Assert.Equal("Remove", localizer["Common_Remove"].Value);
            Assert.Equal("Publishing...", localizer["GroupDetail_Modal_PublishingPost"].Value);
            Assert.Equal("Publish Post", localizer["GroupDetail_Modal_PublishPostBtn"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("မလုပ်တော့ပါ", localizer["Common_Cancel"].Value);
            Assert.Equal("ဖယ်ရှားမည်", localizer["Common_Remove"].Value);
            Assert.Equal("တင်နေသည်...", localizer["GroupDetail_Modal_PublishingPost"].Value);
            Assert.Equal("ပို့စ် တင်မည်", localizer["GroupDetail_Modal_PublishPostBtn"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
