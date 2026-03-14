using BussinessObjects.DTOs;
using Clinic_Appointment_Booking_WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ILogger<DoctorsController> _logger;
        private readonly IDoctorApiService _doctorApiService;
        private readonly ISpecialtyApiService _specialtyApiService;

        public DoctorsController(
            ILogger<DoctorsController> logger,
            IDoctorApiService doctorApiService,
            ISpecialtyApiService specialtyApiService)
        {
            _logger = logger;
            _doctorApiService = doctorApiService;
            _specialtyApiService = specialtyApiService;
        }

        // GET: /Doctors
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? specialtyId,
            string? gender,
            bool? availableToday,
            string? sortBy,
            int pageNumber = 1)
        {
            try
            {
                // Load specialties for filter
                var specialtiesResponse = await _specialtyApiService.GetAllSpecialtiesAsync();
                if (specialtiesResponse?.Success == true && specialtiesResponse.Data != null)
                {
                    ViewBag.Specialties = specialtiesResponse.Data;
                }
                else
                {
                    ViewBag.Specialties = new List<SpecialtyDTO>();
                }

                var response = await _doctorApiService.SearchDoctorsAsync(
                    searchTerm,
                    specialtyId,
                    gender,
                    availableToday,
                    pageNumber,
                    12); // 12 doctors per page

                if (response?.Success == true && response.Data != null)
                {
                    // Apply sorting on client side
                    var doctors = response.Data.Doctors;
                    doctors = sortBy switch
                    {
                        "name_asc" => doctors.OrderBy(d => d.FullName).ToList(),
                        "name_desc" => doctors.OrderByDescending(d => d.FullName).ToList(),
                        "rating_desc" => doctors.OrderByDescending(d => d.Rating).ToList(),
                        "rating_asc" => doctors.OrderBy(d => d.Rating).ToList(),
                        "experience_desc" => doctors.OrderByDescending(d => d.YearsOfExperience).ToList(),
                        "experience_asc" => doctors.OrderBy(d => d.YearsOfExperience).ToList(),
                        "fee_asc" => doctors.OrderBy(d => d.ConsultationFee).ToList(),
                        "fee_desc" => doctors.OrderByDescending(d => d.ConsultationFee).ToList(),
                        _ => doctors.OrderByDescending(d => d.Rating).ToList() // Default: rating high to low
                    };
                    response.Data.Doctors = doctors;

                    ViewBag.SearchTerm = searchTerm;
                    ViewBag.SpecialtyId = specialtyId;
                    ViewBag.Gender = gender;
                    ViewBag.AvailableToday = availableToday;
                    ViewBag.SortBy = sortBy ?? "rating_desc";
                    return View(response.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve doctors: {Message}", response?.Message);
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to retrieve doctors";
                    return View(new DoctorSearchResponseDTO());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors");
                TempData["ErrorMessage"] = "An error occurred while retrieving doctors";
                ViewBag.Specialties = new List<SpecialtyDTO>();
                return View(new DoctorSearchResponseDTO());
            }
        }

        // GET: /Doctors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _doctorApiService.GetDoctorByIdAsync(id);

                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogWarning("Doctor not found with ID: {DoctorId}", id);
                    TempData["ErrorMessage"] = response?.Message ?? "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor details for ID: {DoctorId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving doctor details";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Doctors/Search
        public IActionResult Search(string query)
        {
            return RedirectToAction(nameof(Index), new { searchTerm = query });
        }
    }
}
