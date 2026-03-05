using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(ILogger<DoctorsController> logger)
        {
            _logger = logger;
        }

        // GET: /Doctors
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Doctors/Details/5
        public IActionResult Details(int id)
        {
            // TODO: Implement logic to get doctor details by id
            return View();
        }

        // GET: /Doctors/Search
        public IActionResult Search(string query)
        {
            // TODO: Implement search logic
            ViewBag.Query = query;
            return View();
        }
    }
}
