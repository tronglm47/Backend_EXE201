using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Models;
using Repositories.Data;

namespace Repositories
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
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly VLivingDbContext _context;
        private readonly ILogger<EmailVerificationRepository> _logger;

        public EmailVerificationRepository(VLivingDbContext context, ILogger<EmailVerificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmailVerificationToken> CreateTokenAsync(int userId, string token, DateTime expiresAt)
        {
            var verificationToken = new EmailVerificationToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailVerificationTokens.Add(verificationToken);
            await _context.SaveChangesAsync();
            return verificationToken;
        }

        public async Task<EmailVerificationToken?> GetValidTokenAsync(string token)
        {
            _logger.LogInformation("Getting valid token for: {Token}", token);
            
            var validToken = await _context.EmailVerificationTokens
                .AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && 
                                        t.IsUsed != true && 
                                        t.ExpiresAt > DateTime.UtcNow);
            
            if (validToken != null)
            {
                _logger.LogInformation("Valid token found - TokenID: {TokenId}, UserId: {UserId}, IsUsed: {IsUsed}, ExpiresAt: {ExpiresAt}", 
                    validToken.TokenId, validToken.UserId, validToken.IsUsed, validToken.ExpiresAt);
            }
            else
            {
                _logger.LogWarning("No valid token found for: {Token}", token);
                
                // Check if token exists but is used or expired
                var anyToken = await _context.EmailVerificationTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Token == token);
                    
                if (anyToken != null)
                {
                    _logger.LogWarning("Token exists but invalid - TokenID: {TokenId}, IsUsed: {IsUsed}, ExpiresAt: {ExpiresAt}, Now: {Now}", 
                        anyToken.TokenId, anyToken.IsUsed, anyToken.ExpiresAt, DateTime.UtcNow);
                }
            }
            
            return validToken;
        }

        public async Task<EmailVerificationToken?> GetTokenByValueAsync(string token)
        {
            return await _context.EmailVerificationTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task MarkTokenAsUsedAsync(int tokenId)
        {
            _logger.LogInformation("Marking token as used - TokenId: {TokenId}", tokenId);
            
            try
            {
                // Option 1: Mark as used with EF Core
                var token = await _context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.TokenId == tokenId);
                if (token != null)
                {
                    _logger.LogInformation("Token found - Current IsUsed: {IsUsed}", token.IsUsed);
                    
                    token.IsUsed = true;
                    _context.Entry(token).State = EntityState.Modified;
                    
                    var result = await _context.SaveChangesAsync();
                    _logger.LogInformation("EF Core SaveChanges result: {AffectedRows}", result);
                    
                    // Verify with fresh query
                    await _context.Entry(token).ReloadAsync();
                    _logger.LogInformation("After reload - IsUsed: {IsUsed}", token.IsUsed);
                }
                else
                {
                    _logger.LogWarning("Token not found for TokenId: {TokenId}", tokenId);
                }
                
                // Option 2: Also try direct SQL update as fallback
                var sqlResult = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE EmailVerificationTokens SET IsUsed = 1 WHERE TokenID = {0}", tokenId);
                _logger.LogInformation("Direct SQL update result: {AffectedRows}", sqlResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking token as used for TokenId: {TokenId}", tokenId);
                throw;
            }
        }

        // Alternative method to delete token after use
        public async Task DeleteTokenAfterUseAsync(int tokenId)
        {
            _logger.LogInformation("Deleting token after use - TokenId: {TokenId}", tokenId);
            
            try
            {
                var token = await _context.EmailVerificationTokens.FindAsync(tokenId);
                if (token != null)
                {
                    _context.EmailVerificationTokens.Remove(token);
                    var result = await _context.SaveChangesAsync();
                    _logger.LogInformation("Token deleted - Affected rows: {AffectedRows}", result);
                }
                else
                {
                    _logger.LogWarning("Token not found for deletion - TokenId: {TokenId}", tokenId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting token for TokenId: {TokenId}", tokenId);
                throw;
            }
        }

        public async Task DeleteExpiredTokensAsync()
        {
            _logger.LogInformation("Starting cleanup of expired email verification tokens");
            
            try
            {
                // Use raw SQL to avoid EF tracking issues
                var deletedCount = await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM EmailVerificationTokens WHERE ExpiresAt <= GETUTCDATE() OR IsUsed = 1"
                );
                
                _logger.LogInformation("Deleted {Count} expired email verification tokens using raw SQL", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expired email verification tokens");
                throw;
            }
        }
    }
}