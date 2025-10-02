using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public interface IIncidentService
    {
        // Incident Management
        Task<DisasterIncident> CreateIncidentAsync(IncidentReportViewModel model, int reportedByUserId);
        Task<DisasterIncident?> GetIncidentAsync(int incidentId);
        Task<DisasterIncident> UpdateIncidentAsync(DisasterIncident incident);
        Task<bool> DeleteIncidentAsync(int incidentId);
        Task<List<DisasterIncident>> GetAllIncidentsAsync();
        Task<List<DisasterIncident>> SearchIncidentsAsync(IncidentSearchViewModel searchModel);
        Task<List<DisasterIncident>> GetIncidentsByStatusAsync(IncidentStatus status);
        Task<List<DisasterIncident>> GetIncidentsByTypeAsync(DisasterType type);
        Task<List<DisasterIncident>> GetIncidentsByUserAsync(int userId);
        Task<List<DisasterIncident>> GetRecentIncidentsAsync(int count = 10);
        Task<List<DisasterIncident>> GetCriticalIncidentsAsync();
        
        // Incident Status Management
        Task<bool> UpdateIncidentStatusAsync(int incidentId, IncidentStatus status, int updatedByUserId);
        Task<bool> AssignIncidentAsync(int incidentId, int assignedToUserId);
        Task<bool> VerifyIncidentAsync(int incidentId, int verifiedByUserId, VerificationStatus status);
        Task<bool> SetIncidentPriorityAsync(int incidentId, IncidentPriority priority);
        
        // Incident Updates
        Task<IncidentUpdate> AddIncidentUpdateAsync(int incidentId, IncidentUpdateViewModel model, int updatedByUserId);
        Task<List<IncidentUpdate>> GetIncidentUpdatesAsync(int incidentId);
        Task<IncidentUpdate?> GetIncidentUpdateAsync(int updateId);
        Task<List<IncidentUpdate>> GetRecentUpdatesAsync(int count = 10);
        
        // Resource Management
        Task<IncidentResource> AddIncidentResourceAsync(int incidentId, IncidentResource resource);
        Task<List<IncidentResource>> GetIncidentResourcesAsync(int incidentId);
        Task<IncidentResource> UpdateIncidentResourceAsync(IncidentResource resource);
        Task<bool> DeleteIncidentResourceAsync(int resourceId);
        Task<List<IncidentResource>> GetResourceRequestsByTypeAsync(ResourceType type);
        Task<List<IncidentResource>> GetUrgentResourceRequestsAsync();
        
        // Response Management
        Task<IncidentResponse> AddIncidentResponseAsync(int incidentId, IncidentResponse response);
        Task<List<IncidentResponse>> GetIncidentResponsesAsync(int incidentId);
        Task<IncidentResponse> UpdateIncidentResponseAsync(IncidentResponse response);
        Task<bool> DeleteIncidentResponseAsync(int responseId);
        Task<List<IncidentResponse>> GetResponsesByTypeAsync(ResponseType type);
        Task<List<IncidentResponse>> GetActiveResponsesAsync();
        
        // Media Management
        Task<IncidentMedia> AddIncidentMediaAsync(int incidentId, IncidentMedia media);
        Task<List<IncidentMedia>> GetIncidentMediaAsync(int incidentId);
        Task<IncidentMedia?> GetMediaAsync(int mediaId);
        Task<bool> DeleteIncidentMediaAsync(int mediaId);
        
        // Analytics and Reporting
        Task<IncidentStats> GetIncidentStatsAsync();
        Task<Dictionary<string, object>> GetIncidentAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<DisasterIncident>> GetIncidentsByLocationAsync(string location);
        Task<List<DisasterIncident>> GetIncidentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        
        // Dashboard
        Task<IncidentDashboardViewModel> GetIncidentDashboardAsync(int userId);
        
        // Notifications and Alerts
        Task<List<DisasterIncident>> GetIncidentsRequiringAttentionAsync();
        Task<bool> SendIncidentAlertAsync(int incidentId, string message, List<int> userIds);
        
        // Validation and Business Logic
        Task<bool> CanUserEditIncidentAsync(int incidentId, int userId);
        Task<bool> CanUserDeleteIncidentAsync(int incidentId, int userId);
        Task<List<string>> ValidateIncidentDataAsync(DisasterIncident incident);
        
        // Geographic and Location Services
        Task<List<DisasterIncident>> GetIncidentsNearLocationAsync(double latitude, double longitude, double radiusKm);
        Task<bool> UpdateIncidentLocationAsync(int incidentId, double latitude, double longitude);
        
        // Integration and External Services
        Task<bool> NotifyEmergencyServicesAsync(int incidentId);
        Task<bool> NotifyMediaAsync(int incidentId);
        Task<string> GenerateIncidentReportAsync(int incidentId);
    }
}