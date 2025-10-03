using System.ComponentModel.DataAnnotations;

namespace DisasterAlleviationFoundation.Models
{
    public class EmergencyReportViewModel
    {
        [Required(ErrorMessage = "Your name is required")]
        [Display(Name = "Your Name")]
        public string ReporterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "A valid phone number is required")]
        [Phone]
        [Display(Name = "Contact Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [Display(Name = "Location / Address")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe the emergency")]
        [StringLength(2000)]
        [Display(Name = "Emergency Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Urgency Level")]
        public EmergencyUrgency Urgency { get; set; } = EmergencyUrgency.Medium;
    }

    public enum EmergencyUrgency
    {
        Low,
        Medium,
        High,
        Critical
    }
}
