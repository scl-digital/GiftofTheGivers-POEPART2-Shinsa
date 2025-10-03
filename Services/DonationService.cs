using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public class DonationService : IDonationService
    {
        private readonly List<Donation> _donations = new();
        private readonly List<ResourceDonation> _resourceDonations = new();
        private readonly List<DonationTracking> _tracking = new();
        private readonly List<DonationDistribution> _distributions = new();
        private readonly List<ResourceDistribution> _resourceDistributions = new();
        private readonly List<DonationCenter> _donationCenters = new();
        private readonly IAuthenticationService _authService;

        public DonationService(IAuthenticationService authService)
        {
            _authService = authService;
            InitializeSampleData();
        }

        // Donation Management
        public async Task<Donation> CreateDonationAsync(DonationViewModel model, int donorUserId)
        {
            var user = await _authService.GetUserByIdAsync(donorUserId);
            if (user == null) throw new ArgumentException("User not found");

            var donation = new Donation
            {
                Id = _donations.Count > 0 ? _donations.Max(d => d.Id) + 1 : 1,
                Type = model.Type,
                IsAnonymous = model.IsAnonymous,
                SpecialInstructions = model.SpecialInstructions,
                TargetArea = model.TargetArea,
                UrgencyLevel = model.UrgencyLevel,
                ContactPhone = model.ContactPhone,
                RequiresPickup = model.RequiresPickup,
                PickupAddress = model.PickupAddress,
                PreferredPickupDate = model.PreferredPickupDate,
                DeliveryMethod = model.DeliveryMethod,
                DonorUserId = donorUserId,
                Donor = user,
                DonationDate = DateTime.UtcNow,
                Status = DonationStatus.Pledged
            };

            // Handle financial donation
            if (model.Type == DonationType.Financial || model.Type == DonationType.Mixed)
            {
                donation.Amount = model.Amount;
                donation.PaymentMethod = model.PaymentMethod;
                donation.ReceiptRequired = model.ReceiptRequired;
                donation.Currency = "USD";
            }

            _donations.Add(donation);

            // Add resource donations
            if (model.Type == DonationType.Resource || model.Type == DonationType.Mixed)
            {
                foreach (var resourceModel in model.Resources)
                {
                    await AddResourceDonationAsync(donation.Id, resourceModel);
                }
            }

            // Add initial tracking
            await AddTrackingUpdateAsync(donation.Id, DonationStatus.Pledged, "Initial donation pledge", "Donation created and logged in system");

            return donation;
        }

        public async Task<Donation?> GetDonationAsync(int donationId)
        {
            return _donations.FirstOrDefault(d => d.Id == donationId);
        }

        public async Task<Donation> UpdateDonationAsync(Donation donation)
        {
            var existingDonation = _donations.FirstOrDefault(d => d.Id == donation.Id);
            if (existingDonation != null)
            {
                var index = _donations.IndexOf(existingDonation);
                _donations[index] = donation;
            }
            return donation;
        }

        public async Task<bool> DeleteDonationAsync(int donationId)
        {
            var donation = _donations.FirstOrDefault(d => d.Id == donationId);
            if (donation != null)
            {
                _donations.Remove(donation);
                
                // Remove related data
                _resourceDonations.RemoveAll(r => r.DonationId == donationId);
                _tracking.RemoveAll(t => t.DonationId == donationId);
                _distributions.RemoveAll(d => d.DonationId == donationId);
                
                return true;
            }
            return false;
        }

        public async Task<List<Donation>> GetAllDonationsAsync()
        {
            return _donations.OrderByDescending(d => d.DonationDate).ToList();
        }

        public async Task<List<Donation>> SearchDonationsAsync(DonationSearchViewModel searchModel)
        {
            var query = _donations.AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(d => d.SpecialInstructions.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                        d.TargetArea.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (searchModel.Type.HasValue)
            {
                query = query.Where(d => d.Type == searchModel.Type.Value);
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(d => d.Status == searchModel.Status.Value);
            }

            if (searchModel.UrgencyLevel.HasValue)
            {
                query = query.Where(d => d.UrgencyLevel == searchModel.UrgencyLevel.Value);
            }

            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                query = query.Where(d => d.TargetArea.Contains(searchModel.Location, StringComparison.OrdinalIgnoreCase));
            }

            if (searchModel.FromDate.HasValue)
            {
                query = query.Where(d => d.DonationDate >= searchModel.FromDate.Value);
            }

            if (searchModel.ToDate.HasValue)
            {
                query = query.Where(d => d.DonationDate <= searchModel.ToDate.Value);
            }

            return query.OrderByDescending(d => d.DonationDate).ToList();
        }

        public async Task<List<Donation>> GetDonationsByStatusAsync(DonationStatus status)
        {
            return _donations.Where(d => d.Status == status).OrderByDescending(d => d.DonationDate).ToList();
        }

        public async Task<List<Donation>> GetDonationsByTypeAsync(DonationType type)
        {
            return _donations.Where(d => d.Type == type).OrderByDescending(d => d.DonationDate).ToList();
        }

        public async Task<List<Donation>> GetDonationsByUserAsync(int userId)
        {
            return _donations.Where(d => d.DonorUserId == userId).OrderByDescending(d => d.DonationDate).ToList();
        }

        public async Task<List<Donation>> GetRecentDonationsAsync(int count = 10)
        {
            return _donations.OrderByDescending(d => d.DonationDate).Take(count).ToList();
        }

        public async Task<List<Donation>> GetUrgentDonationsAsync()
        {
            return _donations.Where(d => d.UrgencyLevel == UrgencyLevel.Critical || d.UrgencyLevel == UrgencyLevel.Emergency)
                .OrderByDescending(d => d.UrgencyLevel).ThenByDescending(d => d.DonationDate).ToList();
        }

        // Donation Status Management
        public async Task<bool> UpdateDonationStatusAsync(int donationId, DonationStatus status, int updatedByUserId)
        {
            var donation = await GetDonationAsync(donationId);
            if (donation != null)
            {
                var oldStatus = donation.Status;
                donation.Status = status;
                
                if (status == DonationStatus.Processing)
                {
                    donation.ProcessedDate = DateTime.UtcNow;
                    donation.ProcessedByUserId = updatedByUserId;
                }
                else if (status == DonationStatus.Distributed)
                {
                    donation.DistributionDate = DateTime.UtcNow;
                    donation.DistributedByUserId = updatedByUserId;
                }

                await UpdateDonationAsync(donation);

                // Add tracking update
                await AddTrackingUpdateAsync(donationId, status, "Status Update", $"Status changed from {oldStatus} to {status}", updatedByUserId);

                return true;
            }
            return false;
        }

        public async Task<bool> ConfirmDonationAsync(int donationId, int confirmedByUserId)
        {
            return await UpdateDonationStatusAsync(donationId, DonationStatus.Confirmed, confirmedByUserId);
        }

        public async Task<bool> ProcessDonationAsync(int donationId, int processedByUserId)
        {
            return await UpdateDonationStatusAsync(donationId, DonationStatus.Processing, processedByUserId);
        }

        public async Task<bool> ApproveDonationAsync(int donationId, int approvedByUserId)
        {
            return await UpdateDonationStatusAsync(donationId, DonationStatus.Approved, approvedByUserId);
        }

        public async Task<bool> RejectDonationAsync(int donationId, int rejectedByUserId, string reason)
        {
            var donation = await GetDonationAsync(donationId);
            if (donation != null)
            {
                donation.Status = DonationStatus.Rejected;
                donation.Notes = reason;
                await UpdateDonationAsync(donation);

                await AddTrackingUpdateAsync(donationId, DonationStatus.Rejected, "Rejection", $"Donation rejected: {reason}", rejectedByUserId);
                return true;
            }
            return false;
        }

        // Resource Donation Management
        public async Task<ResourceDonation> AddResourceDonationAsync(int donationId, ResourceDonationViewModel model)
        {
            var resource = new ResourceDonation
            {
                Id = _resourceDonations.Count > 0 ? _resourceDonations.Max(r => r.Id) + 1 : 1,
                DonationId = donationId,
                Category = model.Category,
                ItemName = model.ItemName,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitOfMeasure = model.UnitOfMeasure,
                EstimatedValue = model.EstimatedValue,
                Condition = model.Condition,
                ExpirationDate = model.ExpirationDate,
                Brand = model.Brand,
                Size = model.Size,
                Weight = model.Weight,
                StorageRequirements = model.StorageRequirements,
                AllergenInfo = model.AllergenInfo,
                Status = ResourceStatus.Available
            };

            _resourceDonations.Add(resource);
            return resource;
        }

        public async Task<List<ResourceDonation>> GetResourceDonationsAsync(int donationId)
        {
            return _resourceDonations.Where(r => r.DonationId == donationId).ToList();
        }

        public async Task<ResourceDonation> UpdateResourceDonationAsync(ResourceDonation resource)
        {
            var existingResource = _resourceDonations.FirstOrDefault(r => r.Id == resource.Id);
            if (existingResource != null)
            {
                var index = _resourceDonations.IndexOf(existingResource);
                _resourceDonations[index] = resource;
            }
            return resource;
        }

        public async Task<bool> DeleteResourceDonationAsync(int resourceId)
        {
            var resource = _resourceDonations.FirstOrDefault(r => r.Id == resourceId);
            if (resource != null)
            {
                _resourceDonations.Remove(resource);
                return true;
            }
            return false;
        }

        public async Task<List<ResourceDonation>> GetResourcesByCategoryAsync(ResourceCategory category)
        {
            return _resourceDonations.Where(r => r.Category == category && r.Status == ResourceStatus.Available).ToList();
        }

        public async Task<List<ResourceDonation>> GetAvailableResourcesAsync()
        {
            return _resourceDonations.Where(r => r.Status == ResourceStatus.Available).ToList();
        }

        public async Task<List<ResourceDonation>> GetExpiringResourcesAsync(int daysAhead = 7)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return _resourceDonations.Where(r => r.ExpirationDate.HasValue && r.ExpirationDate.Value <= cutoffDate && r.Status == ResourceStatus.Available).ToList();
        }

        // Quality Control
        public async Task<bool> PerformQualityCheckAsync(int resourceId, int checkedByUserId, string notes, bool approved)
        {
            var resource = _resourceDonations.FirstOrDefault(r => r.Id == resourceId);
            var user = await _authService.GetUserByIdAsync(checkedByUserId);
            
            if (resource != null && user != null)
            {
                resource.QualityCheckDate = DateTime.UtcNow;
                resource.QualityCheckedByUserId = checkedByUserId;
                resource.QualityCheckedBy = user;
                resource.QualityNotes = notes;
                resource.Status = approved ? ResourceStatus.Available : ResourceStatus.Rejected;
                
                await UpdateResourceDonationAsync(resource);
                return true;
            }
            return false;
        }

        public async Task<List<ResourceDonation>> GetResourcesPendingQualityCheckAsync()
        {
            return _resourceDonations.Where(r => !r.QualityCheckDate.HasValue).ToList();
        }

        public async Task<ResourceDonation?> GetResourceDonationAsync(int resourceId)
        {
            return _resourceDonations.FirstOrDefault(r => r.Id == resourceId);
        }

        // Donation Tracking
        public async Task<DonationTracking> AddTrackingUpdateAsync(int donationId, DonationStatus status, string location, string notes, int? updatedByUserId = null)
        {
            var tracking = new DonationTracking
            {
                Id = _tracking.Count > 0 ? _tracking.Max(t => t.Id) + 1 : 1,
                DonationId = donationId,
                Status = status,
                Location = location,
                Notes = notes,
                StatusDate = DateTime.UtcNow,
                UpdatedByUserId = updatedByUserId
            };

            _tracking.Add(tracking);
            return tracking;
        }

        public async Task<List<DonationTracking>> GetDonationTrackingAsync(int donationId)
        {
            return _tracking.Where(t => t.DonationId == donationId).OrderByDescending(t => t.StatusDate).ToList();
        }

        public async Task<DonationTracking?> GetLatestTrackingAsync(int donationId)
        {
            return _tracking.Where(t => t.DonationId == donationId).OrderByDescending(t => t.StatusDate).FirstOrDefault();
        }

        // Distribution Management
        public async Task<DonationDistribution> CreateDistributionAsync(int donationId, DistributionViewModel model, int distributedByUserId)
        {
            var user = await _authService.GetUserByIdAsync(distributedByUserId);
            if (user == null) throw new ArgumentException("User not found");

            var distribution = new DonationDistribution
            {
                Id = _distributions.Count > 0 ? _distributions.Max(d => d.Id) + 1 : 1,
                DonationId = donationId,
                DistributionLocation = model.DistributionLocation,
                NumberOfRecipients = model.NumberOfRecipients,
                RecipientOrganization = model.RecipientOrganization,
                ContactPerson = model.ContactPerson,
                ContactPhone = model.ContactPhone,
                Notes = model.Notes,
                DistributedByUserId = distributedByUserId,
                DistributedBy = user,
                DistributionDate = DateTime.UtcNow
            };

            _distributions.Add(distribution);

            // Update donation status
            await UpdateDonationStatusAsync(donationId, DonationStatus.Distributed, distributedByUserId);

            // Add resource distributions
            foreach (var resourceDist in model.ResourceDistributions)
            {
                await AddResourceDistributionAsync(distribution.Id, resourceDist.ResourceDonationId, resourceDist.QuantityToDistribute, resourceDist.Notes);
            }

            return distribution;
        }

        public async Task<List<DonationDistribution>> GetDonationDistributionsAsync(int donationId)
        {
            return _distributions.Where(d => d.DonationId == donationId).ToList();
        }

        public async Task<DonationDistribution> UpdateDistributionAsync(DonationDistribution distribution)
        {
            var existingDistribution = _distributions.FirstOrDefault(d => d.Id == distribution.Id);
            if (existingDistribution != null)
            {
                var index = _distributions.IndexOf(existingDistribution);
                _distributions[index] = distribution;
            }
            return distribution;
        }

        public async Task<bool> DeleteDistributionAsync(int distributionId)
        {
            var distribution = _distributions.FirstOrDefault(d => d.Id == distributionId);
            if (distribution != null)
            {
                _distributions.Remove(distribution);
                _resourceDistributions.RemoveAll(r => r.DonationDistributionId == distributionId);
                return true;
            }
            return false;
        }

        public async Task<List<DonationDistribution>> GetRecentDistributionsAsync(int count = 10)
        {
            return _distributions.OrderByDescending(d => d.DistributionDate).Take(count).ToList();
        }

        public async Task<List<DonationDistribution>> GetDistributionsByLocationAsync(string location)
        {
            return _distributions.Where(d => d.DistributionLocation.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Resource Distribution
        public async Task<ResourceDistribution> AddResourceDistributionAsync(int distributionId, int resourceDonationId, int quantity, string notes)
        {
            var resourceDistribution = new ResourceDistribution
            {
                Id = _resourceDistributions.Count > 0 ? _resourceDistributions.Max(r => r.Id) + 1 : 1,
                DonationDistributionId = distributionId,
                ResourceDonationId = resourceDonationId,
                QuantityDistributed = quantity,
                Notes = notes
            };

            // Update resource quantities
            await UpdateResourceQuantitiesAsync(resourceDonationId, quantity);

            _resourceDistributions.Add(resourceDistribution);
            return resourceDistribution;
        }

        public async Task<List<ResourceDistribution>> GetResourceDistributionsAsync(int distributionId)
        {
            return _resourceDistributions.Where(r => r.DonationDistributionId == distributionId).ToList();
        }

        public async Task<bool> UpdateResourceQuantitiesAsync(int resourceDonationId, int distributedQuantity)
        {
            var resource = _resourceDonations.FirstOrDefault(r => r.Id == resourceDonationId);
            if (resource != null)
            {
                var totalDistributed = _resourceDistributions.Where(r => r.ResourceDonationId == resourceDonationId).Sum(r => r.QuantityDistributed);
                var remaining = resource.Quantity - totalDistributed - distributedQuantity;
                
                if (remaining <= 0)
                {
                    resource.Status = ResourceStatus.Distributed;
                }
                
                await UpdateResourceDonationAsync(resource);
                return true;
            }
            return false;
        }

        // Analytics and Reporting
        public async Task<DonationStats> GetDonationStatsAsync()
        {
            var stats = new DonationStats
            {
                TotalDonations = _donations.Count,
                TotalFinancialDonations = _donations.Where(d => d.Amount.HasValue).Sum(d => d.Amount.Value),
                TotalResourceDonations = _resourceDonations.Count,
                DonationsThisMonth = _donations.Count(d => d.DonationDate >= DateTime.Now.AddDays(-30)),
                ActiveDonations = _donations.Count(d => d.Status == DonationStatus.Confirmed || d.Status == DonationStatus.Processing),
                DistributedDonations = _donations.Count(d => d.Status == DonationStatus.Distributed || d.Status == DonationStatus.Completed),
                UniqueDonors = _donations.Select(d => d.DonorUserId).Distinct().Count(),
                AverageDonationAmount = _donations.Where(d => d.Amount.HasValue).Average(d => d.Amount.Value)
            };

            // Group by category
            stats.DonationsByCategory = _resourceDonations.GroupBy(r => r.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by status
            stats.DonationsByStatus = _donations.GroupBy(d => d.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        public async Task<Dictionary<string, object>> GetDonationAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _donations.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(d => d.DonationDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(d => d.DonationDate <= toDate.Value);

            var donations = query.ToList();

            return new Dictionary<string, object>
            {
                ["TotalDonations"] = donations.Count,
                ["TotalValue"] = donations.Where(d => d.Amount.HasValue).Sum(d => d.Amount.Value),
                ["AverageValue"] = donations.Where(d => d.Amount.HasValue).Average(d => d.Amount ?? 0),
                ["ResourceDonations"] = donations.Count(d => d.Type == DonationType.Resource || d.Type == DonationType.Mixed),
                ["FinancialDonations"] = donations.Count(d => d.Type == DonationType.Financial || d.Type == DonationType.Mixed),
                ["CompletionRate"] = donations.Count > 0 ? (double)donations.Count(d => d.Status == DonationStatus.Completed || d.Status == DonationStatus.Distributed) / donations.Count * 100 : 0,
                ["AverageProcessingTime"] = 2.5 // Mock data
            };
        }

        public async Task<List<Donation>> GetTopDonationsAsync(int count = 10)
        {
            return _donations.Where(d => d.Amount.HasValue).OrderByDescending(d => d.Amount.Value).Take(count).ToList();
        }

        public async Task<Dictionary<ResourceCategory, int>> GetResourceNeedsAsync()
        {
            // Mock implementation - in real app would analyze current needs vs available resources
            return new Dictionary<ResourceCategory, int>
            {
                [ResourceCategory.Food] = 150,
                [ResourceCategory.Clothing] = 200,
                [ResourceCategory.Medical] = 75,
                [ResourceCategory.BabyChildCare] = 50,
                [ResourceCategory.PersonalHygiene] = 100
            };
        }

        public async Task<Dictionary<string, decimal>> GetDonationTrendsByMonthAsync(int months = 12)
        {
            var trends = new Dictionary<string, decimal>();
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var monthKey = date.ToString("yyyy-MM");
                var monthlyTotal = _donations.Where(d => d.DonationDate.Year == date.Year && d.DonationDate.Month == date.Month && d.Amount.HasValue)
                    .Sum(d => d.Amount.Value);
                trends[monthKey] = monthlyTotal;
            }
            return trends;
        }

        // Dashboard
        public async Task<DonationDashboardViewModel> GetDonationDashboardAsync(int userId)
        {
            var recentDonations = await GetRecentDonationsAsync(10);
            var myDonations = await GetDonationsByUserAsync(userId);
            var urgentNeeds = await GetUrgentDonationsAsync();
            var stats = await GetDonationStatsAsync();
            var nearbyDonationCenters = await GetActiveDonationCentersAsync();
            var resourceNeeds = await GetResourceNeedsAsync();

            return new DonationDashboardViewModel
            {
                RecentDonations = recentDonations,
                MyDonations = myDonations.Take(5).ToList(),
                UrgentNeeds = urgentNeeds.Take(5).ToList(),
                Stats = stats,
                NearbyDonationCenters = nearbyDonationCenters.Take(5).ToList(),
                ResourceNeeds = resourceNeeds
            };
        }

        // Simplified implementations for remaining methods
        public async Task<bool> ProcessFinancialDonationAsync(int donationId, string transactionReference, PaymentMethod paymentMethod)
        {
            var donation = await GetDonationAsync(donationId);
            if (donation != null)
            {
                donation.TransactionReference = transactionReference;
                donation.PaymentMethod = paymentMethod;
                donation.Status = DonationStatus.Confirmed;
                await UpdateDonationAsync(donation);
                return true;
            }
            return false;
        }

        public async Task<decimal> GetTotalFinancialDonationsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _donations.Where(d => d.Amount.HasValue);
            
            if (fromDate.HasValue)
                query = query.Where(d => d.DonationDate >= fromDate.Value);
            
            if (toDate.HasValue)
                query = query.Where(d => d.DonationDate <= toDate.Value);
            
            return query.Sum(d => d.Amount.Value);
        }

        public async Task<List<Donation>> GetFinancialDonationsAsync()
        {
            return _donations.Where(d => d.Type == DonationType.Financial || d.Type == DonationType.Mixed).ToList();
        }

        public async Task<string> GenerateTaxReceiptAsync(int donationId)
        {
            var donation = await GetDonationAsync(donationId);
            if (donation == null) return string.Empty;
            
            return $"Tax Receipt #{donation.Id} - ${donation.Amount:F2} - Generated on {DateTime.Now:yyyy-MM-dd}";
        }

        // Donation Centers
        public async Task<DonationCenter> CreateDonationCenterAsync(DonationCenter center)
        {
            center.Id = _donationCenters.Count > 0 ? _donationCenters.Max(c => c.Id) + 1 : 1;
            _donationCenters.Add(center);
            return center;
        }

        public async Task<List<DonationCenter>> GetAllDonationCentersAsync()
        {
            return _donationCenters.ToList();
        }

        public async Task<List<DonationCenter>> GetActiveDonationCentersAsync()
        {
            return _donationCenters.Where(c => c.IsActive).ToList();
        }

        public async Task<DonationCenter?> GetDonationCenterAsync(int centerId)
        {
            return _donationCenters.FirstOrDefault(c => c.Id == centerId);
        }

        public async Task<DonationCenter> UpdateDonationCenterAsync(DonationCenter center)
        {
            var existingCenter = _donationCenters.FirstOrDefault(c => c.Id == center.Id);
            if (existingCenter != null)
            {
                var index = _donationCenters.IndexOf(existingCenter);
                _donationCenters[index] = center;
            }
            return center;
        }

        public async Task<bool> DeleteDonationCenterAsync(int centerId)
        {
            var center = _donationCenters.FirstOrDefault(c => c.Id == centerId);
            if (center != null)
            {
                _donationCenters.Remove(center);
                return true;
            }
            return false;
        }

        public async Task<List<DonationCenter>> GetNearbyDonationCentersAsync(string location)
        {
            return _donationCenters.Where(c => c.IsActive && c.City.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Simplified implementations for remaining interface methods
        public async Task<Dictionary<ResourceCategory, int>> GetInventoryByLocationAsync(string location) => new();
        public async Task<List<ResourceDonation>> GetLowStockResourcesAsync(int threshold = 10) => new();
        public async Task<bool> UpdateResourceLocationAsync(int resourceId, string location) => true;
        public async Task<List<ResourceDonation>> GetResourcesByLocationAsync(string location) => new();
        public async Task<List<ResourceDonation>> GetMatchingResourcesAsync(string needs) => new();
        public async Task<List<Donation>> GetRecommendedDonationsAsync(int userId) => new();
        public async Task<List<ResourceCategory>> GetMostNeededResourcesAsync() => new() { ResourceCategory.Food, ResourceCategory.Clothing, ResourceCategory.Medical };
        public async Task<bool> SendDonationConfirmationAsync(int donationId) => true;
        public async Task<bool> SendPickupNotificationAsync(int donationId) => true;
        public async Task<bool> SendDistributionNotificationAsync(int distributionId) => true;
        public async Task<bool> NotifyDonorOfDistributionAsync(int donationId, int distributionId) => true;
        public async Task<bool> CanUserEditDonationAsync(int donationId, int userId) => true;

        // Missing interface members (simple stubs)
        public Task<bool> CanUserDeleteDonationAsync(int donationId, int userId) => Task.FromResult(true);
        public Task<List<string>> ValidateDonationAsync(Donation donation) => Task.FromResult(new List<string>());
        public Task<bool> IsResourceAvailableAsync(int resourceId, int requestedQuantity) => Task.FromResult(true);
        public Task<string> ExportDonationsAsync(DateTime? fromDate = null, DateTime? toDate = null) => Task.FromResult("CSV data");
        public Task<bool> ImportDonationsAsync(string csvData) => Task.FromResult(true);
        public Task<bool> SchedulePickupAsync(int donationId, DateTime pickupDate) => Task.FromResult(true);
        public Task<bool> UpdateDeliveryStatusAsync(int donationId, string status, string location) => Task.FromResult(true);
        public Task<List<Donation>> GetDonationsRequiringPickupAsync() => Task.FromResult(new List<Donation>());

        private void InitializeSampleData()
        {
            // Sample donation centers
            var sampleCenters = new List<DonationCenter>
            {
                new DonationCenter
                {
                    Id = 1,
                    Name = "Downtown Distribution Center",
                    Address = "123 Main Street",
                    City = "Downtown",
                    State = "State",
                    ZipCode = "12345",
                    Phone = "(555) 123-4567",
                    Email = "downtown@daf.org",
                    OperatingHours = "Mon-Fri 8AM-6PM, Sat 9AM-4PM",
                    Capacity = 1000,
                    CurrentUtilization = 650,
                    AcceptedResourceTypes = "Food, Clothing, Medical Supplies, Household Items",
                    IsActive = true
                },
                new DonationCenter
                {
                    Id = 2,
                    Name = "North Side Warehouse",
                    Address = "456 North Avenue",
                    City = "Northside",
                    State = "State",
                    ZipCode = "12346",
                    Phone = "(555) 234-5678",
                    Email = "northside@daf.org",
                    OperatingHours = "Mon-Sat 7AM-7PM",
                    Capacity = 1500,
                    CurrentUtilization = 800,
                    AcceptedResourceTypes = "All Categories",
                    IsActive = true
                }
            };

            _donationCenters.AddRange(sampleCenters);

            // Sample donations
            var sampleDonations = new List<Donation>
            {
                new Donation
                {
                    Id = 1,
                    Type = DonationType.Financial,
                    Amount = 500.00m,
                    Currency = "USD",
                    PaymentMethod = PaymentMethod.CreditCard,
                    TransactionReference = "TXN123456",
                    Status = DonationStatus.Completed,
                    DonorUserId = 1,
                    DonationDate = DateTime.Now.AddDays(-5),
                    SpecialInstructions = "Please use for emergency food supplies",
                    ReceiptRequired = true,
                    IsTaxDeductible = true
                },
                new Donation
                {
                    Id = 2,
                    Type = DonationType.Resource,
                    Status = DonationStatus.Processing,
                    DonorUserId = 2,
                    DonationDate = DateTime.Now.AddDays(-2),
                    RequiresPickup = true,
                    PickupAddress = "789 Oak Street",
                    PreferredPickupDate = DateTime.Now.AddDays(1),
                    DeliveryMethod = DeliveryMethod.Pickup,
                    SpecialInstructions = "Large donation of winter clothing",
                    UrgencyLevel = UrgencyLevel.High
                }
            };

            _donations.AddRange(sampleDonations);

            // Sample resource donations
            var sampleResources = new List<ResourceDonation>
            {
                new ResourceDonation
                {
                    Id = 1,
                    DonationId = 2,
                    Category = ResourceCategory.Clothing,
                    ItemName = "Winter Coats",
                    Description = "Adult winter coats in various sizes",
                    Quantity = 25,
                    UnitOfMeasure = "pieces",
                    EstimatedValue = 1250.00m,
                    Condition = ItemCondition.Good,
                    Size = "S-XL",
                    Status = ResourceStatus.Available
                },
                new ResourceDonation
                {
                    Id = 2,
                    DonationId = 2,
                    Category = ResourceCategory.Clothing,
                    ItemName = "Children's Sweaters",
                    Description = "Warm sweaters for children ages 5-12",
                    Quantity = 40,
                    UnitOfMeasure = "pieces",
                    EstimatedValue = 800.00m,
                    Condition = ItemCondition.LikeNew,
                    Size = "Child 5-12",
                    Status = ResourceStatus.Available
                }
            };

            _resourceDonations.AddRange(sampleResources);
        }

    }
}