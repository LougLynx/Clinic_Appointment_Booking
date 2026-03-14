using System.Globalization;
using BussinessObjects.DTOs;
using Clinic_Appointment_Booking_WebClient.Models.ViewModels;
using Clinic_Appointment_Booking_WebClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ILogger<AppointmentController> _logger;
        private readonly IDoctorApiService _doctorApiService;
        private readonly IPaymentService _paymentService;

        public AppointmentController(
            ILogger<AppointmentController> logger,
            IDoctorApiService doctorApiService,
            IPaymentService paymentService)
        {
            _logger = logger;
            _doctorApiService = doctorApiService;
            _paymentService = paymentService;
        }

        // GET: /Appointment/Book
        public async Task<IActionResult> Book(int doctorId, string? date, string? time)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                var returnUrl = Url.Action("Book", "Appointment", new { doctorId, date, time });
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            if (doctorId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a doctor to book an appointment.";
                return RedirectToAction("Index", "Doctors");
            }

            var doctor = await GetDoctorAsync(doctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("Index", "Doctors");
            }

            var days = BuildAvailableDays(doctor, DateTime.Today, 30);
            var selectedDate = ResolveSelectedDate(date, days);
            var selectedTime = ResolveSelectedTime(time, selectedDate, days);

            var model = new AppointmentBookViewModel
            {
                DoctorId = doctorId,
                Doctor = doctor,
                Days = days,
                SelectedDate = selectedDate?.ToString("yyyy-MM-dd"),
                SelectedTime = selectedTime?.ToString(@"hh\:mm")
            };

            return View(model);
        }

        // POST: /Appointment/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentBookViewModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Book", "Appointment", new { doctorId = model.DoctorId }) });
            }

            if (model.DoctorId <= 0)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Invalid doctor selection.");
            }

            var doctor = await GetDoctorAsync(model.DoctorId);
            if (doctor == null)
            {
                ModelState.AddModelError("", "Doctor not found.");
                model.Days = new List<AppointmentDayViewModel>();
                return View(model);
            }

            model.Doctor = doctor;
            model.Days = BuildAvailableDays(doctor, DateTime.Today, 30);

            DateTime? selectedDate = null;
            TimeSpan? selectedTime = null;

            if (!string.IsNullOrWhiteSpace(model.SelectedDate))
            {
                if (DateTime.TryParseExact(model.SelectedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    selectedDate = parsedDate.Date;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.SelectedDate), "Invalid date selection.");
                }
            }

            if (!string.IsNullOrWhiteSpace(model.SelectedTime))
            {
                if (TimeSpan.TryParseExact(model.SelectedTime, "hh\\:mm", CultureInfo.InvariantCulture, out var parsedTime))
                {
                    selectedTime = parsedTime;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.SelectedTime), "Invalid time selection.");
                }
            }

            if (selectedDate.HasValue && selectedTime.HasValue)
            {
                var day = model.Days.FirstOrDefault(d => d.Date.Date == selectedDate.Value.Date);
                var slotExists = day?.Slots.Any(s => s.IsAvailable && s.StartTime == selectedTime.Value) == true;
                if (!slotExists)
                {
                    ModelState.AddModelError(nameof(model.SelectedTime), "Selected time slot is no longer available.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var reasonLabel = MapReasonLabel(model.Reason);
            
            // Generate payment reference
            var paymentReference = $"APT{model.DoctorId}{selectedDate!.Value:yyyyMMdd}{selectedTime!.Value:hhmm}";
            
            // Generate QR code URL
            var amount = doctor.ConsultationFee ?? 0;
            var qrCodeUrl = _paymentService.GenerateQRCodeUrl(amount, paymentReference);
            
            var confirmModel = new AppointmentConfirmViewModel
            {
                Doctor = doctor,
                AppointmentDate = selectedDate!.Value,
                AppointmentTime = selectedTime!.Value,
                Reason = reasonLabel,
                Notes = model.Notes,
                IsFirstTime = model.IsFirstTime,
                PatientName = HttpContext.Session.GetString("UserName"),
                PatientEmail = HttpContext.Session.GetString("UserEmail"),
                PatientPhone = HttpContext.Session.GetString("UserPhone"),
                QRCodeUrl = qrCodeUrl,
                PaymentReference = paymentReference
            };

            return View("Confirm", confirmModel);
        }

        // GET: /Appointment/MyAppointments
        public IActionResult MyAppointments()
        {
            // TODO: Implement logic to get user's appointments
            return View();
        }

        // GET: /Appointment/Details/5
        public IActionResult Details(int id)
        {
            // TODO: Implement logic to get appointment details
            return View();
        }

        private async Task<DoctorDetailDTO?> GetDoctorAsync(int doctorId)
        {
            try
            {
                var response = await _doctorApiService.GetDoctorByIdAsync(doctorId);
                if (response?.Success == true && response.Data != null)
                {
                    return response.Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctor for booking");
            }

            return null;
        }

        private static List<AppointmentDayViewModel> BuildAvailableDays(DoctorDetailDTO doctor, DateTime startDate, int daysToGenerate)
        {
            if (doctor.AvailableTimeSlots != null && doctor.AvailableTimeSlots.Any())
            {
                return doctor.AvailableTimeSlots
                    .GroupBy(s => s.Date.Date)
                    .Select(g => new AppointmentDayViewModel
                    {
                        Date = g.Key,
                        Slots = g.Select(s => new AppointmentTimeSlotViewModel
                        {
                            StartTime = s.StartTime,
                            EndTime = s.EndTime,
                            IsAvailable = s.IsAvailable
                        }).OrderBy(s => s.StartTime).ToList()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
            }

            var schedules = doctor.Schedules?.Where(s => s.IsAvailable).ToList() ?? new List<DoctorScheduleDTO>();
            
            if (schedules.Count == 0)
            {
                schedules = GenerateDefaultSchedule();
            }
            
            var days = new List<AppointmentDayViewModel>();

            for (var i = 0; i < daysToGenerate; i++)
            {
                var date = startDate.Date.AddDays(i);
                var daySchedules = schedules.Where(s =>
                        (s.SpecificDate.HasValue && s.SpecificDate.Value.Date == date) ||
                        (!s.SpecificDate.HasValue && s.DayOfWeek == date.DayOfWeek))
                    .ToList();

                if (daySchedules.Count == 0)
                {
                    continue;
                }

                var slots = new List<AppointmentTimeSlotViewModel>();
                foreach (var schedule in daySchedules)
                {
                    var slotMinutes = schedule.SlotDurationMinutes > 0 ? schedule.SlotDurationMinutes : 30;
                    var cursor = schedule.StartTime;
                    var endTime = schedule.EndTime;

                    while (cursor.Add(TimeSpan.FromMinutes(slotMinutes)) <= endTime)
                    {
                        if (date != DateTime.Today || cursor > DateTime.Now.TimeOfDay)
                        {
                            if (!slots.Any(s => s.StartTime == cursor))
                            {
                                slots.Add(new AppointmentTimeSlotViewModel
                                {
                                    StartTime = cursor,
                                    EndTime = cursor.Add(TimeSpan.FromMinutes(slotMinutes)),
                                    IsAvailable = true
                                });
                            }
                        }
                        cursor = cursor.Add(TimeSpan.FromMinutes(slotMinutes));
                    }
                }

                if (slots.Count > 0)
                {
                    days.Add(new AppointmentDayViewModel
                    {
                        Date = date,
                        Slots = slots.OrderBy(s => s.StartTime).ToList()
                    });
                }
            }

            return days.OrderBy(d => d.Date).ToList();
        }

        private static List<DoctorScheduleDTO> GenerateDefaultSchedule()
        {
            var defaultSchedules = new List<DoctorScheduleDTO>();
            
            var workingDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            
            foreach (var day in workingDays)
            {
                defaultSchedules.Add(new DoctorScheduleDTO
                {
                    ScheduleId = 0,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    SlotDurationMinutes = 30,
                    IsAvailable = true,
                    SpecificDate = null
                });
                
                defaultSchedules.Add(new DoctorScheduleDTO
                {
                    ScheduleId = 0,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    SlotDurationMinutes = 30,
                    IsAvailable = true,
                    SpecificDate = null
                });
            }
            
            return defaultSchedules;
        }

        private static DateTime? ResolveSelectedDate(string? date, List<AppointmentDayViewModel> days)
        {
            if (!string.IsNullOrWhiteSpace(date) &&
                DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) &&
                days.Any(d => d.Date.Date == parsed.Date))
            {
                return parsed.Date;
            }

            return days.FirstOrDefault()?.Date;
        }

        private static TimeSpan? ResolveSelectedTime(string? time, DateTime? selectedDate, List<AppointmentDayViewModel> days)
        {
            if (selectedDate == null)
            {
                return null;
            }

            var day = days.FirstOrDefault(d => d.Date.Date == selectedDate.Value.Date);
            if (day == null || day.Slots.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(time) &&
                TimeSpan.TryParseExact(time, "hh\\:mm", CultureInfo.InvariantCulture, out var parsed) &&
                day.Slots.Any(s => s.StartTime == parsed))
            {
                return parsed;
            }

            return day.Slots.FirstOrDefault(s => s.IsAvailable)?.StartTime;
        }

        private static string MapReasonLabel(string? reason)
        {
            return reason switch
            {
                "routine" => "Routine Checkup",
                "consultation" => "Initial Consultation",
                "followup" => "Follow-up Visit",
                "urgent" => "Urgent Concern",
                _ => reason ?? "Not specified"
            };
        }
    }
}
