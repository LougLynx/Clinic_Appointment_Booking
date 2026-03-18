namespace BussinessObjects.DTOs.admin.dashboard
{
    public class DoctorManagementStatsDto
    {
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int OnLeaveDoctors { get; set; }
        public int TotalSpecialties { get; set; }
    }
}
