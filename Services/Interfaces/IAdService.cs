using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Ad;

namespace Services.Interfaces
{
    public interface IAdService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<Ad>> GetAllAdsAsync();
        Task<PaginationResult<AdResponse>> GetAllAdsPaginatedAsync(int page = 1, int pageSize = 10);
        Task<Ad?> GetAdByIdAsync(int id);
        Task<Ad> CreateAdAsync(CreateAdRequest request);
        Task<Ad> UpdateAdAsync(int id, UpdateAdRequest request);
        Task<bool> DeleteAdAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<AdResponse>> GetAdsByUserAsync(int userId, int page = 1, int pageSize = 10);
        Task<PaginationResult<AdResponse>> GetAdsByRequestAsync(int requestId, int page = 1, int pageSize = 10);
        Task<PaginationResult<AdResponse>> GetAdsByStatusAsync(string status, int page = 1, int pageSize = 10);
        Task<PaginationResult<AdResponse>> SearchAdsAsync(AdSearchRequest request);
        
        // Complex Operations with Transaction
        Task<bool> UpdateAdStatusAsync(int adId, UpdateAdStatusRequest request);
        Task<bool> IncrementAdViewsAsync(int adId);
        Task<bool> IncrementAdClicksAsync(int adId);
        Task<PaginationResult<AdResponse>> GetActiveAdsAsync(int page = 1, int pageSize = 10);
        Task<PaginationResult<AdResponse>> GetExpiredAdsAsync(int page = 1, int pageSize = 10);
    }
}