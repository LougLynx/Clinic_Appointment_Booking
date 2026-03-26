using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class DoctorApiService : IDoctorApiService
    {
        private readonly IApiClient _apiClient;
        private readonly string _apiBaseUrl;

        public DoctorApiService(IApiClient apiClient, IConfiguration configuration)
        {
            _apiClient = apiClient;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
        }

        public async Task<ApiResponse<List<DoctorDTO>>?> GetAllDoctorsAsync()
        {
            var response = await _apiClient.GetAsync<List<DoctorDTO>>("/api/doctor");
            FixDoctorUrls(response?.Data);
            return response;
        }

        public async Task<ApiResponse<DoctorDetailDTO>?> GetDoctorByIdAsync(int id)
        {
            var response = await _apiClient.GetAsync<DoctorDetailDTO>($"/api/doctor/{id}");
            FixDoctorUrls(response?.Data);
            return response;
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
            var response = await _apiClient.GetAsync<DoctorSearchResponseDTO>($"/api/doctor/search?{queryString}");
            FixDoctorUrls(response?.Data?.Doctors);
            return response;
        }

        public async Task<ApiResponse<List<DoctorDTO>>?> GetDoctorsBySpecialtyAsync(int specialtyId)
        {
            var response = await _apiClient.GetAsync<List<DoctorDTO>>($"/api/doctor/specialty/{specialtyId}");
            FixDoctorUrls(response?.Data);
            return response;
        }

        public async Task<ApiResponse<DoctorDTO>?> GetDoctorByUserIdAsync(int userId)
        {
            var response = await _apiClient.GetAsync<DoctorDTO>($"/api/doctor/user/{userId}");
            FixDoctorUrls(response?.Data);
            return response;
        }

        private void FixDoctorUrls(IEnumerable<DoctorDTO>? doctors)
        {
            if (doctors == null) return;
            foreach (var doc in doctors) FixDoctorUrls(doc);
        }

        private void FixDoctorUrls(DoctorDTO? doc)
        {
            if (doc == null || string.IsNullOrEmpty(doc.ProfilePictureUrl)) return;
            if (!doc.ProfilePictureUrl.StartsWith("http") && !string.IsNullOrEmpty(_apiBaseUrl))
            {
                doc.ProfilePictureUrl = _apiBaseUrl + (doc.ProfilePictureUrl.StartsWith("/") ? "" : "/") + doc.ProfilePictureUrl;
            }
        }
    }
}
