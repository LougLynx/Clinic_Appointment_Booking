using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly IApiClient _apiClient;

        public AuthApiService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<RegisterResponseDTO>?> RegisterAsync(RegisterRequestDTO request)
        {
            return await _apiClient.PostAsync<RegisterResponseDTO>("/api/auth/register", request);
        }

        public async Task<ApiResponse<LoginResponseDTO>?> LoginAsync(LoginRequestDTO request)
        {
            return await _apiClient.PostAsync<LoginResponseDTO>("/api/auth/login", request);
        }

        public async Task<ApiResponse<object>?> LogoutAsync(string refreshToken)
        {
            return await _apiClient.PostAsync<object>("/api/auth/logout", new { refreshToken });
        }

        public async Task<ApiResponse<object>?> ForgotPasswordAsync(string email)
        {
            return await _apiClient.PostAsync<object>("/api/auth/forgot-password", new { email });
        }

        public async Task<ApiResponse<object>?> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            return await _apiClient.PostAsync<object>("/api/auth/reset-password", request);
        }

        public async Task<ApiResponse<object>?> VerifyEmailAsync(string token)
        {
            return await _apiClient.GetAsync<object>($"/api/auth/verify-email?token={token}");
        }
    }
}
