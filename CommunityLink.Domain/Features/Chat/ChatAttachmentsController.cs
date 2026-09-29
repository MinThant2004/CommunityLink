using CommunityLink.Domain.Features.Chat;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Chat;
using CommunityLink.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Domain.Features.Chat;

/// <summary>
/// Accepts chat attachments as multipart uploads and hands back a short public URL. The
/// client then posts the message with that URL, so image/video/document bytes never travel
/// inside the message JSON.
/// </summary>
[Route("api/chat/attachments")]
public sealed class ChatAttachmentsController(IChatAttachmentStorage storage) : BaseController
{
    [HttpPost]
    [Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ChatSend)]
    [RequestSizeLimit(ChatAttachmentPolicy.MaxBytes + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = ChatAttachmentPolicy.MaxBytes + 65536)]
    public async Task<IActionResult> Upload([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return ToActionResult(Result<ChatAttachmentUploadResponse>.Failure(
                "No file was provided.", ResultStatus.ValidationError));
        }

        await using var stream = file.OpenReadStream();
        var result = await storage.SaveAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        return ToActionResult(result);
    }
}
