namespace BussinessObjects.DTOs
{
    public class CreateAppointmentRequestDTO
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string ReasonForVisit { get; set; } = string.Empty;
        public string? AdditionalNotes { get; set; }
        public bool IsFirstTime { get; set; }
        public decimal ConsultationFee { get; set; }
    }

    public class UpdateAppointmentRequestDTO
    {
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string ReasonForVisit { get; set; } = string.Empty;
        public string? AdditionalNotes { get; set; }
        public bool IsFirstTime { get; set; }
    }

    public class CancelAppointmentRequestDTO
    {
        public string? CancellationReason { get; set; }
    }

    public class AppointmentDTO
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReasonForVisit { get; set; } = string.Empty;
        public string? AdditionalNotes { get; set; }
        public bool IsFirstTime { get; set; }
        public decimal ConsultationFee { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
    }
}
