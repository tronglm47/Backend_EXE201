using Repositories.Basic;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class PropertyTypeRepository : GenericRepository<PropertyType>
    {
        public PropertyTypeRepository() : base() { }
        public PropertyTypeRepository(VLivingDbContext context) : base(context) { }
        public async Task<IEnumerable<PropertyType>> GetPropertyTypesWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "PropertyTypeId",
            bool isDescending = false)
        {
            Expression<Func<PropertyType, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => c.Name.ToLower().Contains(searchTerm) || 
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm));
            }

            Func<IQueryable<PropertyType>, IOrderedQueryable<PropertyType>>? orderBy = sortBy.ToLower() switch
            {
                "propertytypeid" => q => isDescending ? q.OrderByDescending(c => c.PropertyTypeId) : q.OrderBy(c => c.PropertyTypeId),
                "name" => q => isDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                "description" => q => isDescending ? q.OrderByDescending(c => c.Description) : q.OrderBy(c => c.Description),
                "createdat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                _ => q => q.OrderBy(c => c.PropertyTypeId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<PropertyType, bool>>? filter = null;
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
