using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class ContactApiService : IContactApiService
    {
        private readonly IApiClient _apiClient;

        public ContactApiService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<object>?> SubmitContactAsync(CreateContactRequestDTO request)
        {
            return await _apiClient.PostAsync<object>("/api/contact", request);
        }
    }
}
