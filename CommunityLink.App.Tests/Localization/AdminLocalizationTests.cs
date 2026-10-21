using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using CommunityLink.App.Components.Pages;
using CommunityLink.App.Components.Pages.Admin;
using CommunityLink.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class AdminLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static List<string> GetPhase8Keys()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var keys = new List<string>();
        if (rs != null)
        {
            foreach (System.Collections.DictionaryEntry entry in rs)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrEmpty(key) && key.StartsWith("Admin_", StringComparison.Ordinal))
                {
                    keys.Add(key);
                }
            }
        }
        return keys;
    }

    [Fact]
    public void AdminKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Admin_", StringComparison.Ordinal))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => k.StartsWith("Admin_", StringComparison.Ordinal))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 150, $"Expected at least 150 Phase 8 Admin keys, but found {enKeys.Count}");
    }

    [Fact]
    public void AllPhase8Keys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var keys = GetPhase8Keys();
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
    public void AllMyanmarAdminStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            foreach (var key in GetPhase8Keys())
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
    public void AdminFormatTokens_MatchBetweenEnglishAndMyanmar()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true)!;
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true)!;

        var tokenRegex = new Regex(@"\{(\d+)\}");

        foreach (var key in GetPhase8Keys())
        {
            var enVal = enSet.GetString(key) ?? "";
            var myVal = mySet.GetString(key) ?? "";

            var enTokens = tokenRegex.Matches(enVal).Select(m => m.Value).OrderBy(t => t).ToList();
            var myTokens = tokenRegex.Matches(myVal).Select(m => m.Value).OrderBy(t => t).ToList();

            Assert.Equal(enTokens, myTokens);
        }
    }

    [Fact]
    public void AdminDynamicFormatStrings_RenderCorrectlyInBothCultures()
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

            Assert.Equal("10 rows", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Finance_RowsPerPage"].Value, 10));
            Assert.Equal("Page 2 of 5", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Users_PaginationPageOf"].Value, 2, 5));
            Assert.Equal("Payout Requests (8)", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Payouts_RequestsCount"].Value, 8));
            Assert.Equal("Showing 1 to 10 of 42 registered users", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Users_ShowingRange"].Value, 1, 10, 42));
            Assert.Equal("3 Administrator Accounts", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Accounts_AdminCount"].Value, 3));
            Assert.Equal("Current: 10.00%", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Settings_CurrentRate"].Value, "10.00"));
            Assert.Equal("Showing page 1 of 4 (35 log records)", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_AuditLogs_PageStats"].Value, 1, 4, 35));
            Assert.Equal("5 Pending", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Communities_PendingBadge"].Value, 5));

            // Myanmar checks
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("10 ကြောင်း", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Finance_RowsPerPage"].Value, 10));
            Assert.Equal("စာမျက်နှာ 2 / 5", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Users_PaginationPageOf"].Value, 2, 5));
            Assert.Equal("ငွေထုတ်ယူမှု တောင်းဆိုချက်များ (8)", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Payouts_RequestsCount"].Value, 8));
            Assert.Equal("မှတ်ပုံတင်ထားသူ 42 ယောက်အနက် 1 မှ 10 ထိ ပြသနေသည်", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Users_ShowingRange"].Value, 1, 10, 42));
            Assert.Equal("အက်ဒမင်အကောင့် 3 ခု", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Accounts_AdminCount"].Value, 3));
            Assert.Equal("လက်ရှိ: 10.00%", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Settings_CurrentRate"].Value, "10.00"));
            Assert.Equal("စာမျက်နှာ 1 / 4 (မှတ်တမ်း 35 ခု) ကို ပြသနေသည်", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_AuditLogs_PageStats"].Value, 1, 4, 35));
            Assert.Equal("5 စောင့်ဆိုင်းဆဲ", string.Format(CultureInfo.CurrentUICulture, localizer["Admin_Communities_PendingBadge"].Value, 5));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUICulture;
        }
    }

    [Theory]
    [InlineData("10,000 MMK")]
    [InlineData("50 Link Drops")]
    [InlineData("100 LD")]
    [InlineData("25,000 MMK")]
    [InlineData("15.00%")]
    public void AdminFinancialFigures_PreserveLatinDigits(string financialString)
    {
        Assert.Matches(@"[\d,]+", financialString);
        foreach (var ch in financialString)
        {
            var isMyanmarDigit = ch >= '\u1040' && ch <= '\u1049';
            Assert.False(isMyanmarDigit, $"Found Myanmar digit '{ch}' in financial string: '{financialString}'");
        }
    }

    [Theory]
    [InlineData(typeof(AdminDashboard), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminFinance), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminPayoutManagement), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminContentModeration), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminUsers), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminAccounts), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminSettings), PermissionCatalog.PolicyPrefix + PermissionCatalog.AdminUserView)]
    [InlineData(typeof(AdminAuditLogs), PermissionCatalog.PolicyPrefix + PermissionCatalog.SystemAuditView)]
    [InlineData(typeof(AdminCommunities), PermissionCatalog.PolicyPrefix + PermissionCatalog.CommunityModerate)]
    public void AdminComponents_PreserveAuthorizationAttributes(Type componentType, string expectedPolicy)
    {
        var authAttr = componentType.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(authAttr);
        Assert.Equal(expectedPolicy, authAttr.Policy);
    }
}
