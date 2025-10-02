using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly List<DisasterIncident> _incidents = new();
        private readonly List<IncidentUpdate> _updates = new();
        private readonly List<IncidentResource> _resources = new();
        private readonly List<IncidentResponse> _responses = new();
        private readonly List<IncidentMedia> _media = new();
        private readonly IAuthenticationService _authService;

        public IncidentService(IAuthenticationService authService)
        {
            _authService = authService;
            InitializeSampleData();
        }

        // Incident Management
        public async Task<DisasterIncident> CreateIncidentAsync(IncidentReportViewModel model, int reportedByUserId)
        {
            var user = await _authService.GetUserByIdAsync(reportedByUserId);
            if (user == null) throw new ArgumentException("User not found");

            var incident = new DisasterIncident
            {
                Id = _incidents.Count > 0 ? _incidents.Max(i => i.Id) + 1 : 1,
                Title = model.Title,
                Type = model.Type,
                Description = model.Description,
                Location = model.Location,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                IncidentDate = model.IncidentDate,
                Severity = model.Severity,
                AffectedPopulation = model.AffectedPopulation,
                Casualties = model.Casualties,
                Injuries = model.Injuries,
                PropertyDamageEstimate = model.PropertyDamageEstimate,
                InfrastructureDamage = model.InfrastructureDamage,
                ImmediateNeeds = model.ImmediateNeeds,
                ResourcesRequired = model.ResourcesRequired,
                AccessRoutes = model.AccessRoutes,
                WeatherConditions = model.WeatherConditions,
                ContactInformation = model.ContactInformation,
                Priority = model.Priority,
                ReportedByUserId = reportedByUserId,
                ReportedBy = user,
                ReportedDate = DateTime.UtcNow,
                Status = IncidentStatus.Reported,
                VerificationStatus = VerificationStatus.Pending
            };

            _incidents.Add(incident);

            // Create initial update
            await AddIncidentUpdateAsync(incident.Id, new IncidentUpdateViewModel
            {
                IncidentId = incident.Id,
                UpdateText = "Incident reported and logged into the system",
                Type = UpdateType.StatusChange,
                IsCritical = incident.Priority == IncidentPriority.Critical || incident.Priority == IncidentPriority.Emergency
            }, reportedByUserId);

            return incident;
        }

        public async Task<DisasterIncident?> GetIncidentAsync(int incidentId)
        {
            return _incidents.FirstOrDefault(i => i.Id == incidentId);
        }

        public async Task<DisasterIncident> UpdateIncidentAsync(DisasterIncident incident)
        {
            var existingIncident = _incidents.FirstOrDefault(i => i.Id == incident.Id);
            if (existingIncident != null)
            {
                var index = _incidents.IndexOf(existingIncident);
                _incidents[index] = incident;
            }
            return incident;
        }

        public async Task<bool> DeleteIncidentAsync(int incidentId)
        {
            var incident = _incidents.FirstOrDefault(i => i.Id == incidentId);
            if (incident != null)
            {
                _incidents.Remove(incident);
                
                // Remove related data
                _updates.RemoveAll(u => u.IncidentId == incidentId);
                _resources.RemoveAll(r => r.IncidentId == incidentId);
                _responses.RemoveAll(r => r.IncidentId == incidentId);
                _media.RemoveAll(m => m.IncidentId == incidentId);
                
                return true;
            }
            return false;
        }

        public async Task<List<DisasterIncident>> GetAllIncidentsAsync()
        {
            return _incidents.OrderByDescending(i => i.ReportedDate).ToList();
        }

        public async Task<List<DisasterIncident>> SearchIncidentsAsync(IncidentSearchViewModel searchModel)
        {
            var query = _incidents.AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                query = query.Where(i => i.Title.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                        i.Description.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                        i.Location.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (searchModel.Type.HasValue)
            {
                query = query.Where(i => i.Type == searchModel.Type.Value);
            }

            if (searchModel.Severity.HasValue)
            {
                query = query.Where(i => i.Severity == searchModel.Severity.Value);
            }

            if (searchModel.Status.HasValue)
            {
                query = query.Where(i => i.Status == searchModel.Status.Value);
            }

            if (searchModel.Priority.HasValue)
            {
                query = query.Where(i => i.Priority == searchModel.Priority.Value);
            }

            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                query = query.Where(i => i.Location.Contains(searchModel.Location, StringComparison.OrdinalIgnoreCase));
            }

            if (searchModel.FromDate.HasValue)
            {
                query = query.Where(i => i.IncidentDate >= searchModel.FromDate.Value);
            }

            if (searchModel.ToDate.HasValue)
            {
                query = query.Where(i => i.IncidentDate <= searchModel.ToDate.Value);
            }

            return query.OrderByDescending(i => i.ReportedDate).ToList();
        }

        public async Task<List<DisasterIncident>> GetIncidentsByStatusAsync(IncidentStatus status)
        {
            return _incidents.Where(i => i.Status == status).OrderByDescending(i => i.ReportedDate).ToList();
        }

        public async Task<List<DisasterIncident>> GetIncidentsByTypeAsync(DisasterType type)
        {
            return _incidents.Where(i => i.Type == type).OrderByDescending(i => i.ReportedDate).ToList();
        }

        public async Task<List<DisasterIncident>> GetIncidentsByUserAsync(int userId)
        {
            return _incidents.Where(i => i.ReportedByUserId == userId).OrderByDescending(i => i.ReportedDate).ToList();
        }

        public async Task<List<DisasterIncident>> GetRecentIncidentsAsync(int count = 10)
        {
            return _incidents.OrderByDescending(i => i.ReportedDate).Take(count).ToList();
        }

        public async Task<List<DisasterIncident>> GetCriticalIncidentsAsync()
        {
            return _incidents.Where(i => i.Priority == IncidentPriority.Critical || i.Priority == IncidentPriority.Emergency)
                .OrderByDescending(i => i.Priority).ThenByDescending(i => i.ReportedDate).ToList();
        }

        // Incident Status Management
        public async Task<bool> UpdateIncidentStatusAsync(int incidentId, IncidentStatus status, int updatedByUserId)
        {
            var incident = await GetIncidentAsync(incidentId);
            if (incident != null)
            {
                var oldStatus = incident.Status;
                incident.Status = status;
                await UpdateIncidentAsync(incident);

                // Add update
                await AddIncidentUpdateAsync(incidentId, new IncidentUpdateViewModel
                {
                    IncidentId = incidentId,
                    UpdateText = $"Status changed from {oldStatus} to {status}",
                    Type = UpdateType.StatusChange
                }, updatedByUserId);

                return true;
            }
            return false;
        }

        public async Task<bool> AssignIncidentAsync(int incidentId, int assignedToUserId)
        {
            var incident = await GetIncidentAsync(incidentId);
            var user = await _authService.GetUserByIdAsync(assignedToUserId);
            
            if (incident != null && user != null)
            {
                incident.AssignedToUserId = assignedToUserId;
                incident.AssignedTo = user;
                await UpdateIncidentAsync(incident);

                await AddIncidentUpdateAsync(incidentId, new IncidentUpdateViewModel
                {
                    IncidentId = incidentId,
                    UpdateText = $"Incident assigned to {user.FullName}",
                    Type = UpdateType.General
                }, assignedToUserId);

                return true;
            }
            return false;
        }

        public async Task<bool> VerifyIncidentAsync(int incidentId, int verifiedByUserId, VerificationStatus status)
        {
            var incident = await GetIncidentAsync(incidentId);
            var user = await _authService.GetUserByIdAsync(verifiedByUserId);
            
            if (incident != null && user != null)
            {
                incident.VerificationStatus = status;
                incident.VerifiedByUserId = verifiedByUserId;
                incident.VerifiedBy = user;
                incident.VerifiedDate = DateTime.UtcNow;
                
                if (status == VerificationStatus.Verified)
                {
                    incident.Status = IncidentStatus.Verified;
                }
                
                await UpdateIncidentAsync(incident);

                await AddIncidentUpdateAsync(incidentId, new IncidentUpdateViewModel
                {
                    IncidentId = incidentId,
                    UpdateText = $"Incident verification status: {status}",
                    Type = UpdateType.StatusChange
                }, verifiedByUserId);

                return true;
            }
            return false;
        }

        public async Task<bool> SetIncidentPriorityAsync(int incidentId, IncidentPriority priority)
        {
            var incident = await GetIncidentAsync(incidentId);
            if (incident != null)
            {
                var oldPriority = incident.Priority;
                incident.Priority = priority;
                await UpdateIncidentAsync(incident);

                await AddIncidentUpdateAsync(incidentId, new IncidentUpdateViewModel
                {
                    IncidentId = incidentId,
                    UpdateText = $"Priority changed from {oldPriority} to {priority}",
                    Type = UpdateType.General,
                    IsCritical = priority == IncidentPriority.Critical || priority == IncidentPriority.Emergency
                }, incident.ReportedByUserId);

                return true;
            }
            return false;
        }

        // Incident Updates
        public async Task<IncidentUpdate> AddIncidentUpdateAsync(int incidentId, IncidentUpdateViewModel model, int updatedByUserId)
        {
            var user = await _authService.GetUserByIdAsync(updatedByUserId);
            if (user == null) throw new ArgumentException("User not found");

            var update = new IncidentUpdate
            {
                Id = _updates.Count > 0 ? _updates.Max(u => u.Id) + 1 : 1,
                IncidentId = incidentId,
                UpdateText = model.UpdateText,
                Type = model.Type,
                IsCritical = model.IsCritical,
                UpdateDate = DateTime.UtcNow,
                UpdatedByUserId = updatedByUserId,
                UpdatedBy = user
            };

            _updates.Add(update);
            return update;
        }

        public async Task<List<IncidentUpdate>> GetIncidentUpdatesAsync(int incidentId)
        {
            return _updates.Where(u => u.IncidentId == incidentId).OrderByDescending(u => u.UpdateDate).ToList();
        }

        public async Task<IncidentUpdate?> GetIncidentUpdateAsync(int updateId)
        {
            return _updates.FirstOrDefault(u => u.Id == updateId);
        }

        public async Task<List<IncidentUpdate>> GetRecentUpdatesAsync(int count = 10)
        {
            return _updates.OrderByDescending(u => u.UpdateDate).Take(count).ToList();
        }

        // Resource Management
        public async Task<IncidentResource> AddIncidentResourceAsync(int incidentId, IncidentResource resource)
        {
            resource.Id = _resources.Count > 0 ? _resources.Max(r => r.Id) + 1 : 1;
            resource.IncidentId = incidentId;
            resource.RequestedDate = DateTime.UtcNow;
            _resources.Add(resource);
            return resource;
        }

        public async Task<List<IncidentResource>> GetIncidentResourcesAsync(int incidentId)
        {
            return _resources.Where(r => r.IncidentId == incidentId).ToList();
        }

        public async Task<IncidentResource> UpdateIncidentResourceAsync(IncidentResource resource)
        {
            var existingResource = _resources.FirstOrDefault(r => r.Id == resource.Id);
            if (existingResource != null)
            {
                var index = _resources.IndexOf(existingResource);
                _resources[index] = resource;
            }
            return resource;
        }

        public async Task<bool> DeleteIncidentResourceAsync(int resourceId)
        {
            var resource = _resources.FirstOrDefault(r => r.Id == resourceId);
            if (resource != null)
            {
                _resources.Remove(resource);
                return true;
            }
            return false;
        }

        public async Task<List<IncidentResource>> GetResourceRequestsByTypeAsync(ResourceType type)
        {
            return _resources.Where(r => r.ResourceType == type).ToList();
        }

        public async Task<List<IncidentResource>> GetUrgentResourceRequestsAsync()
        {
            return _resources.Where(r => r.Priority == ResourcePriority.Critical && r.Status == IncidentResourceStatus.Needed)
                .OrderBy(r => r.RequiredByDate).ToList();
        }

        // Response Management
        public async Task<IncidentResponse> AddIncidentResponseAsync(int incidentId, IncidentResponse response)
        {
            response.Id = _responses.Count > 0 ? _responses.Max(r => r.Id) + 1 : 1;
            response.IncidentId = incidentId;
            response.ResponseDate = DateTime.UtcNow;
            _responses.Add(response);
            return response;
        }

        public async Task<List<IncidentResponse>> GetIncidentResponsesAsync(int incidentId)
        {
            return _responses.Where(r => r.IncidentId == incidentId).OrderByDescending(r => r.ResponseDate).ToList();
        }

        public async Task<IncidentResponse> UpdateIncidentResponseAsync(IncidentResponse response)
        {
            var existingResponse = _responses.FirstOrDefault(r => r.Id == response.Id);
            if (existingResponse != null)
            {
                var index = _responses.IndexOf(existingResponse);
                _responses[index] = response;
            }
            return response;
        }

        public async Task<bool> DeleteIncidentResponseAsync(int responseId)
        {
            var response = _responses.FirstOrDefault(r => r.Id == responseId);
            if (response != null)
            {
                _responses.Remove(response);
                return true;
            }
            return false;
        }

        public async Task<List<IncidentResponse>> GetResponsesByTypeAsync(ResponseType type)
        {
            return _responses.Where(r => r.Type == type).ToList();
        }

        public async Task<List<IncidentResponse>> GetActiveResponsesAsync()
        {
            return _responses.Where(r => r.Status == ResponseStatus.InProgress || r.Status == ResponseStatus.Planned)
                .OrderByDescending(r => r.ResponseDate).ToList();
        }

        // Media Management
        public async Task<IncidentMedia> AddIncidentMediaAsync(int incidentId, IncidentMedia media)
        {
            media.Id = _media.Count > 0 ? _media.Max(m => m.Id) + 1 : 1;
            media.IncidentId = incidentId;
            media.UploadDate = DateTime.UtcNow;
            _media.Add(media);
            return media;
        }

        public async Task<List<IncidentMedia>> GetIncidentMediaAsync(int incidentId)
        {
            return _media.Where(m => m.IncidentId == incidentId).ToList();
        }

        public async Task<IncidentMedia?> GetMediaAsync(int mediaId)
        {
            return _media.FirstOrDefault(m => m.Id == mediaId);
        }

        public async Task<bool> DeleteIncidentMediaAsync(int mediaId)
        {
            var media = _media.FirstOrDefault(m => m.Id == mediaId);
            if (media != null)
            {
                _media.Remove(media);
                return true;
            }
            return false;
        }

        // Analytics and Reporting
        public async Task<IncidentStats> GetIncidentStatsAsync()
        {
            var stats = new IncidentStats
            {
                TotalIncidents = _incidents.Count,
                ActiveIncidents = _incidents.Count(i => i.Status == IncidentStatus.ResponseInProgress || i.Status == IncidentStatus.Verified),
                ResolvedIncidents = _incidents.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
                CriticalIncidents = _incidents.Count(i => i.Priority == IncidentPriority.Critical || i.Priority == IncidentPriority.Emergency),
                IncidentsThisMonth = _incidents.Count(i => i.ReportedDate >= DateTime.Now.AddDays(-30)),
                PeopleAffected = _incidents.Where(i => i.AffectedPopulation.HasValue).Sum(i => i.AffectedPopulation.Value)
            };

            // Group by type
            stats.IncidentsByType = _incidents.GroupBy(i => i.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by severity
            stats.IncidentsBySeverity = _incidents.GroupBy(i => i.Severity)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        public async Task<Dictionary<string, object>> GetIncidentAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _incidents.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(i => i.IncidentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.IncidentDate <= toDate.Value);

            var incidents = query.ToList();

            return new Dictionary<string, object>
            {
                ["TotalIncidents"] = incidents.Count,
                ["AverageResponseTime"] = 2.5, // Mock data
                ["TotalCasualties"] = incidents.Where(i => i.Casualties.HasValue).Sum(i => i.Casualties.Value),
                ["TotalInjuries"] = incidents.Where(i => i.Injuries.HasValue).Sum(i => i.Injuries.Value),
                ["TotalPropertyDamage"] = incidents.Where(i => i.PropertyDamageEstimate.HasValue).Sum(i => i.PropertyDamageEstimate.Value),
                ["MostCommonType"] = incidents.GroupBy(i => i.Type).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key.ToString(),
                ["AverageSeverity"] = incidents.Average(i => (int)i.Severity),
                ["VerificationRate"] = incidents.Count > 0 ? (double)incidents.Count(i => i.VerificationStatus == VerificationStatus.Verified) / incidents.Count * 100 : 0
            };
        }

        public async Task<List<DisasterIncident>> GetIncidentsByLocationAsync(string location)
        {
            return _incidents.Where(i => i.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task<List<DisasterIncident>> GetIncidentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return _incidents.Where(i => i.IncidentDate >= fromDate && i.IncidentDate <= toDate).ToList();
        }

        // Dashboard
        public async Task<IncidentDashboardViewModel> GetIncidentDashboardAsync(int userId)
        {
            var recentIncidents = await GetRecentIncidentsAsync(10);
            var criticalIncidents = await GetCriticalIncidentsAsync();
            var myIncidents = await GetIncidentsByUserAsync(userId);
            var stats = await GetIncidentStatsAsync();
            var recentUpdates = await GetRecentUpdatesAsync(10);

            return new IncidentDashboardViewModel
            {
                RecentIncidents = recentIncidents,
                CriticalIncidents = criticalIncidents.Take(5).ToList(),
                MyIncidents = myIncidents.Take(5).ToList(),
                Stats = stats,
                RecentUpdates = recentUpdates
            };
        }

        // Additional methods with simplified implementations
        public async Task<List<DisasterIncident>> GetIncidentsRequiringAttentionAsync()
        {
            return _incidents.Where(i => 
                i.Status == IncidentStatus.Reported || 
                i.VerificationStatus == VerificationStatus.Pending ||
                i.Priority == IncidentPriority.Critical ||
                i.Priority == IncidentPriority.Emergency
            ).ToList();
        }

        public async Task<bool> SendIncidentAlertAsync(int incidentId, string message, List<int> userIds)
        {
            // Mock implementation - in real app would send notifications
            return true;
        }

        public async Task<bool> CanUserEditIncidentAsync(int incidentId, int userId)
        {
            var incident = await GetIncidentAsync(incidentId);
            return incident?.ReportedByUserId == userId || incident?.AssignedToUserId == userId;
        }

        public async Task<bool> CanUserDeleteIncidentAsync(int incidentId, int userId)
        {
            var incident = await GetIncidentAsync(incidentId);
            var user = await _authService.GetUserByIdAsync(userId);
            return incident?.ReportedByUserId == userId || user?.Role == "Admin";
        }

        public async Task<List<string>> ValidateIncidentDataAsync(DisasterIncident incident)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(incident.Title))
                errors.Add("Title is required");
            
            if (string.IsNullOrEmpty(incident.Description))
                errors.Add("Description is required");
            
            if (string.IsNullOrEmpty(incident.Location))
                errors.Add("Location is required");
            
            if (incident.IncidentDate > DateTime.Now)
                errors.Add("Incident date cannot be in the future");
            
            return errors;
        }

        public async Task<List<DisasterIncident>> GetIncidentsNearLocationAsync(double latitude, double longitude, double radiusKm)
        {
            // Simplified implementation - in real app would use proper geographic calculations
            return _incidents.Where(i => i.Latitude.HasValue && i.Longitude.HasValue).ToList();
        }

        public async Task<bool> UpdateIncidentLocationAsync(int incidentId, double latitude, double longitude)
        {
            var incident = await GetIncidentAsync(incidentId);
            if (incident != null)
            {
                incident.Latitude = latitude;
                incident.Longitude = longitude;
                await UpdateIncidentAsync(incident);
                return true;
            }
            return false;
        }

        public async Task<bool> NotifyEmergencyServicesAsync(int incidentId)
        {
            // Mock implementation
            return true;
        }

        public async Task<bool> NotifyMediaAsync(int incidentId)
        {
            // Mock implementation
            return true;
        }

        public async Task<string> GenerateIncidentReportAsync(int incidentId)
        {
            var incident = await GetIncidentAsync(incidentId);
            if (incident == null) return string.Empty;
            
            return $"Incident Report #{incident.Id} - {incident.Title} - Generated on {DateTime.Now:yyyy-MM-dd HH:mm}";
        }

        private void InitializeSampleData()
        {
            // Sample incidents
            var sampleIncidents = new List<DisasterIncident>
            {
                new DisasterIncident
                {
                    Id = 1,
                    Title = "Flash Flood in Downtown Area",
                    Type = DisasterType.Flood,
                    Description = "Heavy rainfall caused flash flooding in the downtown business district. Several streets are impassable and businesses are affected.",
                    Location = "Downtown Business District, Main Street",
                    IncidentDate = DateTime.Now.AddHours(-6),
                    Severity = SeverityLevel.Major,
                    Status = IncidentStatus.ResponseInProgress,
                    Priority = IncidentPriority.High,
                    AffectedPopulation = 500,
                    PropertyDamageEstimate = 250000,
                    ImmediateNeeds = "Water pumping equipment, traffic control, temporary shelters",
                    ResourcesRequired = "Emergency vehicles, pumps, sandbags",
                    ReportedByUserId = 1,
                    ReportedDate = DateTime.Now.AddHours(-6),
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedDate = DateTime.Now.AddHours(-5)
                },
                new DisasterIncident
                {
                    Id = 2,
                    Title = "Apartment Building Fire",
                    Type = DisasterType.Other,
                    Description = "Fire broke out in a 4-story apartment building. Fire department is on scene. Residents evacuated.",
                    Location = "Oak Street Apartments, 123 Oak Street",
                    IncidentDate = DateTime.Now.AddHours(-2),
                    Severity = SeverityLevel.Severe,
                    Status = IncidentStatus.ResponseInProgress,
                    Priority = IncidentPriority.Critical,
                    AffectedPopulation = 48,
                    Casualties = 0,
                    Injuries = 3,
                    ImmediateNeeds = "Temporary housing, medical attention, clothing",
                    ResourcesRequired = "Emergency shelter, medical supplies, clothing donations",
                    ReportedByUserId = 2,
                    ReportedDate = DateTime.Now.AddHours(-2),
                    VerificationStatus = VerificationStatus.Verified,
                    VerifiedDate = DateTime.Now.AddHours(-1)
                }
            };

            _incidents.AddRange(sampleIncidents);

            // Sample updates
            var sampleUpdates = new List<IncidentUpdate>
            {
                new IncidentUpdate
                {
                    Id = 1,
                    IncidentId = 1,
                    UpdateText = "Water levels are receding. Main Street partially reopened to emergency vehicles.",
                    Type = UpdateType.General,
                    UpdateDate = DateTime.Now.AddHours(-2),
                    UpdatedByUserId = 1
                },
                new IncidentUpdate
                {
                    Id = 2,
                    IncidentId = 2,
                    UpdateText = "Fire is under control. All residents safely evacuated. 3 people treated for smoke inhalation.",
                    Type = UpdateType.CasualtyUpdate,
                    UpdateDate = DateTime.Now.AddMinutes(-30),
                    UpdatedByUserId = 1,
                    IsCritical = true
                }
            };

            _updates.AddRange(sampleUpdates);
        }
    }
}