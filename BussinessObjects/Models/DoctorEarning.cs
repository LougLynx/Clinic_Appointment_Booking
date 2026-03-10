using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class DoctorEarning
    {
        [Key]
        public int EarningId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionRate { get; set; } = 70.0m; // Percentage doctor receives

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DoctorAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ClinicAmount { get; set; }

        public DateTime EarningDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Cancelled

        public DateTime? PaidDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Appointment Appointment { get; set; } = null!;
    }
}
