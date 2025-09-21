using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;
using VLivingAPI.RequestsResponses.User;

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

        [HttpGet("test")]
        [Authorize]
        public IActionResult TestAuth()
        {
            _logger.LogDebug("TestAuth method called for user: {Username}", User.Identity?.Name);
            return Ok(new { 
                message = "Authentication successful!", 
                user = User.Identity?.Name,
                authenticated = User.Identity?.IsAuthenticated,
                claims = User.Claims.Select(c => new { type = c.Type, value = c.Value })
            });
        }
    }
}
