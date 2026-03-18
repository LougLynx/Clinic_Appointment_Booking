using Clinic_Appointment_Booking_WebClient.Models.ViewModels;
using Clinic_Appointment_Booking_WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDoctorApiService _doctorApiService;

        public AdminController(ILogger<AdminController> logger, IConfiguration configuration, IDoctorApiService doctorApiService)
        {
            _logger = logger;
            _configuration = configuration;
            _doctorApiService = doctorApiService;
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

        // GET: /Admin/DoctorDetails/5
        public async Task<IActionResult> DoctorDetails(int id)
        {
            try
            {
                var response = await _doctorApiService.GetDoctorByIdAsync(id);
                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Doctor not found.";
                return RedirectToAction(nameof(DoctorManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor details for Admin. DoctorId: {DoctorId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading doctor profile.";
                return RedirectToAction(nameof(DoctorManagement));
            }
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

        // GET: /Admin/PatientDetails/5
        public IActionResult PatientDetails(int id)
        {
            // NOTE: Demo data. Replace with real Patient API when available.
            var model = new AdminPatientProfileViewModel
            {
                PatientId = id,
                PatientCode = $"MED-{(98000 + id):00000}",
                FullName = id switch
                {
                    1 => "Sarah Johnson",
                    2 => "Michael Chen",
                    3 => "Elena Rodriguez",
                    4 => "James Wilson",
                    5 => "Linda Wu",
                    _ => $"Patient #{id}"
                },
                Email = id switch
                {
                    1 => "s.johnson@email.com",
                    2 => "m.chen@hospital.org",
                    3 => "e.rodriguez@mail.com",
                    4 => "jwilson99@webmail.net",
                    5 => "linda.wu@design.com",
                    _ => $"patient{id}@example.com"
                },
                PhoneNumber = "+1 (555) 000-1234",
                Gender = "Female",
                Age = 32,
                BloodType = "O+",
                Allergies = "Penicillin",
                PrimaryPhysicianName = "Dr. Robert Smith",
                LastVisitAt = new DateTime(2023, 10, 12),
                AvatarUrl = null,
                IsActive = true,
                RecentVitals = new List<PatientVitalViewModel>
                {
                    new() { Title = "Heart Rate", Value = "72 bpm", Icon = "favorite", ColorClass = "bg-red-100 dark:bg-red-900/30 text-red-600" },
                    new() { Title = "Blood Pressure", Value = "120/80 mmHg", Icon = "blood_pressure", ColorClass = "bg-blue-100 dark:bg-blue-900/30 text-blue-600" },
                    new() { Title = "BMI Index", Value = "22.4 Normal", Icon = "monitor_weight", ColorClass = "bg-teal-100 dark:bg-teal-900/30 text-teal-600" }
                },
                Timeline = new List<PatientTimelineEntryViewModel>
                {
                    new()
                    {
                        Date = new DateTime(2023, 10, 12),
                        TypeLabel = "Routine Checkup",
                        TypeBadgeClass = "bg-primary/10 text-primary",
                        Title = "General Physical Examination",
                        Notes = "Patient reported mild fatigue. Vitals normal. Recommended blood work for Vitamin D.",
                        PhysicianName = "Dr. Robert Smith",
                        PhysicianAvatarUrl = null,
                        Icon = "stethoscope"
                    },
                    new()
                    {
                        Date = new DateTime(2023, 8, 5),
                        TypeLabel = "Urgent",
                        TypeBadgeClass = "bg-red-100 dark:bg-red-900/30 text-red-600",
                        Title = "Acute Bronchitis",
                        Notes = "Severe persistent cough and mild fever. Prescribed antibiotics and rest.",
                        PhysicianName = "Dr. Elena Vance",
                        PhysicianAvatarUrl = null,
                        Icon = "emergency_home"
                    }
                },
                LabResults = new List<PatientLabResultViewModel>
                {
                    new()
                    {
                        TestName = "Complete Blood Count (CBC)",
                        Date = new DateTime(2023, 10, 14),
                        Result = "4.5 - 11.0 k/uL",
                        Reference = "Normal Range",
                        StatusLabel = "Normal",
                        StatusClass = "bg-green-100 dark:bg-green-900/30 text-green-600",
                        IsCritical = false
                    },
                    new()
                    {
                        TestName = "Vitamin D (25-Hydroxy)",
                        Date = new DateTime(2023, 10, 14),
                        Result = "18 ng/mL",
                        Reference = "30-100 ng/mL",
                        StatusLabel = "Critical",
                        StatusClass = "bg-red-100 dark:bg-red-900/30 text-red-600",
                        IsCritical = true
                    },
                    new()
                    {
                        TestName = "Lipid Profile",
                        Date = new DateTime(2023, 7, 12),
                        Result = "160 mg/dL",
                        Reference = "< 200 mg/dL",
                        StatusLabel = "Normal",
                        StatusClass = "bg-green-100 dark:bg-green-900/30 text-green-600",
                        IsCritical = false
                    }
                }
            };

            return View(model);
        }

        // GET: /Admin/Financials
        public IActionResult Financials()
        {
            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            // Truyền vào ViewData để bên HTML có thể lấy được
            ViewData["ApiBaseUrl"] = baseUrl;
            // TODO: Implement financial reports
            return View();
        }
    }
}
