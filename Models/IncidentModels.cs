using System.ComponentModel.DataAnnotations;

namespace DisasterAlleviationFoundation.Models
{
    // Main Incident Report
    public class DisasterIncident
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Incident Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Incident Type")]
        public DisasterType Type { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
        
        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }
        
        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }
        
        [Required]
        [Display(Name = "Incident Date")]
        public DateTime IncidentDate { get; set; }
        
        [Display(Name = "Severity Level")]
        public SeverityLevel Severity { get; set; }
        
        [Display(Name = "Status")]
        public IncidentStatus Status { get; set; } = IncidentStatus.Reported;
        
        [Display(Name = "Affected Population")]
        public int? AffectedPopulation { get; set; }
        
        [Display(Name = "Casualties")]
        public int? Casualties { get; set; }
        
        [Display(Name = "Injuries")]
        public int? Injuries { get; set; }
        
        [Display(Name = "Property Damage Estimate")]
        [DataType(DataType.Currency)]
        public decimal? PropertyDamageEstimate { get; set; }
        
        [Display(Name = "Infrastructure Damage")]
        public string InfrastructureDamage { get; set; } = string.Empty;
        
        [Display(Name = "Immediate Needs")]
        public string ImmediateNeeds { get; set; } = string.Empty;
        
        [Display(Name = "Resources Required")]
        public string ResourcesRequired { get; set; } = string.Empty;
        
        [Display(Name = "Access Routes")]
        public string AccessRoutes { get; set; } = string.Empty;
        
        [Display(Name = "Weather Conditions")]
        public string WeatherConditions { get; set; } = string.Empty;
        
        [Display(Name = "Contact Information")]
        public string ContactInformation { get; set; } = string.Empty;
        
        [Display(Name = "Reported Date")]
        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;
        
        public int ReportedByUserId { get; set; }
        public User ReportedBy { get; set; } = null!;
        
        public int? AssignedToUserId { get; set; }
        public User? AssignedTo { get; set; }
        
        [Display(Name = "Priority")]
        public IncidentPriority Priority { get; set; } = IncidentPriority.Medium;
        
        [Display(Name = "Verification Status")]
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        
        [Display(Name = "Verified Date")]
        public DateTime? VerifiedDate { get; set; }
        
        public int? VerifiedByUserId { get; set; }
        public User? VerifiedBy { get; set; }
        
        public List<IncidentUpdate> Updates { get; set; } = new();
        public List<IncidentResource> RequiredResources { get; set; } = new();
        public List<IncidentResponse> Responses { get; set; } = new();
        public List<IncidentMedia> MediaFiles { get; set; } = new();
    }

    // Incident Updates
    public class IncidentUpdate
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public DisasterIncident Incident { get; set; } = null!;
        
        [Required]
        [Display(Name = "Update")]
        public string UpdateText { get; set; } = string.Empty;
        
        [Display(Name = "Update Type")]
        public UpdateType Type { get; set; }
        
        [Display(Name = "Update Date")]
        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
        
        public int UpdatedByUserId { get; set; }
        public User UpdatedBy { get; set; } = null!;
        
        [Display(Name = "Is Critical")]
        public bool IsCritical { get; set; } = false;
    }

    // Resources Required for Incident
    public class IncidentResource
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public DisasterIncident Incident { get; set; } = null!;
        
        [Required]
        [Display(Name = "Resource Type")]
        public ResourceType ResourceType { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Quantity Needed")]
        public int QuantityNeeded { get; set; }
        
        [Display(Name = "Quantity Available")]
        public int QuantityAvailable { get; set; }
        
        [Display(Name = "Priority")]
        public ResourcePriority Priority { get; set; }
        
        [Display(Name = "Status")]
        public IncidentResourceStatus Status { get; set; } = IncidentResourceStatus.Needed;
        
        [Display(Name = "Requested Date")]
        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Required By Date")]
        public DateTime? RequiredByDate { get; set; }
    }

    // Response Actions
    public class IncidentResponse
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public DisasterIncident Incident { get; set; } = null!;
        
        [Required]
        [Display(Name = "Response Type")]
        public ResponseType Type { get; set; }
        
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Response Date")]
        public DateTime ResponseDate { get; set; } = DateTime.UtcNow;
        
        public int RespondedByUserId { get; set; }
        public User RespondedBy { get; set; } = null!;
        
        [Display(Name = "Status")]
        public ResponseStatus Status { get; set; } = ResponseStatus.Planned;
        
        [Display(Name = "Resources Used")]
        public string ResourcesUsed { get; set; } = string.Empty;
        
        [Display(Name = "Personnel Involved")]
        public int PersonnelInvolved { get; set; }
        
        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        public decimal? Cost { get; set; }
        
        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
    }

    // Media Files
    public class IncidentMedia
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public DisasterIncident Incident { get; set; } = null!;
        
        [Required]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;
        
        [Display(Name = "File Path")]
        public string FilePath { get; set; } = string.Empty;
        
        [Display(Name = "File Type")]
        public MediaType MediaType { get; set; }
        
        [Display(Name = "File Size")]
        public long FileSize { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Upload Date")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        public int UploadedByUserId { get; set; }
        public User UploadedBy { get; set; } = null!;
    }

    // Enums
    public enum DisasterType
    {
        [Display(Name = "Earthquake")]
        Earthquake,
        [Display(Name = "Flood")]
        Flood,
        [Display(Name = "Hurricane/Typhoon")]
        Hurricane,
        [Display(Name = "Tornado")]
        Tornado,
        [Display(Name = "Wildfire")]
        Wildfire,
        [Display(Name = "Landslide")]
        Landslide,
        [Display(Name = "Tsunami")]
        Tsunami,
        [Display(Name = "Volcanic Eruption")]
        VolcanicEruption,
        [Display(Name = "Drought")]
        Drought,
        [Display(Name = "Extreme Weather")]
        ExtremeWeather,
        [Display(Name = "Industrial Accident")]
        IndustrialAccident,
        [Display(Name = "Building Collapse")]
        BuildingCollapse,
        [Display(Name = "Transportation Accident")]
        TransportationAccident,
        [Display(Name = "Other")]
        Other
    }

    public enum SeverityLevel
    {
        [Display(Name = "Minor")]
        Minor = 1,
        [Display(Name = "Moderate")]
        Moderate = 2,
        [Display(Name = "Major")]
        Major = 3,
        [Display(Name = "Severe")]
        Severe = 4,
        [Display(Name = "Catastrophic")]
        Catastrophic = 5
    }

    public enum IncidentStatus
    {
        Reported,
        UnderReview,
        Verified,
        ResponseInProgress,
        Resolved,
        Closed,
        Duplicate,
        Invalid
    }

    public enum IncidentPriority
    {
        Low,
        Medium,
        High,
        Critical,
        Emergency
    }

    public enum VerificationStatus
    {
        Pending,
        Verified,
        RequiresMoreInfo,
        Rejected,
        Duplicate
    }

    public enum UpdateType
    {
        General,
        StatusChange,
        ResourceUpdate,
        CasualtyUpdate,
        WeatherUpdate,
        AccessUpdate,
        ResponseUpdate
    }

    public enum ResourceType
    {
        [Display(Name = "Food & Water")]
        FoodWater,
        [Display(Name = "Medical Supplies")]
        MedicalSupplies,
        [Display(Name = "Shelter Materials")]
        ShelterMaterials,
        [Display(Name = "Clothing")]
        Clothing,
        [Display(Name = "Transportation")]
        Transportation,
        [Display(Name = "Communication Equipment")]
        CommunicationEquipment,
        [Display(Name = "Heavy Machinery")]
        HeavyMachinery,
        [Display(Name = "Personnel")]
        Personnel,
        [Display(Name = "Financial Aid")]
        FinancialAid,
        [Display(Name = "Other")]
        Other
    }

    public enum ResourcePriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum IncidentResourceStatus
    {
        Needed,
        Requested,
        InTransit,
        Delivered,
        Cancelled
    }

    public enum ResponseType
    {
        [Display(Name = "Search & Rescue")]
        SearchRescue,
        [Display(Name = "Medical Response")]
        MedicalResponse,
        [Display(Name = "Evacuation")]
        Evacuation,
        [Display(Name = "Shelter Setup")]
        ShelterSetup,
        [Display(Name = "Food Distribution")]
        FoodDistribution,
        [Display(Name = "Infrastructure Repair")]
        InfrastructureRepair,
        [Display(Name = "Communication Setup")]
        CommunicationSetup,
        [Display(Name = "Assessment")]
        Assessment,
        [Display(Name = "Cleanup")]
        Cleanup,
        [Display(Name = "Other")]
        Other
    }

    public enum ResponseStatus
    {
        Planned,
        InProgress,
        Completed,
        Cancelled,
        OnHold
    }

    public enum MediaType
    {
        Image,
        Video,
        Audio,
        Document,
        Other
    }

    // View Models
    public class IncidentReportViewModel
    {
        [Required]
        [Display(Name = "Incident Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "What type of disaster/emergency is this?")]
        public DisasterType Type { get; set; }
        
        [Required]
        [Display(Name = "Please describe what happened")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Location (Address or Description)")]
        public string Location { get; set; } = string.Empty;
        
        [Display(Name = "Latitude (if known)")]
        public double? Latitude { get; set; }
        
        [Display(Name = "Longitude (if known)")]
        public double? Longitude { get; set; }
        
        [Required]
        [Display(Name = "When did this incident occur?")]
        public DateTime IncidentDate { get; set; } = DateTime.Now;
        
        [Display(Name = "How severe is this incident?")]
        public SeverityLevel Severity { get; set; }
        
        [Display(Name = "Estimated number of people affected")]
        public int? AffectedPopulation { get; set; }
        
        [Display(Name = "Number of casualties (if known)")]
        public int? Casualties { get; set; }
        
        [Display(Name = "Number of injuries (if known)")]
        public int? Injuries { get; set; }
        
        [Display(Name = "Estimated property damage")]
        [DataType(DataType.Currency)]
        public decimal? PropertyDamageEstimate { get; set; }
        
        [Display(Name = "Infrastructure damage (roads, bridges, utilities, etc.)")]
        [DataType(DataType.MultilineText)]
        public string InfrastructureDamage { get; set; } = string.Empty;
        
        [Display(Name = "What are the immediate needs?")]
        [DataType(DataType.MultilineText)]
        public string ImmediateNeeds { get; set; } = string.Empty;
        
        [Display(Name = "What resources are needed?")]
        [DataType(DataType.MultilineText)]
        public string ResourcesRequired { get; set; } = string.Empty;
        
        [Display(Name = "Are access routes clear? Any road closures?")]
        [DataType(DataType.MultilineText)]
        public string AccessRoutes { get; set; } = string.Empty;
        
        [Display(Name = "Current weather conditions")]
        public string WeatherConditions { get; set; } = string.Empty;
        
        [Display(Name = "Your contact information")]
        public string ContactInformation { get; set; } = string.Empty;
        
        [Display(Name = "How urgent is this situation?")]
        public IncidentPriority Priority { get; set; } = IncidentPriority.Medium;
    }

    public class IncidentSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public DisasterType? Type { get; set; }
        public SeverityLevel? Severity { get; set; }
        public IncidentStatus? Status { get; set; }
        public IncidentPriority? Priority { get; set; }
        public string? Location { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        public List<DisasterIncident> Incidents { get; set; } = new();
        public int TotalIncidents { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class IncidentDashboardViewModel
    {
        public List<DisasterIncident> RecentIncidents { get; set; } = new();
        public List<DisasterIncident> CriticalIncidents { get; set; } = new();
        public List<DisasterIncident> MyIncidents { get; set; } = new();
        public IncidentStats Stats { get; set; } = new();
        public List<IncidentUpdate> RecentUpdates { get; set; } = new();
    }

    public class IncidentStats
    {
        public int TotalIncidents { get; set; }
        public int ActiveIncidents { get; set; }
        public int ResolvedIncidents { get; set; }
        public int CriticalIncidents { get; set; }
        public int IncidentsThisMonth { get; set; }
        public int PeopleAffected { get; set; }
        public Dictionary<DisasterType, int> IncidentsByType { get; set; } = new();
        public Dictionary<SeverityLevel, int> IncidentsBySeverity { get; set; } = new();
    }

    public class IncidentUpdateViewModel
    {
        public int IncidentId { get; set; }
        
        [Required]
        [Display(Name = "Update")]
        [DataType(DataType.MultilineText)]
        public string UpdateText { get; set; } = string.Empty;
        
        [Display(Name = "Update Type")]
        public UpdateType Type { get; set; }
        
        [Display(Name = "This is a critical update")]
        public bool IsCritical { get; set; } = false;
    }
}