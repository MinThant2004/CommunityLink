using System.Globalization;
using CommunityLink.App.Components.Shared.Chat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class ChatLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static List<string> GetAllChatKeys()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var keys = new List<string>();
        if (rs != null)
        {
            foreach (System.Collections.DictionaryEntry entry in rs)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("Chat_", StringComparison.Ordinal))
                {
                    keys.Add(key);
                }
            }
        }
        return keys;
    }

    [Fact]
    public void ChatKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enChatKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Chat_", StringComparison.Ordinal))
            .ToHashSet();

        var myChatKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Chat_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enChatKeys.Except(myChatKeys).ToList();
        var missingInEn = myChatKeys.Except(enChatKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
    }

    [Fact]
    public void AllChatKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Verify English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in GetAllChatKeys())
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Key '{key}' is empty in English AppResources.resx");
            }

            // Verify Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in GetAllChatKeys())
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
    public void AllMyanmarChatStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            foreach (var key in GetAllChatKeys())
            {
                var text = localizer[key].Value;
                foreach (var ch in text)
                {
                    // Private Use Area (E000-F8FF) used by Zawgyi fonts
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
    public void ChatDynamicLabels_FormatCorrectlyInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English checks
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");

            Assert.Equal("12 members", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_MemberCount"].Value, 12));
            Assert.Equal("50 drops", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_FeeDrops"].Value, 50));
            Assert.Equal("Replying to Alice", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_ReplyingTo"].Value, "Alice"));
            Assert.Equal("Bob is typing...", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_TypingGroup"].Value, "Bob"));
            Assert.Equal("Unlock for 100 drops", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_UnlockForDrops"].Value, 100));
            Assert.Equal("Join for 25 drops", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_JoinForDrops"].Value, 25));
            Assert.Equal("by Mg Mg", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_ByCreator"].Value, "Mg Mg"));

            // Myanmar checks
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("အသင်းဝင် 12 ဦး", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_MemberCount"].Value, 12));
            Assert.Equal("50 drops", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_FeeDrops"].Value, 50));
            Assert.Equal("Alice ထံ ပြန်စာပို့နေသည်", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_ReplyingTo"].Value, "Alice"));
            Assert.Equal("Bob စာရိုက်နေသည်...", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_TypingGroup"].Value, "Bob"));
            Assert.Equal("100 drops ဖြင့် ဖွင့်မည်", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_UnlockForDrops"].Value, 100));
            Assert.Equal("25 drops ဖြင့် ဝင်ရောက်မည်", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_JoinForDrops"].Value, 25));
            Assert.Equal("Mg Mg မှ", string.Format(CultureInfo.CurrentUICulture, localizer["Chat_ByCreator"].Value, "Mg Mg"));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ChatThreadMapper_FormatPresenceText_LocalizesCorrectly()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English Online
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enOnline = ChatThreadMapper.FormatPresenceText(true, null, localizer);
            Assert.Equal("online", enOnline);

            // Myanmar Online
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myOnline = ChatThreadMapper.FormatPresenceText(true, null, localizer);
            Assert.Equal("အွန်လိုင်း", myOnline);

            // English Offline (never active)
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enOffline = ChatThreadMapper.FormatPresenceText(false, null, localizer);
            Assert.Equal("offline", enOffline);

            // Myanmar Offline (never active)
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myOffline = ChatThreadMapper.FormatPresenceText(false, null, localizer);
            Assert.Equal("အော့ဖ်လိုင်း", myOffline);

            // Recent last-seen under Myanmar
            var recentSeen = DateTime.UtcNow.AddMinutes(-5);
            var myLastSeen = ChatThreadMapper.FormatPresenceText(false, recentSeen, localizer);
            Assert.Contains("တွေ့ရှိခဲ့သည်", myLastSeen);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ChatThreadMapper_FormatDateSeparator_LocalizesCorrectly()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var today = DateTime.Today;
            var yesterday = DateTime.Today.AddDays(-1);

            // English Today and Yesterday
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            Assert.Equal("Today", ChatThreadMapper.FormatDateSeparator(today, localizer));
            Assert.Equal("Yesterday", ChatThreadMapper.FormatDateSeparator(yesterday, localizer));

            // Myanmar Today and Yesterday
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            Assert.Equal("ယနေ့", ChatThreadMapper.FormatDateSeparator(today, localizer));
            Assert.Equal("မနေ့က", ChatThreadMapper.FormatDateSeparator(yesterday, localizer));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void UserGeneratedContent_PreservedWithoutTranslation()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            // User-generated messages, display names, and group names must remain exactly as supplied
            const string userMessage = "Hello World! မင်္ဂလာပါ";
            const string groupName = "Dev Group Myanmar";
            const string userName = "Zaw_Zaw_123";

            // Verify mapper preview format preserves content
            var preview = ChatThreadMapper.FormatPreview(userMessage, localizer);
            Assert.Equal(userMessage, preview);

            // Verify group live toast preserves group name
            var toastMsg = string.Format(localizer["Chat_Toast_GroupLive"].Value, groupName);
            Assert.Contains(groupName, toastMsg);

            // Verify member toast preserves user name
            var memberToast = string.Format(localizer["Chat_Toast_PromotedToAdmin"].Value, userName);
            Assert.Contains(userName, memberToast);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
