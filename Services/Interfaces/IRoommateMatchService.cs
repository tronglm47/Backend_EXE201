using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.RoommateMatch;

namespace Services.Interfaces
{
    public interface IRoommateMatchService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<RoommateMatch>> GetAllRoommateMatchesAsync();
        Task<PaginationResult<RoommateMatchResponse>> GetAllRoommateMatchesPaginatedAsync(int page = 1, int pageSize = 10);
        Task<RoommateMatch?> GetRoommateMatchByIdAsync(int id);
        Task<RoommateMatch> CreateRoommateMatchAsync(CreateRoommateMatchRequest request);
        Task<RoommateMatch> UpdateRoommateMatchAsync(int id, UpdateRoommateMatchRequest request);
        Task<bool> DeleteRoommateMatchAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<RoommateMatchResponse>> GetRoommateMatchesByUserAsync(int userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<RoommateMatchResponse>> GetRoommateMatchesByStatusAsync(string status, int page = 1, int pageSize = 10);
        Task<PaginationResult<RoommateMatchResponse>> SearchRoommateMatchesAsync(RoommateMatchSearchRequest request);
    }
}