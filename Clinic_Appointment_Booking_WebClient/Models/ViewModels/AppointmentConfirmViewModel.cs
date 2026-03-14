using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Models.ViewModels
{
    public class AppointmentConfirmViewModel
    {
        public DoctorDetailDTO Doctor { get; set; } = new();
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsFirstTime { get; set; }
        public string? PatientName { get; set; }
        public string? PatientEmail { get; set; }
        public string? PatientPhone { get; set; }
        public string? QRCodeUrl { get; set; }
        public string? PaymentReference { get; set; }
    }
}
