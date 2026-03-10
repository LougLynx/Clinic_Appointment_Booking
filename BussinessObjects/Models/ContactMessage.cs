using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class ContactMessage
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "New"; // New, Read, Replied, Closed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ReadAt { get; set; }

        public DateTime? RepliedAt { get; set; }
    }
}
