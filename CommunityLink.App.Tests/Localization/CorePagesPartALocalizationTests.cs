using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class CorePagesPartALocalizationTests
{
    private static readonly string[] GuestKeys =
    [
        "Guest_PageTitle",
        "Guest_Nav_VerifiedNodeProtocol",
        "Guest_Nav_Features",
        "Guest_Nav_HowItWorks",
        "Guest_Nav_ToggleTheme",
        "Guest_Nav_EnterDashboard",
        "Guest_Nav_LogIn",
        "Guest_Nav_SignUpFree",
        "Guest_Hero_Badge",
        "Guest_Hero_Title",
        "Guest_Hero_TitleAccent",
        "Guest_Hero_Subtitle",
        "Guest_Hero_EnterNodeDashboard",
        "Guest_Hero_ExploreCommunities",
        "Guest_Hero_ProvisionAccount",
        "Guest_Hero_Authenticate",
        "Guest_Hero_StatGlobalHubs",
        "Guest_Hero_StatActiveNodes",
        "Guest_Hero_StatContinents",
        "Guest_Hero_ScrollToExplore",
        "Guest_Features_Tag",
        "Guest_Features_Title",
        "Guest_Features_Desc",
        "Guest_Feature_Messaging_Title",
        "Guest_Feature_Messaging_Desc",
        "Guest_Feature_Messaging_DemoLeft",
        "Guest_Feature_Messaging_DemoRight",
        "Guest_Feature_Polls_Title",
        "Guest_Feature_Polls_Desc",
        "Guest_Feature_Polls_DemoUpgrade",
        "Guest_Feature_Polls_DemoDeferred",
        "Guest_Feature_Hubs_Title",
        "Guest_Feature_Hubs_Desc",
        "Guest_Feature_Broadcast_Title",
        "Guest_Feature_Broadcast_Desc",
        "Guest_How_Tag",
        "Guest_How_Title",
        "Guest_How_Desc",
        "Guest_How_Step1_Title",
        "Guest_How_Step1_Desc",
        "Guest_How_Step2_Title",
        "Guest_How_Step2_Desc",
        "Guest_How_Step3_Title",
        "Guest_How_Step3_Desc",
        "Guest_Cta_Title",
        "Guest_Cta_Subtitle",
        "Guest_Cta_EnterHomeHub",
        "Guest_Cta_ProvisionFreeNode",
        "Guest_Cta_AuthenticateExisting",
        "Guest_Footer_Tagline"
    ];

    private static readonly string[] DashboardKeys =
    [
        "Dashboard_PageTitle",
        "Dashboard_Welcome",
        "Dashboard_WelcomeSubtitle",
        "Dashboard_Action_MyFeed",
        "Dashboard_Action_ExploreHubs",
        "Dashboard_Loading",
        "Dashboard_Error_Fallback",
        "Dashboard_Stat_MyHubs",
        "Dashboard_Stat_FeedPosts",
        "Dashboard_Stat_ActivePolls",
        "Dashboard_Stat_Messages",
        "Dashboard_Stat_UnreadCount",
        "Dashboard_Stat_Active",
        "Dashboard_Section_MyCommunities",
        "Dashboard_Action_ViewAll",
        "Dashboard_Empty_CommunitiesJoined",
        "Dashboard_Action_DiscoverHubsNow",
        "Dashboard_Card_MembersAndPosts",
        "Dashboard_Action_Open",
        "Dashboard_Section_RecommendedHubs",
        "Dashboard_Subtitle_TailoredForYou",
        "Dashboard_Empty_AllHubsJoined",
        "Dashboard_Action_Explore",
        "Dashboard_Section_RecentActivity",
        "Dashboard_Action_OpenFeed",
        "Dashboard_Empty_RecentPosts"
    ];

    private static readonly string[] InviteKeys =
    [
        "Invite_PageTitle",
        "Invite_LoadingDetails",
        "Invite_InvalidTitle",
        "Invite_Action_GoToChat",
        "Invite_Members_Count",
        "Invite_Members_Single",
        "Invite_Badge_Private",
        "Invite_Badge_Public",
        "Invite_Badge_PaidDrops",
        "Invite_Badge_Free",
        "Invite_NoLongerValid",
        "Invite_LoginPrompt",
        "Invite_Action_LogIn",
        "Invite_AlreadyMember",
        "Invite_Action_OpenChat",
        "Invite_Banned_Title",
        "Invite_Banned_Desc",
        "Invite_Banned_Help",
        "Invite_Pending_Title",
        "Invite_Pending_Desc",
        "Invite_Action_PendingApproval",
        "Invite_Declined_Title",
        "Invite_Declined_Desc",
        "Invite_Action_RequestDeclined",
        "Invite_Approved_Title",
        "Invite_Approved_Desc",
        "Invite_Action_PayDropsAndJoin",
        "Invite_Action_Processing",
        "Invite_Action_RequestToJoin",
        "Invite_Action_SubmittingRequest",
        "Invite_Action_JoiningGroup",
        "Invite_Action_JoinGroup",
        "Invite_Modal_SubjectSubtitle",
        "Invite_Error_NoToken",
        "Invite_Error_LoadFailed",
        "Invite_Error_JoinFailed",
        "Invite_Error_RequestFailed",
        "Invite_Error_PaymentFailed"
    ];

    private static readonly string[] PollsKeys =
    [
        "Polls_PageTitle",
        "Polls_Header_Title",
        "Polls_Header_Subtitle",
        "Polls_Header_CreateBtn",
        "Polls_Empty_Title",
        "Polls_Empty_Subtitle",
        "Polls_Badge_Private",
        "Polls_Badge_Closed",
        "Polls_Badge_DaysLeft",
        "Polls_Badge_HoursLeft",
        "Polls_Badge_MinutesLeft",
        "Polls_Badge_Active",
        "Polls_Menu_Tooltip",
        "Polls_Menu_Report",
        "Polls_Summary_TotalVotes",
        "Polls_Summary_MultipleAllowed",
        "Polls_Summary_SingleChoice",
        "Polls_Summary_YouVoted",
        "Polls_Summary_VotingEnded",
        "Polls_Summary_ClickToVote",
        "Polls_Action_Like",
        "Polls_Action_Comment",
        "Polls_Action_Share",
        "Polls_Comments_Placeholder",
        "Polls_Comments_PostBtn",
        "Polls_Comments_Empty",
        "Polls_Modal_CreateTitle",
        "Polls_Modal_SelectCommunity",
        "Polls_Modal_SelectCommunityDefault",
        "Polls_Modal_SelectSubCommunity",
        "Polls_Modal_NoSubCommunities",
        "Polls_Modal_SelectSubCommunityDefault",
        "Polls_Modal_DescLabel",
        "Polls_Modal_DescHint",
        "Polls_Modal_DescPlaceholder",
        "Polls_Modal_QuestionLabel",
        "Polls_Modal_QuestionPlaceholder",
        "Polls_Modal_DurationLabel",
        "Polls_Modal_DurationTotal",
        "Polls_Modal_Duration1Hour",
        "Polls_Modal_Duration12Hours",
        "Polls_Modal_Duration1Day",
        "Polls_Modal_Duration3Days",
        "Polls_Modal_Duration7Days",
        "Polls_Modal_Days",
        "Polls_Modal_Hours",
        "Polls_Modal_Minutes",
        "Polls_Modal_DurationHint",
        "Polls_Modal_OptionsLabel",
        "Polls_Modal_AddOption",
        "Polls_Modal_OptionPlaceholder",
        "Polls_Modal_PublishingBtn",
        "Polls_Modal_PublishBtn",
        "Polls_Share_Title",
        "Polls_Share_Desc",
        "Polls_Share_NoteLabel",
        "Polls_Share_NotePlaceholder",
        "Polls_Share_SharingBtn",
        "Polls_Share_ShareBtn",
        "Polls_Toast_VoteSuccess",
        "Polls_Toast_VoteFailed",
        "Polls_Toast_LikeFailed",
        "Polls_Toast_CommentFailed",
        "Polls_Toast_ShareSuccess",
        "Polls_Toast_ShareFailed",
        "Polls_Val_SelectCommunity",
        "Polls_Val_QuestionRequired",
        "Polls_Val_DurationMin",
        "Polls_Val_OptionsMin",
        "Polls_Val_OptionsUnique",
        "Polls_Toast_CreateSuccess",
        "Polls_Toast_CreateFailed"
    ];

    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllGuestKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in GuestKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Guest key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Guest key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in GuestKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Guest key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Guest key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllDashboardKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in DashboardKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Dashboard key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Dashboard key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in DashboardKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Dashboard key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Dashboard key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllInviteKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in InviteKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Invite key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Invite key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in InviteKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Invite key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Invite key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllPollsKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in PollsKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Polls key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Polls key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in PollsKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Polls key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Polls key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void Step3Formatting_WorksCorrectlyInBothLanguages()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English formatting
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            Assert.Equal("Welcome back, Alice!", string.Format(localizer["Dashboard_Welcome"], "Alice"));
            Assert.Equal("5 unread", string.Format(localizer["Dashboard_Stat_UnreadCount"], 5));
            Assert.Equal("12 members · 4 posts", string.Format(localizer["Dashboard_Card_MembersAndPosts"], 12, 4));
            Assert.Equal("25 members", string.Format(localizer["Invite_Members_Count"], 25));
            Assert.Equal("Pay 100 Drops & Join", string.Format(localizer["Invite_Action_PayDropsAndJoin"], 100));
            Assert.Equal("150 total votes", string.Format(localizer["Polls_Summary_TotalVotes"], 150));
            Assert.Equal("Total: 3d 4h 15m", string.Format(localizer["Polls_Modal_DurationTotal"], 3, 4, 15));

            // Myanmar formatting
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            Assert.Equal("ပြန်လည်ကြိုဆိုပါသည်၊ Alice!", string.Format(localizer["Dashboard_Welcome"], "Alice"));
            Assert.Equal("5 ခု မဖတ်ရသေးပါ", string.Format(localizer["Dashboard_Stat_UnreadCount"], 5));
            Assert.Equal("12 အဖွဲ့ဝင်များ · 4 ပို့စ်များ", string.Format(localizer["Dashboard_Card_MembersAndPosts"], 12, 4));
            Assert.Equal("25 အဖွဲ့ဝင်များ", string.Format(localizer["Invite_Members_Count"], 25));
            Assert.Equal("100 Drops ပေးချေပြီး ဝင်မည်", string.Format(localizer["Invite_Action_PayDropsAndJoin"], 100));
            Assert.Equal("စုစုပေါင်း မဲ 150 မဲ", string.Format(localizer["Polls_Summary_TotalVotes"], 150));
            Assert.Equal("စုစုပေါင်း- 3 ရက် 4 နာရီ 15 မိနစ်", string.Format(localizer["Polls_Modal_DurationTotal"], 3, 4, 15));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllStep3MyanmarTranslations_DoNotContainZawgyiSpecificCodepoints()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = GuestKeys
                .Concat(DashboardKeys)
                .Concat(InviteKeys)
                .Concat(PollsKeys);

            foreach (var key in allKeys)
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
}
