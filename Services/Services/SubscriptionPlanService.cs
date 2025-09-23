using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.SubscriptionPlan;
using VLivingAPI.Repositories.Data.Models;
using EVCS.Repositories.HuyCG.Interfaces;

namespace Services.Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Basic CRUD Operations

        public async Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlansAsync()
        {
            return await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
        }

        public async Task<PaginationResult<SubscriptionPlanResponse>> GetAllSubscriptionPlansPaginatedAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            var planResponses = new List<SubscriptionPlanResponse>();

            foreach (var plan in plans)
            {
                var response = await MapToSubscriptionPlanResponseAsync(plan);
                planResponses.Add(response);
            }

            return PaginationResult<SubscriptionPlanResponse>.Create(planResponses, page, pageSize);
        }

        public async Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Plan ID must be greater than 0", nameof(id));

            return await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(id);
        }

        public async Task<SubscriptionPlanResponse?> GetSubscriptionPlanDetailsByIdAsync(int id)
        {
            var plan = await GetSubscriptionPlanByIdAsync(id);
            if (plan == null) return null;

            return await MapToSubscriptionPlanResponseAsync(plan);
        }

        public async Task<SubscriptionPlan> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check if name is unique
            var isNameUnique = await IsSubscriptionPlanNameUniqueAsync(request.Name);
            if (!isNameUnique)
                throw new ArgumentException($"Subscription plan with name '{request.Name}' already exists.");

            var plan = new SubscriptionPlan
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                MonthlyPrice = request.MonthlyPrice,
                Features = request.Features?.Trim(),
                DurationMonths = request.DurationMonths,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Repository<SubscriptionPlan>().CreateAsync(plan);
            return plan;
        }

        public async Task<SubscriptionPlan> UpdateSubscriptionPlanAsync(int id, UpdateSubscriptionPlanRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var existingPlan = await GetSubscriptionPlanByIdAsync(id);
            if (existingPlan == null)
                throw new InvalidOperationException($"Subscription plan with ID {id} not found.");

            // Check if name is unique (excluding current plan)
            var isNameUnique = await IsSubscriptionPlanNameUniqueAsync(request.Name, id);
            if (!isNameUnique)
                throw new ArgumentException($"Subscription plan with name '{request.Name}' already exists.");

            // Update properties
            existingPlan.Name = request.Name.Trim();
            existingPlan.Description = request.Description?.Trim();
            existingPlan.MonthlyPrice = request.MonthlyPrice;
            existingPlan.Features = request.Features?.Trim();
            existingPlan.DurationMonths = request.DurationMonths;

            await _unitOfWork.Repository<SubscriptionPlan>().UpdateAsync(existingPlan);
            return existingPlan;
        }

        public async Task<bool> DeleteSubscriptionPlanAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Plan ID must be greater than 0", nameof(id));

            // Check if plan can be deleted
            var canDelete = await CanDeleteSubscriptionPlanAsync(id);
            if (!canDelete)
                throw new InvalidOperationException("Cannot delete subscription plan with active subscriptions.");

            var plan = await GetSubscriptionPlanByIdAsync(id);
            if (plan == null)
                return false;

            await _unitOfWork.Repository<SubscriptionPlan>().RemoveAsync(plan);
            return true;
        }

        #endregion

        #region Business Logic Operations

        public async Task<PaginationResult<SubscriptionPlanResponse>> SearchSubscriptionPlansAsync(SubscriptionPlanSearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            var filteredPlans = plans.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                filteredPlans = filteredPlans.Where(p => p.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (request.MinPrice.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.MonthlyPrice >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.MonthlyPrice <= request.MaxPrice.Value);
            }

            if (request.MinDurationMonths.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.DurationMonths >= request.MinDurationMonths.Value);
            }

            if (request.MaxDurationMonths.HasValue)
            {
                filteredPlans = filteredPlans.Where(p => p.DurationMonths <= request.MaxDurationMonths.Value);
            }

            // Apply sorting
            switch (request.SortBy?.ToLower())
            {
                case "monthlyprice":
                    filteredPlans = request.SortOrder?.ToLower() == "desc"
                        ? filteredPlans.OrderByDescending(p => p.MonthlyPrice)
                        : filteredPlans.OrderBy(p => p.MonthlyPrice);
                    break;
                case "createdat":
                    filteredPlans = request.SortOrder?.ToLower() == "desc"
                        ? filteredPlans.OrderByDescending(p => p.CreatedAt)
                        : filteredPlans.OrderBy(p => p.CreatedAt);
                    break;
                case "durationmonths":
                    filteredPlans = request.SortOrder?.ToLower() == "desc"
                        ? filteredPlans.OrderByDescending(p => p.DurationMonths)
                        : filteredPlans.OrderBy(p => p.DurationMonths);
                    break;
                default: // name
                    filteredPlans = request.SortOrder?.ToLower() == "desc"
                        ? filteredPlans.OrderByDescending(p => p.Name)
                        : filteredPlans.OrderBy(p => p.Name);
                    break;
            }

            var plansList = filteredPlans.ToList();
            var planResponses = new List<SubscriptionPlanResponse>();

            foreach (var plan in plansList)
            {
                var response = await MapToSubscriptionPlanResponseAsync(plan);
                
                // Filter by active subscriptions if specified
                if (request.HasActiveSubscriptions.HasValue)
                {
                    var hasActive = response.ActiveSubscriptionsCount > 0;
                    if (request.HasActiveSubscriptions.Value != hasActive)
                        continue;
                }

                planResponses.Add(response);
            }

            return PaginationResult<SubscriptionPlanResponse>.Create(planResponses, request.Page, request.PageSize);
        }

        public async Task<PaginationResult<SubscriptionPlanResponse>> GetSubscriptionPlansByPriceRangeAsync(decimal minPrice, decimal maxPrice, int page = 1, int pageSize = 10)
        {
            if (minPrice < 0 || maxPrice < 0)
                throw new ArgumentException("Prices must be non-negative.");

            if (minPrice > maxPrice)
                throw new ArgumentException("Min price cannot be greater than max price.");

            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            var filteredPlans = plans.Where(p => p.MonthlyPrice >= minPrice && p.MonthlyPrice <= maxPrice);

            var planResponses = new List<SubscriptionPlanResponse>();
            foreach (var plan in filteredPlans)
            {
                var response = await MapToSubscriptionPlanResponseAsync(plan);
                planResponses.Add(response);
            }

            return PaginationResult<SubscriptionPlanResponse>.Create(planResponses, page, pageSize);
        }

        public async Task<PaginationResult<SubscriptionPlanResponse>> GetSubscriptionPlansByDurationAsync(int? durationMonths, int page = 1, int pageSize = 10)
        {
            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            var filteredPlans = plans.Where(p => p.DurationMonths == durationMonths);

            var planResponses = new List<SubscriptionPlanResponse>();
            foreach (var plan in filteredPlans)
            {
                var response = await MapToSubscriptionPlanResponseAsync(plan);
                planResponses.Add(response);
            }

            return PaginationResult<SubscriptionPlanResponse>.Create(planResponses, page, pageSize);
        }

        public async Task<IEnumerable<SubscriptionPlanResponse>> GetPopularSubscriptionPlansAsync(int count = 5)
        {
            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            var planResponses = new List<SubscriptionPlanResponse>();

            foreach (var plan in plans)
            {
                var response = await MapToSubscriptionPlanResponseAsync(plan);
                planResponses.Add(response);
            }

            return planResponses
                .OrderByDescending(p => p.ActiveSubscriptionsCount)
                .Take(count);
        }

        public async Task<IEnumerable<SubscriptionPlanResponse>> GetMostRevenueGeneratingPlansAsync(int count = 5)
        {
            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            var planResponses = new List<SubscriptionPlanResponse>();

            foreach (var plan in plans)
            {
                var response = await MapToSubscriptionPlanResponseAsync(plan);
                planResponses.Add(response);
            }

            return planResponses
                .OrderByDescending(p => p.TotalRevenue)
                .Take(count);
        }

        #endregion

        #region Validation and Business Rules

        public async Task<bool> IsSubscriptionPlanNameUniqueAsync(string name, int? excludePlanId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            return !plans.Any(p => p.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && 
                                  (!excludePlanId.HasValue || p.PlanId != excludePlanId.Value));
        }

        public async Task<bool> CanDeleteSubscriptionPlanAsync(int id)
        {
            var hasActiveSubscriptions = await HasActiveSubscriptionsAsync(id);
            return !hasActiveSubscriptions;
        }

        public async Task<bool> HasActiveSubscriptionsAsync(int planId)
        {
            var subscriptions = await _unitOfWork.Repository<UserSubscription>().GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            return subscriptions.Any(s => s.PlanId == planId && 
                                         s.Status == "active" && 
                                         s.EndDate >= today);
        }

        public async Task<int> GetActiveSubscriptionsCountAsync(int planId)
        {
            var subscriptions = await _unitOfWork.Repository<UserSubscription>().GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            return subscriptions.Count(s => s.PlanId == planId && 
                                           s.Status == "active" && 
                                           s.EndDate >= today);
        }

        public async Task<int> GetTotalSubscriptionsCountAsync(int planId)
        {
            var subscriptions = await _unitOfWork.Repository<UserSubscription>().GetAllAsync();
            return subscriptions.Count(s => s.PlanId == planId);
        }

        public async Task<decimal> GetTotalRevenueByPlanAsync(int planId)
        {
            var plan = await GetSubscriptionPlanByIdAsync(planId);
            if (plan == null) return 0;

            var subscriptions = await _unitOfWork.Repository<UserSubscription>().GetAllAsync();
            var planSubscriptions = subscriptions.Where(s => s.PlanId == planId);

            decimal totalRevenue = 0;
            foreach (var subscription in planSubscriptions)
            {
                // Calculate months based on subscription period
                // Since StartDate and EndDate are DateOnly (not nullable), we can use them directly
                var months = ((subscription.EndDate.Year - subscription.StartDate.Year) * 12) +
                            subscription.EndDate.Month - subscription.StartDate.Month;
                
                // Ensure at least 1 month is counted
                totalRevenue += plan.MonthlyPrice * Math.Max(1, months);
            }

            return totalRevenue;
        }

        #endregion

        #region Statistics and Analytics

        public async Task<decimal> GetTotalRevenueFromAllPlansAsync()
        {
            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            decimal totalRevenue = 0;

            foreach (var plan in plans)
            {
                totalRevenue += await GetTotalRevenueByPlanAsync(plan.PlanId);
            }

            return totalRevenue;
        }

        public async Task<int> GetTotalActiveSubscriptionsAsync()
        {
            var subscriptions = await _unitOfWork.Repository<UserSubscription>().GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            return subscriptions.Count(s => s.Status == "active" && s.EndDate >= today);
        }

        public async Task<IEnumerable<SubscriptionPlanResponse>> GetPlansWithMostSubscribersAsync(int count = 5)
        {
            return await GetPopularSubscriptionPlansAsync(count);
        }

        public async Task<decimal> GetAverageSubscriptionPriceAsync()
        {
            var plans = await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync();
            if (!plans.Any()) return 0;

            return plans.Average(p => p.MonthlyPrice);
        }

        #endregion

        #region Helper Methods

        private async Task<SubscriptionPlanResponse> MapToSubscriptionPlanResponseAsync(SubscriptionPlan plan)
        {
            var activeSubscriptionsCount = await GetActiveSubscriptionsCountAsync(plan.PlanId);
            var totalSubscriptionsCount = await GetTotalSubscriptionsCountAsync(plan.PlanId);
            var totalRevenue = await GetTotalRevenueByPlanAsync(plan.PlanId);

            return new SubscriptionPlanResponse
            {
                PlanId = plan.PlanId,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                Features = plan.Features,
                DurationMonths = plan.DurationMonths,
                CreatedAt = plan.CreatedAt,
                ActiveSubscriptionsCount = activeSubscriptionsCount,
                TotalSubscriptionsCount = totalSubscriptionsCount,
                TotalRevenue = totalRevenue
            };
        }

        #endregion
    }
}