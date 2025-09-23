using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Activity;

namespace Services.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ActivityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD Operations
        public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
        {
            return await _unitOfWork.Repository<Activity>().GetAllAsync();
        }

        public async Task<PaginationResult<ActivityResponse>> GetAllActivitiesPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var activities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var activityResponses = activities.Select(MapToActivityResponse);
            
            return PaginationResult<ActivityResponse>.Create(activityResponses, page, pageSize);
        }

        public async Task<Activity?> GetActivityByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Activity>().GetByIdAsync(id);
        }

        public async Task<Activity> CreateActivityAsync(CreateActivityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var activity = new Activity
            {
                CreatorId = request.CreatorId,
                Title = request.Title,
                Description = request.Description,
                LocationId = request.LocationId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.Now,
                Status = "active"
            };

            await _unitOfWork.Repository<Activity>().CreateAsync(activity);
            return activity;
        }

        public async Task<Activity> UpdateActivityAsync(int id, UpdateActivityRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var existingActivity = await _unitOfWork.Repository<Activity>().GetByIdAsync(id);
            if (existingActivity == null)
                throw new KeyNotFoundException($"Activity with ID {id} not found");

            // Update properties
            existingActivity.Title = request.Title;
            existingActivity.Description = request.Description;
            existingActivity.LocationId = request.LocationId;
            existingActivity.StartTime = request.StartTime;
            existingActivity.EndTime = request.EndTime;
            existingActivity.Status = request.Status ?? existingActivity.Status;

            await _unitOfWork.Repository<Activity>().UpdateAsync(existingActivity);
            return existingActivity;
        }

        public async Task<bool> DeleteActivityAsync(int id)
        {
            var activity = await _unitOfWork.Repository<Activity>().GetByIdAsync(id);
            if (activity == null)
                return false;

            await _unitOfWork.Repository<Activity>().RemoveAsync(activity);
            return true;
        }

        // Business Operations with Pagination
        public async Task<PaginationResult<ActivityResponse>> GetActivitiesByCreatorAsync(int creatorId, int page = 1, int pageSize = 10)
        {
            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var creatorActivities = allActivities.Where(a => a.CreatorId == creatorId);
            var activityResponses = creatorActivities.Select(MapToActivityResponse);
            
            return PaginationResult<ActivityResponse>.Create(activityResponses, page, pageSize);
        }

        public async Task<PaginationResult<ActivityResponse>> GetActivitiesByLocationAsync(int locationId, int page = 1, int pageSize = 10)
        {
            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var locationActivities = allActivities.Where(a => a.LocationId == locationId);
            var activityResponses = locationActivities.Select(MapToActivityResponse);
            
            return PaginationResult<ActivityResponse>.Create(activityResponses, page, pageSize);
        }

        public async Task<PaginationResult<ActivityResponse>> GetActivitiesByStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var statusActivities = allActivities.Where(a => a.Status == status);
            var activityResponses = statusActivities.Select(MapToActivityResponse);
            
            return PaginationResult<ActivityResponse>.Create(activityResponses, page, pageSize);
        }

        public async Task<PaginationResult<ActivityResponse>> SearchActivitiesAsync(ActivitySearchRequest request)
        {
            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var query = allActivities.AsEnumerable();

            // Apply filters
            if (request.CreatorId.HasValue)
                query = query.Where(a => a.CreatorId == request.CreatorId.Value);

            if (request.LocationId.HasValue)
                query = query.Where(a => a.LocationId == request.LocationId.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(a => a.Status == request.Status);

            if (request.StartDate.HasValue)
                query = query.Where(a => a.StartTime >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.EndTime <= request.EndDate.Value);

            if (!string.IsNullOrEmpty(request.Title))
                query = query.Where(a => a.Title.Contains(request.Title, StringComparison.OrdinalIgnoreCase));

            var activityResponses = query.Select(MapToActivityResponse);
            return PaginationResult<ActivityResponse>.Create(activityResponses, request.Page, request.PageSize);
        }

        public async Task<bool> UpdateActivityStatusAsync(int activityId, UpdateActivityStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
                return false;

            try
            {
                var activity = await _unitOfWork.Repository<Activity>().GetByIdAsync(activityId);
                if (activity == null)
                    return false;

                activity.Status = request.Status;
                await _unitOfWork.Repository<Activity>().UpdateAsync(activity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PaginationResult<ActivityResponse>> GetUpcomingActivitiesAsync(int page = 1, int pageSize = 10)
        {
            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var upcomingActivities = allActivities.Where(a => 
                a.StartTime.HasValue && a.StartTime.Value > DateTime.Now);
            var activityResponses = upcomingActivities.Select(MapToActivityResponse);
            
            return PaginationResult<ActivityResponse>.Create(activityResponses, page, pageSize);
        }

        public async Task<PaginationResult<ActivityResponse>> GetPastActivitiesAsync(int page = 1, int pageSize = 10)
        {
            var allActivities = await _unitOfWork.Repository<Activity>().GetAllAsync();
            var pastActivities = allActivities.Where(a => 
                a.EndTime.HasValue && a.EndTime.Value < DateTime.Now);
            var activityResponses = pastActivities.Select(MapToActivityResponse);
            
            return PaginationResult<ActivityResponse>.Create(activityResponses, page, pageSize);
        }

        // Helper method to map Activity to ActivityResponse
        private ActivityResponse MapToActivityResponse(Activity activity)
        {
            return new ActivityResponse
            {
                ActivityId = activity.ActivityId,
                CreatorId = activity.CreatorId,
                Title = activity.Title,
                Description = activity.Description,
                LocationId = activity.LocationId,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                Status = activity.Status,
                CreatedAt = activity.CreatedAt
                // TODO: Can add CreatorName and LocationName by joining with related entities
            };
        }
    }
}