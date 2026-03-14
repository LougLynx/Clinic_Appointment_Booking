using BussinessObjects.Models;
using DataAccess;
using Repositories.Interfaces;

namespace Repositories
{
    public class ContactRepository : GenericRepository<ContactMessage>, IContactRepository
    {
        public ContactRepository(ClinicDbContext context) : base(context)
        {
        }
    }
}
