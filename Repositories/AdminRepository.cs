using BussinessObjects.DTOs.admin;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ClinicDbContext _context;

        public AdminRepository(ClinicDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetDashboardStatsAsync(string period)
        {
            DateTime startDate, endDate, prevStartDate, prevEndDate;
            var now = DateTime.Now;

            // Thiết lập mốc thời gian dựa trên lựa chọn
            switch (period.ToLower())
            {
                case "this month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = now;
                    prevStartDate = startDate.AddMonths(-1);
                    prevEndDate = startDate.AddSeconds(-1);
                    break;
                case "this year":
                    startDate = new DateTime(now.Year, 1, 1);
                    endDate = now;
                    prevStartDate = startDate.AddYears(-1);
                    prevEndDate = startDate.AddSeconds(-1);
                    break;
                default: // "last 7 days"
                    startDate = now.AddDays(-7);
                    endDate = now;
                    prevStartDate = startDate.AddDays(-7);
                    prevEndDate = startDate.AddSeconds(-1);
                    break;
            }

            // --- Tính toán kỳ hiện tại ---
            var currRev = await _context.Payments.Where(p => p.PaymentDate >= startDate).SumAsync(p => p.Amount);
            var currAppt = await _context.Appointments.Where(a => a.AppointmentDate >= startDate).CountAsync();
            var currPatient = await _context.Users.Where(u => u.Role == "Patient" && u.CreatedAt >= startDate).CountAsync();
            var currentDoctors = await _context.Doctors.Where(d => d.CreatedAt >= startDate).CountAsync(d => d.IsAvailable);

            // --- Tính toán kỳ trước để ra % tăng trưởng ---
            var prevRev = await _context.Payments.Where(p => p.PaymentDate >= prevStartDate && p.PaymentDate <= prevEndDate).SumAsync(p => p.Amount);
            var prevAppt = await _context.Appointments.Where(a => a.AppointmentDate >= prevStartDate && a.AppointmentDate <= prevEndDate).CountAsync();
            var prevPatient = await _context.Users.Where(u => u.Role == "Patient" && u.CreatedAt >= prevStartDate && u.CreatedAt <= prevEndDate).CountAsync();

            // 2. Tính growth bác sĩ (thường tính dựa trên số bác sĩ mới đăng ký trong kỳ)
            var newDoctorsThisPeriod = await _context.Doctors.CountAsync(d => d.CreatedAt >= startDate);
            var newDoctorsPrevPeriod = await _context.Doctors.CountAsync(d => d.CreatedAt >= prevStartDate && d.CreatedAt < startDate);

            return new AdminDashboardDto
            {
                MonthlyRevenue = currRev,
                RevenueGrowth = CalculateGrowth(currRev, prevRev),
                TotalAppointments = currAppt,
                AppointmentGrowth = CalculateGrowth(currAppt, prevAppt),
                NewPatients = currPatient,
                PatientGrowth = CalculateGrowth(currPatient, prevPatient),
                ActiveDoctors = currentDoctors,
                DoctorGrowth = CalculateGrowth(newDoctorsThisPeriod, newDoctorsPrevPeriod)
            };
        }

        // Hàm phụ tính % tăng trưởng
        private double CalculateGrowth(decimal current, decimal previous)
        {
            if (previous == 0)
            {
                return current > 0 ? 100 : 0;
            }

            return (double)Math.Round((current - previous) / previous * 100, 1);
        }


        public async Task<IEnumerable<ChartDataDTO>> GetRevenueTrendsAsync(string period)
        {
            DateTime startDate = period.ToLower() switch
            {
                "this month" => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                "this year" => new DateTime(DateTime.Now.Year, 1, 1),
                _ => DateTime.Now.AddDays(-7) // default last 7 days
            };

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= startDate)
                .Include(a => a.Payment)
                .ToListAsync();

            // Group theo ngày, tuần hoặc tháng tùy vào độ dài của period
            return appointments
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new ChartDataDTO
                {
                    Label = period == "this year" ? g.Key.ToString("MMM") : g.Key.ToString("dd/MM"),
                    Appointments = g.Count(),
                    Revenue = g.Where(x => x.Payment != null).Sum(x => x.Payment.Amount)
                })
                .OrderBy(x => x.Label)
                .ToList();
        }

        public async Task<IEnumerable<DoctorOverviewDto>> GetStaffOverviewAsync()
        {
            var today = DateTime.Today;
            return await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Select(d => new DoctorOverviewDto
                {
                    FullName = d.User.FullName,
                    Specialty = d.Specialty.Name,
                    ProfileImage = d.ProfileImageUrl,
                    TodayAppointments = d.Appointments.Count(a => a.AppointmentDate == today),
                    Status = d.IsAvailable ? "Active" : "Away"
                })
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetRecentActivitiesAsync()
        {
            return await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(4)
                .Select(n => new
                {
                    n.Title,
                    n.Message,
                    TimeAgo = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentPerformanceDto>> GetDepartmentPerformanceAsync(string period)
        {
            DateTime startDate = period.ToLower() switch
            {
                "this month" => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                "this year" => new DateTime(DateTime.Now.Year, 1, 1),
                _ => DateTime.Now.AddDays(-7)
            };

            // Lấy tổng số lịch hẹn trong kỳ để tính %
            var totalApptsInRange = await _context.Appointments
                .CountAsync(a => a.AppointmentDate >= startDate);

            if (totalApptsInRange == 0)
            {
                return new List<DepartmentPerformanceDto>();
            }

            return await _context.Specialties
                .Select(s => new DepartmentPerformanceDto
                {
                    DepartmentName = s.Name,
                    // Tính % = (Số appt của chuyên khoa này / Tổng appt) * 100
                    Percentage = Math.Round((double)s.Doctors
                        .SelectMany(d => d.Appointments)
                        .Count(a => a.AppointmentDate >= startDate) / totalApptsInRange * 100, 1)
                })
                .OrderByDescending(x => x.Percentage)
                .Take(4)
                .ToListAsync();
        }
    }
}
