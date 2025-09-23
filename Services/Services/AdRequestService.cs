using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.AdRequest;

namespace Services.Services
{
    public class AdRequestService : IAdRequestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD Operations
        public async Task<IEnumerable<AdRequest>> GetAllAdRequestsAsync()
        {
            return await _unitOfWork.Repository<AdRequest>().GetAllAsync();
        }

        public async Task<PaginationResult<AdRequestResponse>> GetAllAdRequestsPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var adRequests = await _unitOfWork.Repository<AdRequest>().GetAllAsync();
            var adRequestResponses = adRequests.Select(MapToAdRequestResponse);
            
            return PaginationResult<AdRequestResponse>.Create(adRequestResponses, page, pageSize);
        }

        public async Task<AdRequest?> GetAdRequestByIdAsync(int id)
        {
            return await _unitOfWork.Repository<AdRequest>().GetByIdAsync(id);
        }

        public async Task<AdRequest> CreateAdRequestAsync(CreateAdRequestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var adRequest = new AdRequest
            {
                UserId = request.UserId,
                CompanyName = request.CompanyName,
                AdContent = request.AdContent,
                TargetAudience = request.TargetAudience,
                Budget = request.Budget,
                DurationDays = request.DurationDays,
                Status = "pending",
                SubmittedAt = DateTime.Now
            };

            await _unitOfWork.Repository<AdRequest>().CreateAsync(adRequest);
            return adRequest;
        }

        public async Task<AdRequest> UpdateAdRequestAsync(int id, UpdateAdRequestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var existingAdRequest = await _unitOfWork.Repository<AdRequest>().GetByIdAsync(id);
            if (existingAdRequest == null)
                throw new KeyNotFoundException($"AdRequest with ID {id} not found");

            // Update properties only if they are provided
            if (!string.IsNullOrEmpty(request.CompanyName))
                existingAdRequest.CompanyName = request.CompanyName;
            
            if (!string.IsNullOrEmpty(request.AdContent))
                existingAdRequest.AdContent = request.AdContent;
            
            if (request.TargetAudience != null)
                existingAdRequest.TargetAudience = request.TargetAudience;
            
            if (request.Budget.HasValue)
                existingAdRequest.Budget = request.Budget;
            
            if (request.DurationDays.HasValue)
                existingAdRequest.DurationDays = request.DurationDays;

            await _unitOfWork.Repository<AdRequest>().UpdateAsync(existingAdRequest);
            return existingAdRequest;
        }

        public async Task<bool> DeleteAdRequestAsync(int id)
        {
            var adRequest = await _unitOfWork.Repository<AdRequest>().GetByIdAsync(id);
            if (adRequest == null)
                return false;

            await _unitOfWork.Repository<AdRequest>().RemoveAsync(adRequest);
            return true;
        }

        // Business Operations with Pagination
        public async Task<PaginationResult<AdRequestResponse>> GetAdRequestsByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            var adRequests = await _unitOfWork.Repository<AdRequest>().GetAllAsync();
            var filteredAdRequests = adRequests.Where(ar => ar.UserId == userId);
            var adRequestResponses = filteredAdRequests.Select(MapToAdRequestResponse);
            
            return PaginationResult<AdRequestResponse>.Create(adRequestResponses, page, pageSize);
        }

        public async Task<PaginationResult<AdRequestResponse>> GetAdRequestsByStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            var adRequests = await _unitOfWork.Repository<AdRequest>().GetAllAsync();
            var filteredAdRequests = adRequests.Where(ar => ar.Status!.Equals(status, StringComparison.OrdinalIgnoreCase));
            var adRequestResponses = filteredAdRequests.Select(MapToAdRequestResponse);
            
            return PaginationResult<AdRequestResponse>.Create(adRequestResponses, page, pageSize);
        }

        public async Task<PaginationResult<AdRequestResponse>> SearchAdRequestsAsync(AdRequestSearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var adRequests = await _unitOfWork.Repository<AdRequest>().GetAllAsync();
            var query = adRequests.AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(ar => ar.UserId == request.UserId.Value);

            if (!string.IsNullOrEmpty(request.CompanyName))
                query = query.Where(ar => ar.CompanyName.Contains(request.CompanyName, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(ar => ar.Status!.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

            if (request.MinBudget.HasValue)
                query = query.Where(ar => ar.Budget >= request.MinBudget.Value);

            if (request.MaxBudget.HasValue)
                query = query.Where(ar => ar.Budget <= request.MaxBudget.Value);

            if (request.SubmittedFrom.HasValue)
                query = query.Where(ar => ar.SubmittedAt >= request.SubmittedFrom.Value);

            if (request.SubmittedTo.HasValue)
                query = query.Where(ar => ar.SubmittedAt <= request.SubmittedTo.Value);

            var filteredAdRequests = query;
            var adRequestResponses = filteredAdRequests.Select(MapToAdRequestResponse);
            
            return PaginationResult<AdRequestResponse>.Create(adRequestResponses, request.Page, request.PageSize);
        }

        // Complex Operations
        public async Task<bool> UpdateAdRequestStatusAsync(int requestId, string status)
        {
            if (string.IsNullOrEmpty(status))
                return false;

            try
            {
                var adRequest = await _unitOfWork.Repository<AdRequest>().GetByIdAsync(requestId);
                if (adRequest == null)
                    return false;

                adRequest.Status = status;
                if (status.Equals("approved", StringComparison.OrdinalIgnoreCase))
                {
                    adRequest.ApprovedAt = DateTime.Now;
                }

                await _unitOfWork.Repository<AdRequest>().UpdateAsync(adRequest);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveAdRequestAsync(int requestId)
        {
            return await UpdateAdRequestStatusAsync(requestId, "approved");
        }

        public async Task<bool> RejectAdRequestAsync(int requestId)
        {
            return await UpdateAdRequestStatusAsync(requestId, "rejected");
        }

        // Helper method to map AdRequest to AdRequestResponse
        private AdRequestResponse MapToAdRequestResponse(AdRequest adRequest)
        {
            return new AdRequestResponse
            {
                RequestId = adRequest.RequestId,
                UserId = adRequest.UserId,
                CompanyName = adRequest.CompanyName,
                AdContent = adRequest.AdContent,
                TargetAudience = adRequest.TargetAudience,
                Budget = adRequest.Budget,
                DurationDays = adRequest.DurationDays,
                Status = adRequest.Status,
                SubmittedAt = adRequest.SubmittedAt,
                ApprovedAt = adRequest.ApprovedAt
                // TODO: Can add UserName and UserEmail by joining with related entities
            };
        }
    }
}