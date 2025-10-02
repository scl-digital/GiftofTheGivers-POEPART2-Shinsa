using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public interface IDonationService
    {
        // Donation Management
        Task<Donation> CreateDonationAsync(DonationViewModel model, int donorUserId);
        Task<Donation?> GetDonationAsync(int donationId);
        Task<Donation> UpdateDonationAsync(Donation donation);
        Task<bool> DeleteDonationAsync(int donationId);
        Task<List<Donation>> GetAllDonationsAsync();
        Task<List<Donation>> SearchDonationsAsync(DonationSearchViewModel searchModel);
        Task<List<Donation>> GetDonationsByStatusAsync(DonationStatus status);
        Task<List<Donation>> GetDonationsByTypeAsync(DonationType type);
        Task<List<Donation>> GetDonationsByUserAsync(int userId);
        Task<List<Donation>> GetRecentDonationsAsync(int count = 10);
        Task<List<Donation>> GetUrgentDonationsAsync();
        
        // Donation Status Management
        Task<bool> UpdateDonationStatusAsync(int donationId, DonationStatus status, int updatedByUserId);
        Task<bool> ConfirmDonationAsync(int donationId, int confirmedByUserId);
        Task<bool> ProcessDonationAsync(int donationId, int processedByUserId);
        Task<bool> ApproveDonationAsync(int donationId, int approvedByUserId);
        Task<bool> RejectDonationAsync(int donationId, int rejectedByUserId, string reason);
        
        // Resource Donation Management
        Task<ResourceDonation> AddResourceDonationAsync(int donationId, ResourceDonationViewModel model);
        Task<List<ResourceDonation>> GetResourceDonationsAsync(int donationId);
        Task<ResourceDonation> UpdateResourceDonationAsync(ResourceDonation resource);
        Task<bool> DeleteResourceDonationAsync(int resourceId);
        Task<List<ResourceDonation>> GetResourcesByCategoryAsync(ResourceCategory category);
        Task<List<ResourceDonation>> GetAvailableResourcesAsync();
        Task<List<ResourceDonation>> GetExpiringResourcesAsync(int daysAhead = 7);
        
        // Quality Control
        Task<bool> PerformQualityCheckAsync(int resourceId, int checkedByUserId, string notes, bool approved);
        Task<List<ResourceDonation>> GetResourcesPendingQualityCheckAsync();
        Task<ResourceDonation?> GetResourceDonationAsync(int resourceId);
        
        // Donation Tracking
        Task<DonationTracking> AddTrackingUpdateAsync(int donationId, DonationStatus status, string location, string notes, int? updatedByUserId = null);
        Task<List<DonationTracking>> GetDonationTrackingAsync(int donationId);
        Task<DonationTracking?> GetLatestTrackingAsync(int donationId);
        
        // Distribution Management
        Task<DonationDistribution> CreateDistributionAsync(int donationId, DistributionViewModel model, int distributedByUserId);
        Task<List<DonationDistribution>> GetDonationDistributionsAsync(int donationId);
        Task<DonationDistribution> UpdateDistributionAsync(DonationDistribution distribution);
        Task<bool> DeleteDistributionAsync(int distributionId);
        Task<List<DonationDistribution>> GetRecentDistributionsAsync(int count = 10);
        Task<List<DonationDistribution>> GetDistributionsByLocationAsync(string location);
        
        // Resource Distribution
        Task<ResourceDistribution> AddResourceDistributionAsync(int distributionId, int resourceDonationId, int quantity, string notes);
        Task<List<ResourceDistribution>> GetResourceDistributionsAsync(int distributionId);
        Task<bool> UpdateResourceQuantitiesAsync(int resourceDonationId, int distributedQuantity);
        
        // Donation Centers
        Task<DonationCenter> CreateDonationCenterAsync(DonationCenter center);
        Task<List<DonationCenter>> GetAllDonationCentersAsync();
        Task<List<DonationCenter>> GetActiveDonationCentersAsync();
        Task<DonationCenter?> GetDonationCenterAsync(int centerId);
        Task<DonationCenter> UpdateDonationCenterAsync(DonationCenter center);
        Task<bool> DeleteDonationCenterAsync(int centerId);
        Task<List<DonationCenter>> GetNearbyDonationCentersAsync(string location);
        
        // Analytics and Reporting
        Task<DonationStats> GetDonationStatsAsync();
        Task<Dictionary<string, object>> GetDonationAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<Donation>> GetTopDonationsAsync(int count = 10);
        Task<Dictionary<ResourceCategory, int>> GetResourceNeedsAsync();
        Task<Dictionary<string, decimal>> GetDonationTrendsByMonthAsync(int months = 12);
        
        // Dashboard
        Task<DonationDashboardViewModel> GetDonationDashboardAsync(int userId);
        
        // Financial Donations
        Task<bool> ProcessFinancialDonationAsync(int donationId, string transactionReference, PaymentMethod paymentMethod);
        Task<decimal> GetTotalFinancialDonationsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<Donation>> GetFinancialDonationsAsync();
        Task<string> GenerateTaxReceiptAsync(int donationId);
        
        // Inventory Management
        Task<Dictionary<ResourceCategory, int>> GetInventoryByLocationAsync(string location);
        Task<List<ResourceDonation>> GetLowStockResourcesAsync(int threshold = 10);
        Task<bool> UpdateResourceLocationAsync(int resourceId, string location);
        Task<List<ResourceDonation>> GetResourcesByLocationAsync(string location);
        
        // Matching and Recommendations
        Task<List<ResourceDonation>> GetMatchingResourcesAsync(string needs);
        Task<List<Donation>> GetRecommendedDonationsAsync(int userId);
        Task<List<ResourceCategory>> GetMostNeededResourcesAsync();
        
        // Notifications and Communication
        Task<bool> SendDonationConfirmationAsync(int donationId);
        Task<bool> SendPickupNotificationAsync(int donationId);
        Task<bool> SendDistributionNotificationAsync(int distributionId);
        Task<bool> NotifyDonorOfDistributionAsync(int donationId, int distributionId);
        
        // Validation and Business Logic
        Task<bool> CanUserEditDonationAsync(int donationId, int userId);
        Task<bool> CanUserDeleteDonationAsync(int donationId, int userId);
        Task<List<string>> ValidateDonationAsync(Donation donation);
        Task<bool> IsResourceAvailableAsync(int resourceId, int requestedQuantity);
        
        // Import/Export
        Task<string> ExportDonationsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<bool> ImportDonationsAsync(string csvData);
        
        // Integration Services
        Task<bool> SchedulePickupAsync(int donationId, DateTime pickupDate);
        Task<bool> UpdateDeliveryStatusAsync(int donationId, string status, string location);
        Task<List<Donation>> GetDonationsRequiringPickupAsync();
    }
}