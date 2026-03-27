using BussinessObjects.Models;
using Clinic_Appointment_Booking.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Clinic_Appointment_Booking.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailVerificationAsync(User user, string token)
        {
            var appSettings = _configuration.GetSection("AppSettings");
            var verificationUrl = $"{appSettings["EmailVerificationUrl"]}?token={WebUtility.UrlEncode(token)}";

            var subject = "Verify Your Email - Clinic Appointment Booking";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Email Verification</h1>
        </div>
        <div class=""content"">
            <h2>Hello {user.FullName},</h2>
            <p>Thank you for registering with Clinic Appointment Booking!</p>
            <p>Please click the button below to verify your email address:</p>
            <a href=""{verificationUrl}"" class=""button"">Verify Email</a>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all;"">{verificationUrl}</p>
            <p>This link will expire in 24 hours.</p>
            <p>If you didn't create an account, please ignore this email.</p>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Clinic Appointment Booking. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(User user, string token)
        {
            var appSettings = _configuration.GetSection("AppSettings");
            var resetUrl = $"{appSettings["PasswordResetUrl"]}?token={WebUtility.UrlEncode(token)}";

            var subject = "Password Reset Request - Clinic Appointment Booking";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF5722; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #FF5722; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Password Reset</h1>
        </div>
        <div class=""content"">
            <h2>Hello {user.FullName},</h2>
            <p>We received a request to reset your password for your Clinic Appointment Booking account.</p>
            <p>Click the button below to reset your password:</p>
            <a href=""{resetUrl}"" class=""button"">Reset Password</a>
            <p>Or copy and paste this link into your browser:</p>
            <p style=""word-break: break-all;"">{resetUrl}</p>
            <div class=""warning"">
                <strong>Security Notice:</strong> This link will expire in 1 hour. If you didn't request a password reset, please ignore this email and your password will remain unchanged.
            </div>
        </div>
        <div class=""footer"">
            <p>&copy; 2026 Clinic Appointment Booking. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // Try to read from environment variables first (.env), then fallback to appsettings.json
                var smtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER") 
                    ?? _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT") 
                    ?? _configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER_EMAIL") 
                    ?? _configuration["EmailSettings:SenderEmail"];
                var senderName = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME") 
                    ?? _configuration["EmailSettings:SenderName"];
                var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") 
                    ?? _configuration["EmailSettings:Username"];
                var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") 
                    ?? _configuration["EmailSettings:Password"];
                var enableSsl = bool.Parse(Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL") 
                    ?? _configuration["EmailSettings:EnableSsl"] ?? "true");

                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {to}: {ex.Message}");
                throw;
            }
        }
    }
}
