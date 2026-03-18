namespace BussinessObjects.DTOs.admin.patient_records
{
    public class PatientRecordResponseDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastVisitDate { get; set; }
        public string PrimaryPhysician { get; set; } = "N/A";
        public string PhysicianImage { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active, Follow-up, Inactive
    }
}
