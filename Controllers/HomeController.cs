using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceHub_IT.Services;

namespace ServiceHub_IT.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly DashboardService _dashboardService;

    public HomeController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var metrics = await _dashboardService.GetMetricsAsync(cancellationToken);
        return View(metrics);
    }

    public IActionResult Privacy() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
