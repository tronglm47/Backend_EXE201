using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Location;

namespace Services.Interfaces
{
    public interface ILocationService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<PaginationResult<LocationResponse>> GetAllLocationsPaginatedAsync(int page = 1, int pageSize = 10);
        Task<Location?> GetLocationByIdAsync(int id);
        Task<LocationResponse?> GetLocationDetailsByIdAsync(int id);
        Task<Location> CreateLocationAsync(CreateLocationRequest request);
        Task<Location> UpdateLocationAsync(int id, UpdateLocationRequest request);
        Task<bool> DeleteLocationAsync(int id);

        // Hierarchical Operations
        Task<PaginationResult<LocationResponse>> GetRootLocationsAsync(int page = 1, int pageSize = 10);
        Task<PaginationResult<LocationResponse>> GetChildLocationsByParentAsync(int parentId, int page = 1, int pageSize = 10);
        Task<IEnumerable<LocationResponse>> GetLocationHierarchyAsync(int locationId);
        Task<IEnumerable<LocationResponse>> GetLocationPathToRootAsync(int locationId);

        // Search Operations
        Task<PaginationResult<LocationResponse>> SearchLocationsAsync(LocationSearchRequest request);
        Task<PaginationResult<LocationResponse>> GetLocationsByNameAsync(string name, int page = 1, int pageSize = 10);

        // Business Operations
        Task<bool> HasChildLocationsAsync(int locationId);
        Task<bool> HasPropertiesAsync(int locationId);
        Task<bool> HasActivitiesAsync(int locationId);
        Task<bool> CanDeleteLocationAsync(int locationId);
    }
}