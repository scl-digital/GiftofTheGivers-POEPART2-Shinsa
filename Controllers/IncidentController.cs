using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers
{
    public class IncidentController : Controller
    {
        private readonly IIncidentService _incidentService;
        private readonly IAuthenticationService _authService;

        public IncidentController(IIncidentService incidentService, IAuthenticationService authService)
        {
            _incidentService = incidentService;
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

            var dashboard = await _incidentService.GetIncidentDashboardAsync(currentUser.Id);
            return View("Dashboard", dashboard);
        }

        [HttpGet]
        public IActionResult Report()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new IncidentReportViewModel
            {
                IncidentDate = DateTime.Now,
                Priority = IncidentPriority.Medium,
                Severity = SeverityLevel.Moderate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(IncidentReportViewModel model)
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
                var incident = await _incidentService.CreateIncidentAsync(model, currentUser.Id);
                
                TempData["SuccessMessage"] = $"Incident report #{incident.Id} has been submitted successfully. Our response team has been notified.";
                return RedirectToAction("Details", new { id = incident.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to submit incident report: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(IncidentSearchViewModel? searchModel = null)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (searchModel == null)
            {
                searchModel = new IncidentSearchViewModel();
            }

            var incidents = await _incidentService.SearchIncidentsAsync(searchModel);
            searchModel.Incidents = incidents;
            searchModel.TotalIncidents = incidents.Count;

            return View(searchModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var incident = await _incidentService.GetIncidentAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            var updates = await _incidentService.GetIncidentUpdatesAsync(id);
            var resources = await _incidentService.GetIncidentResourcesAsync(id);
            var responses = await _incidentService.GetIncidentResponsesAsync(id);
            var media = await _incidentService.GetIncidentMediaAsync(id);

            ViewBag.Updates = updates;
            ViewBag.Resources = resources;
            ViewBag.Responses = responses;
            ViewBag.Media = media;

            var currentUser = _authService.GetCurrentUser();
            ViewBag.CanEdit = await _incidentService.CanUserEditIncidentAsync(id, currentUser!.Id);

            return View(incident);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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

            var incident = await _incidentService.GetIncidentAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            var canEdit = await _incidentService.CanUserEditIncidentAsync(id, currentUser.Id);
            if (!canEdit)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this incident.";
                return RedirectToAction("Details", new { id });
            }

            var model = new IncidentReportViewModel
            {
                Title = incident.Title,
                Type = incident.Type,
                Description = incident.Description,
                Location = incident.Location,
                Latitude = incident.Latitude,
                Longitude = incident.Longitude,
                IncidentDate = incident.IncidentDate,
                Severity = incident.Severity,
                AffectedPopulation = incident.AffectedPopulation,
                Casualties = incident.Casualties,
                Injuries = incident.Injuries,
                PropertyDamageEstimate = incident.PropertyDamageEstimate,
                InfrastructureDamage = incident.InfrastructureDamage,
                ImmediateNeeds = incident.ImmediateNeeds,
                ResourcesRequired = incident.ResourcesRequired,
                AccessRoutes = incident.AccessRoutes,
                WeatherConditions = incident.WeatherConditions,
                ContactInformation = incident.ContactInformation,
                Priority = incident.Priority
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IncidentReportViewModel model)
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

            var incident = await _incidentService.GetIncidentAsync(id);
            if (incident == null)
            {
                return NotFound();
            }

            var canEdit = await _incidentService.CanUserEditIncidentAsync(id, currentUser.Id);
            if (!canEdit)
            {
                TempData["ErrorMessage"] = "You don't have permission to edit this incident.";
                return RedirectToAction("Details", new { id });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Update incident properties
                incident.Title = model.Title;
                incident.Type = model.Type;
                incident.Description = model.Description;
                incident.Location = model.Location;
                incident.Latitude = model.Latitude;
                incident.Longitude = model.Longitude;
                incident.IncidentDate = model.IncidentDate;
                incident.Severity = model.Severity;
                incident.AffectedPopulation = model.AffectedPopulation;
                incident.Casualties = model.Casualties;
                incident.Injuries = model.Injuries;
                incident.PropertyDamageEstimate = model.PropertyDamageEstimate;
                incident.InfrastructureDamage = model.InfrastructureDamage;
                incident.ImmediateNeeds = model.ImmediateNeeds;
                incident.ResourcesRequired = model.ResourcesRequired;
                incident.AccessRoutes = model.AccessRoutes;
                incident.WeatherConditions = model.WeatherConditions;
                incident.ContactInformation = model.ContactInformation;
                incident.Priority = model.Priority;

                await _incidentService.UpdateIncidentAsync(incident);

                // Add update record
                await _incidentService.AddIncidentUpdateAsync(id, new IncidentUpdateViewModel
                {
                    IncidentId = id,
                    UpdateText = "Incident details updated",
                    Type = UpdateType.General
                }, currentUser.Id);

                TempData["SuccessMessage"] = "Incident updated successfully.";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Failed to update incident: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUpdate(int incidentId, IncidentUpdateViewModel model)
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
                TempData["ErrorMessage"] = "Please provide a valid update.";
                return RedirectToAction("Details", new { id = incidentId });
            }

            try
            {
                model.IncidentId = incidentId;
                await _incidentService.AddIncidentUpdateAsync(incidentId, model, currentUser.Id);
                
                TempData["SuccessMessage"] = "Update added successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to add update: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = incidentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int incidentId, IncidentStatus status)
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

            try
            {
                var success = await _incidentService.UpdateIncidentStatusAsync(incidentId, status, currentUser.Id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Incident status updated to {status}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update incident status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = incidentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyIncident(int incidentId, VerificationStatus status)
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

            try
            {
                var success = await _incidentService.VerifyIncidentAsync(incidentId, currentUser.Id, status);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Incident verification status set to {status}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update verification status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating verification: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = incidentId });
        }

        [HttpGet]
        public async Task<IActionResult> MyIncidents()
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

            var incidents = await _incidentService.GetIncidentsByUserAsync(currentUser.Id);
            return View(incidents);
        }

        [HttpGet]
        public async Task<IActionResult> Critical()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var criticalIncidents = await _incidentService.GetCriticalIncidentsAsync();
            return View(criticalIncidents);
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var stats = await _incidentService.GetIncidentStatsAsync();
            var analytics = await _incidentService.GetIncidentAnalyticsAsync();
            
            ViewBag.Analytics = analytics;
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

            var dashboard = await _incidentService.GetIncidentDashboardAsync(currentUser.Id);
            return View(dashboard);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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

            var canDelete = await _incidentService.CanUserDeleteIncidentAsync(id, currentUser.Id);
            if (!canDelete)
            {
                TempData["ErrorMessage"] = "You don't have permission to delete this incident.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var success = await _incidentService.DeleteIncidentAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Incident deleted successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete incident.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting incident: {ex.Message}";
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser?.Role != "Admin")
            {
                TempData["ErrorMessage"] = "You don't have permission to export incident data.";
                return RedirectToAction("Index");
            }

            try
            {
                var reportContent = await _incidentService.GenerateIncidentReportAsync(1); // Mock implementation
                TempData["InfoMessage"] = "Export functionality is available for administrators.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}