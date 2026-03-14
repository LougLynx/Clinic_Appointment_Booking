using Clinic_Appointment_Booking_WebClient.Services;
using DotNetEnv;

namespace Clinic_Appointment_Booking_WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Load .env file from DataAccess folder
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "DataAccess", ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
            }

            var builder = WebApplication.CreateBuilder(args);

            // Override configuration from environment variables
            var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            if (!string.IsNullOrEmpty(googleClientId))
            {
                builder.Configuration["GoogleAuth:ClientId"] = googleClientId;
            }

            // Payment (VietQR) from .env
            var paymentBankCode = Environment.GetEnvironmentVariable("PAYMENT_VIETQR_BANK_CODE");
            var paymentAccountNumber = Environment.GetEnvironmentVariable("PAYMENT_VIETQR_ACCOUNT_NUMBER");
            var paymentAccountName = Environment.GetEnvironmentVariable("PAYMENT_VIETQR_ACCOUNT_NAME");
            var paymentTemplate = Environment.GetEnvironmentVariable("PAYMENT_VIETQR_TEMPLATE");
            if (!string.IsNullOrEmpty(paymentBankCode)) builder.Configuration["Payment:VietQR:BankCode"] = paymentBankCode;
            if (!string.IsNullOrEmpty(paymentAccountNumber)) builder.Configuration["Payment:VietQR:AccountNumber"] = paymentAccountNumber;
            if (!string.IsNullOrEmpty(paymentAccountName)) builder.Configuration["Payment:VietQR:AccountName"] = paymentAccountName;
            if (!string.IsNullOrEmpty(paymentTemplate)) builder.Configuration["Payment:VietQR:Template"] = paymentTemplate;

            // Add services to the container.
            var apiSettings = builder.Configuration.GetSection("ApiSettings");
            var apiBaseUrl = apiSettings["BaseUrl"];

            // Session configuration
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // HttpContextAccessor for accessing session in services
            builder.Services.AddHttpContextAccessor();

            // HttpClient for API communication
            builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl!);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register services
            builder.Services.AddScoped<IAuthApiService, AuthApiService>();
            builder.Services.AddScoped<IDoctorApiService, DoctorApiService>();
            builder.Services.AddScoped<ISpecialtyApiService, SpecialtyApiService>();
            builder.Services.AddScoped<IPaymentService, VietQRPaymentService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
