using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        [Required]
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [MaxLength(500)]
        public string? Symptoms { get; set; }

        [MaxLength(500)]
        public string? Diagnosis { get; set; }

        [MaxLength(1000)]
        public string? Prescription { get; set; }

        [MaxLength(1000)]
        public string? LabTests { get; set; }

        [MaxLength(1000)]
        public string? TreatmentPlan { get; set; }

        [MaxLength(1000)]
        public string? DoctorNotes { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual User Patient { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
