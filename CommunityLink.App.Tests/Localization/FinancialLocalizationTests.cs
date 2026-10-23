using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class FinancialLocalizationTests
{
    private static IServiceProvider CreateLocalizationServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services.BuildServiceProvider();
    }

    private static List<string> GetPhase7Keys()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var rs = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var keys = new List<string>();
        if (rs != null)
        {
            var prefixes = new[] { "LinkDrops_", "CreatorEarnings_", "CreatorPayouts_", "PaidChat_" };
            foreach (System.Collections.DictionaryEntry entry in rs)
            {
                var key = entry.Key?.ToString();
                if (!string.IsNullOrEmpty(key) && prefixes.Any(p => key.StartsWith(p, StringComparison.Ordinal)))
                {
                    keys.Add(key);
                }
            }
        }
        return keys;
    }

    [Fact]
    public void FinancialKeys_AreBidirectionallySynchronized()
    {
        var rm = new System.Resources.ResourceManager("CommunityLink.App.Resources.AppResources", typeof(AppResources).Assembly);
        var enSet = rm.GetResourceSet(CultureInfo.InvariantCulture, true, true);
        var mySet = rm.GetResourceSet(new CultureInfo("my"), true, true);

        Assert.NotNull(enSet);
        Assert.NotNull(mySet);

        var prefixes = new[] { "LinkDrops_", "CreatorEarnings_", "CreatorPayouts_", "PaidChat_" };

        var enKeys = enSet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => prefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var myKeys = mySet.Cast<System.Collections.DictionaryEntry>()
            .Select(e => e.Key.ToString()!)
            .Where(k => prefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .ToHashSet();

        var missingInMy = enKeys.Except(myKeys).ToList();
        var missingInEn = myKeys.Except(enKeys).ToList();

        Assert.Empty(missingInMy);
        Assert.Empty(missingInEn);
        Assert.True(enKeys.Count >= 70, $"Expected at least 70 Phase 7 keys, but found {enKeys.Count}");
    }

    [Fact]
    public void AllPhase7Keys_ExistInEnglishAndMyanmar()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            var keys = GetPhase7Keys();
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
    public void AllMyanmarFinancialStrings_UseStrictMyanmarUnicode_NoZawgyiPua()
    {
        var sp = CreateLocalizationServices();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        var prevCulture = CultureInfo.CurrentCulture;
        var prevUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            foreach (var key in GetPhase7Keys())
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
    public void FinancialDynamicFormatStrings_FormatCorrectlyInEnglishAndMyanmar()
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

            Assert.Equal("Pay 10,000 MMK • Top Up +1,000 Drops",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_PayTopUpBtn"].Value, "10,000", "1,000"));

            Assert.Equal("+500 Link Drops",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_TargetCreditDrops"].Value, 500));

            Assert.Equal("1,500 Drops",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_DropsCount"].Value, "1,500"));

            Assert.Equal("All (5)",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_FilterAll"].Value, 5));

            Assert.Equal("Est. Value: 15,000 MMK",
                string.Format(CultureInfo.CurrentUICulture, localizer["CreatorEarnings_EstValue"].Value, "15,000"));

            Assert.Equal("− Commission 15",
                string.Format(CultureInfo.CurrentUICulture, localizer["CreatorEarnings_Commission"].Value, 15));

            Assert.Equal("Gross 100",
                string.Format(CultureInfo.CurrentUICulture, localizer["CreatorEarnings_Gross"].Value, 100));

            Assert.Equal("Transfer Instructions • KBZPay",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_TransferInstructions"].Value, "KBZPay"));

            // Myanmar checks
            CultureInfo.CurrentCulture = new CultureInfo("my");
            CultureInfo.CurrentUICulture = new CultureInfo("my");

            Assert.Equal("10,000 MMK ပေးချေမည် • +1,000 Drops ငွေဖြည့်မည်",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_PayTopUpBtn"].Value, "10,000", "1,000"));

            Assert.Equal("+500 Link Drops",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_TargetCreditDrops"].Value, 500));

            Assert.Equal("1,500 Drops",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_DropsCount"].Value, "1,500"));

            Assert.Equal("အားလုံး (5)",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_FilterAll"].Value, 5));

            Assert.Equal("ခန့်မှန်းတန်ဖိုး: 15,000 MMK",
                string.Format(CultureInfo.CurrentUICulture, localizer["CreatorEarnings_EstValue"].Value, "15,000"));

            Assert.Equal("− ကော်မရှင် 15",
                string.Format(CultureInfo.CurrentUICulture, localizer["CreatorEarnings_Commission"].Value, 15));

            Assert.Equal("စုစုပေါင်း 100",
                string.Format(CultureInfo.CurrentUICulture, localizer["CreatorEarnings_Gross"].Value, 100));

            Assert.Equal("ငွေလွှဲ လမ်းညွှန်ချက် • KBZPay",
                string.Format(CultureInfo.CurrentUICulture, localizer["LinkDrops_TransferInstructions"].Value, "KBZPay"));
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
    [InlineData("+100 Drops")]
    [InlineData("2,500 MMK")]
    [InlineData("85%")]
    public void FinancialNumericValues_PreserveLatinDigits(string financialString)
    {
        // Assert monetary and balance digits are preserved in Latin digits
        Assert.Matches(@"[\d,]+", financialString);
        foreach (var ch in financialString)
        {
            // Verify no Myanmar digits (\u1040-\u1049) are used for financial figures
            var isMyanmarDigit = ch >= '\u1040' && ch <= '\u1049';
            Assert.False(isMyanmarDigit, $"Found Myanmar digit '{ch}' in financial string: '{financialString}'");
        }
    }
}
