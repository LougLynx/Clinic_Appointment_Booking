using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ClinicDbContext _context;
        public PatientRepository(ClinicDbContext context) => _context = context;

        public async Task<(IEnumerable<dynamic> Data, int TotalCount)> GetPatientsByDoctorIdAsync(
    int doctorId,
    int pageNumber,
    int pageSize,
    string search = null,
    string status = null)
        {
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Include(a => a.Patient)
                .GroupBy(a => a.PatientId)
                .Select(g => new
                {
                    PatientId = g.Key,
                    FullName = g.First().Patient.FullName,
                    Email = g.First().Patient.Email,
                    Reason = g.OrderByDescending(a => a.AppointmentDate).Select(a => a.ReasonForVisit).FirstOrDefault(),
                    LastVisit = g.Where(a => a.AppointmentDate < DateTime.Now)
                                 .OrderByDescending(a => a.AppointmentDate)
                                 .Select(a => a.AppointmentDate)
                                 .FirstOrDefault(),
                    NextAppointment = g.Where(a => a.AppointmentDate >= DateTime.Now)
                                       .OrderBy(a => a.AppointmentDate)
                                       .Select(a => new { a.AppointmentDate, a.AppointmentTime })
                                       .FirstOrDefault(),
                    Status = "Active" // Giả sử mặc định là Active, bạn có thể thay bằng logic thực tế
                });

            // --- LOGIC SEARCH ---
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p => p.FullName.ToLower().Contains(search) ||
                                         p.PatientId.ToString().Contains(search));
            }

            // --- LOGIC FILTER STATUS ---
            if (!string.IsNullOrWhiteSpace(status) && status != "All Statuses")
            {
                query = query.Where(p => p.Status == status);
            }

            int totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
