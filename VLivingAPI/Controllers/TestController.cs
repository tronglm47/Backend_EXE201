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
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    version = "v1.3.0-full-with-db"
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
                // Safely get database context
                var context = HttpContext.RequestServices.GetService<VLivingDbContext>();
                if (context == null)
                {
                    return Ok(new { 
                        message = "Database context not configured",
                        timestamp = DateTime.UtcNow,
                        canConnect = false
                    });
                }
                
                // Test actual database connection
                var connectionString = HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection");
                
                // Try to query database
                var canConnect = await context.Database.CanConnectAsync();
                var userCount = canConnect ? await context.Users.CountAsync() : -1;
                
                return Ok(new { 
                    message = canConnect ? "Database connection test successful" : "Database connection failed",
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

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "healthy", 
                timestamp = DateTime.UtcNow,
                message = "Full API with database support",
                version = "v1.3.0-full-with-db"
            });
        }
    }
}