using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class DoctorApiService : IDoctorApiService
    {
        private readonly IApiClient _apiClient;

        public DoctorApiService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<DoctorDTO>>?> GetAllDoctorsAsync()
        {
            return await _apiClient.GetAsync<List<DoctorDTO>>("/api/doctor");
        }

        public async Task<ApiResponse<DoctorDetailDTO>?> GetDoctorByIdAsync(int id)
        {
            return await _apiClient.GetAsync<DoctorDetailDTO>($"/api/doctor/{id}");
        }

        public async Task<ApiResponse<DoctorSearchResponseDTO>?> SearchDoctorsAsync(
            string? searchTerm = null,
            int? specialtyId = null,
            string? gender = null,
            bool? availableToday = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(searchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");

            if (specialtyId.HasValue)
                queryParams.Add($"specialtyId={specialtyId.Value}");

            if (!string.IsNullOrEmpty(gender))
                queryParams.Add($"gender={Uri.EscapeDataString(gender)}");

            if (availableToday.HasValue)
                queryParams.Add($"availableToday={availableToday.Value}");

            queryParams.Add($"pageNumber={pageNumber}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = string.Join("&", queryParams);
            return await _apiClient.GetAsync<DoctorSearchResponseDTO>($"/api/doctor/search?{queryString}");
        }

        public async Task<ApiResponse<List<DoctorDTO>>?> GetDoctorsBySpecialtyAsync(int specialtyId)
        {
            return await _apiClient.GetAsync<List<DoctorDTO>>($"/api/doctor/specialty/{specialtyId}");
        }

        public async Task<ApiResponse<DoctorDTO>?> GetDoctorByUserIdAsync(int userId)
        {
            return await _apiClient.GetAsync<DoctorDTO>($"/api/doctor/user/{userId}");
        }
    }
}
