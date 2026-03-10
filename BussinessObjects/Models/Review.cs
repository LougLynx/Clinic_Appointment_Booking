using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("Appointment")]
        public int? AppointmentId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual User Patient { get; set; } = null!;
        public virtual Appointment? Appointment { get; set; }
    }
}
