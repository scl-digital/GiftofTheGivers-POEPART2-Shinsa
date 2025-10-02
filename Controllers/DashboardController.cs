using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IAuthenticationService _authService;

        public DashboardController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            // Check if user is authenticated
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new DashboardViewModel
            {
                CurrentUser = currentUser,
                RecentActivities = GetRecentActivities(currentUser),
                Stats = GetDashboardStats()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Volunteer()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Index", "Volunteer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Donate()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Donate", "Donation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReportEmergency()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Report", "Incident");
        }

        private List<ActivityItem> GetRecentActivities(User user)
        {
            var activities = new List<ActivityItem>();

            // Add account creation activity
            activities.Add(new ActivityItem
            {
                Icon = "✅",
                Title = "Account Created Successfully",
                Description = "Welcome to the Disaster Alleviation Foundation community!",
                Timestamp = user.CreatedAt,
                Type = "success"
            });

            // Add profile completion reminder if user is new
            if (user.CreatedAt > DateTime.UtcNow.AddDays(-7))
            {
                activities.Add(new ActivityItem
                {
                    Icon = "ℹ️",
                    Title = "Complete Your Profile",
                    Description = "Add more details to your profile to access additional features.",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    Type = "info"
                });
            }

            // Add last login activity if available
            if (user.LastLoginAt.HasValue && user.LastLoginAt.Value != user.CreatedAt)
            {
                activities.Add(new ActivityItem
                {
                    Icon = "🔐",
                    Title = "Last Login",
                    Description = $"You last logged in on {user.LastLoginAt.Value:MMM dd, yyyy 'at' h:mm tt}",
                    Timestamp = user.LastLoginAt.Value,
                    Type = "info"
                });
            }

            return activities.OrderByDescending(a => a.Timestamp).Take(5).ToList();
        }

        private DashboardStats GetDashboardStats()
        {
            // In a real application, these would come from a database
            return new DashboardStats
            {
                TotalVolunteers = 1247,
                TotalDonations = 89650.75m,
                ActiveEmergencies = 3,
                CommunitiesHelped = 156
            };
        }
    }
}