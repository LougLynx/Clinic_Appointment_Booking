using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IUserApiService
    {
        Task<ApiResponse<UserDTO>?> GetProfileAsync();
        Task<ApiResponse<UserDTO>?> UpdateProfileAsync(UserDTO profile);
        Task<ApiResponse<object>?> ChangePasswordAsync(ChangePasswordRequestDTO request);
    }
}
