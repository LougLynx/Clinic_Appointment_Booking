using BussinessObjects.Models;

namespace Repositories.Interfaces
{
    public interface IDoctorRepository : IGenericRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetAllDoctorsWithDetailsAsync();
        Task<Doctor?> GetDoctorByIdWithDetailsAsync(int doctorId);
        Task<IEnumerable<Doctor>> SearchDoctorsAsync(string? searchTerm, int? specialtyId, string? gender, bool? availableToday);
        Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(int specialtyId);
        Task<Doctor?> GetDoctorByUserIdAsync(int userId);
    }
}
