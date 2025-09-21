
using VLivingAPI.Repositories.Data.Models;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(int userId); // Thêm cho get user info
    }
}
