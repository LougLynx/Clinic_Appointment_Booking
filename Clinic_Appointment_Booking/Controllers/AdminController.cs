using BussinessObjects.DTOs.admin.dashboard;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using System.IO;

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

        [HttpGet("patients-stats")]
        public async Task<IActionResult> GetPatientStats() => Ok(await _adminRepo.GetPatientManagementStatsAsync());

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients(
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 5,
    [FromQuery] string? sortBy = "Last Visit (Newest)")
=> Ok(await _adminRepo.GetPagedPatientsAsync(search, page, pageSize, sortBy));

        [HttpPatch("users/{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var success = await _adminRepo.ToggleUserStatusAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return Ok();
        }
        [HttpGet("financial-stats")]
        public async Task<IActionResult> GetFinancialStats() => Ok(await _adminRepo.GetFinancialStatsAsync());

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
            => Ok(await _adminRepo.GetTransactionsAsync(page, pageSize));

        [HttpGet("financial-analytics")]
        public async Task<IActionResult> GetFinancialAnalytics([FromQuery] string period = "last6months")
     => Ok(await _adminRepo.GetFinancialAnalyticsAsync(period));

        [HttpGet("export-financial-report")]
        public async Task<IActionResult> ExportFinancialReport()
        {
            try
            {
                byte[] content = await _adminRepo.ExportFinancialReportAsync();

                string fileName = $"Financial_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                // Log error here
                return StatusCode(500, "Internal server error during export");
            }
        }
        [HttpPost("doctors")]
        public async Task<IActionResult> CreateDoctor([FromForm] Clinic_Appointment_Booking.Models.CreateDoctorRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.ProfileImage != null)
            {
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "doctors");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ProfileImage.CopyToAsync(fileStream);
                }

                request.ProfileImageUrl = $"/uploads/doctors/{uniqueFileName}";
            }

            try
            {
                var success = await _adminRepo.CreateDoctorAsync(request);
                if (!success)
                {
                    return StatusCode(500, new { message = "An error occurred while creating the doctor." });
                }

                return Ok(new { message = "Doctor created successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An unexpected error occurred." });
            }
        }
    }
}
