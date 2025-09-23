using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interfaces;
using Services.RequestsResponses.SubscriptionPlan;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionPlanController(ISubscriptionPlanService subscriptionPlanService)
        {
            _subscriptionPlanService = subscriptionPlanService;
        }

        /// <summary>
        /// Get all subscription plans with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Paginated list of subscription plans</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptionPlans([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _subscriptionPlanService.GetAllSubscriptionPlansPaginatedAsync(page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get subscription plan by ID with detailed information
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <returns>Subscription plan details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscriptionPlan(int id)
        {
            try
            {
                var plan = await _subscriptionPlanService.GetSubscriptionPlanDetailsByIdAsync(id);
                if (plan == null)
                    return NotFound(new { success = false, message = "Subscription plan not found" });

                return Ok(new { success = true, data = plan });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Create new subscription plan
        /// </summary>
        /// <param name="request">Subscription plan creation request</param>
        /// <returns>Created subscription plan</returns>
        [HttpPost]
        [Authorize(Roles = UserRoleConstants.AdminRole)] // Chỉ admin mới được tạo subscription plan
        public async Task<IActionResult> CreateSubscriptionPlan([FromBody] CreateSubscriptionPlanRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var createdPlan = await _subscriptionPlanService.CreateSubscriptionPlanAsync(request);
                return CreatedAtAction(nameof(GetSubscriptionPlan), new { id = createdPlan.PlanId }, 
                    new { success = true, data = createdPlan });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error creating subscription plan: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update existing subscription plan
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <param name="request">Subscription plan update request</param>
        /// <returns>Updated subscription plan</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleConstants.AdminRole)] // Chỉ admin mới được cập nhật subscription plan
        public async Task<IActionResult> UpdateSubscriptionPlan(int id, [FromBody] UpdateSubscriptionPlanRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

                var updatedPlan = await _subscriptionPlanService.UpdateSubscriptionPlanAsync(id, request);
                return Ok(new { success = true, data = updatedPlan });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating subscription plan: {ex.Message}" });
            }
        }

        /// <summary>
        /// Delete subscription plan
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleConstants.AdminRole)] // Chỉ admin mới được xóa subscription plan
        public async Task<IActionResult> DeleteSubscriptionPlan(int id)
        {
            try
            {
                var deleted = await _subscriptionPlanService.DeleteSubscriptionPlanAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, message = "Subscription plan not found" });

                return Ok(new { success = true, message = "Subscription plan deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error deleting subscription plan: {ex.Message}" });
            }
        }

        /// <summary>
        /// Search subscription plans with advanced filters
        /// </summary>
        /// <param name="request">Search criteria</param>
        /// <returns>Filtered and paginated subscription plans</returns>
        [HttpPost("search")]
        public async Task<IActionResult> SearchSubscriptionPlans([FromBody] SubscriptionPlanSearchRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { success = false, message = "Search request is required" });

                var result = await _subscriptionPlanService.SearchSubscriptionPlansAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get subscription plans by price range
        /// </summary>
        /// <param name="minPrice">Minimum monthly price</param>
        /// <param name="maxPrice">Maximum monthly price</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Subscription plans within price range</returns>
        [HttpGet("price-range")]
        public async Task<IActionResult> GetSubscriptionPlansByPriceRange(
            [FromQuery] decimal minPrice, 
            [FromQuery] decimal maxPrice, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _subscriptionPlanService.GetSubscriptionPlansByPriceRangeAsync(minPrice, maxPrice, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get subscription plans by duration
        /// </summary>
        /// <param name="durationMonths">Duration in months</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <returns>Subscription plans with specified duration</returns>
        [HttpGet("duration/{durationMonths}")]
        public async Task<IActionResult> GetSubscriptionPlansByDuration(
            int? durationMonths, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                    return BadRequest(new { success = false, message = "Page and PageSize must be greater than 0" });

                var result = await _subscriptionPlanService.GetSubscriptionPlansByDurationAsync(durationMonths, page, pageSize);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get most popular subscription plans (by active subscriptions count)
        /// </summary>
        /// <param name="count">Number of plans to return (default: 5)</param>
        /// <returns>Most popular subscription plans</returns>
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSubscriptionPlans([FromQuery] int count = 5)
        {
            try
            {
                if (count < 1)
                    return BadRequest(new { success = false, message = "Count must be greater than 0" });

                var result = await _subscriptionPlanService.GetPopularSubscriptionPlansAsync(count);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get subscription plans generating most revenue
        /// </summary>
        /// <param name="count">Number of plans to return (default: 5)</param>
        /// <returns>Top revenue generating subscription plans</returns>
        [HttpGet("top-revenue")]
        public async Task<IActionResult> GetMostRevenueGeneratingPlans([FromQuery] int count = 5)
        {
            try
            {
                if (count < 1)
                    return BadRequest(new { success = false, message = "Count must be greater than 0" });

                var result = await _subscriptionPlanService.GetMostRevenueGeneratingPlansAsync(count);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Check if subscription plan name is unique
        /// </summary>
        /// <param name="name">Plan name to check</param>
        /// <param name="excludeId">Plan ID to exclude from check (for updates)</param>
        /// <returns>Whether the name is unique</returns>
        [HttpGet("check-name-unique")]
        public async Task<IActionResult> CheckNameUnique([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { success = false, message = "Name is required" });

                var isUnique = await _subscriptionPlanService.IsSubscriptionPlanNameUniqueAsync(name, excludeId);
                return Ok(new { success = true, data = new { isUnique = isUnique } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Check if subscription plan can be deleted
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <returns>Whether the plan can be deleted and reasons if not</returns>
        [HttpGet("{id}/can-delete")]
        public async Task<IActionResult> CanDeleteSubscriptionPlan(int id)
        {
            try
            {
                var canDelete = await _subscriptionPlanService.CanDeleteSubscriptionPlanAsync(id);
                var hasActiveSubscriptions = await _subscriptionPlanService.HasActiveSubscriptionsAsync(id);
                var activeCount = await _subscriptionPlanService.GetActiveSubscriptionsCountAsync(id);

                return Ok(new 
                { 
                    success = true, 
                    data = new 
                    {
                        canDelete = canDelete,
                        hasActiveSubscriptions = hasActiveSubscriptions,
                        activeSubscriptionsCount = activeCount,
                        reasons = hasActiveSubscriptions 
                            ? new[] { $"Has {activeCount} active subscriptions" } 
                            : new string[0]
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get subscription plan statistics
        /// </summary>
        /// <param name="id">Subscription plan ID</param>
        /// <returns>Plan statistics including subscriptions and revenue</returns>
        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetSubscriptionPlanStatistics(int id)
        {
            try
            {
                var plan = await _subscriptionPlanService.GetSubscriptionPlanByIdAsync(id);
                if (plan == null)
                    return NotFound(new { success = false, message = "Subscription plan not found" });

                var activeCount = await _subscriptionPlanService.GetActiveSubscriptionsCountAsync(id);
                var totalCount = await _subscriptionPlanService.GetTotalSubscriptionsCountAsync(id);
                var totalRevenue = await _subscriptionPlanService.GetTotalRevenueByPlanAsync(id);

                return Ok(new 
                { 
                    success = true, 
                    data = new 
                    {
                        planId = id,
                        planName = plan.Name,
                        activeSubscriptions = activeCount,
                        totalSubscriptions = totalCount,
                        totalRevenue = totalRevenue,
                        averageRevenuePerSubscription = totalCount > 0 ? totalRevenue / totalCount : 0
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get overall subscription plans analytics
        /// </summary>
        /// <returns>Overall statistics for all subscription plans</returns>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetSubscriptionPlansAnalytics()
        {
            try
            {
                var totalRevenue = await _subscriptionPlanService.GetTotalRevenueFromAllPlansAsync();
                var totalActiveSubscriptions = await _subscriptionPlanService.GetTotalActiveSubscriptionsAsync();
                var averagePrice = await _subscriptionPlanService.GetAverageSubscriptionPriceAsync();
                var topPlans = await _subscriptionPlanService.GetPlansWithMostSubscribersAsync(3);

                return Ok(new 
                { 
                    success = true, 
                    data = new 
                    {
                        totalRevenue = totalRevenue,
                        totalActiveSubscriptions = totalActiveSubscriptions,
                        averageSubscriptionPrice = averagePrice,
                        topPlansBySubscribers = topPlans
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
