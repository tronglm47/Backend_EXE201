using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VLivingAPI.Repositories.Models;
using VLivingAPI.Repositories.Data;

namespace Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int userId);
        Task<User> GetByEmailAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);

        // New methods for authentication improvements
        Task UpdateEmailVerificationStatusAsync(int userId, bool isVerified);
        Task UpdatePasswordAsync(int userId, string newPassword);
        Task UpdateLastLoginAsync(int userId);
        Task<string> GetPasswordByUserIdAsync(int userId);
    }
    public class UserRepository : IUserRepository
    {
        private readonly VLivingDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(VLivingDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateEmailVerificationStatusAsync(int userId, bool isVerified)
        {
            _logger.LogInformation("Updating email verification status for UserId: {UserId}, IsVerified: {IsVerified}", userId, isVerified);
            
            try 
            {
                // Force tracking và không dùng AsNoTracking
                var user = await _context.Users
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync();
                    
                if (user != null)
                {
                    _logger.LogInformation("User found - Current IsEmailVerified: {CurrentStatus}", user.IsEmailVerified);
                    
                    user.IsEmailVerified = isVerified;
                    _context.Entry(user).State = EntityState.Modified;
                    _logger.LogInformation("User IsEmailVerified set to: {NewStatus}, EntityState: {EntityState}", 
                        user.IsEmailVerified, _context.Entry(user).State);
                    
                    var changesCount = await _context.SaveChangesAsync();
                    _logger.LogInformation("SaveChanges executed - Changes count: {ChangesCount}", changesCount);
                    
                    // Force refresh từ database
                    _context.Entry(user).Reload();
                    _logger.LogInformation("After reload - User IsEmailVerified: {FinalStatus}", user.IsEmailVerified);
                }
                else
                {
                    _logger.LogWarning("User not found for UserId: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email verification status for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task UpdatePasswordAsync(int userId, string newPassword)
        {
            _logger.LogInformation("Updating password for UserId: {UserId}", userId);
            
            try 
            {
                var user = await _context.Users
                    .Where(u => u.UserId == userId)
                    .FirstOrDefaultAsync();
                    
                if (user != null)
                {
                    _logger.LogInformation("User found for password update - UserId: {UserId}", user.UserId);
                    
                    user.Password = newPassword;
                    _context.Entry(user).State = EntityState.Modified;
                    _logger.LogInformation("Password set, EntityState: {EntityState}", _context.Entry(user).State);
                    
                    var changesCount = await _context.SaveChangesAsync();
                    _logger.LogInformation("Password update SaveChanges executed - Changes count: {ChangesCount}", changesCount);
                    
                    // Verify the update
                    _context.Entry(user).Reload();
                    _logger.LogInformation("Password update completed for UserId: {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("User not found for password update - UserId: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for UserId: {UserId}", userId);
                throw;
            }
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetPasswordByUserIdAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            return user?.Password ?? string.Empty;
        }
    }
}
