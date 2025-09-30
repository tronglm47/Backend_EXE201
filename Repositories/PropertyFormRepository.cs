using Repositories.Basic;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class PropertyFormRepository : GenericRepository<PropertyForm>
    {
        public PropertyFormRepository() : base() { }
        public PropertyFormRepository(VLivingDbContext context) : base(context) { }

        public async Task<IEnumerable<PropertyForm>> GetPropertyFormsWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "PropertyFormId",
            bool isDescending = false)
        {
            Expression<Func<PropertyForm, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm));
            }

            Func<IQueryable<PropertyForm>, IOrderedQueryable<PropertyForm>>? orderBy = sortBy.ToLower() switch
            {
                "propertyformid" => q => isDescending ? q.OrderByDescending(c => c.PropertyFormId) : q.OrderBy(c => c.PropertyFormId),
                "name" => q => isDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                "description" => q => isDescending ? q.OrderByDescending(c => c.Description) : q.OrderBy(c => c.Description),
                "createdat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                _ => q => q.OrderBy(c => c.PropertyFormId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<PropertyForm, bool>>? filter = null;
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
