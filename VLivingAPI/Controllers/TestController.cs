using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("connection")]
        public IActionResult TestConnection()
        {
            _logger.LogInformation("TestConnection method called from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
            try
            {
                return Ok(new { 
                    message = "API is working!", 
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestConnection");
                return StatusCode(500, new { message = "An error occurred during test" });
            }
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            _logger.LogInformation("TestDatabase method called from IP: {IP}", HttpContext.Connection.RemoteIpAddress);
            try
            {
                // Test actual database connection
                var connectionString = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection");
                
                using var context = HttpContext.RequestServices.GetRequiredService<VLivingDbContext>();
                
                // Try to query database
                var canConnect = await context.Database.CanConnectAsync();
                var userCount = await context.Users.CountAsync();
                
                return Ok(new { 
                    message = "Database connection test successful",
                    timestamp = DateTime.UtcNow,
                    canConnect = canConnect,
                    userCount = userCount,
                    connectionString = connectionString?.Substring(0, Math.Min(50, connectionString.Length)) + "..."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return StatusCode(500, new { 
                    message = "Database connection failed", 
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("auth")]
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