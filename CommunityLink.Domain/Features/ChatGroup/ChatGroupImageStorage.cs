using CommunityLink.Domain.Services;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.ChatGroup;

namespace CommunityLink.Domain.Features.ChatGroup;

/// <summary>
/// Stores PUBLIC Chat Group avatars. Distinct from <c>IChatAttachmentStorage</c>, which handles
/// private 1:1 chat attachments and lives in a different feature folder on purpose: group avatars
/// are fetched by unauthenticated clients as static files, so the two must never share a policy.
/// </summary>
public interface IChatGroupImageStorage
{
    /// <summary>
    /// Validates the upload and writes it under the API's wwwroot, returning the absolute public
    /// URL to persist on the group row. The caller owns the returned file and should delete it
    /// via <see cref="DeleteAsync"/> when it is replaced or rolled back.
    /// </summary>
    Task<Result<ChatGroupImageUploadResponse>> SaveAsync(
        Stream fileStream,
        string fileName,
        string? contentType,
        long declaredLength,
        CancellationToken cancellationToken = default);

    /// <summary>Best-effort removal of a previously stored avatar. Never throws.</summary>
    Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default);
}

public sealed class ChatGroupImageStorage(IPublicUrlBuilder publicUrlBuilder) : IChatGroupImageStorage
{
    /// <summary>Enough leading bytes to identify every allowed format.</summary>
    private const int SniffLength = 12;

    public async Task<Result<ChatGroupImageUploadResponse>> SaveAsync(
        Stream fileStream,
        string fileName,
        string? contentType,
        long declaredLength,
        CancellationToken cancellationToken = default)
    {
        // Order matters: reject on the cheap, purely-declared facts before buffering any bytes.
        var extension = ChatGroupImagePolicy.ResolveAllowedExtension(fileName);
        if (extension is null)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                $"Unsupported image type. Allowed: {ChatGroupImagePolicy.AllowedExtensionsDisplay}.",
                ResultStatus.ValidationError);
        }

        if (declaredLength <= 0)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                "The selected image is empty.", ResultStatus.ValidationError);
        }

        if (declaredLength > ChatGroupImagePolicy.MaxBytes)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                "Image is too large. The maximum is 5 MB.", ResultStatus.ValidationError);
        }

        // The declared Content-Type is client-supplied and therefore advisory only. Sniff the
        // real bytes: a renamed .exe is served from a public static folder, so the extension
        // check alone is not enough. Content that is not an allowed image is rejected outright.
        var sniffedExtension = await SniffImageExtensionAsync(fileStream, cancellationToken);
        if (sniffedExtension is null)
        {
            return Result<ChatGroupImageUploadResponse>.Failure(
                "The file does not contain a valid JPEG, PNG, or WebP image.", ResultStatus.ValidationError);
        }

        // A PNG renamed to .jpg is harmless, but it means the two disagree, so the sniffed
        // format wins for naming. Persisting the sniffed extension keeps static serving honest.
        if (!string.Equals(sniffedExtension, extension, StringComparison.OrdinalIgnoreCase))
        {
            extension = sniffedExtension;
        }

        var now = DateTime.UtcNow;
        var relativeDirectory = Path.Combine("uploads", "chat-groups");
        var absoluteDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        // Server-generated name: no user input, no path separators, and a fresh GUID so a
        // replaced avatar can never collide with or overwrite the file it supersedes.
        var storedName = $"group-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(absoluteDirectory, storedName);

        await using (var destination = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true))
        {
            await fileStream.CopyToAsync(destination, cancellationToken);
        }

        var publicPath = $"/{relativeDirectory.Replace(Path.DirectorySeparatorChar, '/')}/{storedName}";

        return Result<ChatGroupImageUploadResponse>.Success(new ChatGroupImageUploadResponse(
            Url: publicUrlBuilder.Build(publicPath),
            FileName: Path.GetFileName(fileName),
            FileSizeByte: declaredLength));
    }

    public Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return Task.CompletedTask;
        }

        try
        {
            var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            var candidate = publicUrlBuilder.ToRelativePath(publicUrl);

            if (string.IsNullOrWhiteSpace(candidate))
            {
                return Task.CompletedTask;
            }

            var fullPath = Path.GetFullPath(Path.Combine(root, candidate.TrimStart('/', '\\')));

            // Defence in depth: the URL comes from the database, but a row edited outside the
            // app must never be able to walk the delete out of wwwroot and into arbitrary files.
            if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (IOException)
        {
            // A stale or locked file must not fail the group update that triggered the cleanup.
        }
        catch (UnauthorizedAccessException)
        {
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Identifies the image format from its magic bytes and returns the matching allow-listed
    /// extension, or null when the content is not one of the permitted images. Reads at most
    /// <see cref="SniffLength"/> bytes and rewinds, leaving the stream ready for the copy.
    /// </summary>
    private static async Task<string?> SniffImageExtensionAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        if (!fileStream.CanSeek)
        {
            return null;
        }

        var start = fileStream.Position;
        var header = new byte[SniffLength];
        var read = await fileStream.ReadAsync(header.AsMemory(0, SniffLength), cancellationToken);
        fileStream.Position = start;

        if (read < 4)
        {
            return null;
        }

        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return ".jpg";
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            return ".png";
        }

        // WebP: "RIFF" .... "WEBP" (bytes 0-3 and 8-11)
        if (read >= 12
            && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
        {
            return ".webp";
        }

        return null;
    }
}
