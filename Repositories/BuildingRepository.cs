using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public interface IBuildingRepository : IGenericRepository<Building>
    {
        Task<IEnumerable<Building>> GetBuildingsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "BuildingId",
            bool isDescending = false);

        Task<int> CountBuildingsWithSearchAsync(
            string? searchField = null,
            string? search = null);
        
        Task<Building?> GetByIdWithSubdivisionAsync(int id);
    }
    public class BuildingRepository : GenericRepository<Building>, IBuildingRepository
    {
        public BuildingRepository() : base() { }
        public BuildingRepository(VLivingDbContext context) : base(context) { }

        public async Task<IEnumerable<Building>> GetBuildingsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "BuildingId",
            bool isDescending = false)
        {
            IQueryable<Building> query = _context.Set<Building>()
                .Include(b => b.Subdivision);  // Include Subdivision for SubdivisionName

            // Apply dynamic search filter
            var searchExpression = BuildSearchExpression(searchField, search);
            if (searchExpression != null)
                query = query.Where(searchExpression);

            // Build OrderBy expression
            query = sortBy.ToLower() switch
            {
                "buildingid" => isDescending ? query.OrderByDescending(s => s.BuildingId) : query.OrderBy(s => s.BuildingId),
                "name" => isDescending ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "blockcode" => isDescending ? query.OrderByDescending(s => s.BlockCode) : query.OrderBy(s => s.BlockCode),
                "subdivisionid" => isDescending ? query.OrderByDescending(s => s.SubdivisionId) : query.OrderBy(s => s.SubdivisionId),
                "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                _ => query.OrderBy(s => s.BuildingId),
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Count Building with dynamic search
        /// </summary>
        public async Task<int> CountBuildingsWithSearchAsync(
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

        /// <summary>
        /// Get Building by ID with Subdivision included
        /// </summary>
        public async Task<Building?> GetByIdWithSubdivisionAsync(int id)
        {
            return await _context.Set<Building>()
                .Include(b => b.Subdivision)
                .FirstOrDefaultAsync(b => b.BuildingId == id);
        }
    }
}
