
using VLivingAPI.RequestsResponses.User;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<UserResponse> GetUserInfoAsync(int userId);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
