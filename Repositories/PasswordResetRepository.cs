using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public interface IPasswordResetRepository
    {
        Task<PasswordResetToken> CreateTokenAsync(int userId, string token, DateTime expiresAt);
        Task<PasswordResetToken?> GetValidTokenAsync(string token);
        Task MarkTokenAsUsedAsync(int tokenId);
        Task DeleteExpiredTokensAsync();
    }
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly VLivingDbContext _context;
        private readonly ILogger<PasswordResetRepository> _logger;

        public PasswordResetRepository(VLivingDbContext context, ILogger<PasswordResetRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PasswordResetToken> CreateTokenAsync(int userId, string token, DateTime expiresAt)
        {
            _logger.LogInformation("Creating password reset token for UserId: {UserId}, Token: {Token}", userId, token);
            
            try
            {
                var resetToken = new PasswordResetToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PasswordResetTokens.Add(resetToken);
                var changesCount = await _context.SaveChangesAsync();
                _logger.LogInformation("Password reset token created - Changes count: {ChangesCount}, TokenId: {TokenId}", 
                    changesCount, resetToken.TokenId);
                return resetToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating password reset token for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && 
                                        t.IsUsed != true && 
                                        t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task MarkTokenAsUsedAsync(int tokenId)
        {
            _logger.LogInformation("Marking password reset token as used - TokenId: {TokenId}", tokenId);
            
            try
            {
                var token = await _context.PasswordResetTokens
                    .Where(t => t.TokenId == tokenId)
                    .FirstOrDefaultAsync();
                    
                if (token != null)
                {
                    _logger.LogInformation("Token found - Current IsUsed: {CurrentStatus}", token.IsUsed);
                    
                    token.IsUsed = true;
                    _context.Entry(token).State = EntityState.Modified;
                    _logger.LogInformation("Token IsUsed set to true, EntityState: {EntityState}", _context.Entry(token).State);
                    
                    var changesCount = await _context.SaveChangesAsync();
                    _logger.LogInformation("Token marked as used - Changes count: {ChangesCount}", changesCount);
                    
                    // Verify the update
                    _context.Entry(token).Reload();
                    _logger.LogInformation("After reload - Token IsUsed: {FinalStatus}", token.IsUsed);
                }
                else
                {
                    _logger.LogWarning("Password reset token not found for TokenId: {TokenId}", tokenId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking token as used for TokenId: {TokenId}", tokenId);
                throw;
            }
        }

        public async Task DeleteExpiredTokensAsync()
        {
            _logger.LogInformation("Starting cleanup of expired password reset tokens");
            
            try
            {
                // Use raw SQL to avoid EF tracking issues
                var deletedCount = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM PasswordResetTokens WHERE ExpiresAt <= GETUTCDATE() OR IsUsed = 1"
                );
                
                _logger.LogInformation("Deleted {Count} expired password reset tokens using raw SQL", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expired password reset tokens");
                throw;
            }
        }
    }
}