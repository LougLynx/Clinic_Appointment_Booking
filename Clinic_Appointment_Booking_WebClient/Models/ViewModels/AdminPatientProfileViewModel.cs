namespace Clinic_Appointment_Booking_WebClient.Models.ViewModels
{
    public class AdminPatientProfileViewModel
    {
        public int PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty; // ex: MED-98721
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string PrimaryPhysicianName { get; set; } = string.Empty;
        public DateTime? LastVisitAt { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }

        public List<PatientVitalViewModel> RecentVitals { get; set; } = new();
        public List<PatientTimelineEntryViewModel> Timeline { get; set; } = new();
        public List<PatientLabResultViewModel> LabResults { get; set; } = new();
    }

    public class PatientVitalViewModel
    {
        public string Title { get; set; } = string.Empty; // Heart Rate
        public string Value { get; set; } = string.Empty; // 72 bpm
        public string Icon { get; set; } = string.Empty;  // material symbol name
        public string ColorClass { get; set; } = string.Empty; // bg-red-100 text-red-600
    }

    public class PatientTimelineEntryViewModel
    {
        public DateTime Date { get; set; }
        public string TypeLabel { get; set; } = string.Empty; // Routine Checkup/Urgent
        public string TypeBadgeClass { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string PhysicianName { get; set; } = string.Empty;
        public string? PhysicianAvatarUrl { get; set; }
        public string Icon { get; set; } = "stethoscope";
    }

    public class PatientLabResultViewModel
    {
        public string TestName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Result { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
    }
}

