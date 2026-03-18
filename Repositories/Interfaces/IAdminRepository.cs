using BussinessObjects.DTOs.admin;
using BussinessObjects.DTOs.admin.dashboard;
using BussinessObjects.DTOs.admin.financial;
using BussinessObjects.DTOs.admin.patient_records;

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
        Task<PatientManagementStatsDto> GetPatientManagementStatsAsync();
        Task<PagedPatientResponse> GetPagedPatientsAsync(string? searchTerm, int page, int pageSize, string? sortBy = "Last Visit");
        Task<bool> ToggleUserStatusAsync(int userId);
        Task<FinancialStatsDTO> GetFinancialStatsAsync();
        Task<PagedTransactionResponse> GetTransactionsAsync(int page, int pageSize);
        Task<FinancialAnalyticsDTO> GetFinancialAnalyticsAsync(string period);
        Task<byte[]> ExportFinancialReportAsync();
    }
}
