using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Models;
using VLivingAPI.Repositories.Data;

namespace Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateTokenAsync(int userId, string token, DateTime expiresAt);
        Task<RefreshToken?> GetValidTokenAsync(string token);
        Task RevokeTokenAsync(int tokenId);
        Task RevokeAllUserTokensAsync(int userId);
        Task DeleteExpiredTokensAsync();
    }
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly VLivingDbContext _context;

        public RefreshTokenRepository(VLivingDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> CreateTokenAsync(int userId, string token, DateTime expiresAt)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && 
                                        t.IsRevoked != true && 
                                        t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task RevokeTokenAsync(int tokenId)
        {
            var token = await _context.RefreshTokens.FindAsync(tokenId);
            if (token != null)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var userTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.IsRevoked != true)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredTokensAsync()
        {
            try
            {
                // Use raw SQL to avoid EF tracking issues
                var deletedCount = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM RefreshTokens WHERE ExpiresAt <= GETUTCDATE() OR IsRevoked = 1"
                );
                
                // Log if possible (no logger in this class)
            }
            catch (Exception)
            {
                // Log error if logging is available
                throw;
            }
        }
    }
}