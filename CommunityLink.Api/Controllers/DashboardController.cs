using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLink.Api.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetUserDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetUserDashboardAsync(cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
