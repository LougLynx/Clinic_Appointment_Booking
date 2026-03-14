using Clinic_Appointment_Booking_WebClient.Services;

namespace Clinic_Appointment_Booking_WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
