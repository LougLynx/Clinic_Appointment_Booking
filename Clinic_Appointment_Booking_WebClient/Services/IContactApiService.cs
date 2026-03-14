using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IContactApiService
    {
        Task<ApiResponse<object>?> SubmitContactAsync(CreateContactRequestDTO request);
    }
}
