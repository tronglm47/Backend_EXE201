using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.RoommateMatch;

namespace Services.Services
{
    public class RoommateMatchService : IRoommateMatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoommateMatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD Operations
        public async Task<IEnumerable<RoommateMatch>> GetAllRoommateMatchesAsync()
        {
            return await _unitOfWork.RoommateMatches.GetAllAsync();
        }

        public async Task<PaginationResult<RoommateMatchResponse>> GetAllRoommateMatchesPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var roommateMatches = await _unitOfWork.RoommateMatches.GetAllAsync();
            var roommateMatchResponses = roommateMatches.Select(MapToRoommateMatchResponse);
            
            return PaginationResult<RoommateMatchResponse>.Create(roommateMatchResponses, page, pageSize);
        }

        public async Task<RoommateMatch?> GetRoommateMatchByIdAsync(int id)
        {
            return await _unitOfWork.RoommateMatches.GetByIdAsync(id);
        }

        public async Task<RoommateMatch> CreateRoommateMatchAsync(CreateRoommateMatchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var roommateMatch = new RoommateMatch
            {
                UserId1 = request.UserId1,
                UserId2 = request.UserId2,
                Score = request.Score,
                Status = request.Status,
                MatchedAt = DateTime.Now
            };

            await _unitOfWork.RoommateMatches.CreateAsync(roommateMatch);
            return roommateMatch;
        }

        public async Task<RoommateMatch> UpdateRoommateMatchAsync(int id, UpdateRoommateMatchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var roommateMatch = await _unitOfWork.RoommateMatches.GetByIdAsync(id);
            if (roommateMatch == null)
                throw new KeyNotFoundException($"RoommateMatch with ID {id} not found");

            // Update properties
            if (request.Score.HasValue)
                roommateMatch.Score = request.Score.Value;
            
            if (!string.IsNullOrEmpty(request.Status))
                roommateMatch.Status = request.Status;

            await _unitOfWork.RoommateMatches.UpdateAsync(roommateMatch);
            return roommateMatch;
        }

        public async Task<bool> DeleteRoommateMatchAsync(int id)
        {
            var roommateMatch = await _unitOfWork.RoommateMatches.GetByIdAsync(id);
            if (roommateMatch == null)
                return false;

            await _unitOfWork.RoommateMatches.RemoveAsync(roommateMatch);
            return true;
        }

        // Business Operations with Pagination
        public async Task<PaginationResult<RoommateMatchResponse>> GetRoommateMatchesByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            var roommateMatches = await _unitOfWork.RoommateMatches.GetAllAsync();
            var filteredMatches = roommateMatches.Where(rm => rm.UserId1 == userId || rm.UserId2 == userId);
            var roommateMatchResponses = filteredMatches.Select(MapToRoommateMatchResponse);
            
            return PaginationResult<RoommateMatchResponse>.Create(roommateMatchResponses, page, pageSize);
        }

        public async Task<PaginationResult<RoommateMatchResponse>> GetRoommateMatchesByStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            var roommateMatches = await _unitOfWork.RoommateMatches.GetAllAsync();
            var filteredMatches = roommateMatches.Where(rm => rm.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            var roommateMatchResponses = filteredMatches.Select(MapToRoommateMatchResponse);
            
            return PaginationResult<RoommateMatchResponse>.Create(roommateMatchResponses, page, pageSize);
        }

        public async Task<PaginationResult<RoommateMatchResponse>> SearchRoommateMatchesAsync(RoommateMatchSearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var roommateMatches = await _unitOfWork.RoommateMatches.GetAllAsync();
            var query = roommateMatches.AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(rm => rm.UserId1 == request.UserId.Value || rm.UserId2 == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(rm => rm.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.MinScore.HasValue)
                query = query.Where(rm => rm.Score >= request.MinScore.Value);

            if (request.MaxScore.HasValue)
                query = query.Where(rm => rm.Score <= request.MaxScore.Value);

            var filteredMatches = query.AsEnumerable();
            var roommateMatchResponses = filteredMatches.Select(MapToRoommateMatchResponse);
            
            return PaginationResult<RoommateMatchResponse>.Create(roommateMatchResponses, request.Page, request.PageSize);
        }

        // Helper method to map Entity → DTO
        private RoommateMatchResponse MapToRoommateMatchResponse(RoommateMatch roommateMatch)
        {
            return new RoommateMatchResponse
            {
                MatchId = roommateMatch.MatchId,
                UserId1 = roommateMatch.UserId1,
                UserId2 = roommateMatch.UserId2,
                Score = roommateMatch.Score,
                Status = roommateMatch.Status ?? string.Empty,
                MatchedAt = roommateMatch.MatchedAt,
                // Note: User1Name and User2Name can be populated via additional queries if needed
                User1Name = null,
                User2Name = null
            };
        }
    }
}