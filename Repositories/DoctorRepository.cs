using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ClinicDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsWithDetailsAsync()
        {
            return await _dbSet
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.IsAvailable)
                .OrderByDescending(d => d.Rating)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdWithDetailsAsync(int doctorId)
        {
            return await _dbSet
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.DoctorSchedules)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        }

        public async Task<IEnumerable<Doctor>> SearchDoctorsAsync(string? searchTerm, int? specialtyId, string? gender, bool? availableToday)
        {
            var query = _dbSet
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.IsAvailable)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(d =>
                    d.User.FullName.Contains(searchTerm) ||
                    d.Specialty.Name.Contains(searchTerm) ||
                    (d.Bio != null && d.Bio.Contains(searchTerm)));
            }

            if (specialtyId.HasValue)
            {
                query = query.Where(d => d.SpecialtyId == specialtyId.Value);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(d => d.User.Gender == gender);
            }

            if (availableToday.HasValue && availableToday.Value)
            {
                var today = DateTime.Today.DayOfWeek;
                query = query.Where(d => d.DoctorSchedules.Any(s =>
                    s.DayOfWeek == today &&
                    s.IsAvailable));
            }

            return await query
                .OrderByDescending(d => d.Rating)
                .ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialtyAsync(int specialtyId)
        {
            return await _dbSet
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Where(d => d.SpecialtyId == specialtyId && d.IsAvailable)
                .OrderByDescending(d => d.Rating)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }
    }
}
