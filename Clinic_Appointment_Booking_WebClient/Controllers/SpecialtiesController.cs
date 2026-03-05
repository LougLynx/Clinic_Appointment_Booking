using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class SpecialtiesController : Controller
    {
        private readonly ILogger<SpecialtiesController> _logger;

        public SpecialtiesController(ILogger<SpecialtiesController> logger)
        {
            _logger = logger;
        }

        // GET: /Specialties
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Specialties/Details/5
        public IActionResult Details(int id)
        {
            // TODO: Implement logic to get specialty details by id
            return View();
        }

        // GET: /Specialties/Doctors/5
        public IActionResult Doctors(int id)
        {
            // TODO: Implement logic to get doctors by specialty id
            return View();
        }
    }
}
