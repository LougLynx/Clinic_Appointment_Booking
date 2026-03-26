using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.DTOs.admin.doctor_management
{
    public class CreateDoctorDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public int SpecialtyId { get; set; }

        public decimal ConsultationFee { get; set; }

        public int YearsOfExperience { get; set; }

        [MaxLength(200)]
        public string? Qualifications { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }
    }
}
