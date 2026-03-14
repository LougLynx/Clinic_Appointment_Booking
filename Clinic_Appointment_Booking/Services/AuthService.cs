using BussinessObjects.DTOs;
using BussinessObjects.Models;
using Clinic_Appointment_Booking.Services.Interfaces;
using Repositories.Interfaces;

namespace Clinic_Appointment_Booking.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IPasswordService passwordService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new RegisterResponseDTO
                {
                    Success = false,
                    Message = "Email already registered",
                    EmailVerificationRequired = false
                };
            }

            // Create new user
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Role = "Patient",
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Address = request.Address,
                IsActive = true,
                EmailVerified = false,
                EmailVerificationToken = _passwordService.GenerateRandomToken(),
                EmailVerificationExpiry = DateTime.Now.AddHours(24),
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Send verification email
            try
            {
                await _emailService.SendEmailVerificationAsync(user, user.EmailVerificationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send verification email: {ex.Message}");
            }

            return new RegisterResponseDTO
            {
                Success = true,
                Message = "Registration successful. Please check your email to verify your account.",
                EmailVerificationRequired = true
            };
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is inactive");
            }

            if (!user.EmailVerified)
            {
                throw new UnauthorizedAccessException("Please verify your email before logging in");
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshTokenExpiry = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = DateTime.Now.AddDays(refreshTokenExpiry),
                CreatedAt = DateTime.Now
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            // Update last login
            user.LastLoginAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.Now.AddMinutes(int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "60")),
                User = new UserDTO
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    EmailVerified = user.EmailVerified,
                    LastLoginAt = user.LastLoginAt
                }
            };
        }

        public async Task<TokenResponseDTO> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (tokenEntity == null || !tokenEntity.IsActive)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var user = tokenEntity.User;

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old refresh token
            await _refreshTokenRepository.RevokeTokenAsync(refreshToken);

            // Save new refresh token
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshTokenExpiry = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.Now.AddDays(refreshTokenExpiry),
                CreatedAt = DateTime.Now
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.Now.AddMinutes(int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "60"))
            };
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var user = await _userRepository.GetByEmailVerificationTokenAsync(token);

            if (user == null)
            {
                return false;
            }

            await _userRepository.VerifyEmailAsync(user.UserId);
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                // Don't reveal if user exists
                return true;
            }

            // Generate password reset token
            user.PasswordResetToken = _passwordService.GenerateRandomToken();
            user.PasswordResetExpiry = DateTime.Now.AddHours(1);
            user.UpdatedAt = DateTime.Now;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Send password reset email
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user, user.PasswordResetToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send password reset email: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token);

            if (user == null)
            {
                _logger.LogWarning("Invalid or expired password reset token");
                return false;
            }

            // Check if new password is same as old password
            if (_passwordService.VerifyPassword(request.NewPassword, user.PasswordHash))
            {
                _logger.LogWarning($"User {user.Email} tried to reset password with the same password");
                throw new InvalidOperationException("New password cannot be the same as the old password");
            }

            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(user.UserId, newPasswordHash);

            _logger.LogInformation($"Password reset successful for user {user.Email}");
            return true;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            await _refreshTokenRepository.RevokeTokenAsync(refreshToken);
        }
    }
}
