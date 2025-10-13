using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Data;
using System.Diagnostics;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được truy cập
    public class SystemController : ControllerBase
    {
        private readonly VLivingDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            VLivingDbContext context, 
            IConfiguration configuration,
            ILogger<SystemController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Kiểm tra toàn bộ trạng thái hệ thống (API, Database, Configuration)
        /// Chỉ Admin được truy cập
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetSystemStatus()
        {
            _logger.LogInformation("System status check requested by Admin: {User}", User.Identity?.Name);

            var stopwatch = Stopwatch.StartNew();
            var systemStatus = new
            {
                timestamp = DateTime.UtcNow,
                environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                apiStatus = "Healthy",
                checks = new List<object>()
            };

            var checks = new List<object>();

            // 1. Check API Status
            checks.Add(new
            {
                name = "API Server",
                status = "Healthy",
                message = "API is running and responding",
                responseTime = $"{stopwatch.ElapsedMilliseconds}ms"
            });

            // 2. Check Database Connection
            var dbCheck = await CheckDatabaseConnection();
            checks.Add(dbCheck);

            // 3. Check Database Tables
            var tablesCheck = await CheckDatabaseTables();
            checks.Add(tablesCheck);

            // 4. Check Configuration
            var configCheck = CheckConfiguration();
            checks.Add(configCheck);

            // 5. Check Memory Usage
            var memoryCheck = CheckMemoryUsage();
            checks.Add(memoryCheck);

            stopwatch.Stop();

            // Determine overall health
            var allHealthy = checks.All(c => 
            {
                var statusProp = c.GetType().GetProperty("status");
                return statusProp?.GetValue(c)?.ToString() == "Healthy";
            });

            return Ok(new
            {
                timestamp = DateTime.UtcNow,
                environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                apiStatus = allHealthy ? "Healthy" : "Degraded",
                totalResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                checks = checks
            });
        }

        /// <summary>
        /// Kiểm tra kết nối Database SQL Server
        /// Chỉ Admin được truy cập
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            _logger.LogInformation("Database check requested by Admin: {User}", User.Identity?.Name);

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Test connection
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return Ok(new
                    {
                        status = "Unhealthy",
                        message = "Cannot connect to database",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Execute simple query
                var result = await _context.Database.ExecuteSqlRawAsync("SELECT 1");

                // Get connection string (masked)
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var maskedConnectionString = MaskConnectionString(connectionString);

                // Get database info
                var serverVersion = _context.Database.GetDbConnection().ServerVersion;

                stopwatch.Stop();

                return Ok(new
                {
                    status = "Healthy",
                    message = "Database connection successful",
                    details = new
                    {
                        canConnect = canConnect,
                        serverVersion = serverVersion,
                        connectionString = maskedConnectionString,
                        responseTime = $"{stopwatch.ElapsedMilliseconds}ms"
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database check failed");
                
                return Ok(new
                {
                    status = "Unhealthy",
                    message = "Database connection failed",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết về các bảng trong Database
        /// Chỉ Admin được truy cập
        /// </summary>
        [HttpGet("database/tables")]
        public async Task<IActionResult> GetDatabaseTables()
        {
            _logger.LogInformation("Database tables info requested by Admin: {User}", User.Identity?.Name);

            try
            {
                var tables = new List<object>
                {
                    new { name = "Users", count = await _context.Users.CountAsync() },
                    new { name = "Posts", count = await _context.Posts.CountAsync() },
                    new { name = "Apartments", count = await _context.Apartments.CountAsync() },
                    new { name = "Buildings", count = await _context.Buildings.CountAsync() },
                    new { name = "Subdivisions", count = await _context.Subdivisions.CountAsync() },
                    new { name = "Utilities", count = await _context.Utilities.CountAsync() },
                    new { name = "PostUtilities", count = await _context.PostUtilities.CountAsync() },
                    new { name = "EmailVerificationTokens", count = await _context.EmailVerificationTokens.CountAsync() },
                    new { name = "PasswordResetTokens", count = await _context.PasswordResetTokens.CountAsync() },
                    new { name = "RefreshTokens", count = await _context.RefreshTokens.CountAsync() }
                };

                return Ok(new
                {
                    status = "Success",
                    totalTables = tables.Count,
                    tables = tables,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database tables info");
                return StatusCode(500, new
                {
                    status = "Error",
                    message = "Failed to retrieve database tables information",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Kiểm tra cấu hình hệ thống (JWT, Email, Storage)
        /// Chỉ Admin được truy cập
        /// </summary>
        [HttpGet("configuration")]
        public IActionResult GetConfiguration()
        {
            _logger.LogInformation("Configuration check requested by Admin: {User}", User.Identity?.Name);

            try
            {
                var config = new
                {
                    jwt = new
                    {
                        issuer = _configuration["JWT:Issuer"],
                        audience = _configuration["JWT:Audience"],
                        expiryInMinutes = _configuration["JWT:ExpiryInMinutes"],
                        keyConfigured = !string.IsNullOrEmpty(_configuration["JWT:Key"])
                    },
                    email = new
                    {
                        smtpServer = _configuration["EmailSettings:SmtpServer"],
                        smtpPort = _configuration["EmailSettings:SmtpPort"],
                        fromEmail = _configuration["EmailSettings:FromEmail"],
                        fromName = _configuration["EmailSettings:FromName"],
                        usernameConfigured = !string.IsNullOrEmpty(_configuration["EmailSettings:Username"]),
                        passwordConfigured = !string.IsNullOrEmpty(_configuration["EmailSettings:Password"])
                    },
                    googleCloudStorage = new
                    {
                        bucketName = _configuration["GoogleCloudStorage:BucketName"],
                        projectId = _configuration["GoogleCloudStorage:ProjectId"],
                        credentialPathConfigured = !string.IsNullOrEmpty(_configuration["GoogleCloudStorage:CredentialPath"])
                    },
                    database = new
                    {
                        connectionStringConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection")),
                        connectionString = MaskConnectionString(_configuration.GetConnectionString("DefaultConnection"))
                    }
                };

                return Ok(new
                {
                    status = "Success",
                    environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                    configuration = config,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get configuration");
                return StatusCode(500, new
                {
                    status = "Error",
                    message = "Failed to retrieve configuration",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy thông tin server và runtime
        /// Chỉ Admin được truy cập
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetServerInfo()
        {
            _logger.LogInformation("Server info requested by Admin: {User}", User.Identity?.Name);

            try
            {
                var process = Process.GetCurrentProcess();

                var serverInfo = new
                {
                    server = new
                    {
                        machineName = Environment.MachineName,
                        osVersion = Environment.OSVersion.ToString(),
                        processorCount = Environment.ProcessorCount,
                        is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                        is64BitProcess = Environment.Is64BitProcess
                    },
                    runtime = new
                    {
                        dotnetVersion = Environment.Version.ToString(),
                        frameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                        runtimeIdentifier = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier
                    },
                    application = new
                    {
                        environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                        applicationName = "VLiving API",
                        version = "v1.0.17",
                        startTime = Process.GetCurrentProcess().StartTime,
                        uptime = DateTime.Now - Process.GetCurrentProcess().StartTime
                    },
                    process = new
                    {
                        processId = process.Id,
                        workingSet = $"{process.WorkingSet64 / 1024 / 1024} MB",
                        privateMemorySize = $"{process.PrivateMemorySize64 / 1024 / 1024} MB",
                        threads = process.Threads.Count
                    }
                };

                return Ok(new
                {
                    status = "Success",
                    serverInfo = serverInfo,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get server info");
                return StatusCode(500, new
                {
                    status = "Error",
                    message = "Failed to retrieve server information",
                    error = ex.Message
                });
            }
        }

        #region Private Helper Methods

        private async Task<object> CheckDatabaseConnection()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var canConnect = await _context.Database.CanConnectAsync();
                stopwatch.Stop();

                if (canConnect)
                {
                    var serverVersion = _context.Database.GetDbConnection().ServerVersion;
                    return new
                    {
                        name = "Database Connection",
                        status = "Healthy",
                        message = $"Connected to SQL Server {serverVersion}",
                        responseTime = $"{stopwatch.ElapsedMilliseconds}ms"
                    };
                }
                else
                {
                    return new
                    {
                        name = "Database Connection",
                        status = "Unhealthy",
                        message = "Cannot connect to database",
                        responseTime = $"{stopwatch.ElapsedMilliseconds}ms"
                    };
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    name = "Database Connection",
                    status = "Unhealthy",
                    message = $"Database error: {ex.Message}",
                    responseTime = "N/A"
                };
            }
        }

        private async Task<object> CheckDatabaseTables()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var userCount = await _context.Users.CountAsync();
                var postCount = await _context.Posts.CountAsync();
                stopwatch.Stop();

                return new
                {
                    name = "Database Tables",
                    status = "Healthy",
                    message = $"Tables accessible (Users: {userCount}, Posts: {postCount})",
                    responseTime = $"{stopwatch.ElapsedMilliseconds}ms"
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    name = "Database Tables",
                    status = "Unhealthy",
                    message = $"Table access error: {ex.Message}",
                    responseTime = "N/A"
                };
            }
        }

        private object CheckConfiguration()
        {
            var jwtConfigured = !string.IsNullOrEmpty(_configuration["JWT:Key"]);
            var emailConfigured = !string.IsNullOrEmpty(_configuration["EmailSettings:SmtpServer"]);
            var dbConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection"));

            var allConfigured = jwtConfigured && emailConfigured && dbConfigured;

            return new
            {
                name = "Configuration",
                status = allConfigured ? "Healthy" : "Degraded",
                message = allConfigured 
                    ? "All configurations loaded successfully" 
                    : $"Missing configuration (JWT: {jwtConfigured}, Email: {emailConfigured}, DB: {dbConfigured})",
                responseTime = "0ms"
            };
        }

        private object CheckMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            var workingSetMB = process.WorkingSet64 / 1024 / 1024;
            var privateMemoryMB = process.PrivateMemorySize64 / 1024 / 1024;

            var status = workingSetMB < 500 ? "Healthy" : (workingSetMB < 1000 ? "Warning" : "Critical");

            return new
            {
                name = "Memory Usage",
                status = status,
                message = $"Working Set: {workingSetMB} MB, Private Memory: {privateMemoryMB} MB",
                responseTime = "0ms"
            };
        }

        private string MaskConnectionString(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "Not configured";

            // Mask password in connection string
            var parts = connectionString.Split(';');
            var masked = parts.Select(part =>
            {
                if (part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                    return "Password=***";
                if (part.Trim().StartsWith("User Id=", StringComparison.OrdinalIgnoreCase))
                {
                    var userId = part.Split('=')[1].Trim();
                    return $"User Id={userId[0]}***";
                }
                return part;
            });

            return string.Join("; ", masked);
        }

        #endregion
    }
}
