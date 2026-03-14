namespace Clinic_Appointment_Booking.Services.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateRandomToken();
    }
}
