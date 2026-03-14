using BussinessObjects.Models;

namespace Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task RevokeTokenAsync(string token);
        Task RevokeAllUserTokensAsync(int userId);
    }
}
