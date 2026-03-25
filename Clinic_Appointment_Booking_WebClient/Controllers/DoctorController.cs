using Microsoft.AspNetCore.Mvc;
using Clinic_Appointment_Booking_WebClient.Services;
using Clinic_Appointment_Booking_WebClient.Models.ViewModels;
using BussinessObjects.DTOs;

namespace Clinic_Appointment_Booking_WebClient.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ILogger<DoctorController> _logger;
        private readonly IDoctorApiService _doctorApiService;
        private readonly IAppointmentApiService _appointmentApiService;

        public DoctorController(
            ILogger<DoctorController> logger,
            IDoctorApiService doctorApiService,
            IAppointmentApiService appointmentApiService)
        {
            _logger = logger;
            _doctorApiService = doctorApiService;
            _appointmentApiService = appointmentApiService;
        }

        private async Task<DoctorDTO?> GetCurrentDoctorAsync()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userIdStr) || userRole != "Doctor")
            {
                return null;
            }

            if (int.TryParse(userIdStr, out int userId))
            {
                var response = await _doctorApiService.GetDoctorByUserIdAsync(userId);
                return response?.Data;
            }

            return null;
        }


        // GET: /Doctor/Schedule
        public async Task<IActionResult> Schedule(DateTime? month, string viewType = "Month")
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account");

            var targetMonth = month ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            var firstDayOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            DateTime startDay, endDay;
            // Reference day logic: 
            // 1. If 'month' is provided and is NOT the first of a month, it's likely a specific day selected for Week/Day view.
            // 2. If 'month' IS the first of the month, but it's the current month, use Today as the default for Week/Day.
            // 3. Otherwise, use 'month' if provided, or Today.
            var refDay = month ?? DateTime.Today;
            if (month.HasValue && month.Value.Day == 1 && month.Value.Month == DateTime.Today.Month && month.Value.Year == DateTime.Today.Year)
            {
                refDay = DateTime.Today;
            }
            
            // For Month view, targetMonth should be the first of that month
            var targetMonthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var targetMonthEnd = targetMonthStart.AddMonths(1).AddDays(-1);
            if (viewType == "Week")
            {
                startDay = refDay.AddDays(-(int)refDay.DayOfWeek);
                endDay = startDay.AddDays(6);
            }
            else if (viewType == "Day")
            {
                startDay = refDay.Date;
                endDay = refDay.Date;
            }
            else // Month (default)
            {
                startDay = targetMonthStart.AddDays(-(int)targetMonthStart.DayOfWeek);
                endDay = targetMonthEnd.AddDays(6 - (int)targetMonthEnd.DayOfWeek);
            }

            var calendarDays = new List<DateTime>();
            for (var day = startDay; day <= endDay; day = day.AddDays(1))
            {
                calendarDays.Add(day);
            }

            var appointmentsResponse = await _appointmentApiService.GetDoctorAppointmentsAsync(doctor.DoctorId);
            var appointments = appointmentsResponse?.Data ?? new List<AppointmentDTO>();

            // Filter appointments for the visible calendar grid
            var folderAppointments = appointments
                .Where(a => a.AppointmentDate.Date >= startDay.Date && a.AppointmentDate.Date <= endDay.Date)
                .ToList();

            // We need to get full doctor details for schedules/working hours
            var doctorDetailResponse = await _doctorApiService.GetDoctorByIdAsync(doctor.DoctorId);
            var doctorDetail = doctorDetailResponse?.Data;

            var viewModel = new DoctorScheduleViewModel
            {
                ReferenceDate = refDay,
                ViewType = viewType,
                Appointments = folderAppointments,
                DaysInCalendar = calendarDays,
                WorkingHours = doctorDetail?.Schedules ?? new List<DoctorScheduleDTO>()
            };

            return View(viewModel);
        }

        // GET: /Doctor/Patients
        public async Task<IActionResult> Patients()
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
