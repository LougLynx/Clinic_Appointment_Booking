using BussinessObjects.DTOs;
using System.Collections.Generic;

namespace Clinic_Appointment_Booking_WebClient.Models.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public DateTime ReferenceDate { get; set; }
        public List<AppointmentDTO> Appointments { get; set; } = new List<AppointmentDTO>();
        public List<DoctorScheduleDTO> WorkingHours { get; set; } = new List<DoctorScheduleDTO>();
        public string ViewType { get; set; } = "Month"; // Month, Week, Day
        
        // Helper for calendar rendering
        public List<DateTime> DaysInCalendar { get; set; } = new List<DateTime>();
    }
}
