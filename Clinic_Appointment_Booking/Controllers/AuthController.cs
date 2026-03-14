using BussinessObjects.DTOs;
using Clinic_Appointment_Booking.Models;
using Clinic_Appointment_Booking.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDTO>>> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<RegisterResponseDTO>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.RegisterAsync(request);

                if (!result.Success)
                {
                    return BadRequest(ApiResponse<RegisterResponseDTO>.ErrorResponse(result.Message));
                }

                return Ok(ApiResponse<RegisterResponseDTO>.SuccessResponse(result, result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return StatusCode(500, ApiResponse<RegisterResponseDTO>.ErrorResponse("An error occurred during registration"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoginResponseDTO>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.LoginAsync(request);
                return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(result, "Login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<LoginResponseDTO>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, ApiResponse<LoginResponseDTO>.ErrorResponse("An error occurred during login"));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<TokenResponseDTO>.ErrorResponse("Refresh token is required"));
                }

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);
                return Ok(ApiResponse<TokenResponseDTO>.SuccessResponse(result, "Token refreshed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<TokenResponseDTO>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Refresh token error: {ex.Message}");
                return StatusCode(500, ApiResponse<TokenResponseDTO>.ErrorResponse("An error occurred while refreshing token"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] RefreshTokenRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Refresh token is required"));
                }

                await _authService.LogoutAsync(request.RefreshToken);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Logged out successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during logout"));
            }
        }

        [HttpGet("verify-email")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyEmail([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Token is required"));
                }

                var result = await _authService.VerifyEmailAsync(token);

                if (!result)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid or expired verification token"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Email verified successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email verification error: {ex.Message}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during email verification"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
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

                await _authService.ForgotPasswordAsync(request.Email);
                return Ok(ApiResponse<object>.SuccessResponse(null, "If the email exists, a password reset link has been sent"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password error: {ex.Message}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing your request"));
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequestDTO request)
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

                var result = await _authService.ResetPasswordAsync(request);

                if (!result)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid or expired reset token"));
                }

                return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Reset password validation error: {ex.Message}");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while resetting password"));
            }
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<LoginResponseDTO>.ErrorResponse("Validation failed", errors));
                }

                var result = await _authService.GoogleLoginAsync(request);
                return Ok(ApiResponse<LoginResponseDTO>.SuccessResponse(result, "Google login successful"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<LoginResponseDTO>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Google login error: {ex.Message}");
                return StatusCode(500, ApiResponse<LoginResponseDTO>.ErrorResponse("An error occurred during Google login"));
            }
        }
    }
}
