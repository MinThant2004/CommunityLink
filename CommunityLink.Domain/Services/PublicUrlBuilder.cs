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
}
