using BussinessObjects.Models;

namespace Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByEmailVerificationTokenAsync(string token);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task UpdatePasswordAsync(int userId, string newPasswordHash);
        Task VerifyEmailAsync(int userId);
    }
}
