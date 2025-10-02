using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly IVolunteerService _volunteerService;
        private readonly IAuthenticationService _authService;

        public VolunteerController(IVolunteerService volunteerService, IAuthenticationService authService)
        {
            _volunteerService = volunteerService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            
            if (profile == null)
            {
                // User is not registered as a volunteer, show registration page
                return RedirectToAction("Register");
            }

            // User is already a volunteer, show dashboard
            var dashboard = await _volunteerService.GetVolunteerDashboardAsync(currentUser.Id);
            return View("Dashboard", dashboard);
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VolunteerRegistrationViewModel model)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if user is already registered as volunteer
                var existingProfile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
                if (existingProfile != null)
                {
                    TempData["InfoMessage"] = "You are already registered as a volunteer.";
                    return RedirectToAction("Index");
                }

                var profile = await _volunteerService.CreateVolunteerProfileAsync(currentUser.Id, model);
                
                TempData["SuccessMessage"] = "Thank you for registering as a volunteer! Your application is being processed.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Registration failed: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Tasks(TaskSearchViewModel? searchModel = null)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is registered as volunteer
            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            if (profile == null)
            {
                TempData["WarningMessage"] = "You need to register as a volunteer first to view available tasks.";
                return RedirectToAction("Register");
            }

            if (searchModel == null)
            {
                searchModel = new TaskSearchViewModel();
            }

            var tasks = await _volunteerService.GetAvailableTasksAsync(searchModel);
            searchModel.Tasks = tasks;
            searchModel.TotalTasks = tasks.Count;

            return View(searchModel);
        }

        [HttpGet]
        public async Task<IActionResult> TaskDetails(int id)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var task = await _volunteerService.GetTaskAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var currentUser = _authService.GetCurrentUser();
            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser!.Id);
            
            ViewBag.CanApply = profile != null && await _volunteerService.CanVolunteerTakeTaskAsync(profile.Id, id);
            ViewBag.VolunteerProfile = profile;

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyForTask(int taskId)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            if (profile == null)
            {
                TempData["ErrorMessage"] = "You need to register as a volunteer first.";
                return RedirectToAction("Register");
            }

            try
            {
                var assignment = await _volunteerService.AssignTaskAsync(taskId, profile.Id);
                if (assignment != null)
                {
                    TempData["SuccessMessage"] = "You have successfully applied for this task! Please check your dashboard for updates.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to apply for this task. It may be full or you may already be assigned.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Application failed: {ex.Message}";
            }

            return RedirectToAction("TaskDetails", new { id = taskId });
        }

        [HttpGet]
        public async Task<IActionResult> MyTasks()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            if (profile == null)
            {
                return RedirectToAction("Register");
            }

            var assignments = await _volunteerService.GetVolunteerAssignmentsAsync(profile.Id);
            return View(assignments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTask(int assignmentId)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var success = await _volunteerService.AcceptTaskAssignmentAsync(assignmentId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Task accepted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to accept task.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error accepting task: {ex.Message}";
            }

            return RedirectToAction("MyTasks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineTask(int assignmentId, string reason)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var success = await _volunteerService.DeclineTaskAssignmentAsync(assignmentId, reason ?? "No reason provided");
                if (success)
                {
                    TempData["InfoMessage"] = "Task declined.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to decline task.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error declining task: {ex.Message}";
            }

            return RedirectToAction("MyTasks");
        }

        [HttpGet]
        public async Task<IActionResult> CompleteTask(int assignmentId)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var assignment = await _volunteerService.GetTaskAssignmentAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int assignmentId, decimal hoursWorked, string notes)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var success = await _volunteerService.CompleteTaskAssignmentAsync(assignmentId, hoursWorked, notes ?? string.Empty);
                if (success)
                {
                    TempData["SuccessMessage"] = "Task marked as completed! Thank you for your service.";
                    return RedirectToAction("MyTasks");
                }
                else
                {
                    TempData["ErrorMessage"] = "Unable to complete task.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error completing task: {ex.Message}";
            }

            var assignment = await _volunteerService.GetTaskAssignmentAsync(assignmentId);
            return View(assignment);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            if (profile == null)
            {
                return RedirectToAction("Register");
            }

            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            if (profile == null)
            {
                return RedirectToAction("Register");
            }

            var stats = await _volunteerService.GetVolunteerStatsAsync(profile.Id);
            var history = await _volunteerService.GetVolunteerHistoryAsync(profile.Id);

            ViewBag.History = history;
            return View(stats);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var profile = await _volunteerService.GetVolunteerProfileAsync(currentUser.Id);
            if (profile == null)
            {
                return RedirectToAction("Register");
            }

            var dashboard = await _volunteerService.GetVolunteerDashboardAsync(currentUser.Id);
            return View(dashboard);
        }
    }
}