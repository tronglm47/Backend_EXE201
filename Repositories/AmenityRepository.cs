using Repositories.Basic;
using System;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class AmenityRepository : GenericRepository<Amenity>
    {
        public AmenityRepository() : base() { }
        public AmenityRepository(VLivingDbContext context) : base(context) { }
        public async Task<IEnumerable<Amenity>> GetAmenitiesWithAdvancedQuery(
        string? search = null,
        int page = 1,
        int pageSize = 5,
        string sortBy = "AmenityId",
        bool isDescending = false)
        {
            Expression<Func<Amenity, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm));
            }

            Func<IQueryable<Amenity>, IOrderedQueryable<Amenity>>? orderBy = sortBy.ToLower() switch
            {
                "amenityid" => q => isDescending ? q.OrderByDescending(c => c.AmenityId) : q.OrderBy(c => c.AmenityId),
                "name" => q => isDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                "description" => q => isDescending ? q.OrderByDescending(c => c.Description) : q.OrderBy(c => c.Description),
                "createdat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                _ => q => q.OrderBy(c => c.AmenityId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<Amenity, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => c.Name.ToLower().Contains(searchTerm)
                           || (c.Description != null && c.Description.ToLower().Contains(searchTerm));
            }
            return await CountWithFilter(filter);
        }
    }
}
