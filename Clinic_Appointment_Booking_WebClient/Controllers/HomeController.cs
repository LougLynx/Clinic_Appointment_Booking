using BussinessObjects.DTOs;
using Clinic_Appointment_Booking_WebClient.Models;
using Clinic_Appointment_Booking_WebClient.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IContactApiService _contactApiService;

        public HomeController(ILogger<HomeController> logger, IContactApiService contactApiService)
        {
            _logger = logger;
            _contactApiService = contactApiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(CreateContactRequestDTO request)
        {
            if (request == null)
            {
                TempData["ContactError"] = "Invalid form data.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Subject) ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                TempData["ContactError"] = "Please fill in all required fields.";
                return View();
            }

            var response = await _contactApiService.SubmitContactAsync(request);

            if (response?.Success == true)
            {
                TempData["ContactSuccess"] = response.Message ?? "Your message has been sent successfully. We will get back to you soon.";
                return RedirectToAction(nameof(Contact));
            }

            TempData["ContactError"] = response?.Message ?? response?.Errors?.FirstOrDefault()
                ?? "An error occurred while sending your message. Please try again later.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
