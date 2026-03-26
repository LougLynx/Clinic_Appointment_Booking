using BussinessObjects.DTOs.admin;
using BussinessObjects.DTOs.admin.dashboard;
using BussinessObjects.DTOs.admin.financial;
using BussinessObjects.DTOs.admin.patient_records;
using ClosedXML.Excel;
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
            var now = DateTime.Now;
            var p = period.ToLower();
            DateTime dbStartDate = p switch
            {
                "this month" => now.Date.AddDays(-27),
                "this year" => new DateTime(now.Year, 1, 1),
                _ => now.Date.AddDays(-6)
            };

            var appointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= dbStartDate && a.AppointmentDate <= now)
                .Include(a => a.Payment)
                .ToListAsync();

            if (p == "this year")
            {
                return Enumerable.Range(1, now.Month)
                    .Select(m =>
                    {
                        var monthDate = new DateTime(now.Year, m, 1);
                        var group = appointments.Where(a => a.AppointmentDate.Month == m);
                        return new ChartDataDTO
                        {
                            Label = monthDate.ToString("MMM"),
                            Appointments = group.Count(),
                            Revenue = group.Sum(x => x.Payment?.Amount ?? 0)
                        };
                    }).ToList();
            }
            else if (p == "this month")
            {
                // Chạy i từ 3 xuống 0 (3 là xa nhất, 0 là hôm nay)
                return Enumerable.Range(0, 4)
                    .OrderByDescending(i => i)
                    .Select((i, index) =>
                    { // Dùng index để đặt tên Week từ 1 đến 4
                        var weekEndDate = now.Date.AddDays(-i * 7);
                        var weekStartDate = weekEndDate.AddDays(-6);

                        var group = appointments.Where(a => a.AppointmentDate.Date >= weekStartDate && a.AppointmentDate.Date <= weekEndDate);

                        return new ChartDataDTO
                        {
                            // index chạy từ 0 đến 3 tương ứng với dữ liệu từ cũ nhất đến mới nhất
                            Label = $"Week {index + 1}",
                            Appointments = group.Count(),
                            Revenue = group.Sum(x => x.Payment?.Amount ?? 0)
                        };
                    }).ToList();
            }
            else
            {
                return Enumerable.Range(0, 7)
                    .Select(d => now.Date.AddDays(-6 + d))
                    .Select(date =>
                    {
                        var group = appointments.Where(a => a.AppointmentDate.Date == date);
                        return new ChartDataDTO
                        {
                            Label = date.ToString("dd/MM"),
                            Appointments = group.Count(),
                            Revenue = group.Sum(x => x.Payment?.Amount ?? 0)
                        };
                    }).ToList();
            }
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

        public async Task<FinancialStatsDTO> GetFinancialStatsAsync()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfPrevMonth = startOfMonth.AddMonths(-1);

            // 1. Tính Doanh thu (Revenue)
            var currentRevenue = await _context.Payments
                .Where(p => p.PaymentStatus == "Completed" && p.PaymentDate >= startOfMonth)
                .SumAsync(p => p.Amount);

            var prevRevenue = await _context.Payments
                .Where(p => p.PaymentStatus == "Completed" && p.PaymentDate >= startOfPrevMonth && p.PaymentDate < startOfMonth)
                .SumAsync(p => p.Amount);

            // 2. Tính Chi phí (Expenses) - Giả sử chi phí là 30% doanh thu hoặc lấy từ bảng chi phí nếu có
            decimal currentExpenses = currentRevenue * 0.3m;
            decimal prevExpenses = prevRevenue * 0.3m;

            // 3. Tính Lợi nhuận (Net Profit)
            decimal currentProfit = currentRevenue - currentExpenses;
            decimal prevProfit = prevRevenue - prevExpenses;

            // Hàm tính % tăng trưởng
            double CalculateGrowth(decimal current, decimal prev)
            {
                if (prev == 0)
                {
                    return current > 0 ? 100 : 0;
                }

                return (double)Math.Round((current - prev) / prev * 100, 1);
            }

            return new FinancialStatsDTO
            {
                TotalRevenue = currentRevenue,
                PrevRevenue = prevRevenue,
                RevenueGrowth = CalculateGrowth(currentRevenue, prevRevenue),

                OperationalExpenses = currentExpenses,
                PrevExpenses = prevExpenses,
                ExpensesGrowth = CalculateGrowth(currentExpenses, prevExpenses),

                NetProfit = currentProfit,
                PrevProfit = prevProfit,
                ProfitGrowth = CalculateGrowth(currentProfit, prevProfit)
            };
        }

        public async Task<PagedTransactionResponse> GetTransactionsAsync(int page, int pageSize)
        {
            var query = _context.Payments
                .Include(p => p.Patient)
                .Include(p => p.Appointment)
                .OrderByDescending(p => p.PaymentDate)
                .AsNoTracking();

            int total = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new TransactionResponseDTO
                {
                    TransactionId = p.TransactionId ?? $"#TRX-{p.PaymentId}",
                    EntityName = p.Patient.FullName,
                    Category = p.Appointment.ReasonForVisit,
                    Date = p.PaymentDate ?? p.CreatedAt,
                    Amount = p.Amount,
                    Status = p.PaymentStatus.ToUpper(),
                    Type = "Income"
                }).ToListAsync();

            return new PagedTransactionResponse
            {
                Data = data,
                TotalRecords = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<FinancialAnalyticsDTO> GetFinancialAnalyticsAsync(string period)
        {
            var now = DateTime.Now;
            var startOfCurrentMonth = new DateTime(now.Year, now.Month, 1);

            // Xác định số tháng cần lấy
            int monthsCount = period == "lastyear" ? 12 : 6;
            var startDate = startOfCurrentMonth.AddMonths(-(monthsCount - 1));

            var allPayments = await _context.Payments
                .Where(p => p.PaymentStatus == "Completed" && p.PaymentDate >= startDate)
                .ToListAsync();

            // 1. Dữ liệu cho biểu đồ đường SVG (6 tháng)
            var trends = Enumerable.Range(0, monthsCount).Select(i =>
            {
                var monthDate = startDate.AddMonths(i);
                return new ChartDataPoint
                {
                    Label = period == "lastyear" ? monthDate.ToString("MMM yy") : monthDate.ToString("MMM").ToUpper(),
                    Value = allPayments
                .Where(p => p.PaymentDate?.Month == monthDate.Month && p.PaymentDate?.Year == monthDate.Year)
                .Sum(p => p.Amount)
                };
            }).ToList();

            // 2. Dữ liệu cho danh sách chi phí (Lấy theo tháng hiện tại)
            var currentMonthRev = trends.Last().Value;
            decimal currentExpenses = currentMonthRev * 0.35m; // Giả định 35% doanh thu

            var breakdown = new List<ExpenseBreakdownDTO>
    {
        new ExpenseBreakdownDTO { Name = "Staff Salaries", Amount = currentExpenses * 0.56m, Percentage = 56 },
        new ExpenseBreakdownDTO { Name = "Medical Supplies", Amount = currentExpenses * 0.25m, Percentage = 25 },
        new ExpenseBreakdownDTO { Name = "Facility Maintenance", Amount = currentExpenses * 0.11m, Percentage = 11 },
        new ExpenseBreakdownDTO { Name = "Other Utilities", Amount = currentExpenses * 0.08m, Percentage = 8 }
    };

            return new FinancialAnalyticsDTO
            {
                RevenueTrends = trends,
                ExpenseBreakdown = breakdown
            };
        }

        public async Task<byte[]> ExportFinancialReportAsync()
        {
            // 1. Lấy dữ liệu chi tiết bằng cách Join qua các bảng Appointment và Doctor
            var payments = await _context.Payments
                .Include(p => p.Patient)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User) // Lấy tên bác sĩ từ bảng User
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Detailed Financial Report");

                // --- Định nghĩa Header ---
                string[] headers = {
            "Transaction ID",
            "Payment Date",
            "Patient Name",
            "Doctor Name",
            "Reason For Visit",
            "Method",
            "Amount ($)",
            "Status",
            "Notes"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];

                    // Style cho Header
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.Teal;
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                // --- Điền dữ liệu Rows ---
                int currentRow = 2;
                foreach (var p in payments)
                {
                    worksheet.Cell(currentRow, 1).Value = p.TransactionId ?? $"#TRX-{p.PaymentId}";
                    worksheet.Cell(currentRow, 2).Value = p.PaymentDate?.ToString("dd/MM/yyyy HH:mm") ?? p.CreatedAt.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cell(currentRow, 3).Value = p.Patient?.FullName ?? "N/A";

                    // Lấy tên bác sĩ từ Navigation Property
                    worksheet.Cell(currentRow, 4).Value = p.Appointment?.Doctor?.User?.FullName ?? "N/A";

                    worksheet.Cell(currentRow, 5).Value = p.Appointment?.ReasonForVisit ?? "N/A";
                    worksheet.Cell(currentRow, 6).Value = p.PaymentMethod;

                    // Cột số tiền
                    var amountCell = worksheet.Cell(currentRow, 7);
                    amountCell.Value = p.Amount;
                    amountCell.Style.NumberFormat.Format = "$#,##0.00";

                    // Cột trạng thái với màu sắc trực quan
                    var statusCell = worksheet.Cell(currentRow, 8);
                    statusCell.Value = p.PaymentStatus.ToUpper();
                    if (p.PaymentStatus == "Completed")
                    {
                        statusCell.Style.Font.FontColor = XLColor.Green;
                    }
                    else if (p.PaymentStatus == "Pending")
                    {
                        statusCell.Style.Font.FontColor = XLColor.AirForceBlue;
                    }
                    else
                    {
                        statusCell.Style.Font.FontColor = XLColor.Red;
                    }

                    worksheet.Cell(currentRow, 9).Value = p.PaymentNotes ?? "";

                    currentRow++;
                }

                // --- Căn chỉnh giao diện ---
                // Tự động căn chỉnh độ rộng cột
                worksheet.Columns().AdjustToContents();

                // Kẻ khung cho toàn bộ bảng dữ liệu
                var dataRange = worksheet.Range(1, 1, currentRow - 1, headers.Length);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Thêm dòng tổng cộng ở cuối
                var lastRow = currentRow;
                worksheet.Cell(lastRow, 6).Value = "TOTAL REVENUE:";
                worksheet.Cell(lastRow, 6).Style.Font.Bold = true;

                var totalCell = worksheet.Cell(lastRow, 7);
                totalCell.FormulaA1 = $"=SUM(G2:G{currentRow - 1})";
                totalCell.Style.Font.Bold = true;
                totalCell.Style.NumberFormat.Format = "$#,##0.00";
                totalCell.Style.Fill.BackgroundColor = XLColor.LightGray;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        public async Task<bool> CreateDoctorAsync(BussinessObjects.DTOs.admin.doctor_management.CreateDoctorDto doctorDto)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Check if email already exists
                    var existingUser = await _context.Users.AnyAsync(u => u.Email == doctorDto.Email);
                    if (existingUser)
                    {
                        throw new InvalidOperationException("Email already exists");
                    }

                    // 1. Create User
                    var user = new BussinessObjects.Models.User
                    {
                        FullName = doctorDto.FullName,
                        Email = doctorDto.Email,
                        PhoneNumber = doctorDto.PhoneNumber,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(doctorDto.Password),
                        Role = "Doctor",
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // 2. Create Doctor
                    var doctor = new BussinessObjects.Models.Doctor
                    {
                        UserId = user.UserId,
                        SpecialtyId = doctorDto.SpecialtyId,
                        ConsultationFee = doctorDto.ConsultationFee,
                        YearsOfExperience = doctorDto.YearsOfExperience,
                        Qualifications = doctorDto.Qualifications,
                        Location = doctorDto.Location,
                        Bio = doctorDto.Bio,
                        ProfileImageUrl = doctorDto.ProfileImageUrl,
                        IsAvailable = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.Doctors.Add(doctor);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
