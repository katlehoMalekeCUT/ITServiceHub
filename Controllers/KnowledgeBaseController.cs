using Microsoft.AspNetCore.Mvc;

namespace ServiceHub_IT.Controllers;

public class KnowledgeBaseController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
