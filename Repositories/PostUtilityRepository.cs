using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

namespace Repositories
{
    public interface IPostUtilityRepository : IGenericRepository<PostUtility>
    {
        Task<IEnumerable<PostUtility>> GetPostUtilitiesWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "PostId",
            bool isDescending = false);
        Task<int> CountPostUtilitiesWithSearchAsync(
            string? searchField = null,
            string? search = null);
    }
    public class PostUtilityRepository : GenericRepository<PostUtility>,IPostUtilityRepository
    {
        public PostUtilityRepository() : base()
        {
        }
        public PostUtilityRepository(VLivingDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<PostUtility>> GetPostUtilitiesWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "PostId",
            bool isDescending = false)
        {
            // Build OrderBy expression
            Func<IQueryable<PostUtility>, IOrderedQueryable<PostUtility>>? orderBy = sortBy.ToLower() switch
            {
                "postid" => q => isDescending ? q.OrderByDescending(s => s.PostId) : q.OrderBy(s => s.PostId),
                "utilityid" => q => isDescending ? q.OrderByDescending(s => s.UtilityId) : q.OrderBy(s => s.UtilityId),
                "note" => q => isDescending ? q.OrderByDescending(s => s.Notes) : q.OrderBy(s => s.Notes),
                _ => q => q.OrderBy(s => s.PostId)
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
        /// Count post utilities with dynamic search
        /// </summary>
        public async Task<int> CountPostUtilitiesWithSearchAsync(
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
