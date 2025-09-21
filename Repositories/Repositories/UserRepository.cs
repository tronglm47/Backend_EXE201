using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly VLivingDbContext _context;

        public UserRepository(VLivingDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
