using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Repositories;
using Repositories.Models;
using Services;
using Services.Models;
using Services.RequestsResponses.Chat;
using System.Security.Claims;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;
        private readonly ILogger<ChatController> _logger;
        private readonly IChatUsageRepository _chatUsageRepository;
        private readonly ChatLimitingSettings _chatLimitingSettings;

        public ChatController(
            IAiChatService aiChatService, 
            ILogger<ChatController> logger,
            IChatUsageRepository chatUsageRepository,
            IOptions<ChatLimitingSettings> chatLimitingSettings)
        {
            _aiChatService = aiChatService;
            _logger = logger;
            _chatUsageRepository = chatUsageRepository;
            _chatLimitingSettings = chatLimitingSettings.Value;
        }

        /// <summary>
        /// Send a message to AI chat and get response based on database data
        /// </summary>
        /// <param name="request">Chat message</param>
        /// <returns>AI response with database information</returns>
        /// <response code="200">Returns AI response</response>
        /// <response code="400">Invalid request</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("message")]
        [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid chat request: {ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { message = "Message cannot be empty" });
                }

                // Check rate limiting
                if (_chatLimitingSettings.EnableRateLimiting)
                {
                    var canProceed = await CheckRateLimitAsync();
                    if (!canProceed)
                    {
                        return StatusCode(429, new 
                        { 
                            message = "Rate limit exceeded. Please try again later.",
                            limits = new
                            {
                                maxPerHour = _chatLimitingSettings.MaxRequestsPerHour,
                                maxPerDay = _chatLimitingSettings.MaxRequestsPerDay
                            }
                        });
                    }
                }

                var response = await _aiChatService.ProcessMessageAsync(request.Message.Trim());
                
                // Log usage
                await LogChatUsageAsync(request.Message, response);
                
                _logger.LogInformation("Chat message processed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message: {Message}", request?.Message);
                
                // Log failed usage
                if (request != null)
                {
                    await LogChatUsageAsync(request.Message, null, ex.Message);
                }
                
                return StatusCode(500, new { message = "An error occurred while processing your message" });
            }
        }

        private async Task<bool> CheckRateLimitAsync()
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var now = DateTime.UtcNow;
                
                // Check hourly limit
                var hourlyCount = await _chatUsageRepository.GetUsageCountByIpAsync(
                    ipAddress, 
                    now.AddHours(-1), 
                    now);
                
                if (hourlyCount >= _chatLimitingSettings.MaxRequestsPerHour)
                {
                    _logger.LogWarning("Hourly rate limit exceeded for IP: {IpAddress}, Count: {Count}", 
                        ipAddress, hourlyCount);
                    return false;
                }
                
                // Check daily limit
                var dailyCount = await _chatUsageRepository.GetUsageCountByIpAsync(
                    ipAddress, 
                    now.AddDays(-1), 
                    now);
                
                if (dailyCount >= _chatLimitingSettings.MaxRequestsPerDay)
                {
                    _logger.LogWarning("Daily rate limit exceeded for IP: {IpAddress}, Count: {Count}", 
                        ipAddress, dailyCount);
                    return false;
                }
                
                // Check user-specific limits if authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        var userHourlyCount = await _chatUsageRepository.GetUsageCountByUserAsync(
                            userId, 
                            now.AddHours(-1), 
                            now);
                        
                        var userDailyCount = await _chatUsageRepository.GetUsageCountByUserAsync(
                            userId, 
                            now.AddDays(-1), 
                            now);
                        
                        if (userHourlyCount >= _chatLimitingSettings.MaxRequestsPerHour ||
                            userDailyCount >= _chatLimitingSettings.MaxRequestsPerDay)
                        {
                            _logger.LogWarning("User rate limit exceeded for User: {UserId}", userId);
                            return false;
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limits");
                // If rate limiting check fails, allow the request to proceed
                return true;
            }
        }

        private async Task LogChatUsageAsync(string message, ChatResponse? response, string? errorMessage = null)
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true 
                    ? (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? (int?)id : null)
                    : null;

                var usage = new ChatUsage
                {
                    UserId = userId,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    RequestTime = DateTime.UtcNow,
                    Message = message,
                    Response = response?.Response,
                    TokensUsed = 0, // Could be implemented to track actual tokens
                    IsSuccess = response?.Success == true,
                    ErrorMessage = errorMessage
                };

                await _chatUsageRepository.LogChatUsageAsync(usage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging chat usage");
                // Don't throw - usage logging shouldn't break the main flow
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault();
                }
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                }
                return ipAddress ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        /// <summary>
        /// Get chat help information
        /// </summary>
        /// <returns>Available chat commands and topics</returns>
        /// <response code="200">Returns help information</response>
        [HttpGet("help")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GetHelp()
        {
            var helpInfo = new
            {
                message = "VLiving AI Chat Help",
                availableTopics = new[]
                {
                    "Properties & Posts - Ask about available properties, apartments for rent",
                    "Buildings - Information about buildings and their apartments", 
                    "Users & Community - User statistics and community info",
                    "Reviews & Ratings - Property reviews and ratings",
                    "Bookings - Booking information and statistics",
                    "Pricing - Price ranges and averages",
                    "Utilities & Amenities - Available utilities and amenities"
                },
                exampleQuestions = new[]
                {
                    "How many properties are available?",
                    "What's the average price?",
                    "Tell me about reviews",
                    "Show me building information",
                    "What utilities are available?"
                },
                tips = new[]
                {
                    "Ask specific questions about properties, pricing, or reviews",
                    "Use keywords like 'post', 'apartment', 'price', 'review', etc.",
                    "The AI will provide real-time data from our database"
                }
            };

            return Ok(helpInfo);
        }

        /// <summary>
        /// Health check for chat service
        /// </summary>
        /// <returns>Service status</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "healthy",
                service = "AI Chat Service",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Get current usage limits and remaining quota
        /// </summary>
        /// <returns>Usage information</returns>
        [HttpGet("usage")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsage()
        {
            try
            {
                if (!_chatLimitingSettings.EnableRateLimiting)
                {
                    return Ok(new
                    {
                        rateLimitingEnabled = false,
                        message = "Rate limiting is disabled"
                    });
                }

                var ipAddress = GetClientIpAddress();
                var now = DateTime.UtcNow;
                
                var hourlyCount = await _chatUsageRepository.GetUsageCountByIpAsync(
                    ipAddress, now.AddHours(-1), now);
                
                var dailyCount = await _chatUsageRepository.GetUsageCountByIpAsync(
                    ipAddress, now.AddDays(-1), now);

                var result = new
                {
                    rateLimitingEnabled = true,
                    limits = new
                    {
                        maxPerHour = _chatLimitingSettings.MaxRequestsPerHour,
                        maxPerDay = _chatLimitingSettings.MaxRequestsPerDay
                    },
                    usage = new
                    {
                        hourly = new
                        {
                            used = hourlyCount,
                            remaining = Math.Max(0, _chatLimitingSettings.MaxRequestsPerHour - hourlyCount),
                            resetTime = now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss UTC")
                        },
                        daily = new
                        {
                            used = dailyCount,
                            remaining = Math.Max(0, _chatLimitingSettings.MaxRequestsPerDay - dailyCount),
                            resetTime = now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss UTC")
                        }
                    },
                    ipAddress = ipAddress,
                    timestamp = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage information");
                return StatusCode(500, new { message = "Error retrieving usage information" });
            }
        }
    }
}