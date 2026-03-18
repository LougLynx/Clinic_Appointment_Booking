namespace BussinessObjects.DTOs.admin
{
    public class DoctorManagementStatsDto
    {
        public int TotalDoctors { get; set; }
        public int ActiveDoctors { get; set; }
        public int OnLeaveDoctors { get; set; }
        public int TotalSpecialties { get; set; }
    }
}
