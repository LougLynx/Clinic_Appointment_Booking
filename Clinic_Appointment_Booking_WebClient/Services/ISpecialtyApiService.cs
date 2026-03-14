using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface ISpecialtyApiService
    {
        Task<ApiResponse<List<SpecialtyDTO>>?> GetAllSpecialtiesAsync();
        Task<ApiResponse<SpecialtyDTO>?> GetSpecialtyByIdAsync(int id);
    }
}
