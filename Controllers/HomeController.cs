using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using COMP2139_Assignment1.Models;
using Microsoft.AspNetCore.Authorization;

namespace COMP2139_Assignment1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [Authorize(Roles = "Admin, User")]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Admin, User")]
    public IActionResult About()
    {
        return View();
    }

    [Authorize(Roles = "Admin, User")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}