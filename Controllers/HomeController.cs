using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthenticationService _authService;

    public HomeController(ILogger<HomeController> logger, IAuthenticationService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public IActionResult Index()
    {
        // If user is authenticated, redirect to dashboard
        if (_authService.IsAuthenticated)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Otherwise, redirect to login
        return RedirectToAction("Login", "Account");
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
