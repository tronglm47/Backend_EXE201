using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VLivingAPI.RequestsResponses.User;
using Microsoft.Extensions.Logging;
using Repositories.Constants;
using System.Security.Cryptography;
using Repositories;

namespace Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string usernameOrEmail, string password);
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
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IEmailVerificationRepository _emailVerificationRepo;
        private readonly IPasswordResetRepository _passwordResetRepo;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepo, 
            IEmailVerificationRepository emailVerificationRepo,
            IPasswordResetRepository passwordResetRepo,
            IRefreshTokenRepository refreshTokenRepo,
            IEmailService emailService,
            IConfiguration configuration, 
            ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _emailVerificationRepo = emailVerificationRepo;
            _passwordResetRepo = passwordResetRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(string usernameOrEmail, string password)
        {
            _logger.LogInformation("Login attempt for username/email: {UsernameOrEmail}", usernameOrEmail);
            
            try
            {
                VLivingAPI.Repositories.Data.Models.User? user = null;
                
                // Check if input is email format
                if (usernameOrEmail.Contains("@"))
                {
                    _logger.LogInformation("Login attempt using email format: {Email}", usernameOrEmail);
                    user = await _userRepo.GetByEmailAsync(usernameOrEmail);
                }
                else
                {
                    _logger.LogInformation("Login attempt using username format: {Username}", usernameOrEmail);
                    user = await _userRepo.GetByUsernameAsync(usernameOrEmail);
                }

                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for username/email: {UsernameOrEmail}", usernameOrEmail);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Simple password comparison (not secure, but simple)
                if (user.Password != password)
                {
                    _logger.LogWarning("Login failed: Invalid password for username/email: {UsernameOrEmail}", usernameOrEmail);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Check if email is verified
                if (user.IsEmailVerified != true)
                {
                    _logger.LogWarning("Login failed: Email not verified for username/email: {UsernameOrEmail}", usernameOrEmail);
                    throw new UnauthorizedAccessException("Please verify your email before logging in");
                }

                // Update last login
                await _userRepo.UpdateLastLoginAsync(user.UserId);

                // Generate tokens
                var accessToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

                // Save refresh token to database
                await _refreshTokenRepo.CreateTokenAsync(user.UserId, refreshToken, refreshTokenExpiry);

                _logger.LogInformation("Login successful for username/email: {UsernameOrEmail}, UserId: {UserId}", usernameOrEmail, user.UserId);

                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    IsEmailVerified = user.IsEmailVerified ?? false
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw authorization exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username/email: {UsernameOrEmail}", usernameOrEmail);
                throw new Exception("An error occurred during login");
            }
        }

        public async Task<UserResponse> GetUserInfoAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting user info for userId: {UserId}", userId);
                
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for userId: {UserId}", userId);
                    throw new KeyNotFoundException("User not found");
                }

                return new UserResponse
                {
                    UserID = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    ProfilePictureURL = user.ProfilePictureUrl,
                    Bio = user.Bio
                };
            }
            catch (KeyNotFoundException)
            {
                throw; // Re-throw not found exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info for userId: {UserId}", userId);
                throw new Exception("An error occurred while retrieving user information");
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Registration attempt for username: {Username}", request.Username);

            try
            {
                // Check if username already exists
                if (await _userRepo.UsernameExistsAsync(request.Username))
                {
                    _logger.LogWarning("Registration failed: Username already exists: {Username}", request.Username);
                    throw new InvalidOperationException("Username already exists");
                }

                // Check if email already exists
                if (await _userRepo.EmailExistsAsync(request.Email))
                {
                    _logger.LogWarning("Registration failed: Email already exists: {Email}", request.Email);
                    throw new InvalidOperationException("Email already exists");
                }

                // Create new user
                var newUser = new VLivingAPI.Repositories.Data.Models.User
                {
                    Username = request.Username.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    Password = request.Password, // Plain text as requested
                    Role = string.IsNullOrWhiteSpace(request.Role) ? UserRoleConstants.UserRole : request.Role.Trim(),
                    FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                    ProfilePictureUrl = string.IsNullOrWhiteSpace(request.ProfilePictureUrl) ? null : request.ProfilePictureUrl.Trim(),
                    Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsEmailVerified = false
                };

                // Save user to database
                var createdUser = await _userRepo.CreateUserAsync(newUser);

                // Generate email verification token
                var verificationToken = GenerateSecureToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(24);
                await _emailVerificationRepo.CreateTokenAsync(createdUser.UserId, verificationToken, tokenExpiry);

                // Send verification email
                await _emailService.SendEmailVerificationAsync(createdUser.Email, createdUser.Username, verificationToken);

                _logger.LogInformation("User registered successfully: {Username}, UserId: {UserId}", 
                    createdUser.Username, createdUser.UserId);

                // Return response
                return new RegisterResponse
                {
                    UserId = createdUser.UserId,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    Role = createdUser.Role,
                    FullName = createdUser.FullName
                };
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
                throw new Exception("An error occurred during registration");
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                // Get and validate refresh token from database
                var storedToken = await _refreshTokenRepo.GetValidTokenAsync(refreshToken);
                if (storedToken == null)
                {
                    _logger.LogWarning("Invalid or expired refresh token provided");
                    throw new UnauthorizedAccessException("Invalid refresh token");
                }

                // Get user from database
                var user = await _userRepo.GetByIdAsync(storedToken.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for token refresh, userId: {UserId}", storedToken.UserId);
                    throw new UnauthorizedAccessException("User not found");
                }

                // Revoke the old refresh token
                await _refreshTokenRepo.RevokeTokenAsync(storedToken.RefreshTokenId);

                // Generate new tokens
                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();
                var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

                // Save new refresh token to database
                await _refreshTokenRepo.CreateTokenAsync(user.UserId, newRefreshToken, newRefreshTokenExpiry);
                
                _logger.LogInformation("Token refreshed successfully for user: {Username}", user.Username);
                
                return new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    IsEmailVerified = user.IsEmailVerified ?? false
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                throw new Exception("An error occurred during token refresh");
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                _logger.LogInformation("Email verification attempt");

                // Get and validate verification token
                var storedToken = await _emailVerificationRepo.GetValidTokenAsync(token);
                if (storedToken == null)
                {
                    _logger.LogWarning("Invalid or expired verification token provided");
                    
                    // Check if this token was already used (user already verified)
                    var anyToken = await _emailVerificationRepo.GetTokenByValueAsync(token);
                    if (anyToken != null && anyToken.IsUsed == true)
                    {
                        // Check if user is already verified
                        var existingUser = await _userRepo.GetByIdAsync(anyToken.UserId);
                        if (existingUser?.IsEmailVerified == true)
                        {
                            _logger.LogInformation("User already verified with this token");
                            return true;
                        }
                    }
                    
                    return false;
                }

                // Mark token as used
                await _emailVerificationRepo.MarkTokenAsUsedAsync(storedToken.TokenId);
                _logger.LogInformation("Token marked as used for TokenId: {TokenId}", storedToken.TokenId);

                // Update user email verification status
                _logger.LogInformation("About to update email verification status for UserId: {UserId}", storedToken.UserId);
                await _userRepo.UpdateEmailVerificationStatusAsync(storedToken.UserId, true);
                _logger.LogInformation("Email verification status update completed for UserId: {UserId}", storedToken.UserId);

                // Send welcome email
                var user = await _userRepo.GetByIdAsync(storedToken.UserId);
                if (user != null)
                {
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.Username);
                }

                _logger.LogInformation("Email verified successfully for user: {UserId}", storedToken.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string email)
        {
            try
            {
                _logger.LogInformation("Password reset request for email: {Email}", email);

                // Check if user exists
                var user = await _userRepo.GetByEmailAsync(email);
                if (user == null)
                {
                    // For security, we return true even if user doesn't exist
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return true;
                }

                // Generate password reset token (use 6-digit code like email verification)
                var resetToken = GenerateSecureToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(1);
                await _passwordResetRepo.CreateTokenAsync(user.UserId, resetToken, tokenExpiry);

                // Send password reset email
                await _emailService.SendPasswordResetAsync(user.Email, user.Username, resetToken);

                _logger.LogInformation("Password reset email sent for user: {Username}", user.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request for email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                _logger.LogInformation("Password reset attempt");

                // Get and validate reset token
                var storedToken = await _passwordResetRepo.GetValidTokenAsync(token);
                if (storedToken == null)
                {
                    _logger.LogWarning("Invalid or expired password reset token provided");
                    return false;
                }

                // Get current password for validation
                var currentPassword = await _userRepo.GetPasswordByUserIdAsync(storedToken.UserId);
                if (string.IsNullOrEmpty(currentPassword))
                {
                    _logger.LogWarning("User not found for password reset. UserId: {UserId}", storedToken.UserId);
                    return false;
                }

                // Validate new password is different from current password
                if (currentPassword == newPassword)
                {
                    _logger.LogWarning("New password is same as current password for UserId: {UserId}", storedToken.UserId);
                    throw new InvalidOperationException("New password must be different from your current password");
                }

                // Mark token as used (this prevents reuse)
                await _passwordResetRepo.MarkTokenAsUsedAsync(storedToken.TokenId);
                _logger.LogInformation("Password reset token marked as used for TokenId: {TokenId}", storedToken.TokenId);

                // Update user password
                await _userRepo.UpdatePasswordAsync(storedToken.UserId, newPassword);
                _logger.LogInformation("Password updated successfully for UserId: {UserId}", storedToken.UserId);

                // Revoke all refresh tokens for security (force re-login)
                await _refreshTokenRepo.RevokeAllUserTokensAsync(storedToken.UserId);
                _logger.LogInformation("All refresh tokens revoked for security for UserId: {UserId}", storedToken.UserId);

                // Optional: Delete the used token for extra security
                try
                {
                    await _passwordResetRepo.DeleteExpiredTokensAsync();
                    _logger.LogInformation("Expired and used password reset tokens cleaned up");
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning(cleanupEx, "Failed to cleanup expired tokens, but password reset was successful");
                    // Don't fail the main operation due to cleanup issues
                }

                _logger.LogInformation("Password reset completed successfully for UserId: {UserId}", storedToken.UserId);
                return true;
            }
            catch (InvalidOperationException)
            {
                // Re-throw validation exceptions to be handled by controller
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return false;
            }
        }

        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Resend verification email request for: {Email}", email);

                // Check if user exists
                var user = await _userRepo.GetByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Resend verification requested for non-existent email: {Email}", email);
                    return false;
                }

                _logger.LogInformation("User found: {Username}, IsEmailVerified: {IsVerified}", user.Username, user.IsEmailVerified);

                // Check if already verified
                if (user.IsEmailVerified == true)
                {
                    _logger.LogWarning("Resend verification requested for already verified email: {Email}", email);
                    return false;
                }

                // Generate new verification token
                var verificationToken = GenerateSecureToken();
                var tokenExpiry = DateTime.UtcNow.AddHours(24);
                await _emailVerificationRepo.CreateTokenAsync(user.UserId, verificationToken, tokenExpiry);

                // Send verification email
                await _emailService.SendEmailVerificationAsync(user.Email, user.Username, verificationToken);

                _logger.LogInformation("Verification email resent for user: {Username}", user.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resend verification email for: {Email}", email);
                return false;
            }
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                    return Task.FromResult(false);

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private string GenerateJwtToken(VLivingAPI.Repositories.Data.Models.User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT Key is not configured");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                    return null;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Don't validate expiry for refresh
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                if (validatedToken is not JwtSecurityToken jwtToken || 
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        private string GenerateSecureToken()
        {
            // Generate 6-digit verification code for both email verification and password reset
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private string GenerateSecureTokenLong()
        {
            // Keep this method for future use (currently not used - all tokens are 6-digit)
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Replace("/", "_").Replace("+", "-").Replace("=", "");
        }
    }
}
