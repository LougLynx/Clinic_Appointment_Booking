using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, In Progress, Completed, Cancelled

        [Required]
        [MaxLength(100)]
        public string ReasonForVisit { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? AdditionalNotes { get; set; }

        public bool IsFirstTime { get; set; } = false;

        public int EstimatedDurationMinutes { get; set; } = 30;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        // Navigation properties
        public virtual User Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual MedicalRecord? MedicalRecord { get; set; }
        public virtual Payment? Payment { get; set; }
    }
}
