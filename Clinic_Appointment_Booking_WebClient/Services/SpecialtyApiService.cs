using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class SpecialtyApiService : ISpecialtyApiService
    {
        private readonly IApiClient _apiClient;

        public SpecialtyApiService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<SpecialtyDTO>>?> GetAllSpecialtiesAsync()
        {
            return await _apiClient.GetAsync<List<SpecialtyDTO>>("/api/specialty");
        }

        public async Task<ApiResponse<SpecialtyDTO>?> GetSpecialtyByIdAsync(int id)
        {
            return await _apiClient.GetAsync<SpecialtyDTO>($"/api/specialty/{id}");
        }
    }
}
