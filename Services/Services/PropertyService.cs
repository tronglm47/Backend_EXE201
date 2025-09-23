using Microsoft.EntityFrameworkCore;
using VLivingAPI.Repositories.Data.Models;
using Services.Interfaces;
using EVCS.Repositories.HuyCG.Interfaces;
using Services.RequestsResponses.Basic;
using Services.RequestsResponses.Property;

namespace Services.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PropertyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD Operations
        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _unitOfWork.Properties.GetAllAsync();
        }

        public async Task<PaginationResult<PropertyResponse>> GetAllPropertiesPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var properties = await _unitOfWork.Properties.GetAllAsync();
            var propertyResponses = properties.Select(MapToPropertyResponse);
            
            return PaginationResult<PropertyResponse>.Create(propertyResponses, page, pageSize);
        }

        public async Task<Property?> GetPropertyByIdAsync(int id)
        {
            return await _unitOfWork.Properties.GetByIdAsync(id);
        }

        public async Task<Property> CreatePropertyAsync(CreatePropertyRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var property = new Property
            {
                OwnerId = request.OwnerId,
                LocationId = request.LocationId,
                Type = request.Type,
                Price = request.Price,
                Area = request.Area,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                Description = request.Description,
                Images = request.Images,
                CreatedAt = DateTime.Now,
                Status = "available"
            };

            await _unitOfWork.Properties.CreateAsync(property);
            return property;
        }

        public async Task<Property> UpdatePropertyAsync(int id, UpdatePropertyRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (id != request.PropertyId)
                throw new ArgumentException("ID mismatch");

            var existingProperty = await _unitOfWork.Properties.GetByIdAsync(id);
            if (existingProperty == null)
                throw new KeyNotFoundException($"Property with ID {id} not found");

            // Update properties
            existingProperty.OwnerId = request.OwnerId;
            existingProperty.LocationId = request.LocationId;
            existingProperty.Type = request.Type;
            existingProperty.Price = request.Price;
            existingProperty.Area = request.Area;
            existingProperty.Bedrooms = request.Bedrooms;
            existingProperty.Bathrooms = request.Bathrooms;
            existingProperty.Description = request.Description;
            existingProperty.Images = request.Images;
            existingProperty.Status = request.Status ?? existingProperty.Status;

            await _unitOfWork.Properties.UpdateAsync(existingProperty);
            return existingProperty;
        }

        public async Task<bool> DeletePropertyAsync(int id)
        {
            var property = await _unitOfWork.Properties.GetByIdAsync(id);
            if (property == null)
                return false;

            await _unitOfWork.Properties.RemoveAsync(property);
            return true;
        }

        // Business Operations with Pagination
        public async Task<PaginationResult<PropertyResponse>> GetPropertiesByOwnerAsync(int ownerId, int page = 1, int pageSize = 10)
        {
            var properties = await _unitOfWork.Properties.GetAllAsync();
            var filteredProperties = properties.Where(p => p.OwnerId == ownerId);
            var propertyResponses = filteredProperties.Select(MapToPropertyResponse);
            
            return PaginationResult<PropertyResponse>.Create(propertyResponses, page, pageSize);
        }

        public async Task<PaginationResult<PropertyResponse>> GetPropertiesByLocationAsync(int locationId, int page = 1, int pageSize = 10)
        {
            var properties = await _unitOfWork.Properties.GetAllAsync();
            var filteredProperties = properties.Where(p => p.LocationId == locationId);
            var propertyResponses = filteredProperties.Select(MapToPropertyResponse);
            
            return PaginationResult<PropertyResponse>.Create(propertyResponses, page, pageSize);
        }

        public async Task<PaginationResult<PropertyResponse>> GetPropertiesByTypeAsync(string type, int page = 1, int pageSize = 10)
        {
            var properties = await _unitOfWork.Properties.GetAllAsync();
            var filteredProperties = properties.Where(p => p.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
            var propertyResponses = filteredProperties.Select(MapToPropertyResponse);
            
            return PaginationResult<PropertyResponse>.Create(propertyResponses, page, pageSize);
        }

        public async Task<PaginationResult<PropertyResponse>> SearchPropertiesAsync(PropertySearchRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var properties = await _unitOfWork.Properties.GetAllAsync();
            var query = properties.AsQueryable();

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            if (!string.IsNullOrEmpty(request.Type))
                query = query.Where(p => p.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase));

            if (request.LocationId.HasValue)
                query = query.Where(p => p.LocationId == request.LocationId.Value);

            var filteredProperties = query.Where(p => p.Status == "available");
            var propertyResponses = filteredProperties.Select(MapToPropertyResponse);
            
            return PaginationResult<PropertyResponse>.Create(propertyResponses, request.Page, request.PageSize);
        }

        // Complex Operations with Transaction
        public async Task<Property> CreatePropertyWithPostAsync(CreatePropertyWithPostRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Create Property
                var property = new Property
                {
                    OwnerId = request.OwnerId,
                    LocationId = request.LocationId,
                    Type = request.Type,
                    Price = request.Price,
                    Area = request.Area,
                    Bedrooms = request.Bedrooms,
                    Bathrooms = request.Bathrooms,
                    Description = request.Description,
                    Images = request.Images,
                    CreatedAt = DateTime.Now,
                    Status = "available"
                };

                _unitOfWork.Properties.PrepareCreate(property);
                await _unitOfWork.SaveChangesAsync();

                // Create Post
                var post = new Post
                {
                    UserId = request.OwnerId,
                    PropertyId = property.PropertyId,
                    Type = "property_listing",
                    Title = request.PostTitle,
                    Content = request.PostContent,
                    CreatedAt = DateTime.Now,
                    Views = 0
                };

                _unitOfWork.Posts.PrepareCreate(post);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
                return property;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdatePropertyStatusAsync(int propertyId, UpdatePropertyStatusRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Status))
                return false;

            try
            {
                var property = await _unitOfWork.Properties.GetByIdAsync(propertyId);
                if (property == null)
                    return false;

                property.Status = request.Status;
                await _unitOfWork.Properties.UpdateAsync(property);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to map Property to PropertyResponse
        private PropertyResponse MapToPropertyResponse(Property property)
        {
            return new PropertyResponse
            {
                PropertyId = property.PropertyId,
                OwnerId = property.OwnerId,
                LocationId = property.LocationId,
                Type = property.Type,
                Price = property.Price,
                Area = property.Area,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                Description = property.Description,
                Images = property.Images,
                Status = property.Status,
                CreatedAt = property.CreatedAt
                // TODO: Can add OwnerName and LocationName by joining with related entities
            };
        }
    }
}