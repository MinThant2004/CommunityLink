using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class HomeStep2LocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static readonly string[] Step2KeyPrefixes =
    {
        "Home_Share_",
        "Home_Edit_",
        "Home_Recycle_",
        "Home_Reader_"
    };

    [Fact]
    public void HomeStep2Keys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step2KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step2KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.Equal(13, enKeys.Count);
    }

    [Fact]
    public void AllHomeStep2Keys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        Assert.NotNull(enSet);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step2KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
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
    public void AllMyanmarHomeStep2Strings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);
        Assert.NotNull(mySet);

        var zawgyiPuaRegex = new Regex(@"[\uE000-\uF8FF]", RegexOptions.Compiled);

        var keys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => Step2KeyPrefixes.Any(p => e.Key.ToString()!.StartsWith(p, StringComparison.Ordinal)))
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
    public void HomeStep2FormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var tokenRegex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        var enDict = enSet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => Step2KeyPrefixes.Any(p => e.Key.ToString()!.StartsWith(p, StringComparison.Ordinal)))
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "");

        var myDict = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => Step2KeyPrefixes.Any(p => e.Key.ToString()!.StartsWith(p, StringComparison.Ordinal)))
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
    public void HomeStep2KeyLabels_ResolveExpectedTranslations()
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

            Assert.Equal("ADD A NOTE (OPTIONAL)", localizer["Home_Share_NoteLabel"].Value);
            Assert.Equal("What are your thoughts on this post?", localizer["Home_Share_Placeholder"].Value);
            Assert.Equal("Post shared successfully!", localizer["Home_Share_Toast_Success"].Value);
            Assert.Equal("Failed to share post: {0}", localizer["Home_Share_Toast_Failed"].Value);

            Assert.Equal("POST CONTENT", localizer["Home_Edit_PostContentLabel"].Value);
            Assert.Equal("Note: Post image attachments cannot be modified.", localizer["Home_Edit_ImageNotice"].Value);
            Assert.Equal("Failed to update post: {0}", localizer["Home_Edit_Toast_UpdateFailed"].Value);

            Assert.Equal("This post will be hidden from the feed and community stream immediately. Posts kept in the Recycle Bin for more than 10 days will be permanently erased.", localizer["Home_Recycle_Desc"].Value);
            Assert.Equal("Failed to recycle post: {0}", localizer["Home_Recycle_Toast_Failed"].Value);

            Assert.Equal("Plain Text Document • {0} characters", localizer["Home_Reader_DocSubtitle"].Value);
            Assert.Equal("Press Esc or Close", localizer["Home_Reader_KeyboardHint"].Value);
            Assert.Equal("Close Document", localizer["Home_Reader_CloseBtn"].Value);
            Assert.Equal("Close", localizer["Home_Reader_CloseAria"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("မှတ်စု ရေးသားရန် (စိတ်ကြိုက်)", localizer["Home_Share_NoteLabel"].Value);
            Assert.Equal("ဤပို့စ်နှင့် ပတ်သက်၍ သင့်အမြင် မည်သို့ရှိပါသလဲ?", localizer["Home_Share_Placeholder"].Value);
            Assert.Equal("ပို့စ်ကို အောင်မြင်စွာ မျှဝေပြီးပါပြီ!", localizer["Home_Share_Toast_Success"].Value);
            Assert.Equal("ပို့စ် မျှဝေရန် မအောင်မြင်ပါ: {0}", localizer["Home_Share_Toast_Failed"].Value);

            Assert.Equal("ပို့စ် အကြောင်းအရာ", localizer["Home_Edit_PostContentLabel"].Value);
            Assert.Equal("မှတ်ချက် - ပို့စ်တွင် တွဲတင်ထားသော ပုံများကို ပြင်ဆင်၍ မရပါ။", localizer["Home_Edit_ImageNotice"].Value);
            Assert.Equal("ပို့စ်အား ပြင်ဆင်ရန် မအောင်မြင်ပါ: {0}", localizer["Home_Edit_Toast_UpdateFailed"].Value);

            Assert.Equal("ဤပို့စ်ကို ဖိဒ်နှင့် အသိုက်အဝန်း စထရီမ်းမှ ချက်ချင်း ဝှက်ထားမည် ဖြစ်သည်။ အမှိုက်ပုံးထဲတွင် ၁၀ ရက်ထက်ပို၍ ရှိနေသော ပို့စ်များသည် အပြီးတိုင် ဖျက်ဆီးခံရမည် ဖြစ်သည်။", localizer["Home_Recycle_Desc"].Value);
            Assert.Equal("ပို့စ်အား အမှိုက်ပုံးသို့ ရွှေ့ရန် မအောင်မြင်ပါ: {0}", localizer["Home_Recycle_Toast_Failed"].Value);

            Assert.Equal("ရိုးရိုးစာသား စာရွက်စာတမ်း • စာလုံးရေ {0} လုံး", localizer["Home_Reader_DocSubtitle"].Value);
            Assert.Equal("Esc သို့မဟုတ် ပိတ်မည်ကို နှိပ်ပါ", localizer["Home_Reader_KeyboardHint"].Value);
            Assert.Equal("စာရွက်စာတမ်း ပိတ်မည်", localizer["Home_Reader_CloseBtn"].Value);
            Assert.Equal("ပိတ်မည်", localizer["Home_Reader_CloseAria"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ReusedKeys_ForHomeStep2Modals_ExistAndResolveCorrectly()
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

            // Share Modal Reused Keys
            Assert.Equal("Share Post", localizer["PostUpload_Share_Title"].Value);
            Assert.Equal("Sharing...", localizer["PostUpload_Share_SubmittingBtn"].Value);
            Assert.Equal("Share Now", localizer["PostUpload_Share_ShareBtn"].Value);

            // Edit Modal Reused Keys
            Assert.Equal("Edit Post Description", localizer["GroupDetail_Modal_EditPostTitle"].Value);
            Assert.Equal("Update your post description...", localizer["GroupDetail_Modal_EditPostPlaceholder"].Value);
            Assert.Equal("Saving...", localizer["PostUpload_Edit_SavingBtn"].Value);
            Assert.Equal("Save Changes", localizer["PostUpload_Edit_SaveChangesBtn"].Value);
            Assert.Equal("Post updated successfully!", localizer["PostUpload_Toast_PostUpdated"].Value);

            // Recycle Modal Reused Keys
            Assert.Equal("Move Post to Recycle Bin?", localizer["PostUpload_Recycle_Title"].Value);
            Assert.Equal("You can restore this post anytime within 10 days from your profile.", localizer["PostUpload_Recycle_Subtitle"].Value);
            Assert.Equal("Moving...", localizer["PostUpload_Recycle_SubmittingBtn"].Value);
            Assert.Equal("Move to Recycle Bin", localizer["PostUpload_Recycle_ConfirmBtn"].Value);
            Assert.Equal("Post moved to Recycle Bin.", localizer["PostUpload_Toast_PostRecycled"].Value);

            // Banned Modal Reused Keys
            Assert.Equal("Your post got banned", localizer["PostUpload_Banned_Title"].Value);
            Assert.Equal("This post was removed from public community feeds by platform moderators because it violated community standards.", localizer["PostUpload_Banned_Desc"].Value);
            Assert.Equal("Moderation Reason", localizer["PostUpload_Banned_ReasonLabel"].Value);
            Assert.Equal("Violation of community safety and conduct guidelines.", localizer["PostUpload_Banned_DefaultReason"].Value);
            Assert.Equal("Only you and site administrators can view this post. Other community members cannot see or interact with it.", localizer["PostUpload_Banned_Notice"].Value);
            Assert.Equal("Dismiss", localizer["PostUpload_Banned_DismissBtn"].Value);

            // Common Cancel
            Assert.Equal("Cancel", localizer["Common_Cancel"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            // Share Modal Reused Keys
            Assert.Equal("ပို့စ် မျှဝေမည်", localizer["PostUpload_Share_Title"].Value);
            Assert.Equal("မျှဝေနေပါသည်...", localizer["PostUpload_Share_SubmittingBtn"].Value);
            Assert.Equal("ယခု မျှဝေမည်", localizer["PostUpload_Share_ShareBtn"].Value);

            // Edit Modal Reused Keys
            Assert.Equal("ပို့စ် ဖော်ပြချက် ပြင်ဆင်ရန်", localizer["GroupDetail_Modal_EditPostTitle"].Value);
            Assert.Equal("ပို့စ် ဖော်ပြချက်ကို ပြင်ဆင်ပါ...", localizer["GroupDetail_Modal_EditPostPlaceholder"].Value);
            Assert.Equal("သိမ်းဆည်းနေပါသည်...", localizer["PostUpload_Edit_SavingBtn"].Value);
            Assert.Equal("အပြောင်းအလဲများကို သိမ်းမည်", localizer["PostUpload_Edit_SaveChangesBtn"].Value);
            Assert.Equal("ပို့စ်ကို အောင်မြင်စွာ မွမ်းမံပြီးပါပြီ။", localizer["PostUpload_Toast_PostUpdated"].Value);

            // Recycle Modal Reused Keys
            Assert.Equal("ပို့စ်အား အမှိုက်ပုံးသို့ ရွှေ့မလား?", localizer["PostUpload_Recycle_Title"].Value);
            Assert.Equal("ဤပို့စ်ကို ၁၀ ရက်အတွင်း သင့်ပရိုဖိုင်မှ အချိန်မရွေး ပြန်လည်ရယူနိုင်ပါသည်။", localizer["PostUpload_Recycle_Subtitle"].Value);
            Assert.Equal("ရွှေ့နေပါသည်...", localizer["PostUpload_Recycle_SubmittingBtn"].Value);
            Assert.Equal("အမှိုက်ပုံးသို့ ရွှေ့မည်", localizer["PostUpload_Recycle_ConfirmBtn"].Value);
            Assert.Equal("ပို့စ်အား အမှိုက်ပုံးသို့ ရွှေ့လိုက်ပါပြီ။", localizer["PostUpload_Toast_PostRecycled"].Value);

            // Banned Modal Reused Keys
            Assert.Equal("သင့်ပို့စ်အား ပိတ်ပင်ထားပါသည်", localizer["PostUpload_Banned_Title"].Value);
            Assert.Equal("ဤပို့စ်သည် အသိုက်အဝန်း စံနှုန်းများကို ချိုးဖောက်သဖြင့် ပလက်ဖောင်း စီမံခန့်ခွဲသူများက အများမြင် ဖိဒ်များမှ ဖယ်ရှားခဲ့ပါသည်။", localizer["PostUpload_Banned_Desc"].Value);
            Assert.Equal("ထိန်းကျောင်းမှု အကြောင်းပြချက်", localizer["PostUpload_Banned_ReasonLabel"].Value);
            Assert.Equal("အသိုက်အဝန်း လုံခြုံရေးနှင့် ကျင့်ဝတ်ဆိုင်ရာ လမ်းညွှန်ချက်များကို ချိုးဖောက်ခြင်း။", localizer["PostUpload_Banned_DefaultReason"].Value);
            Assert.Equal("သင်နှင့် ဝက်ဘ်ဆိုက် စီမံခန့်ခွဲသူများသာ ဤပို့စ်ကို ကြည့်ရှုနိုင်ပါသည်။ အခြားအဖွဲ့ဝင်များသည် ၎င်းကို မမြင်တွေ့နိုင်သလို အပြန်အလှန် ဆက်သွယ်၍လည်း မရနိုင်ပါ။", localizer["PostUpload_Banned_Notice"].Value);
            Assert.Equal("ပိတ်မည်", localizer["PostUpload_Banned_DismissBtn"].Value);

            // Common Cancel
            Assert.Equal("မလုပ်တော့ပါ", localizer["Common_Cancel"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
