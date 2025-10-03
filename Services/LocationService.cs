using Services.RequestsResponses.Amenity;
using Services.RequestsResponses;
using Services.RequestsResponses.Location;
using Repositories.Basic;
using Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
using VLivingAPI.Repositories.Data.Models;

namespace Services
{
    public interface ILocationService
    {
        Task<PagedResponse<object>> GetAllAsync(LocationQueryParameters queryParams);
        Task<object?> GetById(int id, List<string> selectedFields);
        Task<int> Create(LocationRequest.LocationCreate item);
        Task<bool> Delete(int id);
        Task<bool> Update(LocationRequest.LocationUpdate item, int id);
    }
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly LocationFieldResponse _fieldResponse;
        private readonly IMapper _mapper;
        private readonly ILogger<LocationService> _logger;
        public LocationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LocationService> logger)
        {
            _unitOfWork = unitOfWork;
            _fieldResponse = new LocationFieldResponse();
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<PagedResponse<object>> GetAllAsync(LocationQueryParameters queryParams)
        {
            var totalItems = await _unitOfWork.Locations.CountWithSearch(queryParams.Search, queryParams.MinParentLocationId);
            var objects = await _unitOfWork.Locations.GetLocationsWithAdvancedQuery(
                queryParams.Search,
                queryParams.Page,
                queryParams.PageSize,
                queryParams.SortBy ?? "LocationId",
                queryParams.IsDescending,
                queryParams.MinParentLocationId);
            var selectedFields = queryParams.GetSelectFields();
            var response = new PagedResponse<object>
            {
                CurrentPage = queryParams.Page,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)queryParams.PageSize),
                Items = objects.Select(c =>
                {
                    var objectResponse = _mapper.Map<LocationResponse.LocationGetAll>(c);
                    return _fieldResponse.SelectFields(objectResponse, selectedFields);
                }).ToList()
            };
            return response;
        }
        public async Task<object?> GetById(int id, List<string> selectedFields)
        {
            var locationRepository = _unitOfWork.Locations as LocationRepository;
            if (locationRepository == null)
            {
                _logger.LogError("LocationRepository not available");
                return null;
            }

            var hierarchy = await locationRepository.GetLocationHierarchyAsync(id);
            if (hierarchy == null || !hierarchy.Any())
            {
                _logger.LogWarning($"Location with ID {id} not found.");
                return null;
            }

            var hierarchyResponse = new LocationResponse.LocationHierarchy
            {
                Hierarchy = hierarchy.Select(location => _mapper.Map<LocationResponse.LocationInfo>(location)).ToList()
            };

            return _fieldResponse.SelectFields(hierarchyResponse, selectedFields);
        }
        #region CUD Operations
        public async Task<int> Create(LocationRequest.LocationCreate item)
        {
            var location = _mapper.Map<Location>(item);
            location.CreatedAt = DateTime.UtcNow;
            location.IsActive = true;
            await _unitOfWork.Locations.CreateAsync(location);
            await _unitOfWork.SaveChangesAsync();
            return location.LocationId;
        }
        public async Task<bool> Delete(int id)
        {
            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null)
            {
                _logger.LogWarning($"Location with ID {id} not found for deletion.");
                return false;
            }
            _unitOfWork.Locations.Remove(location);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Update(LocationRequest.LocationUpdate item, int id)
        {
            var existingLocation = await _unitOfWork.Locations.GetByIdAsync(id);
            if (existingLocation == null)
            {
                _logger.LogWarning($"Location with ID {id} not found for update.");
                return false;
            }
            _mapper.Map(item, existingLocation);
            _unitOfWork.Locations.Update(existingLocation);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
