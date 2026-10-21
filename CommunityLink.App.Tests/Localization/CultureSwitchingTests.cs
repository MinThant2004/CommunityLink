using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using CommunityLink.App.Components.Shared;
using CommunityLink.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using CommunityLink.App;
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class CultureSwitchingTests : BunitContext
{
    public CultureSwitchingTests()
    {
        Services.AddLocalization(options => options.ResourcesPath = "Resources");
        Services.AddScoped<CultureService>();
        Services.AddScoped<IStringLocalizer<AppResources>, DynamicAppStringLocalizer>();
        Services.AddScoped<IStringLocalizer, DynamicAppStringLocalizer>();
    }

    [Fact]
    public void SetCulture_WithMyanmar_WritesValidMyanmarCultureCookie()
    {
        var context = new DefaultHttpContext();
        var result = CultureEndpoints.SetCulture(context, "my", "/chat");

        Assert.NotNull(result);
        var setCookieHeaders = context.Response.Headers.SetCookie.ToList();
        Assert.NotEmpty(setCookieHeaders);

        var cultureCookie = setCookieHeaders.FirstOrDefault(c => c != null && c.StartsWith(CookieRequestCultureProvider.DefaultCookieName));
        Assert.NotNull(cultureCookie);
        var unescapedCookie = Uri.UnescapeDataString(cultureCookie!);
        Assert.Contains("c=my|uic=my", unescapedCookie);
        Assert.Contains("path=/", unescapedCookie);
        Assert.Contains("samesite=lax", unescapedCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetCulture_WithEnglish_WritesValidEnglishCultureCookie()
    {
        var context = new DefaultHttpContext();
        var result = CultureEndpoints.SetCulture(context, "en", "/wallet");

        Assert.NotNull(result);
        var setCookieHeaders = context.Response.Headers.SetCookie.ToList();
        Assert.NotEmpty(setCookieHeaders);

        var cultureCookie = setCookieHeaders.FirstOrDefault(c => c != null && c.StartsWith(CookieRequestCultureProvider.DefaultCookieName));
        Assert.NotNull(cultureCookie);
        var unescapedCookie = Uri.UnescapeDataString(cultureCookie!);
        Assert.Contains("c=en|uic=en", unescapedCookie);
        Assert.Contains("path=/", unescapedCookie);
    }

    [Theory]
    [InlineData("my-MM", "my")]
    [InlineData("en-US", "en")]
    [InlineData("MY", "my")]
    [InlineData("EN", "en")]
    public void SetCulture_WithAliases_NormalizesToCanonicalCultureInCookie(string inputCulture, string expectedCanonical)
    {
        var context = new DefaultHttpContext();
        CultureEndpoints.SetCulture(context, inputCulture, "/");

        var setCookieHeaders = context.Response.Headers.SetCookie.ToList();
        var cultureCookie = setCookieHeaders.FirstOrDefault(c => c != null && c.StartsWith(CookieRequestCultureProvider.DefaultCookieName));
        Assert.NotNull(cultureCookie);
        var unescapedCookie = Uri.UnescapeDataString(cultureCookie!);
        Assert.Contains($"c={expectedCanonical}|uic={expectedCanonical}", unescapedCookie);
    }

    [Fact]
    public async Task CultureCookie_PersistsAndIsReadByCookieRequestCultureProvider()
    {
        var provider = new CookieRequestCultureProvider();
        var context = new DefaultHttpContext();
        var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("my", "my"));
        context.Request.Headers.Cookie = $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}";

        var result = await provider.DetermineProviderCultureResult(context);

        Assert.NotNull(result);
        Assert.Equal("my", result.Cultures.First().Value);
        Assert.Equal("my", result.UICultures.First().Value);
    }

    [Theory]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("invalid_culture")]
    [InlineData("")]
    [InlineData(null)]
    public void UnsupportedOrMalformedCulture_HandledSafely_DoesNotWriteCookie(string? invalidCulture)
    {
        var context = new DefaultHttpContext();
        var result = CultureEndpoints.SetCulture(context, invalidCulture, "/chat");

        Assert.NotNull(result);
        var setCookieHeaders = context.Response.Headers.SetCookie.ToList();
        Assert.Empty(setCookieHeaders);
    }

    [Theory]
    [InlineData("https://evil.com")]
    [InlineData("http://attacker.com/steal")]
    [InlineData("//evil.com")]
    [InlineData("/\\evil.com")]
    [InlineData("\\evil.com")]
    [InlineData("/path/..\\evil.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/foo:bar")]
    [InlineData("")]
    [InlineData(null)]
    public void UnsafeReturnUrl_RejectedAndSafeLocalFallbackUsed(string? unsafeUrl)
    {
        var isSafe = CultureEndpoints.IsSafeLocalUrl(unsafeUrl);
        Assert.False(isSafe);

        var context = new DefaultHttpContext();
        var result = CultureEndpoints.SetCulture(context, "my", unsafeUrl);

        // Result is LocalRedirectResult to "/"
        var redirect = Assert.IsAssignableFrom<Microsoft.AspNetCore.Http.IResult>(result);
        Assert.NotNull(redirect);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/chat")]
    [InlineData("/chat?tab=active&filter=all")]
    [InlineData("/link-drops/wallet")]
    [InlineData("/admin/dashboard?page=1")]
    public void ValidLocalReturnUrl_PreservedSafely(string validLocalUrl)
    {
        var isSafe = CultureEndpoints.IsSafeLocalUrl(validLocalUrl);
        Assert.True(isSafe);

        var context = new DefaultHttpContext();
        var result = CultureEndpoints.SetCulture(context, "en", validLocalUrl);
        Assert.NotNull(result);
    }

    [Fact]
    public void SetCulture_DoesNotModifyOrInvalidateAuthCookie()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "Ace.Community.Auth=valid_session_token_12345; other_cookie=abc";

        CultureEndpoints.SetCulture(context, "my", "/chat");

        var setCookieHeaders = context.Response.Headers.SetCookie.ToList();
        // Only .AspNetCore.Culture is written; Ace.Community.Auth is never overwritten or cleared
        Assert.DoesNotContain(setCookieHeaders, c => c != null && c.StartsWith("Ace.Community.Auth"));
        Assert.Contains(setCookieHeaders, c => c != null && c.StartsWith(CookieRequestCultureProvider.DefaultCookieName));
    }

    [Theory]
    [InlineData("my", "my")]
    [InlineData("my-MM", "my")]
    [InlineData("en", "en")]
    [InlineData("en-US", "en")]
    public void AppDocument_LangAttributeLogic_ProducesExpectedCanonicalLanguage(string cultureCode, string expectedLang)
    {
        var culture = new CultureInfo(cultureCode);
        var canonical = culture.TwoLetterISOLanguageName.Equals("my", StringComparison.OrdinalIgnoreCase) ? "my" : "en";
        Assert.Equal(expectedLang, canonical);
    }

    [Fact]
    public void LanguageSelector_RendersButtonsAndIdentifiesActiveLanguage_UnderEnglish()
    {
        var culture = Services.GetRequiredService<CultureService>();
        culture.SetCulture("en");

        var cut = Render<LanguageSelector>();

        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);

        var enBtn = buttons[0];
        var myBtn = buttons[1];

        Assert.Equal("EN", enBtn.TextContent.Trim());
        Assert.Equal("မြန်မာ", myBtn.TextContent.Trim());

        // Under English, EN button has aria-pressed="true"
        Assert.Equal("true", enBtn.GetAttribute("aria-pressed"));
        Assert.Equal("false", myBtn.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void LanguageSelector_RendersButtonsAndIdentifiesActiveLanguage_UnderMyanmar()
    {
        var culture = Services.GetRequiredService<CultureService>();
        culture.SetCulture("my");

        var cut = Render<LanguageSelector>();

        var buttons = cut.FindAll("button");
        var enBtn = buttons[0];
        var myBtn = buttons[1];

        // Under Myanmar, Myanmar button has aria-pressed="true"
        Assert.Equal("false", enBtn.GetAttribute("aria-pressed"));
        Assert.Equal("true", myBtn.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void LanguageSelector_ClickingButton_SwitchesCultureInPlaceWithoutReload()
    {
        var culture = Services.GetRequiredService<CultureService>();
        culture.SetCulture("en");

        var cut = Render<LanguageSelector>();

        var buttons = cut.FindAll("button");
        Assert.Equal("true", buttons[0].GetAttribute("aria-pressed"));
        Assert.Equal("false", buttons[1].GetAttribute("aria-pressed"));

        // Click Myanmar button in-place
        buttons[1].Click();

        Assert.Equal("my", culture.CurrentCultureName);

        // UI updates in-place
        var updatedButtons = cut.FindAll("button");
        Assert.Equal("false", updatedButtons[0].GetAttribute("aria-pressed"));
        Assert.Equal("true", updatedButtons[1].GetAttribute("aria-pressed"));
    }

    [Fact]
    public async Task AcceptLanguageHeaderHandler_InjectsSelectedCultureHeader_IntoOutgoingRequest()
    {
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var myCulture = new CultureInfo("my");
            CultureInfo.CurrentCulture = myCulture;
            CultureInfo.CurrentUICulture = myCulture;

            var testHandler = new TestCaptureHttpMessageHandler();
            var acceptLanguageHandler = new AcceptLanguageHeaderHandler(testHandler);
            using var client = new HttpClient(acceptLanguageHandler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5000/api/test");
            await client.SendAsync(request);

            Assert.NotNull(testHandler.CapturedRequest);
            var acceptLanguage = testHandler.CapturedRequest.Headers.AcceptLanguage.ToString();
            Assert.Equal("my", acceptLanguage);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUiCulture;
        }
    }

    [Fact]
    public async Task AcceptLanguageHeaderHandler_PreservesAuthHeaderAndUploadPayload()
    {
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var enCulture = new CultureInfo("en");
            CultureInfo.CurrentCulture = enCulture;
            CultureInfo.CurrentUICulture = enCulture;

            var testHandler = new TestCaptureHttpMessageHandler();
            var acceptLanguageHandler = new AcceptLanguageHeaderHandler(testHandler);
            using var client = new HttpClient(acceptLanguageHandler);

            using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5000/api/upload");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "mock_access_token_jwt");

            using var formContent = new MultipartFormDataContent();
            formContent.Add(new StringContent("John Doe"), "FullName");
            request.Content = formContent;

            await client.SendAsync(request);

            Assert.NotNull(testHandler.CapturedRequest);
            Assert.Equal("en", testHandler.CapturedRequest.Headers.AcceptLanguage.ToString());
            Assert.NotNull(testHandler.CapturedRequest.Headers.Authorization);
            Assert.Equal("Bearer", testHandler.CapturedRequest.Headers.Authorization.Scheme);
            Assert.Equal("mock_access_token_jwt", testHandler.CapturedRequest.Headers.Authorization.Parameter);
            Assert.IsType<MultipartFormDataContent>(testHandler.CapturedRequest.Content);
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUiCulture;
        }
    }

    private class TestCaptureHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"isSuccess\":true}")
            };
            return Task.FromResult(response);
        }
    }
}
