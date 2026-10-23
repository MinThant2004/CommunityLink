using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CommunityLink.App.Services;

/// <summary>
/// DelegatingHandler that injects an Accept-Language header corresponding to CultureInfo.CurrentUICulture
/// into each outgoing HttpRequestMessage, without mutating shared HttpClient.DefaultRequestHeaders.
/// </summary>
public sealed class AcceptLanguageHeaderHandler : DelegatingHandler
{
    public AcceptLanguageHeaderHandler()
    {
    }

    public AcceptLanguageHeaderHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var currentCulture = CultureInfo.CurrentUICulture;
        var canonicalCulture = currentCulture.TwoLetterISOLanguageName.Equals("my", StringComparison.OrdinalIgnoreCase)
            ? "my"
            : "en";

        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(canonicalCulture));

        return base.SendAsync(request, cancellationToken);
    }
}
