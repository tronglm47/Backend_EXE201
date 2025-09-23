using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.AdRequest;

namespace Services.Interfaces
{
    public interface IAdRequestService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<AdRequest>> GetAllAdRequestsAsync();
        Task<PaginationResult<AdRequestResponse>> GetAllAdRequestsPaginatedAsync(int page = 1, int pageSize = 10);
        Task<AdRequest?> GetAdRequestByIdAsync(int id);
        Task<AdRequest> CreateAdRequestAsync(CreateAdRequestRequest request);
        Task<AdRequest> UpdateAdRequestAsync(int id, UpdateAdRequestRequest request);
        Task<bool> DeleteAdRequestAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<AdRequestResponse>> GetAdRequestsByUserAsync(int userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<AdRequestResponse>> GetAdRequestsByStatusAsync(string status, int page = 1, int pageSize = 10);
        Task<PaginationResult<AdRequestResponse>> SearchAdRequestsAsync(AdRequestSearchRequest request);
        
        // Complex Operations
        Task<bool> UpdateAdRequestStatusAsync(int requestId, string status);
        Task<bool> ApproveAdRequestAsync(int requestId);
        Task<bool> RejectAdRequestAsync(int requestId);
    }
}