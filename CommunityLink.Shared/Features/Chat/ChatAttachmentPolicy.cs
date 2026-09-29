namespace CommunityLink.Shared.Features.Chat;

/// <summary>
/// Single source of truth for what may be attached to a chat message. The client uses
/// <see cref="AcceptAttribute"/> to filter the file picker and <see cref="MaxBytes"/> to
/// fail fast; the API re-runs the same checks because a browser filter is not a control.
/// </summary>
public static class ChatAttachmentPolicy
{
    public const long MaxBytes = 25L * 1024 * 1024;

    public const string MessageTypeText = "TEXT";
    public const string MessageTypeImage = "IMAGE";
    public const string MessageTypeVideo = "VIDEO";
    public const string MessageTypeFile = "FILE";

    public static readonly IReadOnlyList<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    public static readonly IReadOnlyList<string> VideoExtensions = [".mp4", ".webm", ".mov", ".m4v"];
    public static readonly IReadOnlyList<string> DocumentExtensions = [".txt", ".pdf"];

    /// <summary>Value for the input element's <c>accept</c> attribute.</summary>
    public const string AcceptAttribute =
        "image/jpeg,image/png,image/webp,image/gif," +
        "video/mp4,video/webm,video/quicktime," +
        ".txt,.pdf";

    /// <summary>Lower-cased extension including the leading dot, or an empty string when absent.</summary>
    public static string GetExtension(string? fileName) =>
        string.IsNullOrWhiteSpace(fileName) ? string.Empty : Path.GetExtension(fileName.Trim()).ToLowerInvariant();

    /// <summary>
    /// Maps an attachment to the message type used by both the private and group chat
    /// pipelines. Extension wins over the browser-supplied content type because browsers
    /// report unreliable MIME types on Windows (an .mp4 is often sent as
    /// <c>application/octet-stream</c>).
    /// </summary>
    public static string Classify(string? fileName, string? contentType)
    {
        var extension = GetExtension(fileName);

        if (ImageExtensions.Contains(extension)) return MessageTypeImage;
        if (VideoExtensions.Contains(extension)) return MessageTypeVideo;
        if (DocumentExtensions.Contains(extension)) return MessageTypeFile;

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return MessageTypeImage;
            if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return MessageTypeVideo;
        }

        return MessageTypeFile;
    }

    public static bool IsAllowed(string? fileName)
    {
        var extension = GetExtension(fileName);
        if (extension.Length == 0) return false;

        return ImageExtensions.Contains(extension)
            || VideoExtensions.Contains(extension)
            || DocumentExtensions.Contains(extension);
    }

    /// <summary>Formats the extension allow-list for user-facing rejection messages.</summary>
    public static string AllowedExtensionsDisplay =>
        string.Join(", ", ImageExtensions.Concat(VideoExtensions).Concat(DocumentExtensions));
}
