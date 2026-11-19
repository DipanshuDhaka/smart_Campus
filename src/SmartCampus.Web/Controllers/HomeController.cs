using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartCampus.Web.Models;

namespace SmartCampus.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult CampusMap()
    {
        return View();
    }

    public IActionResult ScanQR()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
