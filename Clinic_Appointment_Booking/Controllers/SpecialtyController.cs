using BussinessObjects.DTOs;
using Clinic_Appointment_Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Clinic_Appointment_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyRepository _specialtyRepository;
        private readonly ILogger<SpecialtyController> _logger;

        public SpecialtyController(
            ISpecialtyRepository specialtyRepository,
            ILogger<SpecialtyController> logger)
        {
            _specialtyRepository = specialtyRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SpecialtyDTO>>>> GetAllSpecialties()
        {
            try
            {
                var specialties = await _specialtyRepository.GetActiveSpecialtiesAsync();
                var specialtyDTOs = specialties.Select(s => new SpecialtyDTO
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description,
                    IconName = s.IconName,
                    DoctorCount = s.Doctors.Count(d => d.IsAvailable)
                }).ToList();

                return Ok(ApiResponse<List<SpecialtyDTO>>.SuccessResponse(specialtyDTOs, "Specialties retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specialties");
                return StatusCode(500, ApiResponse<List<SpecialtyDTO>>.ErrorResponse("An error occurred while retrieving specialties"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SpecialtyDTO>>> GetSpecialtyById(int id)
        {
            try
            {
                var specialty = await _specialtyRepository.GetByIdAsync(id);

                if (specialty == null)
                {
                    return NotFound(ApiResponse<SpecialtyDTO>.ErrorResponse("Specialty not found"));
                }

                var specialtyDTO = new SpecialtyDTO
                {
                    SpecialtyId = specialty.SpecialtyId,
                    Name = specialty.Name,
                    Description = specialty.Description,
                    IconName = specialty.IconName,
                    DoctorCount = specialty.Doctors.Count(d => d.IsAvailable)
                };

                return Ok(ApiResponse<SpecialtyDTO>.SuccessResponse(specialtyDTO, "Specialty retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specialty with ID {SpecialtyId}", id);
                return StatusCode(500, ApiResponse<SpecialtyDTO>.ErrorResponse("An error occurred while retrieving specialty"));
            }
        }
    }
}
