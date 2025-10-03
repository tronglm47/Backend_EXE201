using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

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
            // Build OrderBy expression
            Func<IQueryable<Building>, IOrderedQueryable<Building>>? orderBy = sortBy.ToLower() switch
            {
                "buildingid" => q => isDescending ? q.OrderByDescending(s => s.BuildingId) : q.OrderBy(s => s.BuildingId),
                "name" => q => isDescending ? q.OrderByDescending(s => s.Name) : q.OrderBy(s => s.Name),
                "blockcode" => q => isDescending ? q.OrderByDescending(s => s.BlockCode) : q.OrderBy(s => s.BlockCode),
                "subdivisionid" => q => isDescending ? q.OrderByDescending(s => s.SubdivisionId) : q.OrderBy(s => s.SubdivisionId),
                "createdat" => q => isDescending ? q.OrderByDescending(s => s.CreatedAt) : q.OrderBy(s => s.CreatedAt),
                _ => q => q.OrderBy(s => s.BuildingId),
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
    }
}
