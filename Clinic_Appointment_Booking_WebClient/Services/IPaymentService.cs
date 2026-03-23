namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IPaymentService
    {
        string CreatePaymentUrl(decimal amount, string appointmentReference, HttpContext httpContext);
        bool ValidateSignature(IQueryCollection query);
    }
}
