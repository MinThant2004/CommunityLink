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
using CommunityLink.Domain.Services;
using CommunityLink.Shared;
using CommunityLink.Shared.Features.Premium;
using CommunityLink.Shared.Security;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/premium")]
public class PremiumUpgradeController(
    IIdentityVerificationService verificationService,
    IPublicUrlBuilder publicUrlBuilder,
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
        [FromForm] string? responseSlaText,
        [FromForm] string? skills,
        [FromForm] string? location,
        [FromForm] IFormFile? idCardFront,
        [FromForm] IFormFile? idCardBack,
        [FromForm] List<IFormFile>? certificates,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUserId;
        if (userId == null) return Unauthorized(Result.Failure("Authentication required.", ResultStatus.Unauthorized));

        if (string.IsNullOrWhiteSpace(skills))
        {
            return BadRequest(Result.Failure("Skills & Competencies are mandatory.", ResultStatus.ValidationError));
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            return BadRequest(Result.Failure("Location is mandatory.", ResultStatus.ValidationError));
        }

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
        var frontUrl = publicUrlBuilder.Build($"/uploads/identities/{frontFileName}");

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
            backUrl = publicUrlBuilder.Build($"/uploads/identities/{backFileName}");
        }

        var certUrls = new List<string>();
        if (certificates != null)
        {
            foreach (var cert in certificates)
            {
                if (cert.Length == 0) continue;
                var certExt = Path.GetExtension(cert.FileName).ToLowerInvariant();
                var certFileName = $"cert_{userId}_{Guid.NewGuid():N}{certExt}";
                var certFilePath = Path.Combine(uploadDir, certFileName);
                using (var stream = new FileStream(certFilePath, FileMode.Create))
                {
                    await cert.CopyToAsync(stream, cancellationToken);
                }
                certUrls.Add(publicUrlBuilder.Build($"/uploads/identities/{certFileName}"));
            }
        }

        var dto = new SubmitIdentityVerificationRequestDto(
            PlanId: planId,
            FullLegalName: fullLegalName,
            WorkEmail: workEmail,
            ProfessionalUrl: professionalUrl,
            IdCardFrontUrl: frontUrl,
            IdCardBackUrl: backUrl,
            PaymentMethod: paymentMethod ?? "LinkDropPoints",
            ResponseSlaText: responseSlaText,
            Skills: skills.Trim(),
            Location: location.Trim(),
            CertificateUrls: certUrls
        );

        var result = await verificationService.SubmitVerificationAsync(userId.Value, dto, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
