using BussinessObjects.DTOs.admin;

namespace Repositories.Interfaces
{
    public interface IAdminRepository
    {
        Task<AdminDashboardDto> GetDashboardStatsAsync(string period);
        Task<IEnumerable<ChartDataDTO>> GetRevenueTrendsAsync(string period);
        Task<IEnumerable<DoctorOverviewDto>> GetStaffOverviewAsync();
        Task<IEnumerable<object>> GetRecentActivitiesAsync();
        Task<IEnumerable<DepartmentPerformanceDto>> GetDepartmentPerformanceAsync(string period);

        Task<PagedMedicalStaffResponse> GetAllDoctorsAsync(string? specialty = null, string? status = null, int page = 1, int pageSize = 5);

        Task<DoctorManagementStatsDto> GetDoctorManagementStatsAsync();
        Task<bool> ToggleDoctorStatusAsync(int doctorId);


    }
}
