namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IPaymentService
    {
        string GenerateQRCodeUrl(decimal amount, string appointmentReference, string accountName = "MediClinic");
        Task<bool> VerifyPaymentAsync(string transactionReference);
    }
}
