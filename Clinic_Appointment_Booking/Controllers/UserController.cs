using BussinessObjects.DTOs;
using Clinic_Appointment_Booking.Models;
using Clinic_Appointment_Booking.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Interfaces;
using System.Security.Claims;

namespace Clinic_Appointment_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            IPasswordService passwordService,
            ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<UserDTO>.ErrorResponse("Invalid user token"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDTO>.ErrorResponse("User not found"));
                }

                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    LastLoginAt = user.LastLoginAt
                };

                return Ok(ApiResponse<UserDTO>.SuccessResponse(userDto, "Profile retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get profile error: {ex.Message}");
                return StatusCode(500, ApiResponse<UserDTO>.ErrorResponse("An error occurred while retrieving profile"));
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> UpdateProfile([FromBody] UserDTO updateRequest)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<UserDTO>.ErrorResponse("Invalid user token"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserDTO>.ErrorResponse("User not found"));
                }

                // Update allowed fields
                user.FullName = updateRequest.FullName;
                user.PhoneNumber = updateRequest.PhoneNumber;
                user.UpdatedAt = DateTime.Now;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    LastLoginAt = user.LastLoginAt
                };

                return Ok(ApiResponse<UserDTO>.SuccessResponse(userDto, "Profile updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update profile error: {ex.Message}");
                return StatusCode(500, ApiResponse<UserDTO>.ErrorResponse("An error occurred while updating profile"));
            }
        }

        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user token"));
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
                }

                // Verify old password
                if (!_passwordService.VerifyPassword(request.OldPassword, user.PasswordHash))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Current password is incorrect"));
                }

                // Update password
                var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
                await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);

                return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Change password error: {ex.Message}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while changing password"));
            }
        }
    }
}
