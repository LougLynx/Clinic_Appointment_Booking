using BussinessObjects.DTOs.admin;
using BussinessObjects.DTOs.admin.dashboard;
using BussinessObjects.DTOs.admin.patient_records;
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

        public async Task<PagedMedicalStaffResponse> GetAllDoctorsAsync(string? specialty = null, string? status = null, int page = 1, int pageSize = 5)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .AsNoTracking()
                .AsQueryable();

            // Lọc dữ liệu (giữ nguyên logic cũ)
            if (!string.IsNullOrWhiteSpace(specialty) && !specialty.Equals("All Specialties", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => d.Specialty.Name == specialty);
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Any Status", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(d => d.IsAvailable == (status == "Active"));
            }

            // Đếm tổng số bản ghi trước khi cắt
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Cắt dữ liệu theo trang
            var doctors = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new MedicalStaffResponseDTO
                {
                    DoctorId = d.DoctorId,
                    FullName = d.User.FullName,
                    Specialty = d.Specialty.Name,
                    ProfileImage = d.ProfileImageUrl,
                    Email = d.User.Email,
                    PhoneNumber = d.User.PhoneNumber,
                    Status = d.IsAvailable ? "Active" : "On Leave"
                }).ToListAsync();

            return new PagedMedicalStaffResponse
            {
                Data = doctors,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page
            };
        }

        public async Task<DoctorManagementStatsDto> GetDoctorManagementStatsAsync()
        {
            return new DoctorManagementStatsDto
            {
                // Tổng số bác sĩ trong hệ thống
                TotalDoctors = await _context.Doctors.CountAsync(),

                // Bác sĩ có IsAvailable = true
                ActiveDoctors = await _context.Doctors.CountAsync(d => d.IsAvailable),

                // Bác sĩ có IsAvailable = false (Đang nghỉ)
                OnLeaveDoctors = await _context.Doctors.CountAsync(d => !d.IsAvailable),

                // Tổng số chuyên khoa đang hoạt động
                TotalSpecialties = await _context.Specialties.CountAsync(s => s.IsActive)
            };
        }

        public async Task<bool> ToggleDoctorStatusAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                return false;
            }

            // Đảo ngược trạng thái hiện tại
            doctor.IsAvailable = !doctor.IsAvailable;
            doctor.UpdatedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<PatientManagementStatsDto> GetPatientManagementStatsAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

            return new PatientManagementStatsDto
            {
                TotalPatients = await _context.Users.CountAsync(u => u.Role == "Patient"),
                NewThisMonth = await _context.Users.CountAsync(u => u.Role == "Patient" && u.CreatedAt >= startOfMonth),
                PendingFollowUp = await _context.Appointments.CountAsync(a => a.Status == "Confirmed" && a.AppointmentDate >= now),
                ActiveThisWeek = await _context.Appointments.Where(a => a.AppointmentDate >= startOfWeek)
                                                           .Select(a => a.PatientId).Distinct().CountAsync()
            };
        }

        public async Task<PagedPatientResponse> GetPagedPatientsAsync(string? searchTerm, int page, int pageSize, string? sortBy = "Last Visit")
        {
            var query = _context.Users
                .Where(u => u.Role == "Patient")
                .Include(u => u.Appointments)
                    .ThenInclude(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            query = sortBy switch
            {
                "Name (A-Z)" => query.OrderBy(u => u.FullName),
                "Patient ID" => query.OrderBy(u => u.UserId),
                _ => query.OrderByDescending(u => u.Appointments.Max(a => a.AppointmentDate)) // Mặc định: Last Visit
            };

            int totalRecords = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new PatientRecordResponseDto
                {
                    PatientId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    LastVisitDate = u.Appointments.OrderByDescending(a => a.AppointmentDate).Select(a => a.AppointmentDate).FirstOrDefault(),
                    PrimaryPhysician = u.Appointments.OrderByDescending(a => a.AppointmentDate).Select(a => a.Doctor.User.FullName).FirstOrDefault() ?? "None",
                    PhysicianImage = u.Appointments.OrderByDescending(a => a.AppointmentDate).Select(a => a.Doctor.ProfileImageUrl).FirstOrDefault() ?? "",
                    Status = u.IsActive ? "Active" : "Inactive"
                }).ToListAsync();

            return new PagedPatientResponse
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                CurrentPage = page
            };
        }
        public async Task<bool> ToggleUserStatusAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
