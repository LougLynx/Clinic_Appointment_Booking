using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Specialty")]
        public int SpecialtyId { get; set; }

        [MaxLength(200)]
        public string? Qualifications { get; set; }

        public int YearsOfExperience { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(100)]
        public string? Languages { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Rating { get; set; } = 0.0m;

        public int ReviewCount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ConsultationFee { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Specialty Specialty { get; set; } = null!;
        public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public virtual ICollection<DoctorEarning> DoctorEarnings { get; set; } = new List<DoctorEarning>();
    }
}
