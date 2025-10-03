using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

namespace Repositories
{
    public interface ISubdivisionRepository : IGenericRepository<Subdivision>
    {
        Task<IEnumerable<Subdivision>> GetSubdivisionsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "SubdivisionId",
            bool isDescending = false);
        Task<int> CountSubdivisionsWithSearchAsync(
            string? searchField = null,
            string? search = null);
    }

    public class SubdivisionRepository : GenericRepository<Subdivision>, ISubdivisionRepository
    {
        public SubdivisionRepository() : base() { }
        public SubdivisionRepository(VLivingDbContext context) : base(context) { }

        /// <summary>
        /// Get subdivisions with advanced query using dynamic search feature
        /// </summary>
        public async Task<IEnumerable<Subdivision>> GetSubdivisionsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "SubdivisionId",
            bool isDescending = false)
        {
            // Build OrderBy expression
            Func<IQueryable<Subdivision>, IOrderedQueryable<Subdivision>>? orderBy = sortBy.ToLower() switch
            {
                "subdivisionid" => q => isDescending ? q.OrderByDescending(s => s.SubdivisionId) : q.OrderBy(s => s.SubdivisionId),
                "name" => q => isDescending ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name),
                "type" => q => isDescending ? q.OrderByDescending(s => s.Type) : q.OrderBy(s => s.Type),
                "description" => q => isDescending ? q.OrderByDescending(s => s.Description) : q.OrderBy(s => s.Description),
                "createdat" => q => isDescending ? q.OrderByDescending(s => s.CreatedAt) : q.OrderBy(s => s.CreatedAt),
                _ => q => q.OrderBy(s => s.SubdivisionId),
            };

            // Use dynamic search from base GenericRepository
            return await base.GetWithAdvancedQuery(
                filter: null,  // No custom filter needed
                page: page,
                pageSize: pageSize,
                orderBy: orderBy,
                searchField: searchField,  // Dynamic search handles this
                searchValue: search
            );
        }

        /// <summary>
        /// Count subdivisions with dynamic search
        /// </summary>
        public async Task<int> CountSubdivisionsWithSearchAsync(
            string? searchField = null,
            string? search = null)
        {
            // Use dynamic search from base GenericRepository
            return await base.CountWithFilter(
                filter: null,  // No custom filter needed
                searchField: searchField,  // Dynamic search handles this
                searchValue: search
            );
        }
    }
}
