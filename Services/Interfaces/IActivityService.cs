using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Activity;

namespace Services.Interfaces
{
    public interface IActivityService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<Activity>> GetAllActivitiesAsync();
        Task<PaginationResult<ActivityResponse>> GetAllActivitiesPaginatedAsync(int page = 1, int pageSize = 10);
        Task<Activity?> GetActivityByIdAsync(int id);
        Task<Activity> CreateActivityAsync(CreateActivityRequest request);
        Task<Activity> UpdateActivityAsync(int id, UpdateActivityRequest request);
        Task<bool> DeleteActivityAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<ActivityResponse>> GetActivitiesByCreatorAsync(int creatorId, int page = 1, int pageSize = 10);
        Task<PaginationResult<ActivityResponse>> GetActivitiesByLocationAsync(int locationId, int page = 1, int pageSize = 10);
        Task<PaginationResult<ActivityResponse>> GetActivitiesByStatusAsync(string status, int page = 1, int pageSize = 10);
        Task<PaginationResult<ActivityResponse>> SearchActivitiesAsync(ActivitySearchRequest request);
        
        // Complex Operations
        Task<bool> UpdateActivityStatusAsync(int activityId, UpdateActivityStatusRequest request);
        Task<PaginationResult<ActivityResponse>> GetUpcomingActivitiesAsync(int page = 1, int pageSize = 10);
        Task<PaginationResult<ActivityResponse>> GetPastActivitiesAsync(int page = 1, int pageSize = 10);
    }
}