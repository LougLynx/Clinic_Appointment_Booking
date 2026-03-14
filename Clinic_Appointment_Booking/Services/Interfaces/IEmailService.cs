using BussinessObjects.Models;

namespace Clinic_Appointment_Booking.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailVerificationAsync(User user, string token);
        Task SendPasswordResetEmailAsync(User user, string token);
        Task SendEmailAsync(string to, string subject, string body);
    }
}
