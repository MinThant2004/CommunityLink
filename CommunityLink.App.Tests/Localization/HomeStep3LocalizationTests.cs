using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class HomeStep3LocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static readonly string[] Step3KeyPrefixes =
    {
        "Home_Feed_",
        "Home_Comments_",
        "Home_Sidebar_"
    };

    [Fact]
    public void HomeStep3Keys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step3KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step3KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.Equal(61, enKeys.Count);
    }

    [Fact]
    public void AllHomeStep3Keys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        Assert.NotNull(enSet);

        var keys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => Step3KeyPrefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
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
    public void AllMyanmarHomeStep3Strings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);
        Assert.NotNull(mySet);

        var zawgyiPuaRegex = new Regex(@"[\uE000-\uF8FF]", RegexOptions.Compiled);

        var keys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => Step3KeyPrefixes.Any(p => e.Key.ToString()!.StartsWith(p, StringComparison.Ordinal)))
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
    public void HomeStep3FormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var tokenRegex = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        var enDict = enSet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => Step3KeyPrefixes.Any(p => e.Key.ToString()!.StartsWith(p, StringComparison.Ordinal)))
            .ToDictionary(e => e.Key.ToString()!, e => e.Value?.ToString() ?? "");

        var myDict = mySet.Cast<System.Collections.DictionaryEntry>()
            .Where(e => Step3KeyPrefixes.Any(p => e.Key.ToString()!.StartsWith(p, StringComparison.Ordinal)))
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
    public void HomeStep3KeyLabels_ResolveExpectedTranslations()
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

            Assert.Equal("Unable to synchronize workspace", localizer["Home_Feed_SyncErrorTitle"].Value);
            Assert.Equal("Retry Sync", localizer["Home_Feed_RetrySync"].Value);
            Assert.Equal("Posts", localizer["Home_Feed_TabPosts"].Value);
            Assert.Equal("People", localizer["Home_Feed_TabPeople"].Value);
            Assert.Equal("Groups", localizer["Home_Feed_TabGroups"].Value);
            Assert.Equal("Audited Network", localizer["Home_Feed_AuditedNetwork"].Value);

            Assert.Equal("No broadcasts published yet", localizer["Home_Feed_EmptyTitle"].Value);
            Assert.Equal("Publish your first dispatch above to start the discussion.", localizer["Home_Feed_EmptyDesc"].Value);
            Assert.Equal("Lead", localizer["Home_Feed_RoleLead"].Value);
            Assert.Equal("Pro", localizer["Home_Feed_RolePro"].Value);
            Assert.Equal("Only you can view this post", localizer["Home_Feed_PrivateTooltip"].Value);
            Assert.Equal("Save", localizer["Home_Feed_SaveTooltip"].Value);
            Assert.Equal("Unsave", localizer["Home_Feed_UnsaveTooltip"].Value);
            Assert.Equal("More options", localizer["Home_Feed_MoreOptions"].Value);
            Assert.Equal("This content isn't available right now", localizer["Home_Feed_ContentUnavailableTitle"].Value);
            Assert.Equal("Shared attachment", localizer["Home_Feed_SharedAttachmentAlt"].Value);
            Assert.Equal("Copy", localizer["Home_Feed_CopyCode"].Value);
            Assert.Equal("Media attachment {0}", localizer["Home_Feed_MediaAttachmentAlt"].Value);
            Assert.Equal("{0} of {1}", localizer["Home_Feed_CarouselCounter"].Value);
            Assert.Equal("Loading more posts...", localizer["Home_Feed_LoadingMore"].Value);
            Assert.Equal("You've reached the end of the feed", localizer["Home_Feed_EndOfFeed"].Value);

            Assert.Equal("Add a comment or technical inquiry...", localizer["Home_Comments_Placeholder"].Value);
            Assert.Equal("Post", localizer["Home_Comments_PostBtn"].Value);
            Assert.Equal("No comments recorded yet.", localizer["Home_Comments_Empty"].Value);
            Assert.Equal("Comment posted!", localizer["Home_Comments_Toast_Success"].Value);
            Assert.Equal("Failed to post comment: {0}", localizer["Home_Comments_Toast_Failed"].Value);

            Assert.Equal("People", localizer["Home_Sidebar_PeopleTitle"].Value);
            Assert.Equal("See all ({0})", localizer["Home_Sidebar_SeeAll"].Value);
            Assert.Equal("New", localizer["Home_Sidebar_RatingNew"].Value);
            Assert.Equal("Verified Member", localizer["Home_Sidebar_VerifiedMember"].Value);
            Assert.Equal("Active Member", localizer["Home_Sidebar_ActiveMember"].Value);
            Assert.Equal("Connect", localizer["Home_Sidebar_Connect"].Value);

            Assert.Equal("Collaborative Groups", localizer["Home_Sidebar_GroupsTitle"].Value);
            Assert.Equal("Explore ({0})", localizer["Home_Sidebar_Explore"].Value);
            Assert.Equal("{0} members • {1} posts", localizer["Home_Sidebar_GroupMeta"].Value);
            Assert.Equal("Enter", localizer["Home_Sidebar_Enter"].Value);
            Assert.Equal("No collaborative groups joined yet.", localizer["Home_Sidebar_GroupsEmpty"].Value);

            Assert.Equal("Active Ballots & Polls", localizer["Home_Sidebar_PollsTitle"].Value);
            Assert.Equal("View all", localizer["Home_Sidebar_ViewAll"].Value);
            Assert.Equal("Poll", localizer["Home_Sidebar_PollBadge"].Value);
            Assert.Equal("{0} votes • {1}", localizer["Home_Sidebar_PollVotes"].Value);
            Assert.Equal("Open", localizer["Home_Sidebar_PollOpen"].Value);
            Assert.Equal("Closed", localizer["Home_Sidebar_PollClosed"].Value);
            Assert.Equal("Audit →", localizer["Home_Sidebar_PollAudit"].Value);

            Assert.Equal("Vote cast successfully!", localizer["Home_Feed_Toast_VoteSuccess"].Value);
            Assert.Equal("Failed to vote: {0}", localizer["Home_Feed_Toast_VoteFailed"].Value);
            Assert.Equal("Code snippet copied to clipboard!", localizer["Home_Feed_Toast_CopySuccess"].Value);
            Assert.Equal("Failed to copy snippet: {0}", localizer["Home_Feed_Toast_CopyFailed"].Value);

            Assert.Equal("Following member!", localizer["Home_Sidebar_Toast_Following"].Value);
            Assert.Equal("Unfollowed member.", localizer["Home_Sidebar_Toast_Unfollowed"].Value);
            Assert.Equal("Follow action failed: {0}", localizer["Home_Sidebar_Toast_FollowFailed"].Value);
            Assert.Equal("Joined group successfully!", localizer["Home_Sidebar_Toast_JoinedGroup"].Value);
            Assert.Equal("Join group failed: {0}", localizer["Home_Sidebar_Toast_JoinGroupFailed"].Value);
            Assert.Equal("Failed to load dashboard data.", localizer["Home_Feed_Toast_DashboardLoadFailed"].Value);
            Assert.Equal("Public Figure", localizer["Home_Sidebar_RolePublicFigure"].Value);
            Assert.Equal("Domain Pro", localizer["Home_Sidebar_RoleDomainPro"].Value);
            Assert.Equal("Creator", localizer["Home_Sidebar_RoleCreator"].Value);
            Assert.Equal("Community Lead", localizer["Home_Sidebar_RoleLead"].Value);
            Assert.Equal("Moderator", localizer["Home_Sidebar_RoleModerator"].Value);

            // Myanmar Culture
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("လုပ်ငန်းခွင် ချိတ်ဆက်ညှိနှိုင်း၍ မရပါ", localizer["Home_Feed_SyncErrorTitle"].Value);
            Assert.Equal("ပြန်လည် ချိတ်ဆက်မည်", localizer["Home_Feed_RetrySync"].Value);
            Assert.Equal("ပို့စ်များ", localizer["Home_Feed_TabPosts"].Value);
            Assert.Equal("လူပုဂ္ဂိုလ်များ", localizer["Home_Feed_TabPeople"].Value);
            Assert.Equal("အဖွဲ့များ", localizer["Home_Feed_TabGroups"].Value);
            Assert.Equal("စိစစ်ပြီး ကွန်ရက်", localizer["Home_Feed_AuditedNetwork"].Value);

            Assert.Equal("ထုတ်လွှင့်ချက် မရှိသေးပါ", localizer["Home_Feed_EmptyTitle"].Value);
            Assert.Equal("ဆွေးနွေးမှု စတင်ရန် သင့်ပထမဆုံး ထုတ်လွှင့်ချက်ကို အထက်တွင် တင်ပါ။", localizer["Home_Feed_EmptyDesc"].Value);
            Assert.Equal("ခေါင်းဆောင်", localizer["Home_Feed_RoleLead"].Value);
            Assert.Equal("ကျွမ်းကျင်", localizer["Home_Feed_RolePro"].Value);
            Assert.Equal("သင်တစ်ဦးတည်းသာ ဤပို့စ်ကို ကြည့်ရှုနိုင်ပါသည်", localizer["Home_Feed_PrivateTooltip"].Value);
            Assert.Equal("သိမ်းမည်", localizer["Home_Feed_SaveTooltip"].Value);
            Assert.Equal("သိမ်းထားမှု ပယ်ဖျက်မည်", localizer["Home_Feed_UnsaveTooltip"].Value);
            Assert.Equal("နောက်ထပ် ရွေးချယ်စရာများ", localizer["Home_Feed_MoreOptions"].Value);
            Assert.Equal("ဤအကြောင်းအရာကို လောလောဆယ် မရရှိနိုင်ပါ", localizer["Home_Feed_ContentUnavailableTitle"].Value);
            Assert.Equal("မျှဝေထားသော ပူးတွဲဖိုင်", localizer["Home_Feed_SharedAttachmentAlt"].Value);
            Assert.Equal("ကူးယူမည်", localizer["Home_Feed_CopyCode"].Value);
            Assert.Equal("မီဒီယာ ပူးတွဲဖိုင် {0}", localizer["Home_Feed_MediaAttachmentAlt"].Value);
            Assert.Equal("{0} / {1}", localizer["Home_Feed_CarouselCounter"].Value);
            Assert.Equal("ပို့စ်များ ထပ်မံ ရယူနေသည်...", localizer["Home_Feed_LoadingMore"].Value);
            Assert.Equal("ဖိဒ်၏ အဆုံးသို့ ရောက်ရှိပါပြီ", localizer["Home_Feed_EndOfFeed"].Value);

            Assert.Equal("မှတ်ချက် သို့မဟုတ် နည်းပညာဆိုင်ရာ မေးမြန်းချက် ရေးရန်...", localizer["Home_Comments_Placeholder"].Value);
            Assert.Equal("တင်မည်", localizer["Home_Comments_PostBtn"].Value);
            Assert.Equal("မှတ်ချက်များ မရှိသေးပါ။", localizer["Home_Comments_Empty"].Value);
            Assert.Equal("မှတ်ချက် တင်ပြီးပါပြီ!", localizer["Home_Comments_Toast_Success"].Value);
            Assert.Equal("မှတ်ချက် တင်ရန် မအောင်မြင်ပါ: {0}", localizer["Home_Comments_Toast_Failed"].Value);

            Assert.Equal("လူပုဂ္ဂိုလ်များ", localizer["Home_Sidebar_PeopleTitle"].Value);
            Assert.Equal("အားလုံး ကြည့်မည် ({0})", localizer["Home_Sidebar_SeeAll"].Value);
            Assert.Equal("အသစ်", localizer["Home_Sidebar_RatingNew"].Value);
            Assert.Equal("အတည်ပြုပြီး အဖွဲ့ဝင်", localizer["Home_Sidebar_VerifiedMember"].Value);
            Assert.Equal("လှုပ်ရှားနေသော အဖွဲ့ဝင်", localizer["Home_Sidebar_ActiveMember"].Value);
            Assert.Equal("ချိတ်ဆက်မည်", localizer["Home_Sidebar_Connect"].Value);

            Assert.Equal("ပူးပေါင်းဆောင်ရွက်ရေး အဖွဲ့များ", localizer["Home_Sidebar_GroupsTitle"].Value);
            Assert.Equal("ရှာဖွေလေ့လာမည် ({0})", localizer["Home_Sidebar_Explore"].Value);
            Assert.Equal("အဖွဲ့ဝင် {0} ဦး • ပို့စ် {1} ခု", localizer["Home_Sidebar_GroupMeta"].Value);
            Assert.Equal("ဝင်ရောက်မည်", localizer["Home_Sidebar_Enter"].Value);
            Assert.Equal("ပူးပေါင်းဆောင်ရွက်ရေး အဖွဲ့များ မရှိသေးပါ။", localizer["Home_Sidebar_GroupsEmpty"].Value);

            Assert.Equal("လက်ရှိ မဲဆန္ဒများနှင့် စစ်တမ်းများ", localizer["Home_Sidebar_PollsTitle"].Value);
            Assert.Equal("အားလုံး ကြည့်မည်", localizer["Home_Sidebar_ViewAll"].Value);
            Assert.Equal("မဲဆန္ဒ", localizer["Home_Sidebar_PollBadge"].Value);
            Assert.Equal("မဲ {0} မဲ • {1}", localizer["Home_Sidebar_PollVotes"].Value);
            Assert.Equal("ဖွင့်ထားသည်", localizer["Home_Sidebar_PollOpen"].Value);
            Assert.Equal("ပိတ်ထားသည်", localizer["Home_Sidebar_PollClosed"].Value);
            Assert.Equal("စစ်ဆေးမည် →", localizer["Home_Sidebar_PollAudit"].Value);

            Assert.Equal("မဲဆန္ဒ အောင်မြင်စွာ ပေးပြီးပါပြီ!", localizer["Home_Feed_Toast_VoteSuccess"].Value);
            Assert.Equal("မဲပေးရန် မအောင်မြင်ပါ: {0}", localizer["Home_Feed_Toast_VoteFailed"].Value);
            Assert.Equal("ကုဒ်အပိုင်းအစကို ကလစ်ဘုတ်သို့ ကူးယူပြီးပါပြီ!", localizer["Home_Feed_Toast_CopySuccess"].Value);
            Assert.Equal("ကုဒ်အပိုင်းအစ ကူးယူရန် မအောင်မြင်ပါ: {0}", localizer["Home_Feed_Toast_CopyFailed"].Value);

            Assert.Equal("အဖွဲ့ဝင်အား Follow လုပ်ပြီးပါပြီ!", localizer["Home_Sidebar_Toast_Following"].Value);
            Assert.Equal("အဖွဲ့ဝင်အား Unfollow လုပ်လိုက်ပါပြီ။", localizer["Home_Sidebar_Toast_Unfollowed"].Value);
            Assert.Equal("Follow လုပ်ဆောင်ချက် မအောင်မြင်ပါ: {0}", localizer["Home_Sidebar_Toast_FollowFailed"].Value);
            Assert.Equal("အဖွဲ့သို့ အောင်မြင်စွာ ဝင်ရောက်ပြီးပါပြီ!", localizer["Home_Sidebar_Toast_JoinedGroup"].Value);
            Assert.Equal("အဖွဲ့သို့ ဝင်ရောက်ရန် မအောင်မြင်ပါ: {0}", localizer["Home_Sidebar_Toast_JoinGroupFailed"].Value);
            Assert.Equal("ဒက်ရှ်ဘုတ် အချက်အလက်များ ရယူရန် မအောင်မြင်ပါ။", localizer["Home_Feed_Toast_DashboardLoadFailed"].Value);
            Assert.Equal("အများပြည်သူဆိုင်ရာ ပုဂ္ဂိုလ်", localizer["Home_Sidebar_RolePublicFigure"].Value);
            Assert.Equal("နယ်ပယ် ကျွမ်းကျင်သူ", localizer["Home_Sidebar_RoleDomainPro"].Value);
            Assert.Equal("ဖန်တီးသူ", localizer["Home_Sidebar_RoleCreator"].Value);
            Assert.Equal("အသိုက်အဝန်း ခေါင်းဆောင်", localizer["Home_Sidebar_RoleLead"].Value);
            Assert.Equal("ကြီးကြပ်သူ", localizer["Home_Sidebar_RoleModerator"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
