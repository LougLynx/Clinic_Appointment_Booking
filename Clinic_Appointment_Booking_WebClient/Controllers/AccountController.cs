using BussinessObjects.DTOs;
using Clinic_Appointment_Booking_WebClient.Models.ViewModels;
using Clinic_Appointment_Booking_WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthApiService _authApiService;
        private readonly IUserApiService _userApiService;
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(
            IAuthApiService authApiService,
            IUserApiService userApiService,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _authApiService = authApiService;
            _userApiService = userApiService;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in, redirect to returnUrl or home
            if (HttpContext.Session.GetString("AccessToken") != null)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            
            // Clear any success messages when accessing login page directly
            TempData.Remove("SuccessMessage");
            
            // Pass Google Client ID and returnUrl to view
            ViewBag.GoogleClientId = _configuration["GoogleAuth:ClientId"];
            ViewBag.ReturnUrl = returnUrl;
            
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            try
            {
                var loginRequest = new LoginRequestDTO
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var response = await _authApiService.LoginAsync(loginRequest);

                if (response != null && response.Success && response.Data != null)
                {
                    // Store tokens and user info in session
                    HttpContext.Session.SetString("AccessToken", response.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", response.Data.RefreshToken);
                    HttpContext.Session.SetString("UserEmail", response.Data.User.Email);
                    HttpContext.Session.SetString("UserRole", response.Data.User.Role);
                    HttpContext.Session.SetString("UserName", response.Data.User.FullName);
                    HttpContext.Session.SetString("UserPhone", response.Data.User.PhoneNumber ?? string.Empty);

                    TempData["SuccessMessage"] = "Login successful!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Login failed");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during login");
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            // Clear any success messages when accessing register page directly
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registerRequest = new RegisterRequestDTO
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword,
                    PhoneNumber = model.PhoneNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Address = model.Address
                };

                var response = await _authApiService.RegisterAsync(registerRequest);

                if (response != null && response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                    return RedirectToAction("EmailVerificationSent");
                }
                else
                {
                    if (response?.Errors != null && response.Errors.Any())
                    {
                        foreach (var error in response.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", response?.Message ?? "Registration failed");
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during registration");
                return View(model);
            }
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = HttpContext.Session.GetString("RefreshToken");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _authApiService.LogoutAsync(refreshToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
            }

            // Clear session
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully";
            return RedirectToAction("Login");
        }

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            // Clear any success messages when accessing forgot password page directly
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _authApiService.ForgotPasswordAsync(model.Email);

                if (response != null)
                {
                    TempData["SuccessMessage"] = response.Message;
                    return RedirectToAction("ForgotPasswordConfirmation");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Forgot password error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while processing your request");
                return View(model);
            }
        }

        // GET: /Account/ResetPassword
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            // Clear any success messages when accessing reset password page directly
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var resetRequest = new ResetPasswordRequestDTO
                {
                    Token = model.Token,
                    NewPassword = model.NewPassword,
                    ConfirmPassword = model.ConfirmPassword
                };

                var response = await _authApiService.ResetPasswordAsync(resetRequest);

                if (response != null && response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                    return RedirectToAction("PasswordResetSuccess");
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Password reset failed");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while resetting password");
                return View(model);
            }
        }

        // GET: /Account/VerifyEmail
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            try
            {
                var response = await _authApiService.VerifyEmailAsync(token);

                if (response != null && response.Success)
                {
                    TempData["SuccessMessage"] = "Email verified successfully! You can now log in.";
                    return View("EmailVerified");
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Email verification failed";
                    return View("EmailVerificationFailed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email verification error: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred during email verification";
                return View("EmailVerificationFailed");
            }
        }

        // GET: /Account/EmailVerificationSent
        public IActionResult EmailVerificationSent()
        {
            return View();
        }

        // GET: /Account/ForgotPasswordConfirmation
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/PasswordResetSuccess
        public IActionResult PasswordResetSuccess()
        {
            return View();
        }

        // GET: /Account/Profile (requires login)
        public async Task<IActionResult> Profile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                TempData["ErrorMessage"] = "Please log in to view your profile.";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile") });
            }

            try
            {
                var response = await _userApiService.GetProfileAsync();
                if (response?.Success == true && response.Data != null)
                {
                    var model = new ProfileViewModel
                    {
                        FullName = response.Data.FullName,
                        Email = response.Data.Email,
                        PhoneNumber = response.Data.PhoneNumber ?? "",
                        Role = response.Data.Role ?? ""
                    };
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
            }

            // Fallback to session data
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userRole = HttpContext.Session.GetString("UserRole");
            return View(new ProfileViewModel
            {
                FullName = userName ?? "",
                Email = userEmail ?? "",
                PhoneNumber = HttpContext.Session.GetString("UserPhone") ?? "",
                Role = userRole ?? "Patient"
            });
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile") });
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _userApiService.UpdateProfileAsync(new UserDTO
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber ?? ""
                });

                if (response?.Success == true)
                {
                    HttpContext.Session.SetString("UserName", model.FullName);
                    HttpContext.Session.SetString("UserPhone", model.PhoneNumber ?? string.Empty);
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction("Profile");
                }

                ModelState.AddModelError("", response?.Message ?? "Failed to update profile.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                ModelState.AddModelError("", "An error occurred while updating profile.");
            }

            return View(model);
        }

        // GET: /Account/Settings (requires login)
        public IActionResult Settings()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                TempData["ErrorMessage"] = "Please log in to access settings.";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Settings") });
            }

            return View(new ChangePasswordViewModel());
        }

        // POST: /Account/Settings (Change password)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(ChangePasswordViewModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Settings") });
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _userApiService.ChangePasswordAsync(new ChangePasswordRequestDTO
                {
                    OldPassword = model.OldPassword,
                    NewPassword = model.NewPassword,
                    ConfirmPassword = model.ConfirmPassword
                });

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Password changed successfully. Please log in again.";
                    return RedirectToAction("Logout");
                }

                ModelState.AddModelError("", response?.Message ?? "Failed to change password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                ModelState.AddModelError("", "An error occurred while changing password.");
            }

            return View(model);
        }

        // POST: /Account/GoogleLogin
        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.IdToken))
                {
                    return BadRequest(new { success = false, message = "Invalid Google token" });
                }

                var response = await _authApiService.GoogleLoginAsync(request);

                if (response != null && response.Success && response.Data != null)
                {
                    HttpContext.Session.SetString("AccessToken", response.Data.AccessToken);
                    HttpContext.Session.SetString("RefreshToken", response.Data.RefreshToken);
                    HttpContext.Session.SetString("UserEmail", response.Data.User.Email);
                    HttpContext.Session.SetString("UserRole", response.Data.User.Role);
                    HttpContext.Session.SetString("UserName", response.Data.User.FullName);
                    HttpContext.Session.SetString("UserPhone", response.Data.User.PhoneNumber ?? string.Empty);

                    return Ok(new { success = true, message = "Google login successful" });
                }
                else
                {
                    return BadRequest(new { success = false, message = response?.Message ?? "Google login failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Google login error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred during Google login" });
            }
        }
    }
}
