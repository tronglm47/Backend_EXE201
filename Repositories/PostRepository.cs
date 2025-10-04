using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;
using Repositories.Constants;

namespace Repositories
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<IEnumerable<Post>> GetPostsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "postId",
            bool isDescending = false);
        Task<IEnumerable<Post>> GetPostsForLandLordWithDetailsAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "postId",
            bool isDescending = false);
        Task<IEnumerable<Post>> GetPostsForUserAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "postId",
            bool isDescending = false);
        Task<Post?> GetByIdForLandLordAsync(int id);
        Task<Post?> GetByIdForUserAsync(int id);
        Task<int> CountPostsWithSearchAsync(
            string? searchField = null,
            string? search = null);
        Task<int> CountPostsForLandLordAsync(
            string? searchField = null,
            string? search = null);
        Task<int> CountPostsForUserAsync(
            string? searchField = null,
            string? search = null);
    }
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        public PostRepository() : base() { }
        public PostRepository(VLivingDbContext context) : base(context) { }
        /// <summary>
        /// Get posts with advanced query using dynamic search feature
        /// </summary>
        public async Task<IEnumerable<Post>> GetPostsWithAdvancedQueryAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "PostId",
            bool isDescending = false)
        {
            // Build OrderBy expression
            Func<IQueryable<Post>, IOrderedQueryable<Post>>? orderBy = sortBy.ToLower() switch
            {
                "postid" => q => isDescending ? q.OrderByDescending(s => s.PostId) : q.OrderBy(s => s.PostId),
                "apartmentid" => q => isDescending ? q.OrderByDescending(s => s.ApartmentId) : q.OrderBy(s => s.ApartmentId),
                "userid" => q => isDescending ? q.OrderByDescending(s => s.UserId) : q.OrderBy(s => s.UserId),
                "title" => q => isDescending ? q.OrderByDescending(s => s.Title) : q.OrderBy(s => s.Title),
                "posttype" => q => isDescending ? q.OrderByDescending(s => s.PostType) : q.OrderBy(s => s.PostType),
                "status" => q => isDescending ? q.OrderByDescending(s => s.Status) : q.OrderBy(s => s.Status),
                "createdat" => q => isDescending ? q.OrderByDescending(s => s.CreatedAt) : q.OrderBy(s => s.CreatedAt),
                _ => q => q.OrderBy(s => s.PostId),
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
        /// Count posts with dynamic search
        /// </summary>
        public async Task<int> CountPostsWithSearchAsync(
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
        /// Count posts for landlord (only ForRent and ForSale posts)
        /// </summary>
        public async Task<int> CountPostsForLandLordAsync(
            string? searchField = null,
            string? search = null)
        {
            IQueryable<Post> query = _context.Set<Post>()
                .Where(p => p.PostType == PostTypeConstants.ForRent || p.PostType == PostTypeConstants.ForSale);

            // Apply dynamic search filter if provided
            if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(search))
            {
                var searchExpression = BuildSearchExpression(searchField, search);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Get posts for landlord with full details (Apartment, Building, Subdivision)
        /// Includes navigation properties for detailed information
        /// Only returns posts with PostType = ForRent or ForSale
        /// </summary>
        public async Task<IEnumerable<Post>> GetPostsForLandLordWithDetailsAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "PostId",
            bool isDescending = false)
        {
            IQueryable<Post> query = _context.Set<Post>()
                .Include(p => p.Apartment)
                    .ThenInclude(a => a.Building)
                        .ThenInclude(b => b.Subdivision)
                .Include(p => p.User)
                .Where(p => p.PostType == PostTypeConstants.ForRent || p.PostType == PostTypeConstants.ForSale);

            // Apply dynamic search filter if provided
            if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(search))
            {
                var searchExpression = BuildSearchExpression(searchField, search);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            // Build OrderBy expression
            query = sortBy.ToLower() switch
            {
                "postid" => isDescending ? query.OrderByDescending(s => s.PostId) : query.OrderBy(s => s.PostId),
                "apartmentid" => isDescending ? query.OrderByDescending(s => s.ApartmentId) : query.OrderBy(s => s.ApartmentId),
                "userid" => isDescending ? query.OrderByDescending(s => s.UserId) : query.OrderBy(s => s.UserId),
                "title" => isDescending ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
                "posttype" => isDescending ? query.OrderByDescending(s => s.PostType) : query.OrderBy(s => s.PostType),
                "status" => isDescending ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
                "createdat" => isDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                _ => query.OrderBy(s => s.PostId),
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get a single post by ID for landlord with full details (Apartment, Building, Subdivision)
        /// Only returns post if PostType = ForRent or ForSale
        /// </summary>
        public async Task<Post?> GetByIdForLandLordAsync(int id)
        {
            return await _context.Set<Post>()
                .Include(p => p.Apartment)
                    .ThenInclude(a => a.Building)
                        .ThenInclude(b => b.Subdivision)
                .Include(p => p.User)
                .Where(p => p.PostId == id && (p.PostType == PostTypeConstants.ForRent || p.PostType == PostTypeConstants.ForSale))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Count posts for user (only FindRoom posts)
        /// </summary>
        public async Task<int> CountPostsForUserAsync(
            string? searchField = null,
            string? search = null)
        {
            IQueryable<Post> query = _context.Set<Post>()
                .Where(p => p.PostType == PostTypeConstants.FindRoom);

            // Apply dynamic search filter if provided
            if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(search))
            {
                var searchExpression = BuildSearchExpression(searchField, search);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Get posts for user with user details
        /// Includes User navigation property for user information
        /// Only returns posts with PostType = FindRoom
        /// </summary>
        public async Task<IEnumerable<Post>> GetPostsForUserAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "PostId",
            bool isDescending = false)
        {
            IQueryable<Post> query = _context.Set<Post>()
                .Include(p => p.User)
                .Where(p => p.PostType == PostTypeConstants.FindRoom);

            // Apply dynamic search filter if provided
            if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(search))
            {
                var searchExpression = BuildSearchExpression(searchField, search);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            // Build OrderBy expression
            Func<IQueryable<Post>, IOrderedQueryable<Post>> orderBy = sortBy.ToLower() switch
            {
                "postid" => isDescending ? query => query.OrderByDescending(s => s.PostId) : query => query.OrderBy(s => s.PostId),
                "userid" => isDescending ? query => query.OrderByDescending(s => s.UserId) : query => query.OrderBy(s => s.UserId),
                "title" => isDescending ? query => query.OrderByDescending(s => s.Title) : query => query.OrderBy(s => s.Title),
                "posttype" => isDescending ? query => query.OrderByDescending(s => s.PostType) : query => query.OrderBy(s => s.PostType),
                "status" => isDescending ? query => query.OrderByDescending(s => s.Status) : query => query.OrderBy(s => s.Status),
                "createdat" => isDescending ? query => query.OrderByDescending(s => s.CreatedAt) : query => query.OrderBy(s => s.CreatedAt),
                _ => query => query.OrderBy(s => s.PostId),
            };

            query = orderBy(query);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get a single post by ID for user with user details
        /// Only returns post if PostType = FindRoom
        /// </summary>
        public async Task<Post?> GetByIdForUserAsync(int id)
        {
            return await _context.Set<Post>()
                .Include(p => p.User)
                .Where(p => p.PostId == id && p.PostType == PostTypeConstants.FindRoom)
                .FirstOrDefaultAsync();
        }
    }
}
