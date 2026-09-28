using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.Report;
using CommunityLink.Shared.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = PermissionCatalog.PolicyPrefix + PermissionCatalog.ReportView)]
public sealed class ReportController(IReportService reportService) : ControllerBase
{
    [HttpGet("communities")]
    public async Task<IActionResult> GetCommunityReport(CancellationToken cancellationToken)
    {
        var result = await reportService.GetCommunityReportAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUserReport(CancellationToken cancellationToken)
    {
        var result = await reportService.GetUserReportAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("users/{userId:int}/top-content")]
    public async Task<IActionResult> GetUserTopContent(int userId, CancellationToken cancellationToken)
    {
        var result = await reportService.GetUserTopContentAsync(userId, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
