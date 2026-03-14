using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IAuthApiService
    {
        Task<ApiResponse<RegisterResponseDTO>?> RegisterAsync(RegisterRequestDTO request);
        Task<ApiResponse<LoginResponseDTO>?> LoginAsync(LoginRequestDTO request);
        Task<ApiResponse<object>?> LogoutAsync(string refreshToken);
        Task<ApiResponse<object>?> ForgotPasswordAsync(string email);
        Task<ApiResponse<object>?> ResetPasswordAsync(ResetPasswordRequestDTO request);
        Task<ApiResponse<object>?> VerifyEmailAsync(string token);
    }
}
