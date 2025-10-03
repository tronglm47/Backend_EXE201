using Repositories.Basic;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class LocationRepository : GenericRepository<Location>
    {
        public LocationRepository() : base() { }
        public LocationRepository(VLivingDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Location>> GetLocationsWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "LocationId",
            bool isDescending = false,
            int? minParentLocationId = null)
        {
            Expression<Func<Location, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search) || minParentLocationId.HasValue)
            {
                var searchTerm = search?.ToLower();
                filter = c =>
                    (string.IsNullOrWhiteSpace(search) || 
                     c.Name.ToLower().Contains(searchTerm!) ||
                     (c.Description != null && c.Description.ToLower().Contains(searchTerm!)) ||
                     (c.LocationType != null && c.LocationType.ToLower().Contains(searchTerm!)) ||
                     (c.LocationCode != null && c.LocationCode.ToLower().Contains(searchTerm!)) ||
                     (c.FullAddress != null && c.FullAddress.ToLower().Contains(searchTerm!)) ||
                     (c.ParentLocationId.HasValue && c.ParentLocationId.Value.ToString().Contains(searchTerm!))) &&
                    (!minParentLocationId.HasValue || (c.ParentLocationId.HasValue && c.ParentLocationId >= minParentLocationId.Value));
            }

            Func<IQueryable<Location>, IOrderedQueryable<Location>>? orderBy = sortBy.ToLower() switch
            {
                "LocationId" => q => isDescending ? q.OrderByDescending(c => c.LocationId) : q.OrderBy(c => c.LocationId),
                "name" => q => isDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                "description" => q => isDescending ? q.OrderByDescending(c => c.Description) : q.OrderBy(c => c.Description),
                "createdat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                "locationtype" => q => isDescending ? q.OrderByDescending(c => c.LocationType) : q.OrderBy(c => c.LocationType),
                "locationcode" => q => isDescending ? q.OrderByDescending(c => c.LocationCode) : q.OrderBy(c => c.LocationCode),
                "fulladdress" => q => isDescending ? q.OrderByDescending(c => c.FullAddress) : q.OrderBy(c => c.FullAddress),
                "level" => q => isDescending ? q.OrderByDescending(c => c.Level) : q.OrderBy(c => c.Level),
                "parentlocationid" => q => isDescending ? q.OrderByDescending(c => c.ParentLocationId) : q.OrderBy(c => c.ParentLocationId),
                "isactive" => q => isDescending ? q.OrderByDescending(c => c.IsActive) : q.OrderBy(c => c.IsActive),
                "createat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                _ => q => q.OrderBy(c => c.LocationId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null, int? minParentLocationId = null)
        {
            Expression<Func<Location, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search) || minParentLocationId.HasValue)
            {
                var searchTerm = search?.ToLower();
                filter = c =>
                    (string.IsNullOrWhiteSpace(search) || 
                     c.Name.ToLower().Contains(searchTerm!) ||
                     (c.Description != null && c.Description.ToLower().Contains(searchTerm!)) ||
                     (c.LocationType != null && c.LocationType.ToLower().Contains(searchTerm!)) ||
                     (c.LocationCode != null && c.LocationCode.ToLower().Contains(searchTerm!)) ||
                     (c.FullAddress != null && c.FullAddress.ToLower().Contains(searchTerm!)) ||
                     (c.ParentLocationId.HasValue && c.ParentLocationId.Value.ToString().Contains(searchTerm!))) &&
                    (!minParentLocationId.HasValue || (c.ParentLocationId.HasValue && c.ParentLocationId >= minParentLocationId.Value));
            }
            return await CountWithFilter(filter);
        }

        public async Task<List<Location>> GetLocationHierarchyAsync(int locationId)
        {
            var hierarchy = new List<Location>();
            var currentLocation = await GetByIdAsync(locationId);
            
            if (currentLocation == null)
                return hierarchy;

            // Build hierarchy from bottom to top, then reverse
            while (currentLocation != null)
            {
                hierarchy.Add(currentLocation);
                
                if (currentLocation.ParentLocationId.HasValue)
                {
                    currentLocation = await GetByIdAsync(currentLocation.ParentLocationId.Value);
                }
                else
                {
                    break;
                }
            }

            // Reverse to get hierarchy from top (parent) to bottom (requested location)
            hierarchy.Reverse();
            return hierarchy;
        }
    }
}
