namespace BussinessObjects.DTOs.admin.patient_records
{
    public class PatientManagementStatsDto
    {
        public int TotalPatients { get; set; }
        public int NewThisMonth { get; set; }
        public int PendingFollowUp { get; set; }
        public int ActiveThisWeek { get; set; }
    }
}
