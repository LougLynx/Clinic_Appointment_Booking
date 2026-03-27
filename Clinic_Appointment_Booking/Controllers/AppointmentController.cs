using BussinessObjects.DTOs;
using BussinessObjects.Models;
using Clinic_Appointment_Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using System.Security.Claims;

namespace Clinic_Appointment_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            ILogger<AppointmentController> logger)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppointmentDTO>>> Create([FromBody] CreateAppointmentRequestDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int patientId))
                {
                    return Unauthorized(ApiResponse<AppointmentDTO>.ErrorResponse("Invalid user token"));
                }

                if (string.IsNullOrWhiteSpace(request.ReasonForVisit))
                {
                    return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("Reason for visit is required"));
                }

                var doctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
                if (doctor == null)
                {
                    return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("Doctor not found"));
                }


                var slotExists = await _appointmentRepository.ExistsSlotAsync(
                    request.DoctorId, request.AppointmentDate, request.AppointmentTime);
                if (slotExists)
                {
                    return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("This time slot is no longer available"));
                }

                var appointment = new Appointment
                {
                    PatientId = patientId,
                    DoctorId = request.DoctorId,
                    AppointmentDate = request.AppointmentDate,
                    AppointmentTime = request.AppointmentTime,
                    Status = "Confirmed",
                    ReasonForVisit = request.ReasonForVisit,
                    AdditionalNotes = request.AdditionalNotes,
                    IsFirstTime = request.IsFirstTime,
                    ConsultationFee = request.ConsultationFee,
                    EstimatedDurationMinutes = 30
                };

                await _appointmentRepository.AddAsync(appointment);
                await _appointmentRepository.SaveChangesAsync();

                var doctorWithDetails = await _doctorRepository.GetDoctorByIdWithDetailsAsync(request.DoctorId);
                var appointmentDto = new AppointmentDTO
                {
                    AppointmentId = appointment.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    DoctorName = doctorWithDetails?.User.FullName ?? "",
                    SpecialtyName = doctorWithDetails?.Specialty?.Name ?? "",
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    ReasonForVisit = appointment.ReasonForVisit,
                    AdditionalNotes = appointment.AdditionalNotes,
                    IsFirstTime = appointment.IsFirstTime,
                    ConsultationFee = appointment.ConsultationFee,
                    PaymentStatus = appointment.Payment?.PaymentStatus ?? "Pending",
                    CreatedAt = appointment.CreatedAt
                };

                return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(appointmentDto, "Appointment created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse("An error occurred while creating appointment"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDTO>>> GetById(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<AppointmentDTO>.ErrorResponse("Invalid user token"));
                }

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                if (appointment == null)
                {
                    return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse("Appointment not found"));
                }

                // Check if user is either the patient or the doctor of this appointment
                var isPatient = appointment.PatientId == userId;

                // For doctors, we need to check if they are the assigned doctor
                var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
                var isDoctor = doctor != null && appointment.DoctorId == doctor.DoctorId;

                if (!isPatient && !isDoctor)
                {
                    return Forbid();
                }

                var dto = new AppointmentDTO
                {
                    AppointmentId = appointment.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.Doctor?.User?.FullName ?? "",
                    PatientName = appointment.Patient?.FullName ?? "",
                    SpecialtyName = appointment.Doctor?.Specialty?.Name ?? "",
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    ReasonForVisit = appointment.ReasonForVisit,
                    AdditionalNotes = appointment.AdditionalNotes,
                    IsFirstTime = appointment.IsFirstTime,
                    ConsultationFee = appointment.ConsultationFee,
                    PaymentStatus = appointment.Payment?.PaymentStatus ?? "Pending",
                    CreatedAt = appointment.CreatedAt
                };

                return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto, "Appointment retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointment");
                return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse("An error occurred while retrieving appointment"));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDTO>>> Update(int id, [FromBody] UpdateAppointmentRequestDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int patientId))
                {
                    return Unauthorized(ApiResponse<AppointmentDTO>.ErrorResponse("Invalid user token"));
                }

                if (string.IsNullOrWhiteSpace(request.ReasonForVisit))
                {
                    return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("Reason for visit is required"));
                }

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                if (appointment == null || appointment.PatientId != patientId)
                {
                    return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse("Appointment not found"));
                }

                if (appointment.Status == "Cancelled" || appointment.Status == "Completed")
                {
                    return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("This appointment can no longer be updated"));
                }

                var isSameSlot = appointment.AppointmentDate.Date == request.AppointmentDate.Date &&
                                 appointment.AppointmentTime == request.AppointmentTime;
                if (!isSameSlot)
                {
                    var slotExists = await _appointmentRepository.ExistsSlotAsync(
                        appointment.DoctorId, request.AppointmentDate, request.AppointmentTime);
                    if (slotExists)
                    {
                        return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("This time slot is no longer available"));
                    }
                }

                appointment.AppointmentDate = request.AppointmentDate;
                appointment.AppointmentTime = request.AppointmentTime;
                appointment.ReasonForVisit = request.ReasonForVisit;
                appointment.AdditionalNotes = request.AdditionalNotes;
                appointment.IsFirstTime = request.IsFirstTime;
                appointment.UpdatedAt = DateTime.Now;

                await _appointmentRepository.UpdateAsync(appointment);
                await _appointmentRepository.SaveChangesAsync();

                var dto = new AppointmentDTO
                {
                    AppointmentId = appointment.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.Doctor?.User?.FullName ?? "",
                    SpecialtyName = appointment.Doctor?.Specialty?.Name ?? "",
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    ReasonForVisit = appointment.ReasonForVisit,
                    AdditionalNotes = appointment.AdditionalNotes,
                    IsFirstTime = appointment.IsFirstTime,
                    ConsultationFee = appointment.ConsultationFee,
                    PaymentStatus = appointment.Payment?.PaymentStatus ?? "Pending",
                    CreatedAt = appointment.CreatedAt
                };

                return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto, "Appointment updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment");
                return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse("An error occurred while updating appointment"));
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<AppointmentDTO>>> Cancel(int id, [FromBody] CancelAppointmentRequestDTO request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int patientId))
                {
                    return Unauthorized(ApiResponse<AppointmentDTO>.ErrorResponse("Invalid user token"));
                }

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                if (appointment == null || appointment.PatientId != patientId)
                {
                    return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse("Appointment not found"));
                }

                if (appointment.Status == "Cancelled")
                {
                    return BadRequest(ApiResponse<AppointmentDTO>.ErrorResponse("This appointment is already cancelled"));
                }

                appointment.Status = "Cancelled";
                appointment.CancelledAt = DateTime.Now;
                appointment.CancellationReason = request.CancellationReason;
                appointment.UpdatedAt = DateTime.Now;

                await _appointmentRepository.UpdateAsync(appointment);
                await _appointmentRepository.SaveChangesAsync();

                var dto = new AppointmentDTO
                {
                    AppointmentId = appointment.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.Doctor?.User?.FullName ?? "",
                    SpecialtyName = appointment.Doctor?.Specialty?.Name ?? "",
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    ReasonForVisit = appointment.ReasonForVisit,
                    AdditionalNotes = appointment.AdditionalNotes,
                    IsFirstTime = appointment.IsFirstTime,
                    ConsultationFee = appointment.ConsultationFee,
                    PaymentStatus = appointment.Payment?.PaymentStatus ?? "Pending",
                    CreatedAt = appointment.CreatedAt
                };

                return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto, "Appointment cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment");
                return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse("An error occurred while cancelling appointment"));
            }
        }

        [HttpGet("my")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetMyAppointments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int patientId))
                {
                    return Unauthorized(ApiResponse<List<AppointmentDTO>>.ErrorResponse("Invalid user token"));
                }

                var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
                var dtos = appointments.Select(a => new AppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor?.User?.FullName ?? "",
                    SpecialtyName = a.Doctor?.Specialty?.Name ?? "",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    ReasonForVisit = a.ReasonForVisit,
                    AdditionalNotes = a.AdditionalNotes,
                    IsFirstTime = a.IsFirstTime,
                    ConsultationFee = a.ConsultationFee,
                    PaymentStatus = a.Payment?.PaymentStatus ?? "Pending",
                    CreatedAt = a.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<AppointmentDTO>>.SuccessResponse(dtos, "Appointments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments");
                return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse("An error occurred while retrieving appointments"));
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<ApiResponse<List<AppointmentDTO>>>> GetByDoctorId(int doctorId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<List<AppointmentDTO>>.ErrorResponse("Invalid user token"));
                }

                // Verify the doctor is requesting their own appointments
                var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
                if (doctor == null || doctor.DoctorId != doctorId)
                {
                    return Forbid();
                }

                var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
                var dtos = appointments.Select(a => new AppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor?.User?.FullName ?? "",
                    PatientName = a.Patient?.FullName ?? "Unknown",
                    SpecialtyName = a.Doctor?.Specialty?.Name ?? "",
                    AppointmentDate = a.AppointmentDate,
                    AppointmentTime = a.AppointmentTime,
                    Status = a.Status,
                    ReasonForVisit = a.ReasonForVisit,
                    AdditionalNotes = a.AdditionalNotes,
                    IsFirstTime = a.IsFirstTime,
                    ConsultationFee = a.ConsultationFee,
                    PaymentStatus = a.Payment?.PaymentStatus ?? "Pending",
                    CreatedAt = a.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<AppointmentDTO>>.SuccessResponse(dtos, "Doctor appointments retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor appointments");
                return StatusCode(500, ApiResponse<List<AppointmentDTO>>.ErrorResponse("An error occurred while retrieving doctor appointments"));
            }
        }

        [HttpPut("{id}/confirm-payment")]
        public async Task<ActionResult<ApiResponse<AppointmentDTO>>> ConfirmPayment(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<AppointmentDTO>.ErrorResponse("Invalid user token"));
                }

                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id);
                if (appointment == null)
                {
                    return NotFound(ApiResponse<AppointmentDTO>.ErrorResponse("Appointment not found"));
                }

                // Verify the doctor is confirming payment for their own appointment
                var doctor = await _doctorRepository.GetDoctorByUserIdAsync(userId);
                if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                {
                    return Forbid();
                }

                if (appointment.Payment == null)
                {
                    appointment.Payment = new Payment
                    {
                        AppointmentId = appointment.AppointmentId,
                        PatientId = appointment.PatientId,
                        Amount = appointment.ConsultationFee,
                        PaymentMethod = "Cash",
                        PaymentStatus = "Completed",
                        PaymentDate = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };
                }
                else
                {
                    appointment.Payment.PaymentStatus = "Completed";
                    appointment.Payment.PaymentDate = DateTime.Now;
                    appointment.Payment.UpdatedAt = DateTime.Now;
                }

                // If payment is completed, also mark the appointment as Confirmed/Completed or stay same
                // Typically, if payment is confirmed, appointment is confirmed
                if (appointment.Status == "Pending")
                {
                    appointment.Status = "Confirmed";
                }

                await _appointmentRepository.UpdateAsync(appointment);
                await _appointmentRepository.SaveChangesAsync();

                var dto = new AppointmentDTO
                {
                    AppointmentId = appointment.AppointmentId,
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.Doctor?.User?.FullName ?? "",
                    PatientName = appointment.Patient?.FullName ?? "",
                    SpecialtyName = appointment.Doctor?.Specialty?.Name ?? "",
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status,
                    ReasonForVisit = appointment.ReasonForVisit,
                    AdditionalNotes = appointment.AdditionalNotes,
                    IsFirstTime = appointment.IsFirstTime,
                    ConsultationFee = appointment.ConsultationFee,
                    PaymentStatus = appointment.Payment.PaymentStatus,
                    CreatedAt = appointment.CreatedAt
                };

                return Ok(ApiResponse<AppointmentDTO>.SuccessResponse(dto, "Payment confirmed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return StatusCode(500, ApiResponse<AppointmentDTO>.ErrorResponse("An error occurred while confirming payment"));
            }
        }
    }
}
