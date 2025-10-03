using System.ComponentModel.DataAnnotations;

namespace DisasterAlleviationFoundation.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}