using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class UserApiService : IUserApiService
    {
        private readonly IApiClient _apiClient;

        public UserApiService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<UserDTO>?> GetProfileAsync()
        {
            return await _apiClient.GetAsync<UserDTO>("/api/User/profile");
        }

        public async Task<ApiResponse<UserDTO>?> UpdateProfileAsync(UserDTO profile)
        {
            return await _apiClient.PutAsync<UserDTO>("/api/User/profile", profile);
        }

        public async Task<ApiResponse<object>?> ChangePasswordAsync(ChangePasswordRequestDTO request)
        {
            return await _apiClient.PostAsync<object>("/api/User/change-password", request);
        }
    }
}
