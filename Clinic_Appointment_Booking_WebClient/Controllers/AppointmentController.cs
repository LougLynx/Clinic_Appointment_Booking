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
        private readonly IAppointmentApiService _appointmentApiService;
        private readonly IPaymentService _paymentService;

        public AppointmentController(
            ILogger<AppointmentController> logger,
            IDoctorApiService doctorApiService,
            IAppointmentApiService appointmentApiService,
            IPaymentService paymentService)
        {
            _logger = logger;
            _doctorApiService = doctorApiService;
            _appointmentApiService = appointmentApiService;
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
            if (!IsValidReason(model.Reason))
            {
                ModelState.AddModelError(nameof(model.Reason), "Please select a valid reason for your visit.");
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
                PaymentReference = paymentReference
            };

            return View("Confirm", confirmModel);
        }

        // POST: /Appointment/CreatePaymentUrl
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePaymentUrl(decimal amount, string reference)
        {
            try
            {
                var url = _paymentService.CreatePaymentUrl(amount, reference, HttpContext);
                return Json(new { success = true, url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment URL");
                return Json(new { success = false, message = "Failed to initiate payment." });
            }
        }

        // GET: /Appointment/PaymentCallback
        public IActionResult PaymentCallback()
        {
            if (!_paymentService.ValidateSignature(Request.Query))
            {
                TempData["ErrorMessage"] = "Invalid payment signature.";
                return RedirectToAction("MyAppointments");
            }

            var vnp_ResponseCode = Request.Query["vnp_ResponseCode"];
            var vnp_TransactionStatus = Request.Query["vnp_TransactionStatus"];
            var vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString();

            if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
            {


                // For this demo, we'll try to find if there's a way to recover data or just show success
                TempData["SuccessMessage"] = "Payment successful! Your appointment has been confirmed.";
                return RedirectToAction("MyAppointments");
            }
            else
            {
                TempData["ErrorMessage"] = "Payment failed or was cancelled.";
                return RedirectToAction("MyAppointments");
            }
        }

        // POST: /Appointment/ConfirmBook - Lưu appointment vào database qua API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBook([FromBody] CreateAppointmentRequestDTO request)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return Json(new { success = false, message = "Please login to confirm appointment." });
            }

            try
            {
                var response = await _appointmentApiService.CreateAppointmentAsync(request);
                if (response?.Success == true && response.Data != null)
                {
                    return Json(new { success = true, message = "Appointment created successfully.", appointmentId = response.Data.AppointmentId });
                }

                return Json(new { success = false, message = response?.Message ?? "Failed to create appointment." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                return Json(new { success = false, message = "An error occurred. Please try again." });
            }
        }

        // GET: /Appointment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Edit", "Appointment", new { id }) });
            }

            var appointmentResponse = await _appointmentApiService.GetAppointmentByIdAsync(id);
            if (appointmentResponse?.Success != true || appointmentResponse.Data == null)
            {
                TempData["ErrorMessage"] = appointmentResponse?.Message ?? "Appointment not found.";
                return RedirectToAction("MyAppointments");
            }

            var appointment = appointmentResponse.Data;
            if (appointment.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                appointment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "This appointment can no longer be edited.";
                return RedirectToAction("MyAppointments");
            }
            var doctor = await GetDoctorAsync(appointment.DoctorId);
            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Doctor not found.";
                return RedirectToAction("MyAppointments");
            }

            var days = BuildAvailableDays(doctor, DateTime.Today, 30);

            var model = new AppointmentBookViewModel
            {
                AppointmentId = appointment.AppointmentId,
                DoctorId = appointment.DoctorId,
                Doctor = doctor,
                Days = days,
                SelectedDate = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                SelectedTime = appointment.AppointmentTime.ToString(@"hh\:mm"),
                Reason = MapReasonValue(appointment.ReasonForVisit),
                Notes = appointment.AdditionalNotes,
                IsFirstTime = appointment.IsFirstTime
            };

            return View("Book", model);
        }

        // POST: /Appointment/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(AppointmentBookViewModel model)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Edit", "Appointment", new { id = model.AppointmentId }) });
            }

            if (!model.AppointmentId.HasValue)
            {
                TempData["ErrorMessage"] = "Invalid appointment.";
                return RedirectToAction("MyAppointments");
            }

            if (model.DoctorId <= 0)
            {
                ModelState.AddModelError(nameof(model.DoctorId), "Invalid doctor selection.");
            }
            if (!IsValidReason(model.Reason))
            {
                ModelState.AddModelError(nameof(model.Reason), "Please select a valid reason for your visit.");
            }

            var doctor = await GetDoctorAsync(model.DoctorId);
            if (doctor == null)
            {
                ModelState.AddModelError("", "Doctor not found.");
                model.Days = new List<AppointmentDayViewModel>();
                return View("Book", model);
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
                return View("Book", model);
            }

            var reasonLabel = MapReasonLabel(model.Reason);
            var updateRequest = new UpdateAppointmentRequestDTO
            {
                AppointmentDate = selectedDate!.Value,
                AppointmentTime = selectedTime!.Value,
                ReasonForVisit = reasonLabel,
                AdditionalNotes = model.Notes,
                IsFirstTime = model.IsFirstTime
            };

            var response = await _appointmentApiService.UpdateAppointmentAsync(model.AppointmentId.Value, updateRequest);
            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = "Appointment updated successfully.";
                return RedirectToAction("MyAppointments");
            }

            ModelState.AddModelError("", response?.Message ?? "Failed to update appointment.");
            return View("Book", model);
        }

        // POST: /Appointment/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int appointmentId, string? reason)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyAppointments") });
            }

            var response = await _appointmentApiService.CancelAppointmentAsync(appointmentId, new CancelAppointmentRequestDTO
            {
                CancellationReason = reason
            });

            if (response?.Success == true)
            {
                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = response?.Message ?? "Failed to cancel appointment.";
            }

            return RedirectToAction("MyAppointments");
        }

        // GET: /Appointment/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyAppointments") });
            }

            try
            {
                var response = await _appointmentApiService.GetMyAppointmentsAsync();
                if (response?.Success == true && response.Data != null)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = response?.Message ?? "Failed to load appointments.";
                return View(new List<AppointmentDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments");
                TempData["ErrorMessage"] = "An error occurred while loading appointments.";
                return View(new List<AppointmentDTO>());
            }
        }

        // GET: /Appointment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Appointment", new { id }) });
            }

            var appointmentResponse = await _appointmentApiService.GetAppointmentByIdAsync(id);
            if (appointmentResponse?.Success != true || appointmentResponse.Data == null)
            {
                TempData["ErrorMessage"] = appointmentResponse?.Message ?? "Appointment not found.";
                return RedirectToAction("MyAppointments");
            }

            return View(appointmentResponse.Data);
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
                                // Check if this slot is busy
                                var isBusy = doctor.BusySlots?.Any(b => b.Date.Date == date.Date && b.StartTime == cursor) == true;

                                slots.Add(new AppointmentTimeSlotViewModel
                                {
                                    StartTime = cursor,
                                    EndTime = cursor.Add(TimeSpan.FromMinutes(slotMinutes)),
                                    IsAvailable = !isBusy
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

            return null;
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

            return null;
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

        private static bool IsValidReason(string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return false;
            }

            return reason.Equals("routine", StringComparison.OrdinalIgnoreCase) ||
                   reason.Equals("consultation", StringComparison.OrdinalIgnoreCase) ||
                   reason.Equals("followup", StringComparison.OrdinalIgnoreCase) ||
                   reason.Equals("urgent", StringComparison.OrdinalIgnoreCase);
        }

        private static string? MapReasonValue(string? reasonLabel)
        {
            if (string.IsNullOrWhiteSpace(reasonLabel))
            {
                return null;
            }

            var normalized = reasonLabel.Trim().ToLowerInvariant();
            return normalized switch
            {
                "routine checkup" => "routine",
                "initial consultation" => "consultation",
                "follow-up visit" => "followup",
                "follow up visit" => "followup",
                "urgent concern" => "urgent",
                "routine" => "routine",
                "consultation" => "consultation",
                "followup" => "followup",
                "urgent" => "urgent",
                _ => reasonLabel
            };
        }
    }
}
