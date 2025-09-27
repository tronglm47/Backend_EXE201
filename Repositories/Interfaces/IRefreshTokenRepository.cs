using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateTokenAsync(int userId, string token, DateTime expiresAt);
        Task<RefreshToken?> GetValidTokenAsync(string token);
        Task RevokeTokenAsync(int tokenId);
        Task RevokeAllUserTokensAsync(int userId);
        Task DeleteExpiredTokensAsync();
    }
}