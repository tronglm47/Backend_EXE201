
using VLivingAPI.RequestsResponses.User;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string username, string password);
        Task<UserResponse> GetUserInfoAsync(int userId);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);

        // New methods for authentication improvements
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> SendPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ResendVerificationEmailAsync(string email);
    }
}
