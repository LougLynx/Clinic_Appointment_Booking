using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public interface IDoctorApiService
    {
        Task<ApiResponse<List<DoctorDTO>>?> GetAllDoctorsAsync();
        Task<ApiResponse<DoctorDetailDTO>?> GetDoctorByIdAsync(int id);
        Task<ApiResponse<DoctorSearchResponseDTO>?> SearchDoctorsAsync(
            string? searchTerm = null,
            int? specialtyId = null,
            string? gender = null,
            bool? availableToday = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<ApiResponse<List<DoctorDTO>>?> GetDoctorsBySpecialtyAsync(int specialtyId);
        Task<ApiResponse<DoctorDTO>?> GetDoctorByUserIdAsync(int userId);
    }
}
