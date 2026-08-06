using Microsoft.AspNetCore.Mvc;

namespace ServiceHub_IT.Controllers;

public class TicketsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
