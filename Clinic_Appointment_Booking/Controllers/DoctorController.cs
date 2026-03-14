using BussinessObjects.DTOs;
using BussinessObjects.Models;
using Clinic_Appointment_Booking.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Clinic_Appointment_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(
            IDoctorRepository doctorRepository,
            ILogger<DoctorController> logger)
        {
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<DoctorDTO>>>> GetAllDoctors()
        {
            try
            {
                var doctors = await _doctorRepository.GetAllDoctorsWithDetailsAsync();
                var doctorDTOs = doctors.Select(MapToDoctorDTO).ToList();

                return Ok(ApiResponse<List<DoctorDTO>>.SuccessResponse(doctorDTOs, "Doctors retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors");
                return StatusCode(500, ApiResponse<List<DoctorDTO>>.ErrorResponse("An error occurred while retrieving doctors"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DoctorDetailDTO>>> GetDoctorById(int id)
        {
            try
            {
                var doctor = await _doctorRepository.GetDoctorByIdWithDetailsAsync(id);

                if (doctor == null)
                {
                    return NotFound(ApiResponse<DoctorDetailDTO>.ErrorResponse("Doctor not found"));
                }

                var doctorDetailDTO = MapToDoctorDetailDTO(doctor);

                return Ok(ApiResponse<DoctorDetailDTO>.SuccessResponse(doctorDetailDTO, "Doctor retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor with ID {DoctorId}", id);
                return StatusCode(500, ApiResponse<DoctorDetailDTO>.ErrorResponse("An error occurred while retrieving doctor details"));
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<DoctorSearchResponseDTO>>> SearchDoctors(
            [FromQuery] string? searchTerm,
            [FromQuery] int? specialtyId,
            [FromQuery] string? gender,
            [FromQuery] bool? availableToday,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var doctors = await _doctorRepository.SearchDoctorsAsync(searchTerm, specialtyId, gender, availableToday);
                var totalCount = doctors.Count();

                var paginatedDoctors = doctors
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToDoctorDTO)
                    .ToList();

                var response = new DoctorSearchResponseDTO
                {
                    Doctors = paginatedDoctors,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(ApiResponse<DoctorSearchResponseDTO>.SuccessResponse(response, "Search completed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching doctors");
                return StatusCode(500, ApiResponse<DoctorSearchResponseDTO>.ErrorResponse("An error occurred while searching doctors"));
            }
        }

        [HttpGet("specialty/{specialtyId}")]
        public async Task<ActionResult<ApiResponse<List<DoctorDTO>>>> GetDoctorsBySpecialty(int specialtyId)
        {
            try
            {
                var doctors = await _doctorRepository.GetDoctorsBySpecialtyAsync(specialtyId);
                var doctorDTOs = doctors.Select(MapToDoctorDTO).ToList();

                return Ok(ApiResponse<List<DoctorDTO>>.SuccessResponse(doctorDTOs, "Doctors retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors for specialty {SpecialtyId}", specialtyId);
                return StatusCode(500, ApiResponse<List<DoctorDTO>>.ErrorResponse("An error occurred while retrieving doctors"));
            }
        }

        private static DoctorDTO MapToDoctorDTO(Doctor doctor)
        {
            return new DoctorDTO
            {
                DoctorId = doctor.DoctorId,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email,
                PhoneNumber = doctor.User.PhoneNumber,
                Gender = doctor.User.Gender,
                SpecialtyId = doctor.SpecialtyId,
                SpecialtyName = doctor.Specialty.Name,
                Qualifications = doctor.Qualifications,
                YearsOfExperience = doctor.YearsOfExperience,
                Location = doctor.Location,
                Languages = doctor.Languages,
                Rating = doctor.Rating,
                ReviewCount = doctor.ReviewCount,
                ConsultationFee = doctor.ConsultationFee,
                Bio = doctor.Bio,
                ProfilePictureUrl = doctor.ProfileImageUrl,
                IsAvailable = doctor.IsAvailable
            };
        }

        private static DoctorDetailDTO MapToDoctorDetailDTO(Doctor doctor)
        {
            var languages = !string.IsNullOrEmpty(doctor.Languages) 
                ? doctor.Languages.Split(',').Select(l => l.Trim()).ToList() 
                : new List<string>();

            var specializations = !string.IsNullOrEmpty(doctor.Qualifications) 
                ? doctor.Qualifications.Split(',').Select(q => q.Trim()).ToList() 
                : new List<string>();

            return new DoctorDetailDTO
            {
                DoctorId = doctor.DoctorId,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email,
                PhoneNumber = doctor.User.PhoneNumber,
                Gender = doctor.User.Gender,
                SpecialtyId = doctor.SpecialtyId,
                SpecialtyName = doctor.Specialty.Name,
                Qualifications = doctor.Qualifications,
                YearsOfExperience = doctor.YearsOfExperience,
                Location = doctor.Location,
                Languages = languages,
                Rating = doctor.Rating,
                ReviewCount = doctor.ReviewCount,
                ConsultationFee = doctor.ConsultationFee,
                Bio = doctor.Bio,
                ProfilePictureUrl = doctor.ProfileImageUrl,
                IsAvailable = doctor.IsAvailable,
                Education = doctor.Qualifications,
                Specializations = specializations,
                Schedules = doctor.DoctorSchedules.Select(s => new DoctorScheduleDTO
                {
                    ScheduleId = s.ScheduleId,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SlotDurationMinutes = s.SlotDurationMinutes,
                    IsAvailable = s.IsAvailable,
                    SpecificDate = s.SpecificDate
                }).ToList(),
                AvailableTimeSlots = new List<TimeSlotDTO>()
            };
        }
    }
}
