using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [Required]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, Insurance, Online

        [Required]
        [MaxLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded

        [MaxLength(200)]
        public string? TransactionId { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(500)]
        public string? PaymentNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual User Patient { get; set; } = null!;
    }
}
