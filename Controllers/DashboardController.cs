using Microsoft.AspNetCore.Mvc;

namespace ServiceHub_IT.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
