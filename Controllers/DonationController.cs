using Microsoft.AspNetCore.Mvc;
using DisasterAlleviationFoundation.Models;
using DisasterAlleviationFoundation.Services;

namespace DisasterAlleviationFoundation.Controllers
{
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly IAuthenticationService _authService;

        public DonationController(IDonationService donationService, IAuthenticationService authService)
        {
            _donationService = donationService;
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

            var dashboard = await _donationService.GetDonationDashboardAsync(currentUser.Id);
            return View("Dashboard", dashboard);
        }

        [HttpGet]
        public IActionResult Donate()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new DonationViewModel
            {
                Type = DonationType.Financial,
                UrgencyLevel = UrgencyLevel.Normal,
                DeliveryMethod = DeliveryMethod.DropOff,
                ReceiptRequired = true,
                Resources = new List<ResourceDonationViewModel>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonationViewModel model)
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

            // Custom validation for donation type
            if (model.Type == DonationType.Financial || model.Type == DonationType.Mixed)
            {
                if (!model.Amount.HasValue || model.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Please enter a valid donation amount.");
                }
                if (!model.PaymentMethod.HasValue)
                {
                    ModelState.AddModelError("PaymentMethod", "Please select a payment method.");
                }
            }

            if (model.Type == DonationType.Resource || model.Type == DonationType.Mixed)
            {
                if (model.Resources == null || !model.Resources.Any())
                {
                    ModelState.AddModelError("Resources", "Please add at least one resource item.");
                }
                else
                {
                    for (int i = 0; i < model.Resources.Count; i++)
                    {
                        var resource = model.Resources[i];
                        if (string.IsNullOrEmpty(resource.ItemName))
                        {
                            ModelState.AddModelError($"Resources[{i}].ItemName", "Item name is required.");
                        }
                        if (resource.Quantity <= 0)
                        {
                            ModelState.AddModelError($"Resources[{i}].Quantity", "Quantity must be greater than 0.");
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var donation = await _donationService.CreateDonationAsync(model, currentUser.Id);
                
                // For financial donations, simulate payment processing
                if (model.Type == DonationType.Financial || model.Type == DonationType.Mixed)
                {
                    if (model.Amount.HasValue && model.PaymentMethod.HasValue)
                    {
                        var transactionRef = $"TXN{DateTime.Now:yyyyMMddHHmmss}{donation.Id}";
                        await _donationService.ProcessFinancialDonationAsync(donation.Id, transactionRef, model.PaymentMethod.Value);
                    }
                }

                TempData["SuccessMessage"] = $"Thank you for your generous donation! Donation #{donation.Id} has been recorded.";
                
                if (model.RequiresPickup)
                {
                    TempData["InfoMessage"] = "We will contact you within 24 hours to schedule a pickup.";
                }

                return RedirectToAction("Details", new { id = donation.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Donation failed: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var donation = await _donationService.GetDonationAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            var resources = await _donationService.GetResourceDonationsAsync(id);
            var tracking = await _donationService.GetDonationTrackingAsync(id);
            var distributions = await _donationService.GetDonationDistributionsAsync(id);

            ViewBag.Resources = resources;
            ViewBag.Tracking = tracking;
            ViewBag.Distributions = distributions;

            var currentUser = _authService.GetCurrentUser();
            ViewBag.CanEdit = await _donationService.CanUserEditDonationAsync(id, currentUser!.Id);

            return View(donation);
        }

        [HttpGet]
        public async Task<IActionResult> Search(DonationSearchViewModel? searchModel = null)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (searchModel == null)
            {
                searchModel = new DonationSearchViewModel();
            }

            var donations = await _donationService.SearchDonationsAsync(searchModel);
            searchModel.Donations = donations;
            searchModel.TotalDonations = donations.Count;

            return View(searchModel);
        }

        [HttpGet]
        public async Task<IActionResult> MyDonations()
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

            var donations = await _donationService.GetDonationsByUserAsync(currentUser.Id);
            return View(donations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int donationId, DonationStatus status)
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
                var success = await _donationService.UpdateDonationStatusAsync(donationId, status, currentUser.Id);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Donation status updated to {status}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update donation status.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating status: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = donationId });
        }

        [HttpGet]
        public async Task<IActionResult> Distribute(int id)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser?.Role != "Admin" && currentUser?.Role != "Volunteer")
            {
                TempData["ErrorMessage"] = "You don't have permission to distribute donations.";
                return RedirectToAction("Details", new { id });
            }

            var donation = await _donationService.GetDonationAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            var resources = await _donationService.GetResourceDonationsAsync(id);
            
            var model = new DistributionViewModel
            {
                DonationId = id,
                ResourceDistributions = resources.Select(r => new ResourceDistributionViewModel
                {
                    ResourceDonationId = r.Id,
                    ItemName = r.ItemName,
                    AvailableQuantity = r.Quantity,
                    QuantityToDistribute = 0
                }).ToList()
            };

            ViewBag.Donation = donation;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Distribute(DistributionViewModel model)
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

            if (currentUser.Role != "Admin" && currentUser.Role != "Volunteer")
            {
                TempData["ErrorMessage"] = "You don't have permission to distribute donations.";
                return RedirectToAction("Details", new { id = model.DonationId });
            }

            if (!ModelState.IsValid)
            {
                var donation = await _donationService.GetDonationAsync(model.DonationId);
                var resources = await _donationService.GetResourceDonationsAsync(model.DonationId);
                ViewBag.Donation = donation;
                return View(model);
            }

            try
            {
                var distribution = await _donationService.CreateDistributionAsync(model.DonationId, model, currentUser.Id);
                
                TempData["SuccessMessage"] = "Distribution recorded successfully!";
                return RedirectToAction("Details", new { id = model.DonationId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Distribution failed: {ex.Message}");
                
                var donation = await _donationService.GetDonationAsync(model.DonationId);
                var resources = await _donationService.GetResourceDonationsAsync(model.DonationId);
                ViewBag.Donation = donation;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Resources()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var availableResources = await _donationService.GetAvailableResourcesAsync();
            var resourcesByCategory = availableResources.GroupBy(r => r.Category).ToDictionary(g => g.Key, g => g.ToList());
            
            return View(resourcesByCategory);
        }

        [HttpGet]
        public async Task<IActionResult> ResourcesByCategory(ResourceCategory category)
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var resources = await _donationService.GetResourcesByCategoryAsync(category);
            ViewBag.Category = category;
            
            return View(resources);
        }

        [HttpGet]
        public async Task<IActionResult> DonationCenters()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var centers = await _donationService.GetActiveDonationCentersAsync();
            return View(centers);
        }

        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var stats = await _donationService.GetDonationStatsAsync();
            var analytics = await _donationService.GetDonationAnalyticsAsync();
            var trends = await _donationService.GetDonationTrendsByMonthAsync();
            
            ViewBag.Analytics = analytics;
            ViewBag.Trends = trends;
            
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

            var dashboard = await _donationService.GetDonationDashboardAsync(currentUser.Id);
            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> UrgentNeeds()
        {
            if (!_authService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var urgentDonations = await _donationService.GetUrgentDonationsAsync();
            var resourceNeeds = await _donationService.GetResourceNeedsAsync();
            
            ViewBag.ResourceNeeds = resourceNeeds;
            return View(urgentDonations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QualityCheck(int resourceId, bool approved, string notes)
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

            if (currentUser.Role != "Admin" && currentUser.Role != "Volunteer")
            {
                TempData["ErrorMessage"] = "You don't have permission to perform quality checks.";
                return RedirectToAction("Index");
            }

            try
            {
                var success = await _donationService.PerformQualityCheckAsync(resourceId, currentUser.Id, notes ?? string.Empty, approved);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Quality check completed. Resource {(approved ? "approved" : "rejected")}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to perform quality check.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Quality check failed: {ex.Message}";
            }

            return RedirectToAction("Resources");
        }

        [HttpGet]
        public async Task<IActionResult> TaxReceipt(int id)
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

            var donation = await _donationService.GetDonationAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            if (donation.DonorUserId != currentUser.Id)
            {
                TempData["ErrorMessage"] = "You can only view receipts for your own donations.";
                return RedirectToAction("MyDonations");
            }

            if (!donation.IsTaxDeductible || !donation.ReceiptRequired)
            {
                TempData["InfoMessage"] = "This donation is not eligible for a tax receipt.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var receipt = await _donationService.GenerateTaxReceiptAsync(id);
                ViewBag.Receipt = receipt;
                return View(donation);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to generate receipt: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        // AJAX endpoint for adding resource items dynamically
        [HttpGet]
        public IActionResult AddResourceItem()
        {
            var model = new ResourceDonationViewModel
            {
                Category = ResourceCategory.Other,
                Condition = ItemCondition.New,
                Quantity = 1
            };
            
            return PartialView("_ResourceDonationItem", model);
        }
    }
}