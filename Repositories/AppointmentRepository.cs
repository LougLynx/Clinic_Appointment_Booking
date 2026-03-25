using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ClinicDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await _dbSet
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.Specialty)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(int appointmentId)
        {
            return await _dbSet
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.User)
                .Include(a => a.Doctor)
                .ThenInclude(d => d!.Specialty)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<bool> ExistsSlotAsync(int doctorId, DateTime date, TimeSpan time)
        {
            return await _dbSet
                .AnyAsync(a => a.DoctorId == doctorId &&
                              a.AppointmentDate.Date == date.Date &&
                              a.AppointmentTime == time &&
                              a.Status != "Cancelled");
        }
    }
}
