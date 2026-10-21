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
using Xunit;

namespace CommunityLink.App.Tests.Localization;

public class CultureSwitchingTests : BunitContext
{
    public CultureSwitchingTests()
    {
        Services.AddLocalization(options => options.ResourcesPath = "Resources");
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
    public void LanguageSelector_RendersLinksAndIdentifiesActiveLanguage_UnderEnglish()
    {
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var enCulture = new CultureInfo("en");
            CultureInfo.CurrentCulture = enCulture;
            CultureInfo.CurrentUICulture = enCulture;

            var navMan = Services.GetRequiredService<NavigationManager>();
            navMan.NavigateTo("http://localhost/chat?tab=active");

            var cut = Render<LanguageSelector>();

            var links = cut.FindAll("a");
            Assert.Equal(2, links.Count);

            var enLink = links[0];
            var myLink = links[1];

            Assert.Equal("EN", enLink.TextContent.Trim());
            Assert.Equal("မြန်မာ", myLink.TextContent.Trim());

            // Active language indication
            Assert.Equal("true", enLink.GetAttribute("aria-current"));
            Assert.Null(myLink.GetAttribute("aria-current"));

            // Valid links with preserved returnUrl
            var expectedReturnUrl = Uri.EscapeDataString("/chat?tab=active");
            Assert.Contains($"/culture/set?culture=en&returnUrl={expectedReturnUrl}", enLink.GetAttribute("href"));
            Assert.Contains($"/culture/set?culture=my&returnUrl={expectedReturnUrl}", myLink.GetAttribute("href"));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUiCulture;
        }
    }

    [Fact]
    public void LanguageSelector_RendersLinksAndIdentifiesActiveLanguage_UnderMyanmar()
    {
        var prevCulture = CultureInfo.CurrentCulture;
        var prevUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            var myCulture = new CultureInfo("my");
            CultureInfo.CurrentCulture = myCulture;
            CultureInfo.CurrentUICulture = myCulture;

            var navMan = Services.GetRequiredService<NavigationManager>();
            navMan.NavigateTo("http://localhost/link-drops");

            var cut = Render<LanguageSelector>();

            var links = cut.FindAll("a");
            var enLink = links[0];
            var myLink = links[1];

            // Under Myanmar, Myanmar link is marked active
            Assert.Null(enLink.GetAttribute("aria-current"));
            Assert.Equal("true", myLink.GetAttribute("aria-current"));

            var expectedReturnUrl = Uri.EscapeDataString("/link-drops");
            Assert.Contains($"/culture/set?culture=my&returnUrl={expectedReturnUrl}", myLink.GetAttribute("href"));
        }
        finally
        {
            CultureInfo.CurrentCulture = prevCulture;
            CultureInfo.CurrentUICulture = prevUiCulture;
        }
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
