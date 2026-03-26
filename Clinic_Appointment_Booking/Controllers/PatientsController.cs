using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Clinic_Appointment_Booking_WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsApiController : ControllerBase
    {
        private readonly IPatientRepository _patientRepo;
        public PatientsApiController(IPatientRepository patientRepo) => _patientRepo = patientRepo;

        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetPatients(
    int doctorId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 6,
    [FromQuery] string search = null,
    [FromQuery] string status = null)
        {
            var result = await _patientRepo.GetPatientsByDoctorIdAsync(doctorId, page, pageSize, search, status);

            return Ok(new
            {
                TotalItems = result.TotalCount,
                Page = page,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                Items = result.Data
            });
        }
    }
}
