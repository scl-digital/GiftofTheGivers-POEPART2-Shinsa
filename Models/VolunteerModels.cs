using System.ComponentModel.DataAnnotations;

namespace DisasterAlleviationFoundation.Models
{
    // Volunteer Profile
    public class VolunteerProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; } = string.Empty;
        
        [Display(Name = "Emergency Contact Phone")]
        [Phone]
        public string EmergencyContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "Skills")]
        public string Skills { get; set; } = string.Empty;
        
        [Display(Name = "Availability")]
        public string Availability { get; set; } = string.Empty;
        
        [Display(Name = "Transportation")]
        public bool HasTransportation { get; set; }
        
        [Display(Name = "Medical Training")]
        public bool HasMedicalTraining { get; set; }
        
        [Display(Name = "Languages Spoken")]
        public string Languages { get; set; } = string.Empty;
        
        [Display(Name = "Previous Experience")]
        public string PreviousExperience { get; set; } = string.Empty;
        
        [Display(Name = "Background Check Status")]
        public BackgroundCheckStatus BackgroundCheckStatus { get; set; } = BackgroundCheckStatus.Pending;
        
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Status")]
        public VolunteerStatus Status { get; set; } = VolunteerStatus.Active;
        
        public List<VolunteerTaskAssignment> TaskAssignments { get; set; } = new();
        public List<VolunteerAvailability> AvailabilitySchedule { get; set; } = new();
    }

    // Volunteer Tasks
    public class VolunteerTask
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Task Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Category")]
        public TaskCategory Category { get; set; }
        
        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }
        
        [Display(Name = "Required Skills")]
        public string RequiredSkills { get; set; } = string.Empty;
        
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;
        
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Estimated Hours")]
        public int EstimatedHours { get; set; }
        
        [Display(Name = "Maximum Volunteers")]
        public int MaxVolunteers { get; set; }
        
        [Display(Name = "Status")]
        public TaskStatus Status { get; set; } = TaskStatus.Open;
        
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public int? CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        
        public List<VolunteerTaskAssignment> Assignments { get; set; } = new();
    }

    // Task Assignment
    public class VolunteerTaskAssignment
    {
        public int Id { get; set; }
        public int VolunteerProfileId { get; set; }
        public VolunteerProfile VolunteerProfile { get; set; } = null!;
        public int TaskId { get; set; }
        public VolunteerTask Task { get; set; } = null!;
        
        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Status")]
        public AssignmentStatus Status { get; set; } = AssignmentStatus.Assigned;
        
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }
        
        [Display(Name = "Notes")]
        public string Notes { get; set; } = string.Empty;
        
        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }
        
        [Display(Name = "Rating")]
        [Range(1, 5)]
        public int? Rating { get; set; }
        
        [Display(Name = "Feedback")]
        public string Feedback { get; set; } = string.Empty;
    }

    // Volunteer Availability
    public class VolunteerAvailability
    {
        public int Id { get; set; }
        public int VolunteerProfileId { get; set; }
        public VolunteerProfile VolunteerProfile { get; set; } = null!;
        
        [Display(Name = "Day of Week")]
        public DayOfWeek DayOfWeek { get; set; }
        
        [Display(Name = "Start Time")]
        public TimeSpan StartTime { get; set; }
        
        [Display(Name = "End Time")]
        public TimeSpan EndTime { get; set; }
        
        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;
    }

    // Communication
    public class VolunteerCommunication
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
        
        [Display(Name = "Communication Type")]
        public CommunicationType Type { get; set; }
        
        [Display(Name = "Sent Date")]
        public DateTime SentDate { get; set; } = DateTime.UtcNow;
        
        public int SentByUserId { get; set; }
        public User SentBy { get; set; } = null!;
        
        public int? VolunteerProfileId { get; set; }
        public VolunteerProfile? VolunteerProfile { get; set; }
        
        public int? TaskId { get; set; }
        public VolunteerTask? Task { get; set; }
        
        [Display(Name = "Is Read")]
        public bool IsRead { get; set; } = false;
    }

    // Enums
    public enum VolunteerStatus
    {
        Active,
        Inactive,
        Suspended,
        Training
    }

    public enum BackgroundCheckStatus
    {
        Pending,
        Approved,
        Rejected,
        NotRequired
    }

    public enum TaskCategory
    {
        [Display(Name = "Emergency Response")]
        EmergencyResponse,
        [Display(Name = "Food Distribution")]
        FoodDistribution,
        [Display(Name = "Medical Support")]
        MedicalSupport,
        [Display(Name = "Shelter Management")]
        ShelterManagement,
        [Display(Name = "Transportation")]
        Transportation,
        [Display(Name = "Communication")]
        Communication,
        [Display(Name = "Administrative")]
        Administrative,
        [Display(Name = "Cleanup")]
        Cleanup,
        [Display(Name = "Fundraising")]
        Fundraising,
        [Display(Name = "Training")]
        Training
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        Open,
        InProgress,
        Completed,
        Cancelled,
        OnHold
    }

    public enum AssignmentStatus
    {
        Assigned,
        Accepted,
        InProgress,
        Completed,
        Declined,
        Cancelled
    }

    public enum CommunicationType
    {
        General,
        TaskAssignment,
        TaskUpdate,
        Emergency,
        Training,
        Feedback
    }

    // View Models
    public class VolunteerRegistrationViewModel
    {
        [Required]
        [Display(Name = "Phone Number")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContact { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Emergency Contact Phone")]
        [Phone]
        public string EmergencyContactPhone { get; set; } = string.Empty;
        
        [Display(Name = "Skills (comma separated)")]
        public string Skills { get; set; } = string.Empty;
        
        [Display(Name = "General Availability")]
        public string Availability { get; set; } = string.Empty;
        
        [Display(Name = "I have reliable transportation")]
        public bool HasTransportation { get; set; }
        
        [Display(Name = "I have medical training")]
        public bool HasMedicalTraining { get; set; }
        
        [Display(Name = "Languages Spoken")]
        public string Languages { get; set; } = string.Empty;
        
        [Display(Name = "Previous Volunteer Experience")]
        public string PreviousExperience { get; set; } = string.Empty;
    }

    public class TaskSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public TaskCategory? Category { get; set; }
        public TaskPriority? Priority { get; set; }
        public string? Location { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool OnlyAvailable { get; set; } = true;
        
        public List<VolunteerTask> Tasks { get; set; } = new();
        public int TotalTasks { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class VolunteerDashboardViewModel
    {
        public VolunteerProfile Profile { get; set; } = null!;
        public List<VolunteerTaskAssignment> ActiveAssignments { get; set; } = new();
        public List<VolunteerTaskAssignment> CompletedAssignments { get; set; } = new();
        public List<VolunteerCommunication> RecentCommunications { get; set; } = new();
        public VolunteerStats Stats { get; set; } = new();
        public List<VolunteerTask> AvailableTasks { get; set; } = new();
    }

    public class VolunteerStats
    {
        public int TotalHoursWorked { get; set; }
        public int TasksCompleted { get; set; }
        public int ActiveTasks { get; set; }
        public double AverageRating { get; set; }
        public int DaysActive { get; set; }
    }
}