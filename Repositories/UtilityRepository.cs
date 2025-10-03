using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public interface IUtilityRepository : IGenericRepository<Utility>
    {
        Task<IEnumerable<Utility>> GetUtilitiesWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "SubdivisionId",
            bool isDescending = false);
        Task<int> CountUtilitiesWithSearchAsync(
            string? searchField = null,
            string? search = null);
        Task<List<int>> GetExistingUtilityIdsAsync(List<int> utilityIds);
    }
    public class UtilityRepository : GenericRepository<Utility>, IUtilityRepository
    {
        public UtilityRepository() : base() { }
        public UtilityRepository(VLivingDbContext context) : base(context) { }

        /// <summary>
        /// Get utilities with advanced query using dynamic search feature
        /// </summary>
        public async Task<IEnumerable<Utility>> GetUtilitiesWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "UtilityId",
            bool isDescending = false)
        {
            // Build OrderBy expression
            Func<IQueryable<Utility>, IOrderedQueryable<Utility>>? orderBy = sortBy.ToLower() switch
            {
                "utilityid" => q => isDescending ? q.OrderByDescending(s => s.UtilityId) : q.OrderBy(s => s.UtilityId),
                "name" => q => isDescending ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name),
                "createdat" => q => isDescending ? q.OrderByDescending(s => s.CreatedAt) : q.OrderBy(s => s.CreatedAt),
                _ => q => q.OrderBy(s => s.UtilityId),
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
        /// Count utilities with dynamic search
        /// </summary>
        public async Task<int> CountUtilitiesWithSearchAsync(
            string? searchField = null,
            string? search = null)
        {
            return await base.CountWithFilter(
                filter: null,  // No custom filter needed
                searchField: searchField,  // Dynamic search handles this
                searchValue: search
            );
        }

        /// <summary>
        /// Get list of existing utility IDs from provided list
        /// Used for validation before creating PostUtility records
        /// </summary>
        public async Task<List<int>> GetExistingUtilityIdsAsync(List<int> utilityIds)
        {
            if (utilityIds == null || !utilityIds.Any())
            {
                return new List<int>();
            }

            return await _context.Set<Utility>()
                .Where(u => utilityIds.Contains(u.UtilityId))
                .Select(u => u.UtilityId)
                .ToListAsync();
        }
    }
}
