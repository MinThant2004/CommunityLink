using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class CorePagesPartBLocalizationTests
{
    private static readonly string[] UpgradeKeys =
    [
        "Upgrade_PageTitle",
        "Upgrade_Nav_Platform",
        "Upgrade_Nav_TierMembership",
        "Upgrade_Strip_EscrowProtocol",
        "Upgrade_Strip_EscrowActive",
        "Upgrade_Nav_PremiumMembership",
        "Upgrade_Hero_VerifiedTiers",
        "Upgrade_Hero_YourBalance",
        "Upgrade_Hero_DropsCount",
        "Upgrade_Hero_TopUp",
        "Upgrade_Hero_Title",
        "Upgrade_Hero_Subtitle",
        "Upgrade_Billing_Monthly",
        "Upgrade_Billing_Annual",
        "Upgrade_Billing_Month",
        "Upgrade_Billing_Year",
        "Upgrade_Status_ActiveTitle",
        "Upgrade_Status_ActiveDesc",
        "Upgrade_Status_VerifiedActive",
        "Upgrade_Status_PendingTitle",
        "Upgrade_Status_PendingDesc",
        "Upgrade_Status_PendingBadge",
        "Upgrade_Plan_DefaultTierBadge",
        "Upgrade_Plan_NormalUserTitle",
        "Upgrade_Plan_NormalUserDesc",
        "Upgrade_Plan_DropsForever",
        "Upgrade_Plan_NormalParticipation",
        "Upgrade_Plan_Perk_DiscoverFeeds",
        "Upgrade_Plan_Perk_JoinCommunities",
        "Upgrade_Plan_Perk_CreateArticles",
        "Upgrade_Plan_Perk_DirectMessaging",
        "Upgrade_Plan_Perk_NoAdvisory",
        "Upgrade_Plan_Perk_NoVerifiedBadge",
        "Upgrade_Plan_Btn_StandardDefaultTier",
        "Upgrade_Plan_Btn_CurrentPlan",
        "Upgrade_Plan_DomainProBadge",
        "Upgrade_Plan_PractitionerTier",
        "Upgrade_Plan_DomainProTitle",
        "Upgrade_Plan_DomainProDesc",
        "Upgrade_Plan_DropsPerInterval",
        "Upgrade_Plan_BilledInDrops",
        "Upgrade_Plan_NotActive",
        "Upgrade_Plan_DomainProPerkFallback",
        "Upgrade_Plan_Btn_CurrentActiveTier",
        "Upgrade_Plan_Btn_AuditPendingReview",
        "Upgrade_Plan_Btn_UpgradeToDomainPro",
        "Upgrade_Plan_Btn_PlanUnavailable",
        "Upgrade_Plan_PublicFigureBadge",
        "Upgrade_Plan_LuminaryTier",
        "Upgrade_Plan_PublicFigureTitle",
        "Upgrade_Plan_PublicFigureDesc",
        "Upgrade_Plan_PublicFigurePerkFallback",
        "Upgrade_Plan_Btn_ApplyForVIP",
        "Upgrade_Sop_Badge",
        "Upgrade_Sop_Title",
        "Upgrade_Sop_Subtitle",
        "Upgrade_Sop_Step1_Title",
        "Upgrade_Sop_Step1_Desc",
        "Upgrade_Sop_Step2_Title",
        "Upgrade_Sop_Step2_Desc",
        "Upgrade_Sop_Step3_Title",
        "Upgrade_Sop_Step3_Desc",
        "Upgrade_Sop_Step4_Title",
        "Upgrade_Sop_Step4_Desc",
        "Upgrade_Modal_Badge",
        "Upgrade_Modal_Title",
        "Upgrade_Modal_Duration",
        "Upgrade_Modal_DropsCost",
        "Upgrade_Modal_EscrowDeposit",
        "Upgrade_Modal_LegalNameLabel",
        "Upgrade_Modal_LegalNamePlaceholder",
        "Upgrade_Modal_WorkEmailLabel",
        "Upgrade_Modal_WorkEmailPlaceholder",
        "Upgrade_Modal_PortfolioUrlLabel",
        "Upgrade_Modal_PortfolioUrlPlaceholder",
        "Upgrade_Modal_IdFrontLabel",
        "Upgrade_Modal_IdFrontHint",
        "Upgrade_Modal_FileSelected",
        "Upgrade_Modal_IdBackLabel",
        "Upgrade_Modal_EscrowPayment",
        "Upgrade_Modal_EscrowDeducted",
        "Upgrade_Modal_SufficientBalance",
        "Upgrade_Modal_InsufficientDrops",
        "Upgrade_Modal_Required",
        "Upgrade_Modal_YourBalance",
        "Upgrade_Modal_Remaining",
        "Upgrade_Modal_NeedMoreDrops",
        "Upgrade_Modal_TopUpBtn",
        "Upgrade_Modal_EscrowProtectionLabel",
        "Upgrade_Modal_EscrowProtectionDesc",
        "Upgrade_Modal_Submitting",
        "Upgrade_Modal_SubmitBtn",
        "Upgrade_Error_FrontIdSize",
        "Upgrade_Error_BackIdSize",
        "Upgrade_Error_LegalNameRequired",
        "Upgrade_Error_FrontIdRequired",
        "Upgrade_Error_InsufficientDrops",
        "Upgrade_Success_Submitted",
        "Upgrade_Error_Failed"
    ];

    private static readonly string[] ProfileKeys =
    [
        "Profile_PageTitle_User",
        "Profile_PageTitle_Default",
        "Profile_Avatar_Uploading",
        "Profile_Avatar_UploadNewTitle",
        "Profile_Avatar_Change",
        "Profile_Avatar_UploadPhotoTitle",
        "Profile_Status_OnlineTitle",
        "Profile_Badge_PublicFigureVIP",
        "Profile_Badge_DomainPro",
        "Profile_Badge_Governor",
        "Profile_Badge_CredentialAudited",
        "Profile_Action_Follow",
        "Profile_Action_Following",
        "Profile_Action_Chat",
        "Profile_Action_ChatDrops",
        "Profile_Action_SendMessage",
        "Profile_Action_Save",
        "Profile_Action_Saved",
        "Profile_Action_Rate",
        "Profile_Action_EditReview",
        "Profile_Action_EditProfile",
        "Profile_Action_CopyLinkTitle",
        "Profile_Availability_Default",
        "Profile_Stats_Followers",
        "Profile_Stats_Following",
        "Profile_Stats_PublishedGuides",
        "Profile_Stats_CommunitiesLed",
        "Profile_Reputation_RatingsCount",
        "Profile_Reputation_ZeroRatings",
        "Profile_Reputation_VerifiedMember",
        "Profile_Reputation_ActiveMember",
        "Profile_Reputation_PeerRatings",
        "Profile_Reputation_NoPeerRatings",
        "Profile_Reputation_ActiveNow",
        "Profile_Reputation_StandardDispatch",
        "Profile_Reputation_MemberResponseStatus",
        "Profile_Reputation_AuditedNode",
        "Profile_Reputation_StandardContributor",
        "Profile_Reputation_ReputationTier",
        "Profile_Tab_PostsPolls",
        "Profile_Tab_Reviews",
        "Profile_Tab_Communities",
        "Profile_Tab_Saved",
        "Profile_Tab_Followers",
        "Profile_Tab_RecycleBin",
        "Profile_Tab_Filter",
        "Profile_Tab_Latest",
        "Profile_Feed_Syncing",
        "Profile_Feed_EmptySavedTitle",
        "Profile_Feed_EmptyPostsTitle",
        "Profile_Feed_EmptySavedDesc",
        "Profile_Feed_EmptyPostsDesc",
        "Profile_Post_SharedBy",
        "Profile_Post_PrivateTooltip",
        "Profile_Post_PrivateBadge",
        "Profile_Post_SaveTooltip",
        "Profile_Post_UnsaveTooltip",
        "Profile_Post_OptionsTooltip",
        "Profile_Post_AttachedFiles",
        "Profile_Post_CharsClickToRead",
        "Profile_Post_ClickToExpand",
        "Profile_Post_TextBadge",
        "Profile_Post_SharedPostHeader",
        "Profile_Post_LikesCount",
        "Profile_Post_CommentsCount",
        "Profile_Post_SharesCount",
        "Profile_Post_ActionLike",
        "Profile_Post_ActionComment",
        "Profile_Post_ActionShare",
        "Profile_Post_WriteCommentPlaceholder",
        "Profile_Post_SubmitCommentBtn",
        "Profile_Post_NoComments",
        "Profile_Reviews_Empty",
        "Profile_Review_EditBtn",
        "Profile_Review_DeleteBtn",
        "Profile_Communities_Empty",
        "Profile_Communities_VisitBtn",
        "Profile_Followers_Empty",
        "Profile_Followers_ViewBtn",
        "Profile_Saved_EmptyTitle",
        "Profile_Saved_EmptyDesc",
        "Profile_Recycle_EmptyTitle",
        "Profile_Recycle_EmptyDesc",
        "Profile_Recycle_ItemCount",
        "Profile_Recycle_RecycledAt",
        "Profile_Recycle_RestoreBtn",
        "Profile_Recycle_DeleteForeverTitle",
        "Profile_Recycle_DeleteForeverBtn",
        "Profile_Widget_HubsTitle",
        "Profile_Widget_HubsViewAll",
        "Profile_Widget_HubsEmpty",
        "Profile_Widget_HubsMembers",
        "Profile_Widget_HubsViewBtn",
        "Profile_Widget_SkillsTitle",
        "Profile_Widget_SkillsPeerVerified",
        "Profile_Widget_SkillsEndorseTooltip",
        "Profile_Widget_SkillsEmpty",
        "Profile_Audit_Title",
        "Profile_Audit_Desc",
        "Profile_Audit_IdLabel",
        "Profile_Audit_AuthorityLabel",
        "Profile_Audit_StatusLabel",
        "Profile_Audit_StatusActive",
        "Profile_Modal_RateEditTitle",
        "Profile_Modal_RateTitle",
        "Profile_Modal_RateSubtitle",
        "Profile_Modal_RateFeedbackLabel",
        "Profile_Modal_RatePlaceholder",
        "Profile_Modal_RateDeleting",
        "Profile_Modal_RateDeleteBtn",
        "Profile_Modal_RateSubmitting",
        "Profile_Modal_RateUpdateBtn",
        "Profile_Modal_RateSubmitBtn",
        "Profile_Modal_EditTitle",
        "Profile_Modal_EditPicLabel",
        "Profile_Modal_EditPicHint",
        "Profile_Modal_EditPicUploading",
        "Profile_Modal_EditPicUploadBtn",
        "Profile_Modal_EditNameLabel",
        "Profile_Modal_EditUsernameLabel",
        "Profile_Modal_EditHeadlineLabel",
        "Profile_Modal_EditHeadlinePlaceholder",
        "Profile_Modal_EditPronounsLabel",
        "Profile_Modal_EditPronounsPlaceholder",
        "Profile_Modal_EditLocationLabel",
        "Profile_Modal_EditLocationPlaceholder",
        "Profile_Modal_EditBioLabel",
        "Profile_Modal_EditBioPlaceholder",
        "Profile_Modal_EditSkillsLabel",
        "Profile_Modal_EditSkillsPlaceholder",
        "Profile_Modal_EditAvailabilityLabel",
        "Profile_Modal_EditAvailabilityPlaceholder",
        "Profile_Modal_EditLinksHeader",
        "Profile_Modal_EditGithubLabel",
        "Profile_Modal_EditLinkedinLabel",
        "Profile_Modal_EditTwitterLabel",
        "Profile_Modal_EditWebsiteLabel",
        "Profile_Modal_EditSaveBtn",
        "Profile_DeleteModal_Title",
        "Profile_DeleteModal_Message",
        "Profile_DeleteModal_ConfirmBtn",
        "Profile_Unlock_Badge",
        "Profile_Unlock_Title",
        "Profile_Unlock_Desc",
        "Profile_Unlock_FeeLabel",
        "Profile_Unlock_FeeAmount",
        "Profile_Unlock_BalanceLabel",
        "Profile_Unlock_BalanceAmount",
        "Profile_Unlock_NeedMoreDrops",
        "Profile_Unlock_Unlocking",
        "Profile_Unlock_PayBtn",
        "Profile_Unlock_TopUpBtn",
        "Profile_TextModal_Subtitle",
        "Profile_TextModal_EscHint",
        "Profile_ShareModal_Title",
        "Profile_ShareModal_NoteLabel",
        "Profile_ShareModal_Placeholder",
        "Profile_ShareModal_Sharing",
        "Profile_ShareModal_ShareNowBtn",
        "Profile_BannedModal_Desc",
        "Profile_BannedModal_ReasonLabel",
        "Profile_BannedModal_AppealDesc",
        "Profile_EditPost_Title",
        "Profile_EditPost_ContentLabel",
        "Profile_EditPost_Placeholder",
        "Profile_EditPost_Note",
        "Profile_EditPost_SaveBtn",
        "Profile_Toast_PostPrivate",
        "Profile_Toast_PostPublic",
        "Profile_Toast_UpdatePrivacyError",
        "Profile_Toast_PostRecycled",
        "Profile_Toast_RecyclePostError",
        "Profile_Toast_PostUpdated",
        "Profile_Toast_UpdatePostError",
        "Profile_Toast_PostSaved",
        "Profile_Toast_PostUnsaved",
        "Profile_Toast_PostSaveError",
        "Profile_Error_LoadFailed",
        "Profile_Toast_SkillEndorsed",
        "Profile_Toast_SkillEndorsementRemoved",
        "Profile_Toast_SkillError",
        "Profile_Toast_VoteRecorded",
        "Profile_Toast_VoteError",
        "Profile_Toast_AccountSaved",
        "Profile_Toast_AccountUnsaved",
        "Profile_Toast_AccountSaveError",
        "Profile_Toast_FollowingUser",
        "Profile_Toast_UnfollowedUser",
        "Profile_Toast_FollowError",
        "Profile_Toast_LinkCopied",
        "Profile_Toast_LinkInfo",
        "Profile_Toast_AvatarSizeError",
        "Profile_Toast_AvatarUpdated",
        "Profile_Toast_AvatarUploadError",
        "Profile_Toast_ProfileUpdated",
        "Profile_Toast_ProfileUpdateError",
        "Profile_Toast_RatingSubmitted",
        "Profile_Toast_RatingRemoved",
        "Profile_Toast_RatingError",
        "Profile_Toast_PostRestored",
        "Profile_Toast_RestorePostError",
        "Profile_Toast_PostDeleted",
        "Profile_Toast_DeletePostError",
        "Profile_Toast_PrivateChatUnlocked",
        "Profile_Toast_PrivateChatUnlockFailed",
        "Profile_Toast_PrivateChatError",
        "Profile_Toast_CommentPosted",
        "Profile_Toast_PostCommentError",
        "Profile_Toast_PostShared",
        "Profile_Toast_SharePostError"
    ];

    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllUpgradeKeys_ExistInBothCultures_AndAreNonEmpty()
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
            foreach (var key in UpgradeKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Upgrade key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Upgrade key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in UpgradeKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Upgrade key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Upgrade key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void AllProfileKeys_ExistInBothCultures_AndAreNonEmpty()
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
            foreach (var key in ProfileKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Profile key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Profile key '{key}' is empty in English AppResources.resx");
            }

            // Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in ProfileKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Profile key '{key}' not found in Myanmar AppResources.my.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Profile key '{key}' is empty in Myanmar AppResources.my.resx");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void UpgradeKeys_WithPlaceholders_FormatSymmetrically()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enStatus = localizer["Upgrade_Status_ActiveTitle", "Domain Pro"].Value;
            Assert.Contains("Domain Pro", enStatus);

            var enDrops = localizer["Upgrade_Hero_DropsCount", "1,500"].Value;
            Assert.Contains("1,500", enDrops);

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myStatus = localizer["Upgrade_Status_ActiveTitle", "Domain Pro"].Value;
            Assert.Contains("Domain Pro", myStatus);

            var myDrops = localizer["Upgrade_Hero_DropsCount", "1,500"].Value;
            Assert.Contains("1,500", myDrops);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void ProfileKeys_WithPlaceholders_FormatSymmetrically()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enPage = localizer["Profile_PageTitle_User", "Elena Rostova", "elena"].Value;
            Assert.Contains("Elena Rostova", enPage);
            Assert.Contains("elena", enPage);

            var enFollow = localizer["Profile_Toast_FollowingUser", "elena"].Value;
            Assert.Contains("elena", enFollow);

            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myPage = localizer["Profile_PageTitle_User", "Elena Rostova", "elena"].Value;
            Assert.Contains("Elena Rostova", myPage);
            Assert.Contains("elena", myPage);

            var myFollow = localizer["Profile_Toast_FollowingUser", "elena"].Value;
            Assert.Contains("elena", myFollow);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void MyanmarTranslations_ContainNoZawgyiPuaCharacters()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allKeys = UpgradeKeys.Concat(ProfileKeys);
            foreach (var key in allKeys)
            {
                var value = localizer[key].Value;
                foreach (var ch in value)
                {
                    int codePoint = ch;
                    Assert.False(codePoint >= 0xE000 && codePoint <= 0xF8FF,
                        $"Key '{key}' contains a Zawgyi PUA character: 0x{codePoint:X4} in value '{value}'");
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
