using BussinessObjects.DTOs;
using BussinessObjects.Models;
using Clinic_Appointment_Booking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;

namespace Clinic_Appointment_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            IContactRepository contactRepository,
            ILogger<ContactController> logger)
        {
            _contactRepository = contactRepository;
            _logger = logger;
        }

        /// <summary>
        /// Submit a contact form message. This endpoint is public and does not require authentication.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> Submit([FromBody] CreateContactRequestDTO request)
        {
            try
            {
                var contactMessage = new ContactMessage
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.Trim(),
                    Subject = request.Subject.Trim(),
                    Message = request.Message.Trim(),
                    Status = "New",
                    CreatedAt = DateTime.UtcNow
                };

                await _contactRepository.AddAsync(contactMessage);
                await _contactRepository.SaveChangesAsync();

                _logger.LogInformation("Contact message received from {Email}, MessageId: {MessageId}",
                    contactMessage.Email, contactMessage.MessageId);

                return Ok(ApiResponse<object>.SuccessResponse(null!, "Your message has been sent successfully. We will get back to you soon."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting contact form");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while submitting your message. Please try again later."));
            }
        }
    }
}
