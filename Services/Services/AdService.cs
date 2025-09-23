using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Ad;

namespace Services.Services
{
    public class AdService : IAdService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD Operations
        public async Task<IEnumerable<Ad>> GetAllAdsAsync()
        {
            return await _unitOfWork.Repository<Ad>().GetAllAsync();
        }

        public async Task<PaginationResult<AdResponse>> GetAllAdsPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var ads = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var adResponses = ads.Select(MapToAdResponse);
            
            return PaginationResult<AdResponse>.Create(adResponses, page, pageSize);
        }

        public async Task<Ad?> GetAdByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Ad>().GetByIdAsync(id);
        }

        public async Task<Ad> CreateAdAsync(CreateAdRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var ad = new Ad
            {
                RequestId = request.RequestId,
                UserId = request.UserId,
                Title = request.Title,
                Content = request.Content,
                ImageUrl = request.ImageUrl,
                LinkUrl = request.LinkUrl,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Views = 0,
                Clicks = 0,
                Status = request.Status,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Repository<Ad>().CreateAsync(ad);
            return ad;
        }

        public async Task<Ad> UpdateAdAsync(int id, UpdateAdRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var existingAd = await _unitOfWork.Repository<Ad>().GetByIdAsync(id);
            if (existingAd == null)
                throw new KeyNotFoundException($"Ad with ID {id} not found");

            // Update only non-null properties
            if (!string.IsNullOrEmpty(request.Title))
                existingAd.Title = request.Title;
            
            if (!string.IsNullOrEmpty(request.Content))
                existingAd.Content = request.Content;
            
            if (request.ImageUrl != null)
                existingAd.ImageUrl = request.ImageUrl;
            
            if (request.LinkUrl != null)
                existingAd.LinkUrl = request.LinkUrl;
            
            if (request.StartDate.HasValue)
                existingAd.StartDate = request.StartDate;
            
            if (request.EndDate.HasValue)
                existingAd.EndDate = request.EndDate;
            
            if (!string.IsNullOrEmpty(request.Status))
                existingAd.Status = request.Status;

            await _unitOfWork.Repository<Ad>().UpdateAsync(existingAd);
            return existingAd;
        }

        public async Task<bool> DeleteAdAsync(int id)
        {
            var ad = await _unitOfWork.Repository<Ad>().GetByIdAsync(id);
            if (ad == null)
                return false;

            await _unitOfWork.Repository<Ad>().RemoveAsync(ad);
            return true;
        }

        // Business Operations with Pagination
        public async Task<PaginationResult<AdResponse>> GetAdsByUserAsync(int userId, int page = 1, int pageSize = 10)
        {
            var allAds = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var userAds = allAds.Where(a => a.UserId == userId);
            var adResponses = userAds.Select(MapToAdResponse);
            
            return PaginationResult<AdResponse>.Create(adResponses, page, pageSize);
        }

        public async Task<PaginationResult<AdResponse>> GetAdsByRequestAsync(int requestId, int page = 1, int pageSize = 10)
        {
            var allAds = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var requestAds = allAds.Where(a => a.RequestId == requestId);
            var adResponses = requestAds.Select(MapToAdResponse);
            
            return PaginationResult<AdResponse>.Create(adResponses, page, pageSize);
        }

        public async Task<PaginationResult<AdResponse>> GetAdsByStatusAsync(string status, int page = 1, int pageSize = 10)
        {
            var allAds = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var statusAds = allAds.Where(a => a.Status == status);
            var adResponses = statusAds.Select(MapToAdResponse);
            
            return PaginationResult<AdResponse>.Create(adResponses, page, pageSize);
        }

        public async Task<PaginationResult<AdResponse>> SearchAdsAsync(AdSearchRequest request)
        {
            var allAds = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var query = allAds.AsQueryable();

            // Apply filters
            if (request.UserId.HasValue)
                query = query.Where(a => a.UserId == request.UserId.Value);

            if (request.RequestId.HasValue)
                query = query.Where(a => a.RequestId == request.RequestId.Value);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(a => a.Status == request.Status);

            if (!string.IsNullOrEmpty(request.Title))
                query = query.Where(a => a.Title.Contains(request.Title));

            if (request.StartDate.HasValue)
                query = query.Where(a => a.StartDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.EndDate <= request.EndDate.Value);

            if (request.MinViews.HasValue)
                query = query.Where(a => a.Views >= request.MinViews.Value);

            if (request.MaxViews.HasValue)
                query = query.Where(a => a.Views <= request.MaxViews.Value);

            if (request.MinClicks.HasValue)
                query = query.Where(a => a.Clicks >= request.MinClicks.Value);

            if (request.MaxClicks.HasValue)
                query = query.Where(a => a.Clicks <= request.MaxClicks.Value);

            if (request.CreatedAfter.HasValue)
                query = query.Where(a => a.CreatedAt >= request.CreatedAfter.Value);

            if (request.CreatedBefore.HasValue)
                query = query.Where(a => a.CreatedAt <= request.CreatedBefore.Value);

            var adResponses = query.Select(MapToAdResponse);
            return PaginationResult<AdResponse>.Create(adResponses, request.Page, request.PageSize);
        }

        // Complex Operations with Transaction
        public async Task<bool> UpdateAdStatusAsync(int adId, UpdateAdStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
                return false;

            try
            {
                var ad = await _unitOfWork.Repository<Ad>().GetByIdAsync(adId);
                if (ad == null)
                    return false;

                ad.Status = request.Status;
                await _unitOfWork.Repository<Ad>().UpdateAsync(ad);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IncrementAdViewsAsync(int adId)
        {
            try
            {
                var ad = await _unitOfWork.Repository<Ad>().GetByIdAsync(adId);
                if (ad == null)
                    return false;

                ad.Views = (ad.Views ?? 0) + 1;
                await _unitOfWork.Repository<Ad>().UpdateAsync(ad);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IncrementAdClicksAsync(int adId)
        {
            try
            {
                var ad = await _unitOfWork.Repository<Ad>().GetByIdAsync(adId);
                if (ad == null)
                    return false;

                ad.Clicks = (ad.Clicks ?? 0) + 1;
                await _unitOfWork.Repository<Ad>().UpdateAsync(ad);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PaginationResult<AdResponse>> GetActiveAdsAsync(int page = 1, int pageSize = 10)
        {
            var allAds = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            var activeAds = allAds.Where(a => 
                a.Status == "active" && 
                a.StartDate <= today && 
                a.EndDate >= today);
                
            var adResponses = activeAds.Select(MapToAdResponse);
            return PaginationResult<AdResponse>.Create(adResponses, page, pageSize);
        }

        public async Task<PaginationResult<AdResponse>> GetExpiredAdsAsync(int page = 1, int pageSize = 10)
        {
            var allAds = await _unitOfWork.Repository<Ad>().GetAllAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            
            var expiredAds = allAds.Where(a => a.EndDate < today);
                
            var adResponses = expiredAds.Select(MapToAdResponse);
            return PaginationResult<AdResponse>.Create(adResponses, page, pageSize);
        }

        // Helper method to map Ad to AdResponse
        private AdResponse MapToAdResponse(Ad ad)
        {
            return new AdResponse
            {
                AdId = ad.AdId,
                RequestId = ad.RequestId,
                UserId = ad.UserId,
                Title = ad.Title,
                Content = ad.Content,
                ImageUrl = ad.ImageUrl,
                LinkUrl = ad.LinkUrl,
                StartDate = ad.StartDate,
                EndDate = ad.EndDate,
                Views = ad.Views,
                Clicks = ad.Clicks,
                Status = ad.Status,
                CreatedAt = ad.CreatedAt
                // TODO: Can add UserName and CompanyName by joining with related entities
            };
        }
    }
}