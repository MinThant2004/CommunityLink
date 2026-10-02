namespace CommunityLink.Shared.Features.ChatGroup;

/// <summary>
/// Single source of truth for what may be uploaded as a Chat Group avatar. The client uses
/// <see cref="AcceptAttribute"/> to filter the file picker and <see cref="MaxBytes"/> to fail
/// fast; the API re-runs the same checks because a browser filter is not a control.
/// <para>
/// Scoped to PUBLIC group avatars. Private chat attachments go through
/// <c>ChatAttachmentPolicy</c> and must not be routed through this policy.
/// </para>
/// </summary>
public static class ChatGroupImagePolicy
{
    /// <summary>5 MB. A group avatar is a single framed image and never needs more.</summary>
    public const long MaxBytes = 5L * 1024 * 1024;

    /// <summary>Formats accepted for a group avatar, lower-case and dot-prefixed.</summary>
    public static readonly IReadOnlyList<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>Value for the file picker's <c>accept</c> attribute.</summary>
    public const string AcceptAttribute = "image/jpeg,image/png,image/webp,.jpg,.jpeg,.png,.webp";

    /// <summary>Lower-cased extension including the leading dot, or empty when absent.</summary>
    public static string GetExtension(string? fileName) =>
        string.IsNullOrWhiteSpace(fileName) ? string.Empty : Path.GetExtension(fileName.Trim()).ToLowerInvariant();

    /// <summary>
    /// The extension from the allow-list matching <paramref name="fileName"/>, or null when the
    /// name is not an allowed image. The returned value is the only part of the client-supplied
    /// name that is ever used to build a storage path.
    /// </summary>
    public static string? ResolveAllowedExtension(string? fileName)
    {
        var extension = GetExtension(fileName);
        return AllowedExtensions.Contains(extension) ? extension : null;
    }

    public static bool IsAllowed(string? fileName) => ResolveAllowedExtension(fileName) is not null;

    /// <summary>Formats the allow-list for user-facing rejection messages.</summary>
    public static string AllowedExtensionsDisplay => string.Join(", ", AllowedExtensions);
}
