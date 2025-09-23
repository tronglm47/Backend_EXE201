using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.Interfaces;
using Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VLivingAPI.RequestsResponses.User;
using Microsoft.Extensions.Logging;
using Repositories.Constants;

namespace Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepo, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Login attempt for username: {Username}", username);
            
            try
            {
                var user = await _userRepo.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for username: {Username}", username);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                // Simple password comparison (not secure, but simple)
                if (user.Password != password)
                {
                    _logger.LogWarning("Login failed: Invalid password for username: {Username}", username);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                _logger.LogInformation("Login successful for username: {Username}", username);

                return GenerateJwtToken(user);
            }
            catch (UnauthorizedAccessException)
            {
                throw; // Re-throw authorization exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", username);
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
                    CreatedAt = DateTime.UtcNow
                };

                // Save user to database
                var createdUser = await _userRepo.CreateUserAsync(newUser);

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

        public async Task<string> RefreshTokenAsync(string token)
        {
            try
            {
                _logger.LogInformation("Token refresh attempt");

                // Validate the current token (even if expired)
                var principal = GetPrincipalFromExpiredToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid token provided for refresh");
                    throw new UnauthorizedAccessException("Invalid token");
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("Invalid user ID in token for refresh");
                    throw new UnauthorizedAccessException("Invalid token claims");
                }

                // Get user from database to ensure they still exist
                var user = await _userRepo.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for token refresh, userId: {UserId}", userId);
                    throw new UnauthorizedAccessException("User not found");
                }

                // Generate new token
                var newToken = GenerateJwtToken(user);
                
                _logger.LogInformation("Token refreshed successfully for user: {Username}", user.Username);
                return newToken;
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
    }
}
