using System.Globalization;
using CommunityLink.App.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class InPlaceLocalizationSwitchTests
{
    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<CultureService>();
        services.AddScoped<IStringLocalizer<AppResources>, DynamicAppStringLocalizer>();
        services.AddScoped<IStringLocalizer, DynamicAppStringLocalizer>();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void DynamicLocalizer_SwitchesLanguageInPlace_WithoutReload()
    {
        var sp = CreateServices();
        var cultureService = sp.GetRequiredService<CultureService>();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        // 1. Initial culture is "en"
        cultureService.SetCulture("en");
        Assert.Equal("en", cultureService.CurrentCultureName);
        Assert.Equal("Cancel", localizer["Common_Cancel"].Value);
        Assert.Equal("Save Changes", localizer["Profile_Modal_SaveBtn"].Value);

        // 2. Switch to "my" (Myanmar) in-place without reload
        bool eventFired = false;
        cultureService.OnCultureChanged += () => eventFired = true;

        cultureService.SetCulture("my");

        Assert.True(eventFired, "OnCultureChanged event was not fired.");
        Assert.Equal("my", cultureService.CurrentCultureName);
        Assert.Equal("မလုပ်တော့ပါ", localizer["Common_Cancel"].Value);
        Assert.Equal("ပြောင်းလဲမှုများကို သိမ်းဆည်းရန်", localizer["Profile_Modal_SaveBtn"].Value);

        // 3. Switch back to "en" in-place
        cultureService.SetCulture("en");
        Assert.Equal("en", cultureService.CurrentCultureName);
        Assert.Equal("Cancel", localizer["Common_Cancel"].Value);
        Assert.Equal("Save Changes", localizer["Profile_Modal_SaveBtn"].Value);
    }

    [Fact]
    public void DynamicLocalizer_FormatsParameterizedStrings_DynamicallyInPlace()
    {
        var sp = CreateServices();
        var cultureService = sp.GetRequiredService<CultureService>();
        var localizer = sp.GetRequiredService<IStringLocalizer<AppResources>>();

        // In English
        cultureService.SetCulture("en");
        var enDesc = localizer["Profile_Unlock_DirectFeeDesc", "Ko Aung"].Value;
        Assert.Equal("Ko Aung requires a one-time unlock fee for direct 1-on-1 messaging.", enDesc);

        // In Myanmar
        cultureService.SetCulture("my");
        var myDesc = localizer["Profile_Unlock_DirectFeeDesc", "Ko Aung"].Value;
        Assert.Equal("Ko Aung ထံသို့ တိုက်ရိုက် စကားပြောဆိုနိုင်ရန် တစ်ကြိမ်တည်း သော့ဖွင့်ခ ပေးဆောင်ရန် လိုအပ်ပါသည်။", myDesc);
    }

    [Fact]
    public void CultureService_NormalizesAliases_Correctly()
    {
        var sp = CreateServices();
        var cultureService = sp.GetRequiredService<CultureService>();

        cultureService.SetCulture("my-MM");
        Assert.Equal("my", cultureService.CurrentCultureName);

        cultureService.SetCulture("en-US");
        Assert.Equal("en", cultureService.CurrentCultureName);
    }
}
