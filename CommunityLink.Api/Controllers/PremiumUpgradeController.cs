using System;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommunityLink.Domain.Features.Premium;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;
using CommunityLink.Shared.Security;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/premium")]
public class PremiumUpgradeController(
    IIdentityVerificationService verificationService,
    IWebHostEnvironment env) : ControllerBase
{
    private int? CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value
                        ?? User.FindFirst("userId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    [Authorize]
    [HttpGet("my-status")]
    public async Task<IActionResult> GetMyStatus(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        var result = await verificationService.GetUserSubscriptionStatusAsync(userId.Value, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyForUpgrade(
        [FromForm] int planId,
        [FromForm] string fullLegalName,
        [FromForm] string? workEmail,
        [FromForm] string? professionalUrl,
        [FromForm] string? paymentMethod,
        [FromForm] IFormFile? idCardFront,
        [FromForm] IFormFile? idCardBack,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        if (idCardFront == null || idCardFront.Length == 0)
        {
            return BadRequest(Result.Failure("Front of Identity Card image file is required.", ResultStatus.ValidationError));
        }

        // Save ID Card files securely into uploads folder
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "identities");
        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        var frontExt = Path.GetExtension(idCardFront.FileName).ToLowerInvariant();
        var frontFileName = $"id_front_{userId}_{Guid.NewGuid():N}{frontExt}";
        var frontFilePath = Path.Combine(uploadDir, frontFileName);

        using (var stream = new FileStream(frontFilePath, FileMode.Create))
        {
            await idCardFront.CopyToAsync(stream, cancellationToken);
        }
        var frontUrl = $"/uploads/identities/{frontFileName}";

        string? backUrl = null;
        if (idCardBack != null && idCardBack.Length > 0)
        {
            var backExt = Path.GetExtension(idCardBack.FileName).ToLowerInvariant();
            var backFileName = $"id_back_{userId}_{Guid.NewGuid():N}{backExt}";
            var backFilePath = Path.Combine(uploadDir, backFileName);
            using (var stream = new FileStream(backFilePath, FileMode.Create))
            {
                await idCardBack.CopyToAsync(stream, cancellationToken);
            }
            backUrl = $"/uploads/identities/{backFileName}";
        }

        var dto = new SubmitIdentityVerificationRequestDto(
            PlanId: planId,
            FullLegalName: fullLegalName,
            WorkEmail: workEmail,
            ProfessionalUrl: professionalUrl,
            IdCardFrontUrl: frontUrl,
            IdCardBackUrl: backUrl,
            PaymentMethod: paymentMethod ?? "LinkDropPoints"
        );

        var result = await verificationService.SubmitVerificationAsync(userId.Value, dto, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
