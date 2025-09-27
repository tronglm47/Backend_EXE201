using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Interfaces
{
    public interface IPasswordResetRepository
    {
        Task<PasswordResetToken> CreateTokenAsync(int userId, string token, DateTime expiresAt);
        Task<PasswordResetToken?> GetValidTokenAsync(string token);
        Task MarkTokenAsUsedAsync(int tokenId);
        Task DeleteExpiredTokensAsync();
    }
}