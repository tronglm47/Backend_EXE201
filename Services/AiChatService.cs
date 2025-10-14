using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Repositories.Data;
using Repositories.Models;
using Services.RequestsResponses.Chat;
using Services.Models;
using System.Text;

namespace Services
{
    public class AiChatService : IAiChatService
    {
        private readonly VLivingDbContext _context;
        private readonly ILogger<AiChatService> _logger;
        private readonly OpenAISettings _openAISettings;
        private readonly ChatClient _chatClient;

        public AiChatService(
            VLivingDbContext context, 
            ILogger<AiChatService> logger,
            IOptions<OpenAISettings> openAISettings)
        {
            _context = context;
            _logger = logger;
            _openAISettings = openAISettings.Value;
            
            var client = new OpenAIClient(_openAISettings.ApiKey);
            _chatClient = client.GetChatClient(_openAISettings.Model);
        }

        public async Task<ChatResponse> ProcessMessageAsync(string message)
        {
            try
            {
                var lowercaseMessage = message.ToLower();
                
                // Skip OpenAI due to quota limit - use database responses directly
                return await GetSimpleResponseAsync(lowercaseMessage);
                
                // Disabled OpenAI due to quota limit
                /*
                // Get relevant database context
                var databaseContext = await GetDatabaseContextAsync(lowercaseMessage);
                
                // Create system prompt with database context
                var systemPrompt = $@"You are VLiving AI Assistant, a helpful chatbot for a property rental platform.
You have access to real-time database information about the platform.

Current Database Context:
{databaseContext}

Guidelines:
- Be helpful and friendly
- Provide accurate information based on the database context
- If asked about specific data not in context, politely explain you need more specific information
- Keep responses concise but informative
- Focus on helping users find properties, understand pricing, or get platform information";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(message)
                };

                var chatCompletionOptions = new ChatCompletionOptions()
                {
                    Temperature = (float)_openAISettings.Temperature
                };

                var response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions);
                
                if (response?.Value?.Content?.Count > 0)
                {
                    var aiResponse = response.Value.Content[0].Text;
                    
                    return new ChatResponse
                    {
                        Response = aiResponse,
                        Source = "OpenAI + Database",
                        Success = true
                    };
                }
                else
                {
                    // Fallback to simple keyword responses
                    return await GetSimpleResponseAsync(lowercaseMessage);
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat message with OpenAI: {Message}", message);
                
                // Fallback to simple responses
                try
                {
                    return await GetSimpleResponseAsync(message.ToLower());
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Error in fallback response for message: {Message}", message);
                    return new ChatResponse
                    {
                        Response = "Sorry, I'm experiencing technical difficulties. Please try again later.",
                        Source = "Error",
                        Success = false
                    };
                }
            }
        }

        private async Task<string> GetDatabaseContextAsync(string message)
        {
            var context = new StringBuilder();
            
            try
            {
                // Get general platform statistics
                var totalPosts = await _context.Posts.CountAsync(p => !p.Status.Equals("deleted"));
                var availablePosts = await _context.Posts.CountAsync(p => p.Status.Equals("available"));
                var totalUsers = await _context.Users.CountAsync();
                var totalReviews = await _context.Reviews.CountAsync(r => !r.IsDeleted);
                
                context.AppendLine($"Platform Statistics:");
                context.AppendLine($"- Total Properties: {totalPosts}");
                context.AppendLine($"- Available Properties: {availablePosts}");
                context.AppendLine($"- Total Users: {totalUsers}");
                context.AppendLine($"- Total Reviews: {totalReviews}");
                
                // Get pricing information if relevant
                if (ContainsKeywords(message, new[] { "price", "cost", "expensive", "cheap", "budget" }))
                {
                    var priceStats = await GetPriceStatisticsAsync();
                    if (priceStats != null)
                    {
                        context.AppendLine($"\nPricing Information:");
                        context.AppendLine($"- Price Range: {priceStats.Min:C} - {priceStats.Max:C}");
                        context.AppendLine($"- Average Price: {priceStats.Average:C}");
                    }
                }
                
                // Get review information if relevant
                if (ContainsKeywords(message, new[] { "review", "rating", "feedback", "quality" }))
                {
                    var avgRating = await _context.Reviews
                        .Where(r => !r.IsDeleted)
                        .AverageAsync(r => (double)r.Rating);
                    
                    context.AppendLine($"\nReview Information:");
                    context.AppendLine($"- Average Rating: {avgRating:F1}/5 stars");
                    context.AppendLine($"- Total Reviews: {totalReviews}");
                }
                
                // Get utilities if relevant
                if (ContainsKeywords(message, new[] { "utility", "utilities", "amenity", "amenities", "facility" }))
                {
                    var utilities = await _context.Utilities
                        .Select(u => u.Name)
                        .Take(10)
                        .ToListAsync();
                    
                    if (utilities.Any())
                    {
                        context.AppendLine($"\nAvailable Utilities: {string.Join(", ", utilities)}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database context");
                context.AppendLine("Database information temporarily unavailable.");
            }
            
            return context.ToString();
        }

        private async Task<dynamic?> GetPriceStatisticsAsync()
        {
            try
            {
                var priceStats = await _context.Posts
                    .Where(p => p.Price.HasValue && !p.Status.Equals("deleted"))
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        Min = g.Min(p => p.Price!.Value),
                        Max = g.Max(p => p.Price!.Value),
                        Average = g.Average(p => (double)p.Price!.Value),
                        Count = g.Count()
                    })
                    .FirstOrDefaultAsync();

                return priceStats;
            }
            catch
            {
                return null;
            }
        }

        private async Task<ChatResponse> GetSimpleResponseAsync(string lowercaseMessage)
        {
            // Simple keyword-based responses with database data
            if (ContainsKeywords(lowercaseMessage, new[] { "post", "apartment", "rent", "property", "properties", "available" }))
            {
                return await GetPostsInfoAsync();
            }
            else if (ContainsKeywords(lowercaseMessage, new[] { "building", "buildings" }))
            {
                return await GetBuildingsInfoAsync();
            }
            else if (ContainsKeywords(lowercaseMessage, new[] { "user", "users", "account" }))
            {
                return await GetUsersInfoAsync();
            }
            else if (ContainsKeywords(lowercaseMessage, new[] { "review", "rating", "feedback" }))
            {
                return await GetReviewsInfoAsync();
            }
            else if (ContainsKeywords(lowercaseMessage, new[] { "booking", "bookings", "reservation" }))
            {
                return await GetBookingsInfoAsync();
            }
            else if (ContainsKeywords(lowercaseMessage, new[] { "price", "cost", "expensive", "cheap" }))
            {
                return await GetPriceInfoAsync();
            }
            else if (ContainsKeywords(lowercaseMessage, new[] { "utility", "utilities", "amenity" }))
            {
                return await GetUtilitiesInfoAsync();
            }
            else
            {
                return GetDefaultResponse();
            }
        }

        private bool ContainsKeywords(string message, string[] keywords)
        {
            return keywords.Any(keyword => message.Contains(keyword));
        }

        private async Task<ChatResponse> GetPostsInfoAsync()
        {
            var postsCount = await _context.Posts.CountAsync(p => !p.Status.Equals("deleted"));
            var availablePosts = await _context.Posts.CountAsync(p => !p.Status.Equals("deleted")); // Count all non-deleted as available
            
            var averagePrice = 0.0;
            var priceCount = await _context.Posts.CountAsync(p => p.Price.HasValue && !p.Status.Equals("deleted"));
            if (priceCount > 0)
            {
                averagePrice = await _context.Posts
                    .Where(p => p.Price.HasValue && !p.Status.Equals("deleted"))
                    .AverageAsync(p => (double)p.Price!.Value);
            }

            var response = $"Here's information about our properties:\n\n" +
                          $"📊 Total Properties: {postsCount}\n" +
                          $"🏠 Available Properties: {availablePosts}\n" +
                          $"💰 Average Price: {averagePrice:C}\n\n" +
                          $"You can browse through our available properties to find your perfect home!";

            return new ChatResponse
            {
                Response = response,
                Source = "Posts Database"
            };
        }

        private async Task<ChatResponse> GetBuildingsInfoAsync()
        {
            var buildingsCount = await _context.Buildings.CountAsync();
            var buildingsWithApartments = await _context.Buildings
                .Include(b => b.Apartments)
                .Select(b => new { b.Name, ApartmentCount = b.Apartments.Count })
                .Take(5)
                .ToListAsync();

            var response = new StringBuilder();
            response.AppendLine($"Here's information about our buildings:\n");
            response.AppendLine($"🏢 Total Buildings: {buildingsCount}\n");
            
            if (buildingsWithApartments.Any())
            {
                response.AppendLine("Top Buildings with Apartments:");
                foreach (var building in buildingsWithApartments)
                {
                    response.AppendLine($"• {building.Name}: {building.ApartmentCount} apartments");
                }
            }

            return new ChatResponse
            {
                Response = response.ToString(),
                Source = "Buildings Database"
            };
        }

        private async Task<ChatResponse> GetUsersInfoAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var usersWithPosts = await _context.Users
                .Include(u => u.Posts)
                .CountAsync(u => u.Posts.Any());

            var response = $"Here's information about our community:\n\n" +
                          $"👥 Total Users: {totalUsers}\n" +
                          $"🏠 Property Owners: {usersWithPosts}\n\n" +
                          $"Join our growing community of property owners and renters!";

            return new ChatResponse
            {
                Response = response,
                Source = "Users Database"
            };
        }

        private async Task<ChatResponse> GetReviewsInfoAsync()
        {
            var totalReviews = await _context.Reviews.CountAsync(r => !r.IsDeleted);
            var averageRating = await _context.Reviews
                .Where(r => !r.IsDeleted)
                .AverageAsync(r => (double)r.Rating);

            var topRatedPosts = await _context.Posts
                .Where(p => p.AverageRating.HasValue)
                .OrderByDescending(p => p.AverageRating)
                .Take(3)
                .Select(p => new { p.Title, p.AverageRating, p.TotalReviews })
                .ToListAsync();

            var response = new StringBuilder();
            response.AppendLine($"Here's what our users say:\n");
            response.AppendLine($"⭐ Total Reviews: {totalReviews}");
            response.AppendLine($"📊 Average Rating: {averageRating:F1}/5 stars\n");

            if (topRatedPosts.Any())
            {
                response.AppendLine("Top Rated Properties:");
                foreach (var post in topRatedPosts)
                {
                    response.AppendLine($"• {post.Title}: {post.AverageRating:F1}⭐ ({post.TotalReviews} reviews)");
                }
            }

            return new ChatResponse
            {
                Response = response.ToString(),
                Source = "Reviews Database"
            };
        }

        private async Task<ChatResponse> GetBookingsInfoAsync()
        {
            var totalBookings = await _context.Bookings.CountAsync();
            var completedBookings = await _context.Bookings.CountAsync(b => b.Status.Equals("complete"));
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status.Equals("pending"));

            var response = $"Here's booking information:\n\n" +
                          $"📋 Total Bookings: {totalBookings}\n" +
                          $"✅ Completed Bookings: {completedBookings}\n" +
                          $"⏳ Pending Bookings: {pendingBookings}\n\n" +
                          $"Ready to make your next booking?";

            return new ChatResponse
            {
                Response = response,
                Source = "Bookings Database"
            };
        }

        private async Task<ChatResponse> GetPriceInfoAsync()
        {
            var priceStats = await _context.Posts
                .Where(p => p.Price.HasValue && !p.Status.Equals("deleted"))
                .GroupBy(p => 1)
                .Select(g => new
                {
                    Min = g.Min(p => p.Price!.Value),
                    Max = g.Max(p => p.Price!.Value),
                    Average = g.Average(p => (double)p.Price!.Value),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            if (priceStats == null)
            {
                return new ChatResponse
                {
                    Response = "No pricing information available at the moment.",
                    Source = "Posts Database"
                };
            }

            var response = $"Here's pricing information:\n\n" +
                          $"💰 Price Range: {priceStats.Min:C} - {priceStats.Max:C}\n" +
                          $"📊 Average Price: {priceStats.Average:C}\n" +
                          $"🏠 Properties with pricing: {priceStats.Count}\n\n" +
                          $"Find properties that fit your budget!";

            return new ChatResponse
            {
                Response = response,
                Source = "Posts Database"
            };
        }

        private async Task<ChatResponse> GetUtilitiesInfoAsync()
        {
            var utilities = await _context.Utilities
                .Select(u => u.Name)
                .ToListAsync();

            if (!utilities.Any())
            {
                return new ChatResponse
                {
                    Response = "No utilities information available.",
                    Source = "Utilities Database"
                };
            }

            var response = $"Available utilities and amenities:\n\n" +
                          $"🏠 {string.Join(", ", utilities)}\n\n" +
                          $"These amenities are available across our properties!";

            return new ChatResponse
            {
                Response = response,
                Source = "Utilities Database"
            };
        }

        private ChatResponse GetDefaultResponse()
        {
            var responses = new[]
            {
                "Hello! I can help you with information about properties, buildings, users, reviews, bookings, prices, or utilities. What would you like to know?",
                "Hi there! I'm here to assist you with questions about our rental platform. Ask me about available properties, pricing, or reviews!",
                "Welcome! I can provide information about our database including properties, bookings, and user reviews. How can I help you today?",
                "I'm your AI assistant for VLiving! Ask me about posts, apartments, buildings, or anything else you'd like to know!"
            };

            var random = new Random();
            var selectedResponse = responses[random.Next(responses.Length)];

            return new ChatResponse
            {
                Response = selectedResponse,
                Source = "Default Response"
            };
        }
    }
}