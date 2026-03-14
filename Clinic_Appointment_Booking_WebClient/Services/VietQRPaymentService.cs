using System.Text;
using System.Web;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class VietQRPaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VietQRPaymentService> _logger;

        // VietQR Configuration
        private readonly string _bankCode;
        private readonly string _accountNumber;
        private readonly string _accountName;
        private readonly string _template;

        public VietQRPaymentService(
            IConfiguration configuration,
            ILogger<VietQRPaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Read from appsettings.json or use defaults
            _bankCode = _configuration["Payment:VietQR:BankCode"] ?? "MB";
            _accountNumber = _configuration["Payment:VietQR:AccountNumber"] ?? "0123456789";
            _accountName = _configuration["Payment:VietQR:AccountName"] ?? "MediClinic";
            _template = _configuration["Payment:VietQR:Template"] ?? "compact2";
        }

        public string GenerateQRCodeUrl(decimal amount, string appointmentReference, string accountName = "MediClinic")
        {
            try
            {
                // Format: https://img.vietqr.io/image/{BANK_ID}-{ACCOUNT_NO}-{TEMPLATE}.jpg?amount={AMOUNT}&addInfo={INFO}&accountName={NAME}
                
                var amountValue = ((int)amount).ToString();
                var info = HttpUtility.UrlEncode($"Appointment {appointmentReference}");
                var name = HttpUtility.UrlEncode(accountName);

                var qrUrl = $"https://img.vietqr.io/image/{_bankCode}-{_accountNumber}-{_template}.jpg" +
                           $"?amount={amountValue}" +
                           $"&addInfo={info}" +
                           $"&accountName={name}";

                _logger.LogInformation("Generated QR code URL for appointment {Reference}", appointmentReference);

                return qrUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code URL");
                return string.Empty;
            }
        }

        public async Task<bool> VerifyPaymentAsync(string transactionReference)
        {
            try
            {
                // In a real implementation, this would:
                // 1. Call bank API to verify transaction
                // 2. Check transaction status in database
                // 3. Verify amount matches
                
                // For now, simulate verification
                await Task.Delay(1000); // Simulate API call
                
                _logger.LogInformation("Payment verification requested for transaction {Reference}", transactionReference);
                
                // In production, implement actual verification logic here
                // This could involve:
                // - Webhook from payment gateway
                // - Polling bank API
                // - Checking payment confirmation table
                
                return true; // Simulated success
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return false;
            }
        }
    }
}
