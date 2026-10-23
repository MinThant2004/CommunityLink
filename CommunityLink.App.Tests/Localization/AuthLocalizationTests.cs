using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class AuthLocalizationTests
{
    private static readonly string[] SplitScreenKeys =
    [
        "Auth_VerifiedNodeProtocol",
        "Auth_NodeEncryptionActive",
        "Auth_Feature_TopicalCommunities",
        "Auth_Feature_RealtimePolls",
        "Auth_Feature_PeerMessaging",
        "Auth_Feature_ManageMembers",
        "Auth_Feature_RbacCompliance",
        "Auth_Feature_AuditTrail"
    ];

    private static readonly string[] UserLoginKeys =
    [
        "Auth_SignInPageTitle",
        "Auth_MemberGateway",
        "Auth_MemberLoginTitle",
        "Auth_MemberLoginSubtitle",
        "Auth_MemberPortal",
        "Auth_Node01Verified",
        "Auth_SignInHeading",
        "Auth_SignInSubheading",
        "Auth_EmailAddress",
        "Auth_Password",
        "Auth_KeepSessionActive",
        "Auth_ForgotPassword",
        "Auth_AuthenticateAndEnter",
        "Auth_DontHaveAccount",
        "Auth_CreateOneNow",
        "Auth_TogglePasswordVisibility",
        "Auth_ToggleConfirmPasswordVisibility"
    ];

    private static readonly string[] UserRegisterKeys =
    [
        "Auth_RegisterPageTitle",
        "Auth_MemberProvisioning",
        "Auth_RegisterTitle",
        "Auth_RegisterSubtitle",
        "Auth_MemberRegistry",
        "Auth_NodeProvisioning",
        "Auth_CreateMemberAccount",
        "Auth_CreateAccountSubheading",
        "Auth_FullName",
        "Auth_Username",
        "Auth_UsernameHint",
        "Auth_ConfirmPassword",
        "Auth_PasswordRequirementHint",
        "Auth_ProvisionAccount",
        "Auth_AlreadyHaveAccount",
        "Auth_SignInHere"
    ];

    private static readonly string[] AdminLoginKeys =
    [
        "Auth_AdminSignInPageTitle",
        "Auth_AdminGateway",
        "Auth_AdminLoginTitle",
        "Auth_AdminLoginSubtitle",
        "Auth_RestrictedAccess",
        "Auth_ConsoleLevel03",
        "Auth_OperatorAuthentication",
        "Auth_OperatorSubheading",
        "Auth_AdminEmail",
        "Auth_MaintainOperatorSession",
        "Auth_AuthenticateOperator",
        "Auth_NeedOperatorAccount",
        "Auth_CreatedByInvitationOnly"
    ];

    private static readonly string[] ChangePasswordKeys =
    [
        "Auth_ChangePasswordSubtitle",
        "Auth_CurrentPassword",
        "Auth_CurrentPasswordPlaceholder",
        "Auth_NewPassword",
        "Auth_NewPasswordPlaceholder",
        "Auth_ConfirmNewPassword",
        "Auth_ConfirmNewPasswordPlaceholder",
        "Auth_UpdatingPassword",
        "Auth_UpdatePassword",
        "Auth_Validation_CurrentPasswordRequired",
        "Auth_Validation_NewPasswordRequired",
        "Auth_Validation_PasswordMustBeDifferent",
        "Auth_Validation_PasswordMinLength",
        "Auth_Validation_PasswordUppercase",
        "Auth_Validation_PasswordLowercase",
        "Auth_Validation_PasswordDigit",
        "Auth_Validation_PasswordSpecialChar",
        "Auth_Validation_PasswordsDoNotMatch",
        "Auth_Success_PasswordChanged"
    ];

    private static readonly string[] ForgotPasswordKeys =
    [
        "Auth_ResetPasswordTitle",
        "Auth_StepIndicator",
        "Auth_Step1Desc",
        "Auth_Step2Desc",
        "Auth_Step3Desc",
        "Auth_AccountEmailAddress",
        "Auth_OtpHelpText",
        "Auth_SendingOtp",
        "Auth_SendVerificationCode",
        "Auth_EnterOtp",
        "Auth_OtpExpiration",
        "Auth_VerifyCode",
        "Auth_ResendOtp",
        "Auth_NewPasswordAtLeast8Placeholder",
        "Auth_ResettingPassword",
        "Auth_SetNewPassword",
        "Auth_Validation_EnterEmail",
        "Auth_OtpSentSuccess",
        "Auth_OtpTimerExpired",
        "Auth_Validation_EnterOtpCode",
        "Auth_OtpVerifiedSuccess",
        "Auth_Validation_PasswordFieldsRequired",
        "Auth_PasswordResetSuccess"
    ];

    private static readonly string[] AccessDeniedKeys =
    [
        "Auth_AccessDeniedPageTitle",
        "Auth_AccessDeniedHeading",
        "Auth_AccessDeniedDesc",
        "Auth_ReturnToHome"
    ];

    private static readonly string[] AdminSetupPasswordKeys =
    [
        "Auth_AdminPasswordSetupPageTitle",
        "Auth_AdminOnboarding",
        "Auth_AdminSetupTitle",
        "Auth_AdminSetupSubtitle",
        "Auth_InvitationAuthenticated",
        "Auth_15MinToken",
        "Auth_SetUpPasswordHeading",
        "Auth_SetUpPasswordSubheading",
        "Auth_ValidatingInvitationToken",
        "Auth_InvalidOrExpiredLink",
        "Auth_ReturnToAdminLogin",
        "Auth_PasswordConfigured",
        "Auth_AdminAccountActive",
        "Auth_GoToAdminSignIn",
        "Auth_ProvisioningEmail",
        "Auth_AdministratorType",
        "Auth_EnterAPassword",
        "Auth_ConfirmAPassword",
        "Auth_SavingPassword",
        "Auth_CompleteSetupActivate",
        "Auth_MissingInvitationToken"
    ];

    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllAuthKeys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var allAuthKeys = SplitScreenKeys
            .Concat(UserLoginKeys)
            .Concat(UserRegisterKeys)
            .Concat(AdminLoginKeys)
            .Concat(ChangePasswordKeys)
            .Concat(ForgotPasswordKeys)
            .Concat(AccessDeniedKeys)
            .Concat(AdminSetupPasswordKeys);

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // Verify English
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            foreach (var key in allAuthKeys)
            {
                var localized = localizer[key];
                Assert.False(localized.ResourceNotFound, $"Key '{key}' not found in English AppResources.resx");
                Assert.False(string.IsNullOrWhiteSpace(localized.Value), $"Key '{key}' is empty in English AppResources.resx");
            }

            // Verify Myanmar
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            foreach (var key in allAuthKeys)
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
    public void AllMyanmarAuthStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            var allAuthKeys = SplitScreenKeys
                .Concat(UserLoginKeys)
                .Concat(UserRegisterKeys)
                .Concat(AdminLoginKeys)
                .Concat(ChangePasswordKeys)
                .Concat(ForgotPasswordKeys)
                .Concat(AccessDeniedKeys)
                .Concat(AdminSetupPasswordKeys);

            foreach (var key in allAuthKeys)
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
    public void StepIndicator_FormatsCorrectlyInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            // English format
            CultureInfo.CurrentCulture = new CultureInfo("en");
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            var enTemplate = localizer["Auth_StepIndicator"];
            var enFormatted = string.Format(CultureInfo.CurrentUICulture, enTemplate.Value, 2, 3);
            Assert.Equal("STEP 2 OF 3", enFormatted);

            // Myanmar format
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            var myTemplate = localizer["Auth_StepIndicator"];
            var myFormatted = string.Format(CultureInfo.CurrentUICulture, myTemplate.Value, 2, 3);
            Assert.Equal("အဆင့် 2 / 3", myFormatted);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Fact]
    public void SpecificAuthHeadings_RenderAccurateTranslations()
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
            Assert.Equal("Sign in to account", localizer["Auth_SignInHeading"].Value);
            Assert.Equal("Create member account", localizer["Auth_CreateMemberAccount"].Value);
            Assert.Equal("Operator Authentication", localizer["Auth_OperatorAuthentication"].Value);
            Assert.Equal("Access Denied", localizer["Auth_AccessDeniedHeading"].Value);

            // Myanmar checks
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");
            Assert.Equal("အကောင့်သို့ ဝင်ရောက်မည်", localizer["Auth_SignInHeading"].Value);
            Assert.Equal("အသင်းဝင် အကောင့်အသစ် ဖွင့်မည်", localizer["Auth_CreateMemberAccount"].Value);
            Assert.Equal("စီမံခန့်ခွဲသူ အတည်ပြုခြင်း", localizer["Auth_OperatorAuthentication"].Value);
            Assert.Equal("ဝင်ရောက်ခွင့် မရှိပါ", localizer["Auth_AccessDeniedHeading"].Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }
}
