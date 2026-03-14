using BussinessObjects.Models;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ClinicDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailVerificationToken == token 
                    && u.EmailVerificationExpiry > DateTime.Now);
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string token)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token 
                    && u.PasswordResetExpiry > DateTime.Now);
        }

        public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.PasswordHash = newPasswordHash;
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                user.UpdatedAt = DateTime.Now;
                
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task VerifyEmailAsync(int userId)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.EmailVerified = true;
                user.EmailVerificationToken = null;
                user.EmailVerificationExpiry = null;
                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
