using Microsoft.AspNetCore.Mvc;

namespace HospitalTriage.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
