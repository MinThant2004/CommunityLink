using CommunityLink.Shared;
using Microsoft.Extensions.Configuration;

namespace CommunityLink.Domain.Services;

/// <summary>
/// Builds absolute URLs for files the API writes under its own wwwroot.
///
/// The Blazor App serves the pages, so a relative "/uploads/..." is resolved by the browser
/// against the App's origin and 404s: the App has a different wwwroot from the API. Every
/// stored-file URL must therefore be absolute.
/// </summary>
public interface IPublicUrlBuilder
{
    string Build(string relativePath);

    /// <summary>
    /// Inverse of <see cref="Build"/>: strips the configured public base URL back to a
    /// wwwroot-relative path so a stored file can be located on disk again. Returns null for
    /// input that is not one of our URLs (an empty string, a data: URL, a foreign origin).
    /// </summary>
    string? ToRelativePath(string publicUrl);
}

public sealed class PublicUrlBuilder(IConfiguration configuration) : IPublicUrlBuilder
{
    private readonly string _publicBaseUrl = (configuration["ApiBase:PublicBaseUrl"] ?? string.Empty).TrimEnd('/');

    public string Build(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return relativePath;

        // Already absolute (or a data: URL) — leave it alone.
        if (Uri.TryCreate(relativePath, UriKind.Absolute, out _)) return relativePath;

        return $"{_publicBaseUrl}/{relativePath.TrimStart('/')}";
    }

    public string? ToRelativePath(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl)) return null;
        if (Uri.TryCreate(publicUrl, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme is not ("http" or "https")) return null;
        }
        else if (!publicUrl.StartsWith('/'))
        {
            return null;
        }

        if (string.IsNullOrEmpty(_publicBaseUrl)) return publicUrl;

        return publicUrl.StartsWith(_publicBaseUrl + "/", StringComparison.OrdinalIgnoreCase)
            ? publicUrl[_publicBaseUrl.Length..]
            : null;
    }
}
