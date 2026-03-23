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

            // VNPay from .env
            var vnpTmnCode = Environment.GetEnvironmentVariable("VNPAY_TMN_CODE");
            var vnpHashSecret = Environment.GetEnvironmentVariable("VNPAY_HASH_SECRET");
            var vnpUrl = Environment.GetEnvironmentVariable("VNPAY_URL");
            var vnpReturnUrl = Environment.GetEnvironmentVariable("VNPAY_RETURN_URL");
            if (!string.IsNullOrEmpty(vnpTmnCode)) builder.Configuration["VNPAY_TMN_CODE"] = vnpTmnCode;
            if (!string.IsNullOrEmpty(vnpHashSecret)) builder.Configuration["VNPAY_HASH_SECRET"] = vnpHashSecret;
            if (!string.IsNullOrEmpty(vnpUrl)) builder.Configuration["VNPAY_URL"] = vnpUrl;
            if (!string.IsNullOrEmpty(vnpReturnUrl)) builder.Configuration["VNPAY_RETURN_URL"] = vnpReturnUrl;

            // Add services to the container.
            var apiSettings = builder.Configuration.GetSection("ApiSettings");
            var apiBaseUrl = apiSettings["BaseUrl"];

            // Session configuration (dùng Session cookie để lưu trạng thái đăng nhập)
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.Name = ".ClinicAppointment.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
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
            builder.Services.AddScoped<IPaymentService, VnPayService>();
            builder.Services.AddScoped<IAppointmentApiService, AppointmentApiService>();
            builder.Services.AddScoped<IUserApiService, UserApiService>();
            builder.Services.AddScoped<IContactApiService, ContactApiService>();

            builder.Services.AddControllersWithViews();

            // Antiforgery: cho phép nhận token từ header (cho AJAX/fetch)
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
            });

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
