using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Interfaces
{
    public interface IEmailVerificationRepository
    {
        Task<EmailVerificationToken> CreateTokenAsync(int userId, string token, DateTime expiresAt);
        Task<EmailVerificationToken?> GetValidTokenAsync(string token);
        Task<EmailVerificationToken?> GetTokenByValueAsync(string token);
        Task MarkTokenAsUsedAsync(int tokenId);
        Task DeleteTokenAfterUseAsync(int tokenId);
        Task DeleteExpiredTokensAsync();
    }
}