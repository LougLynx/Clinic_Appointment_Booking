using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BussinessObjects.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "Info"; // Info, Warning, Error, Success, Urgent

        public bool IsRead { get; set; } = false;

        [MaxLength(200)]
        public string? ActionUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
