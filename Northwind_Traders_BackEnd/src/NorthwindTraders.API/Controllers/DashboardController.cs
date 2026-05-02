using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NorthwindTraders.Infrastructure.Services;

namespace NorthwindTraders.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET api/v1/dashboard
    [HttpGet]
    [Authorize]    // only logged in employees can see the dashboard
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }
}