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
                var loginResponse = await _authService.LoginAsync(request.Username.Trim(), request.Password);
                
                _logger.LogInformation("Login successful for username: {Username}", request.Username);
                return Ok(loginResponse);
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

            // Password strength validation
            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters long" });
            }

            // Optional: Add password complexity requirement
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Password, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]"))
            {
                return BadRequest(new { message = "Password must contain at least one letter, one number, and one special character" });
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
                var refreshResponse = await _authService.RefreshTokenAsync(request.Token);
                
                _logger.LogInformation("Token refreshed successfully");
                return Ok(refreshResponse);
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

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Email verification request validation failed");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                _logger.LogWarning("Email verification request with empty verification code from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "6-digit verification code is required" });
            }

            try
            {
                var isVerified = await _authService.VerifyEmailAsync(request.Token);
                if (isVerified)
                {
                    _logger.LogInformation("Email verified successfully");
                    return Ok(new { message = "Email verified successfully" });
                }
                else
                {
                    _logger.LogWarning("Email verification failed with verification code");
                    return BadRequest(new { message = "Invalid or expired verification code. Please check your email for the correct 6-digit code." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return StatusCode(500, new { message = "An error occurred during email verification" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Forgot password request validation failed");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Forgot password request with empty email from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                var isSuccess = await _authService.SendPasswordResetAsync(request.Email);
                if (isSuccess)
                {
                    _logger.LogInformation("Password reset email sent for: {Email}", request.Email);
                    return Ok(new { message = "If your email exists in our system, you will receive a password reset link" });
                }
                else
                {
                    _logger.LogWarning("Password reset failed for email: {Email}", request.Email);
                    return StatusCode(500, new { message = "An error occurred while processing your request" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Reset password request validation failed");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                _logger.LogWarning("Reset password request with empty fields from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "Token and new password are required" });
            }

            // Additional password strength validation
            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters long" });
            }

            // Optional: Add more password strength requirements
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.NewPassword, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&])[A-Za-z\d@$!%*#?&]"))
            {
                return BadRequest(new { message = "Password must contain at least one letter, one number, and one special character" });
            }

            try
            {
                var isSuccess = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
                if (isSuccess)
                {
                    _logger.LogInformation("Password reset successfully");
                    return Ok(new { message = "Password has been reset successfully" });
                }
                else
                {
                    _logger.LogWarning("Password reset failed with token");
                    return BadRequest(new { message = "Invalid or expired reset token" });
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Password reset validation failed: {Error}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new { message = "An error occurred during password reset" });
            }
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Resend verification request validation failed");
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Resend verification request with empty email from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                var isSuccess = await _authService.ResendVerificationEmailAsync(request.Email);
                if (isSuccess)
                {
                    _logger.LogInformation("Verification email resent for: {Email}", request.Email);
                    return Ok(new { message = "Verification email has been sent" });
                }
                else
                {
                    _logger.LogWarning("Resend verification failed for email: {Email}", request.Email);
                    return BadRequest(new { message = "Email not found or already verified" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during resend verification for email: {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred while processing your request" });
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
