using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;
using VLivingAPI.RequestsResponses.User;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login request validation failed for username: {Username}", request?.Username);
                return BadRequest(ModelState);
            }

            // Additional null checks
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login request with empty credentials from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "Username and password are required" });
            }

            try
            {
                var token = await _authService.LoginAsync(request.Username.Trim(), request.Password);
                var response = new LoginResponse { Token = token };
                
                _logger.LogInformation("Login successful for username: {Username}", request.Username);
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Login failed for username: {Username}, IP: {IP}", 
                    request.Username, HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for username: {Username}", request.Username);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration request validation failed for username: {Username}", request?.Username);
                return BadRequest(ModelState);
            }

            // Additional null checks
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Registration request with empty required fields from IP: {IP}", 
                    HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "Username, email, and password are required" });
            }

            // Set default role if not provided
            if (string.IsNullOrWhiteSpace(request.Role))
            {
                request.Role = UserRoleConstants.UserRole;
                _logger.LogDebug("Setting default role '{Role}' for registration: {Username}", request.Role, request.Username);
            }

            // Validate role
            if (!UserRoleConstants.IsValidRole(request.Role))
            {
                _logger.LogWarning("Invalid role '{Role}' provided for registration: {Username}", request.Role, request.Username);
                return BadRequest(new { message = $"Invalid role. Valid roles are: {string.Join(", ", UserRoleConstants.GetAllRoles())}" });
            }

            try
            {
                var response = await _authService.RegisterAsync(request);
                
                _logger.LogInformation("Registration successful for username: {Username}, UserId: {UserId}", 
                    request.Username, response.UserId);
                return CreatedAtAction(nameof(GetUserInfo), new { }, response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed for username: {Username}, Email: {Email}, Error: {Error}", 
                    request.Username, request.Email, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for username: {Username}", request.Username);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            try
            {
                var username = User.Identity?.Name;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                _logger.LogInformation("User logout: {Username} (ID: {UserId})", username, userId);
                
                // In a stateless JWT implementation, logout is mainly client-side
                // The client should remove the token from storage
                // For additional security, you could implement a token blacklist
                
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {Username}", User.Identity?.Name);
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Token))
            {
                _logger.LogWarning("Refresh token request with empty token from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "Token is required" });
            }

            try
            {
                var newToken = await _authService.RefreshTokenAsync(request.Token);
                var response = new LoginResponse { Token = newToken };
                
                _logger.LogInformation("Token refreshed successfully");
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Token refresh failed: {Error}, IP: {IP}", ex.Message, HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { message = "Invalid or expired token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new { message = "An error occurred during token refresh" });
            }
        }

        [HttpGet("userinfo")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            _logger.LogDebug("GetUserInfo method called for user: {Username}", User.Identity?.Name);
            
            try
            {
                // Get userId from JWT claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("GetUserInfo called with invalid token - no NameIdentifier claim");
                    return Unauthorized(new { message = "Invalid token" });
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning("GetUserInfo called with invalid userId claim: {UserIdClaim}", userIdClaim);
                    return BadRequest(new { message = "Invalid user identifier" });
                }

                var userResponse = await _authService.GetUserInfoAsync(userId);
                return Ok(userResponse);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("GetUserInfo: User not found for claims userId: {UserIdClaim}", 
                    User.FindFirstValue(ClaimTypes.NameIdentifier));
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserInfo for user: {Username}", User.Identity?.Name);
                return StatusCode(500, new { message = "An error occurred while retrieving user information" });
            }
        }
    }
}
