using Services.RequestsResponses.Basic;
using Services.RequestsResponses.RoommatePreference;
using VLivingAPI.Repositories.Data.Models;

namespace Services.Interfaces
{
    public interface IRoommatePreferenceService
    {
        // Basic CRUD Operations with Pagination
        Task<IEnumerable<RoommatePreference>> GetAllRoommatePreferencesAsync();
        Task<PaginationResult<RoommatePreferenceResponse>> GetAllRoommatePreferencesPaginatedAsync(int page = 1, int pageSize = 10);
        Task<RoommatePreference?> GetRoommatePreferenceByIdAsync(int id);
        Task<RoommatePreference> CreateRoommatePreferenceAsync(CreateRoommatePreferenceRequest request);
        Task<RoommatePreference> UpdateRoommatePreferenceAsync(int id, UpdateRoommatePreferenceRequest request);
        Task<bool> DeleteRoommatePreferenceAsync(int id);

        // Business Operations
        Task<RoommatePreference?> GetRoommatePreferenceByUserIdAsync(int userId);
        Task<PaginationResult<RoommatePreferenceResponse>> GetRoommatePreferencesByLocationAsync(int locationId, int page = 1, int pageSize = 10);
    }
}