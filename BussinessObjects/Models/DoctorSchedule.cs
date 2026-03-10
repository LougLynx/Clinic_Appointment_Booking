using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class DoctorSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public int SlotDurationMinutes { get; set; } = 30;

        public bool IsAvailable { get; set; } = true;

        public DateTime? SpecificDate { get; set; } // For specific date overrides

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
