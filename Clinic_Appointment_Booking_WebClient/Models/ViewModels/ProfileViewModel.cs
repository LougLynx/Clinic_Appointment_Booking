using System.ComponentModel.DataAnnotations;

namespace Clinic_Appointment_Booking_WebClient.Models.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        //[Phone]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;

        public bool EmailVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
