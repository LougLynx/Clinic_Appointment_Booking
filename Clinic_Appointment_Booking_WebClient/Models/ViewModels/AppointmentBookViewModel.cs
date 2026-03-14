using System.ComponentModel.DataAnnotations;
using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Models.ViewModels
{
    public class AppointmentBookViewModel
    {
        public int? AppointmentId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        public DoctorDetailDTO Doctor { get; set; } = new();

        public List<AppointmentDayViewModel> Days { get; set; } = new();

        [Required(ErrorMessage = "Please select a date.")]
        public string? SelectedDate { get; set; }

        [Required(ErrorMessage = "Please select a time.")]
        public string? SelectedTime { get; set; }

        [Required(ErrorMessage = "Please select a reason for your visit.")]
        public string? Reason { get; set; }

        public string? Notes { get; set; }

        public bool IsFirstTime { get; set; }
    }

    public class AppointmentDayViewModel
    {
        public DateTime Date { get; set; }
        public List<AppointmentTimeSlotViewModel> Slots { get; set; } = new();
    }

    public class AppointmentTimeSlotViewModel
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
