using Services.RequestsResponses.Basic;
using Services.RequestsResponses.SubscriptionPlan;
using VLivingAPI.Repositories.Data.Models;

namespace Services.Interfaces
{
    public interface ISubscriptionPlanService
    {
        // Basic CRUD Operations
        Task<IEnumerable<SubscriptionPlan>> GetAllSubscriptionPlansAsync();
        Task<PaginationResult<SubscriptionPlanResponse>> GetAllSubscriptionPlansPaginatedAsync(int page = 1, int pageSize = 10);
        Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(int id);
        Task<SubscriptionPlanResponse?> GetSubscriptionPlanDetailsByIdAsync(int id);
        Task<SubscriptionPlan> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request);
        Task<SubscriptionPlan> UpdateSubscriptionPlanAsync(int id, UpdateSubscriptionPlanRequest request);
        Task<bool> DeleteSubscriptionPlanAsync(int id);

        // Business Logic Operations
        Task<PaginationResult<SubscriptionPlanResponse>> SearchSubscriptionPlansAsync(SubscriptionPlanSearchRequest request);
        Task<PaginationResult<SubscriptionPlanResponse>> GetSubscriptionPlansByPriceRangeAsync(decimal minPrice, decimal maxPrice, int page = 1, int pageSize = 10);
        Task<PaginationResult<SubscriptionPlanResponse>> GetSubscriptionPlansByDurationAsync(int? durationMonths, int page = 1, int pageSize = 10);
        Task<IEnumerable<SubscriptionPlanResponse>> GetPopularSubscriptionPlansAsync(int count = 5);
        Task<IEnumerable<SubscriptionPlanResponse>> GetMostRevenueGeneratingPlansAsync(int count = 5);

        // Validation and Business Rules
        Task<bool> IsSubscriptionPlanNameUniqueAsync(string name, int? excludePlanId = null);
        Task<bool> CanDeleteSubscriptionPlanAsync(int id);
        Task<bool> HasActiveSubscriptionsAsync(int planId);
        Task<int> GetActiveSubscriptionsCountAsync(int planId);
        Task<int> GetTotalSubscriptionsCountAsync(int planId);
        Task<decimal> GetTotalRevenueByPlanAsync(int planId);

        // Statistics and Analytics
        Task<decimal> GetTotalRevenueFromAllPlansAsync();
        Task<int> GetTotalActiveSubscriptionsAsync();
        Task<IEnumerable<SubscriptionPlanResponse>> GetPlansWithMostSubscribersAsync(int count = 5);
        Task<decimal> GetAverageSubscriptionPriceAsync();
    }
}