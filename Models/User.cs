using System.ComponentModel.DataAnnotations;

namespace DisasterAlleviationFoundation.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Last Login")]
        public DateTime? LastLoginAt { get; set; }
        
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Role")]
        public string Role { get; set; } = "User"; // User, Admin, Volunteer, etc.
        
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}