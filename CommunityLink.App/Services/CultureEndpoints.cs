using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;

namespace CommunityLink.App.Services;

public static class CultureEndpoints
{
    public const string DefaultCulture = "en";
    public const string MyanmarCulture = "my";

    private static readonly HashSet<string> EnglishAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "en", "en-US"
    };

    private static readonly HashSet<string> MyanmarAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        "my", "my-MM"
    };

    public static IEndpointRouteBuilder MapCultureEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/culture/set", SetCulture);
        return endpoints;
    }

    public static IResult SetCulture(HttpContext context, string? culture, string? returnUrl)
    {
        var target = IsSafeLocalUrl(returnUrl) ? returnUrl! : "/";

        if (TryNormalizeCulture(culture, out var canonicalCulture))
        {
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(canonicalCulture, canonicalCulture));

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Secure = context.Request.IsHttps,
                HttpOnly = false
            };

            context.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                cookieValue,
                cookieOptions);
        }

        return Results.LocalRedirect(target);
    }

    /// <summary>
    /// Normalizes culture aliases to canonical culture names ("en" or "my").
    /// Rejects unsupported, empty, or malformed culture values safely.
    /// </summary>
    public static bool TryNormalizeCulture(string? input, out string canonicalCulture)
    {
        canonicalCulture = DefaultCulture;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        if (EnglishAliases.Contains(trimmed))
        {
            canonicalCulture = DefaultCulture;
            return true;
        }

        if (MyanmarAliases.Contains(trimmed))
        {
            canonicalCulture = MyanmarCulture;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Validates that a URL is a safe local relative URL.
    /// Rejects protocol-relative URLs (//), backslash bypasses (/\ or \\), external URLs,
    /// and paths containing URI schemes.
    /// </summary>
    public static bool IsSafeLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Must start with a single forward slash
        if (!url.StartsWith('/') || url.StartsWith("//", StringComparison.Ordinal) || url.StartsWith("/\\", StringComparison.Ordinal))
            return false;

        // Must not contain backslashes anywhere in the string
        if (url.Contains('\\'))
            return false;

        // Must not have a URI scheme before query string or anchor (e.g. /foo:bar)
        var pathOnly = url.Split('?', '#')[0];
        if (pathOnly.Contains(':'))
            return false;

        return Uri.TryCreate(url, UriKind.Relative, out _);
    }
}
