namespace BussinessObjects.DTOs.admin
{
    public class MedicalStaffResponseDTO
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public int TodayAppointments { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
