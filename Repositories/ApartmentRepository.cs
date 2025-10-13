using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public interface IApartmentRepository : IGenericRepository<Apartment>
    {
        Task<IEnumerable<Apartment>> GetApartmentsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "ApartmentId",
            bool isDescending = false);

        Task<int> CountApartmentsWithSearchAsync(
            string? searchField = null,
            string? search = null);
        
        Task<Apartment?> GetByIdWithBuildingAsync(int id);
    }
    public class ApartmentRepository : GenericRepository<Apartment>, IApartmentRepository
    {
        public ApartmentRepository() : base() { }
        public ApartmentRepository(VLivingDbContext context) : base(context) { }

        public async Task<IEnumerable<Apartment>> GetApartmentsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "ApartmentId",
            bool isDescending = false)
        {
            IQueryable<Apartment> query = _context.Set<Apartment>()
                .Include(a => a.Building)   // Include Building for BuildingName
                .Include(a => a.Posts);     // Include Posts for PostIds

            // Apply dynamic search filter
            var searchExpression = BuildSearchExpression(searchField, search);
            if (searchExpression != null)
                query = query.Where(searchExpression);

            // Build OrderBy expression
            query = sortBy.ToLower() switch
            {
                "apartmentid" => isDescending ? query.OrderByDescending(a => a.ApartmentId) : query.OrderBy(a => a.ApartmentId),
                "apartmentcode" => isDescending ? query.OrderByDescending(a => a.ApartmentCode) : query.OrderBy(a => a.ApartmentCode),
                "buildingid" => isDescending ? query.OrderByDescending(a => a.BuildingId) : query.OrderBy(a => a.BuildingId),
                "floor" => isDescending ? query.OrderByDescending(a => a.Floor) : query.OrderBy(a => a.Floor),
                "area" => isDescending ? query.OrderByDescending(a => a.Area) : query.OrderBy(a => a.Area),
                "apartmenttype" => isDescending ? query.OrderByDescending(a => a.ApartmentType) : query.OrderBy(a => a.ApartmentType),
                "status" => isDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                "createdat" => isDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
                _ => query.OrderBy(a => a.ApartmentId),
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Count Apartment with dynamic search
        /// </summary>
        public async Task<int> CountApartmentsWithSearchAsync(
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
        /// Get Apartment by ID with Building and Subdivision included
        /// </summary>
        public async Task<Apartment?> GetByIdWithBuildingAsync(int id)
        {
            return await _context.Set<Apartment>()
                .Include(a => a.Building)
                    .ThenInclude(b => b.Subdivision)
                .FirstOrDefaultAsync(a => a.ApartmentId == id);
        }
    }
}
