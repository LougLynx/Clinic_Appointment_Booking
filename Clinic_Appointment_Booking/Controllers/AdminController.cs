using BussinessObjects.DTOs.admin;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Clinic_Appointment_Booking_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepo;

        public AdminController(IAdminRepository adminRepo)
        {
            _adminRepo = adminRepo;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminDashboardDto>> GetStats([FromQuery] string period = "last 7 days")
    => Ok(await _adminRepo.GetDashboardStatsAsync(period));

        [HttpGet("trends")]
        public async Task<ActionResult<IEnumerable<ChartDataDTO>>> GetTrends([FromQuery] string period = "last 7 days")
    => Ok(await _adminRepo.GetRevenueTrendsAsync(period));

        [HttpGet("staff-overview")]
        public async Task<ActionResult<IEnumerable<DoctorOverviewDto>>> GetStaffOverview()
            => Ok(await _adminRepo.GetStaffOverviewAsync());

        [HttpGet("recent-activities")]
        public async Task<IActionResult> GetRecentActivities()
            => Ok(await _adminRepo.GetRecentActivitiesAsync());

        [HttpGet("department-performance")]
        public async Task<ActionResult<IEnumerable<DepartmentPerformanceDto>>> GetDeptPerformance([FromQuery] string period = "last 7 days")
    => Ok(await _adminRepo.GetDepartmentPerformanceAsync(period));

        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors(
    [FromQuery] string? specialty = null,
    [FromQuery] string? status = null,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 5)
        {
            var result = await _adminRepo.GetAllDoctorsAsync(specialty, status, page, pageSize);
            return Ok(result);
        }

        [HttpGet("management-stats")]
        public async Task<ActionResult<DoctorManagementStatsDto>> GetManagementStats()
        {
            return Ok(await _adminRepo.GetDoctorManagementStatsAsync());
        }

        [HttpPatch("doctors/{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var success = await _adminRepo.ToggleDoctorStatusAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Doctor not found" });
            }

            return Ok(new { message = "Status updated successfully" });
        }
    }
}
