
using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int userId);
        Task<User> GetByEmailAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
    }
}
