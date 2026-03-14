using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class SpecialtyRepository : GenericRepository<Specialty>, ISpecialtyRepository
    {
        public SpecialtyRepository(ClinicDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Specialty>> GetActiveSpecialtiesAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}
