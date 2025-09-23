using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Location;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;

namespace Services.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region CRUD Operations

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _unitOfWork.Locations.GetAllAsync();
        }

        public async Task<PaginationResult<LocationResponse>> GetAllLocationsPaginatedAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var locations = await _unitOfWork.Locations.GetAllAsync();
            var locationResponses = locations.Select(MapToLocationResponse);
            
            return PaginationResult<LocationResponse>.Create(locationResponses, page, pageSize);
        }

        public async Task<Location?> GetLocationByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid location ID");

            return await _unitOfWork.Locations.GetByIdAsync(id);
        }

        public async Task<LocationResponse?> GetLocationDetailsByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid location ID");

            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null)
                return null;

            return await MapToLocationResponseWithDetailsAsync(location);
        }

        public async Task<Location> CreateLocationAsync(CreateLocationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate parent location exists if provided
            if (request.ParentLocationId.HasValue)
            {
                var parentLocation = await _unitOfWork.Locations.GetByIdAsync(request.ParentLocationId.Value);
                if (parentLocation == null)
                    throw new ArgumentException("Parent location does not exist");
            }

            var location = new Location
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                ParentLocationId = request.ParentLocationId
            };

            await _unitOfWork.Locations.CreateAsync(location);
            return location;
        }

        public async Task<Location> UpdateLocationAsync(int id, UpdateLocationRequest request)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid location ID");
            
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null)
                throw new ArgumentException("Location not found");

            // Validate parent location exists if provided and different from current
            if (request.ParentLocationId.HasValue)
            {
                if (request.ParentLocationId.Value == id)
                    throw new ArgumentException("Location cannot be its own parent");

                var parentLocation = await _unitOfWork.Locations.GetByIdAsync(request.ParentLocationId.Value);
                if (parentLocation == null)
                    throw new ArgumentException("Parent location does not exist");

                // Check for circular reference
                if (await WouldCreateCircularReferenceAsync(id, request.ParentLocationId.Value))
                    throw new ArgumentException("This update would create a circular reference");
            }

            location.Name = request.Name.Trim();
            location.Description = request.Description?.Trim();
            location.ParentLocationId = request.ParentLocationId;

            await _unitOfWork.Locations.UpdateAsync(location);
            return location;
        }

        public async Task<bool> DeleteLocationAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid location ID");

            var location = await _unitOfWork.Locations.GetByIdAsync(id);
            if (location == null)
                return false;

            // Check if location can be deleted
            if (!await CanDeleteLocationAsync(id))
                throw new InvalidOperationException("Cannot delete location with child locations, properties, or activities");

            return await _unitOfWork.Locations.RemoveAsync(location);
        }

        #endregion

        #region Hierarchical Operations

        public async Task<PaginationResult<LocationResponse>> GetRootLocationsAsync(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            var rootLocations = allLocations.Where(l => l.ParentLocationId == null);
            
            var locationResponses = new List<LocationResponse>();
            foreach (var location in rootLocations)
            {
                locationResponses.Add(await MapToLocationResponseWithDetailsAsync(location));
            }

            return PaginationResult<LocationResponse>.Create(locationResponses, page, pageSize);
        }

        public async Task<PaginationResult<LocationResponse>> GetChildLocationsByParentAsync(int parentId, int page = 1, int pageSize = 10)
        {
            if (parentId <= 0)
                throw new ArgumentException("Invalid parent location ID");
            
            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            var childLocations = allLocations.Where(l => l.ParentLocationId == parentId);
            
            var locationResponses = new List<LocationResponse>();
            foreach (var location in childLocations)
            {
                locationResponses.Add(await MapToLocationResponseWithDetailsAsync(location));
            }

            return PaginationResult<LocationResponse>.Create(locationResponses, page, pageSize);
        }

        public async Task<IEnumerable<LocationResponse>> GetLocationHierarchyAsync(int locationId)
        {
            if (locationId <= 0)
                throw new ArgumentException("Invalid location ID");

            var location = await _unitOfWork.Locations.GetByIdAsync(locationId);
            if (location == null)
                return new List<LocationResponse>();

            var hierarchy = new List<LocationResponse>();
            await BuildLocationHierarchyAsync(location, hierarchy);
            
            return hierarchy;
        }

        public async Task<IEnumerable<LocationResponse>> GetLocationPathToRootAsync(int locationId)
        {
            if (locationId <= 0)
                throw new ArgumentException("Invalid location ID");

            var path = new List<LocationResponse>();
            var currentLocation = await _unitOfWork.Locations.GetByIdAsync(locationId);

            while (currentLocation != null)
            {
                path.Insert(0, await MapToLocationResponseWithDetailsAsync(currentLocation));
                
                if (currentLocation.ParentLocationId.HasValue)
                {
                    currentLocation = await _unitOfWork.Locations.GetByIdAsync(currentLocation.ParentLocationId.Value);
                }
                else
                {
                    break;
                }
            }

            return path;
        }

        #endregion

        #region Search Operations

        public async Task<PaginationResult<LocationResponse>> SearchLocationsAsync(LocationSearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Page < 1 || request.PageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            var query = allLocations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(l => l.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                query = query.Where(l => !string.IsNullOrEmpty(l.Description) && 
                                       l.Description.Contains(request.Description, StringComparison.OrdinalIgnoreCase));
            }

            if (request.ParentLocationId.HasValue)
            {
                query = query.Where(l => l.ParentLocationId == request.ParentLocationId.Value);
            }

            if (request.HasChildren.HasValue)
            {
                if (request.HasChildren.Value)
                {
                    query = query.Where(l => l.InverseParentLocation.Any());
                }
                else
                {
                    query = query.Where(l => !l.InverseParentLocation.Any());
                }
            }

            if (request.HasProperties.HasValue)
            {
                if (request.HasProperties.Value)
                {
                    query = query.Where(l => l.Properties.Any());
                }
                else
                {
                    query = query.Where(l => !l.Properties.Any());
                }
            }

            if (request.HasActivities.HasValue)
            {
                if (request.HasActivities.Value)
                {
                    query = query.Where(l => l.Activities.Any());
                }
                else
                {
                    query = query.Where(l => !l.Activities.Any());
                }
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
            {
                switch (request.SortBy.ToLower())
                {
                    case "name":
                        query = request.SortDescending ? 
                            query.OrderByDescending(l => l.Name) : 
                            query.OrderBy(l => l.Name);
                        break;
                    case "description":
                        query = request.SortDescending ? 
                            query.OrderByDescending(l => l.Description) : 
                            query.OrderBy(l => l.Description);
                        break;
                    default:
                        query = query.OrderBy(l => l.Name);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(l => l.Name);
            }

            var filteredLocations = query.ToList();
            var locationResponses = new List<LocationResponse>();
            
            foreach (var location in filteredLocations)
            {
                locationResponses.Add(await MapToLocationResponseWithDetailsAsync(location));
            }

            return PaginationResult<LocationResponse>.Create(locationResponses, request.Page, request.PageSize);
        }

        public async Task<PaginationResult<LocationResponse>> GetLocationsByNameAsync(string name, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty");

            if (page < 1 || pageSize < 1)
                throw new ArgumentException("Page and PageSize must be greater than 0");

            var searchRequest = new LocationSearchRequest
            {
                Name = name,
                Page = page,
                PageSize = pageSize
            };

            return await SearchLocationsAsync(searchRequest);
        }

        #endregion

        #region Business Operations

        public async Task<bool> HasChildLocationsAsync(int locationId)
        {
            if (locationId <= 0)
                return false;

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            return allLocations.Any(l => l.ParentLocationId == locationId);
        }

        public async Task<bool> HasPropertiesAsync(int locationId)
        {
            if (locationId <= 0)
                return false;

            var allProperties = await _unitOfWork.Properties.GetAllAsync();
            return allProperties.Any(p => p.LocationId == locationId);
        }

        public async Task<bool> HasActivitiesAsync(int locationId)
        {
            if (locationId <= 0)
                return false;

            var allActivities = await _unitOfWork.Activities.GetAllAsync();
            return allActivities.Any(a => a.LocationId == locationId);
        }

        public async Task<bool> CanDeleteLocationAsync(int locationId)
        {
            var hasChildren = await HasChildLocationsAsync(locationId);
            var hasProperties = await HasPropertiesAsync(locationId);
            var hasActivities = await HasActivitiesAsync(locationId);

            return !hasChildren && !hasProperties && !hasActivities;
        }

        #endregion

        #region Helper Methods

        private LocationResponse MapToLocationResponse(Location location)
        {
            return new LocationResponse
            {
                LocationId = location.LocationId,
                Name = location.Name,
                Description = location.Description,
                ParentLocationId = location.ParentLocationId
            };
        }

        private async Task<LocationResponse> MapToLocationResponseWithDetailsAsync(Location location)
        {
            var response = MapToLocationResponse(location);

            // Get parent location name
            if (location.ParentLocationId.HasValue)
            {
                var parentLocation = await _unitOfWork.Locations.GetByIdAsync(location.ParentLocationId.Value);
                response.ParentLocationName = parentLocation?.Name;
            }

            // Count related entities
            response.PropertiesCount = location.Properties?.Count ?? 0;
            response.ActivitiesCount = location.Activities?.Count ?? 0;

            // Get child locations
            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            var childLocations = allLocations.Where(l => l.ParentLocationId == location.LocationId);
            response.ChildLocations = childLocations.Select(MapToLocationResponse).ToList();

            return response;
        }

        private async Task BuildLocationHierarchyAsync(Location location, List<LocationResponse> hierarchy)
        {
            hierarchy.Add(await MapToLocationResponseWithDetailsAsync(location));

            var allLocations = await _unitOfWork.Locations.GetAllAsync();
            var childLocations = allLocations.Where(l => l.ParentLocationId == location.LocationId);

            foreach (var child in childLocations)
            {
                await BuildLocationHierarchyAsync(child, hierarchy);
            }
        }

        private async Task<bool> WouldCreateCircularReferenceAsync(int locationId, int newParentId)
        {
            var currentLocation = await _unitOfWork.Locations.GetByIdAsync(newParentId);
            
            while (currentLocation != null)
            {
                if (currentLocation.LocationId == locationId)
                    return true;

                if (currentLocation.ParentLocationId.HasValue)
                {
                    currentLocation = await _unitOfWork.Locations.GetByIdAsync(currentLocation.ParentLocationId.Value);
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        #endregion
    }
}