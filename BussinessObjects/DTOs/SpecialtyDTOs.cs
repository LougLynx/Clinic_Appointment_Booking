namespace BussinessObjects.DTOs
{
    public class SpecialtyDTO
    {
        public int SpecialtyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconName { get; set; }
        public int DoctorCount { get; set; }
    }
}
