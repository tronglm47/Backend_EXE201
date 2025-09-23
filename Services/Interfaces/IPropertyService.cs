using VLivingAPI.Repositories.Data.Models;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Property;

namespace Services.Interfaces
{
    public interface IPropertyService
    {
        // CRUD Operations with Pagination
        Task<IEnumerable<Property>> GetAllPropertiesAsync();
        Task<PaginationResult<PropertyResponse>> GetAllPropertiesPaginatedAsync(int page = 1, int pageSize = 10);
        Task<Property?> GetPropertyByIdAsync(int id);
        Task<Property> CreatePropertyAsync(CreatePropertyRequest request);
        Task<Property> UpdatePropertyAsync(int id, UpdatePropertyRequest request);
        Task<bool> DeletePropertyAsync(int id);

        // Business Operations with Pagination
        Task<PaginationResult<PropertyResponse>> GetPropertiesByOwnerAsync(int ownerId, int page = 1, int pageSize = 10);
        Task<PaginationResult<PropertyResponse>> GetPropertiesByLocationAsync(int locationId, int page = 1, int pageSize = 10);
        Task<PaginationResult<PropertyResponse>> GetPropertiesByTypeAsync(string type, int page = 1, int pageSize = 10);
        Task<PaginationResult<PropertyResponse>> SearchPropertiesAsync(PropertySearchRequest request);
        
        // Complex Operations with Transaction
        Task<Property> CreatePropertyWithPostAsync(CreatePropertyWithPostRequest request);
        Task<bool> UpdatePropertyStatusAsync(int propertyId, UpdatePropertyStatusRequest request);
    }
}