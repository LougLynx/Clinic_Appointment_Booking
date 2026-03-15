namespace BussinessObjects.DTOs.admin
{
    public class DoctorOverviewDto
    {
        public string FullName { get; set; }
        public string Specialty { get; set; }

        public string? ProfileImage { get; set; }
        public int TodayAppointments { get; set; }
        public string Status { get; set; } // Active, Away
    }
}
