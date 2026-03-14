using BussinessObjects.Models;

namespace Repositories.Interfaces
{
    public interface ISpecialtyRepository : IGenericRepository<Specialty>
    {
        Task<IEnumerable<Specialty>> GetActiveSpecialtiesAsync();
    }
}
