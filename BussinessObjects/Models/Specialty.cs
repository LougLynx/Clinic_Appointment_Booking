using System.ComponentModel.DataAnnotations;

namespace BussinessObjects.Models
{
    public class Specialty
    {
        [Key]
        public int SpecialtyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? IconName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
