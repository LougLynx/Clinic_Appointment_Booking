using BussinessObjects.DTOs.admin.doctor_management;
using Microsoft.AspNetCore.Http;

namespace Clinic_Appointment_Booking.Models
{
    public class CreateDoctorRequest : CreateDoctorDto
    {
        public IFormFile? ProfileImage { get; set; }
    }
}
