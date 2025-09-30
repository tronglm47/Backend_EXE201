using AutoMapper;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Services.RequestsResponses;
using Services.RequestsResponses.Amenity;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface IAmenityService
    {
        Task<PagedResponse<object>> GetAllAsync(AmenityQueryParameters queryParams);
        Task<object?> GetById(int id, List<string> selectedFields);
        Task<int> Create(AmenityRequest.CreateAmenity item);
        Task<bool> Delete(int id);
        Task<bool> Update(AmenityRequest.UpdateAmenity item, int id);
    }
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AmenityFieldResponse _fieldResponse;

        private readonly IMapper _mapper;
        private readonly ILogger<AmenityService> _logger;
        public AmenityService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AmenityService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new AmenityFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<object>> GetAllAsync(AmenityQueryParameters queryParams)
        {
            var totalItems = await _unitOfWork.Amenities.CountWithSearch(queryParams.Search);
            var objects = await _unitOfWork.Amenities.GetAmenitiesWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "AmenityId",
                queryParams.IsDescending);

            var selectedFields = queryParams.GetSelectFields();

            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = objects.Select(c =>
                {
                    var objectResponse = _mapper.Map<AmenityResponse.GetALlResponse>(c);

                    return _fieldResponse.SelectFields(objectResponse, selectedFields);
                }).ToList()
            };

            return response;
        }

        public async Task<object?> GetById(int id, List<string> selectedFields)
        {
            var item = await _unitOfWork.Amenities.GetByIdAsync(id);
            if (item == null || item.AmenityId == 0)
            {
                return null;
            }

            var itemResponse = _mapper.Map<AmenityResponse.GetByIdResponse>(item);
            return _fieldResponse.SelectFields(item, selectedFields);
        }
        #region CUD Operations
        // Implement Create, Update, Delete methods if needed

        public async Task<int> Create(AmenityRequest.CreateAmenity item)
        {
            if (item == null)
            {
                return 0;
            }
            var amenity = _mapper.Map<Amenity>(item);
            var result = await _unitOfWork.Amenities.CreateAsync(amenity);
            await _unitOfWork.SaveChangesAsync();
            return result > 0 ? amenity.AmenityId : 0;
        }
        public async Task<bool> Delete(int id)
        {
            var item = await _unitOfWork.Amenities.GetByIdAsync(id);
            if (item == null || item.AmenityId == 0)
            {
                return false;
            }
            return await _unitOfWork.Amenities.RemoveAsync(item);
        }
        public async Task<bool> Update(AmenityRequest.UpdateAmenity item, int id)
        {
            if (item == null)
            {
                return false;
            }
            var amenity = await _unitOfWork.Amenities.GetByIdAsync(id);
            if (amenity == null)
            {
                return false;
            }
            amenity.Name = item.Name;
            amenity.Description = item.Description;
            if (await _unitOfWork.Amenities.UpdateAsync(amenity) > 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
