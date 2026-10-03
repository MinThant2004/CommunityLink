using CommunityLink.Domain.Services;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;

namespace CommunityLink.Domain.Features.Chat;

public interface IChatAttachmentStorage
{
    /// <summary>
    /// Validates and writes a chat attachment to disk, returning the public URL plus the
    /// metadata the client echoes back when it posts the message.
    /// </summary>
    Task<Result<ChatAttachmentUploadResponse>> SaveAsync(
        Stream fileStream,
        string fileName,
        string? contentType,
        long declaredLength,
        CancellationToken cancellationToken = default);
}

public sealed class ChatAttachmentStorage(IPublicUrlBuilder publicUrlBuilder) : IChatAttachmentStorage
{
    public async Task<Result<ChatAttachmentUploadResponse>> SaveAsync(
        Stream fileStream,
        string fileName,
        string? contentType,
        long declaredLength,
        CancellationToken cancellationToken = default)
    {
        if (!ChatAttachmentPolicy.IsAllowed(fileName))
        {
            return Result<ChatAttachmentUploadResponse>.Failure(
                $"Unsupported file type. Allowed: {ChatAttachmentPolicy.AllowedExtensionsDisplay}.",
                ResultStatus.ValidationError);
        }

        if (declaredLength <= 0)
        {
            return Result<ChatAttachmentUploadResponse>.Failure("The selected file is empty.", ResultStatus.ValidationError);
        }

        if (declaredLength > ChatAttachmentPolicy.MaxBytes)
        {
            return Result<ChatAttachmentUploadResponse>.Failure(
                $"File is too large. The maximum is 25 MB.",
                ResultStatus.ValidationError);
        }

        var now = DateTime.UtcNow;
        var relativeDirectory = Path.Combine("uploads", "chat", now.ToString("yyyy"), now.ToString("MM"));
        var absoluteDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var extension = ChatAttachmentPolicy.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(absoluteDirectory, storedName);

        // Streamed rather than buffered: a 25 MB video must never be held in memory in
        // addition to the request buffers.
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

        // ChatAttachmentPolicy.IsAllowed already restricted the extension to a fixed
        // non-executable set, so the stored name is safe to serve statically.
        var publicPath = $"/{relativeDirectory.Replace(Path.DirectorySeparatorChar, '/')}/{storedName}";

        return Result<ChatAttachmentUploadResponse>.Success(new ChatAttachmentUploadResponse(
            Url: publicUrlBuilder.Build(publicPath),
            FileName: Path.GetFileName(fileName),
            FileSizeByte: declaredLength,
            MessageType: ChatAttachmentPolicy.Classify(fileName, contentType)));
    }
}
