using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Truyền vào ViewData để bên HTML có thể lấy được
            ViewData["ApiBaseUrl"] = baseUrl;
            return View();
        }

        // GET: /Admin/DoctorManagement
        public IActionResult DoctorManagement()
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Truyền vào ViewData để bên HTML có thể lấy được
            ViewData["ApiBaseUrl"] = baseUrl;
            // TODO: Implement doctor management
            return View();
        }

        // GET: /Admin/PatientRecords
        public IActionResult PatientRecords()
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Truyền vào ViewData để bên HTML có thể lấy được
            ViewData["ApiBaseUrl"] = baseUrl;
            // TODO: Implement patient records management
            return View();
        }

        // GET: /Admin/Financials
        public IActionResult Financials()
        {
            // TODO: Implement financial reports
            return View();
        }
    }
}
