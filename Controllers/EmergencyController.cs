using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly IAuthenticationService _authService;

        public EmergencyController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Report()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new EmergencyReportViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Report(EmergencyReportViewModel model)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // In a future iteration, persist to storage or notify responders.
            TempData["SuccessMessage"] = "Thank you. Your emergency report has been submitted. Our team will review it promptly.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
