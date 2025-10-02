using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;

        public AccountController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If user is already authenticated, redirect to dashboard
            if (_authService.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Welcome back! You have successfully logged in.";
                
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            // If user is already authenticated, redirect to dashboard
            if (_authService.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Registration successful! Welcome to the Disaster Alleviation Foundation.";
                return RedirectToAction("Index", "Dashboard");
            }

            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["InfoMessage"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}