using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<LoginResponseDTO> GoogleLoginAsync(GoogleLoginRequestDTO request);
        Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO request);
        Task LogoutAsync(string refreshToken);
    }
}
