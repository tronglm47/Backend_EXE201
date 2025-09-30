using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class PostRepository : GenericRepository<Post>
    {
        public PostRepository() : base()
        {
        }
        public PostRepository(VLivingDbContext context) : base(context)
        {
        }

        public async Task<Post> GetByIdAdvancedAsync(int id)
        {
            var item = await _context.Posts
                .Include(pt => pt.PropertyType)
                .Include(pf => pf.PropertyForm)
                .Include(pty => pty.PostType)
                .Include(pa => pa.PostAmenities).ThenInclude(a => a.Amenity)
                .FirstOrDefaultAsync(p => p.PostId == id);
            return item ?? new Post();
        }

        public async Task<IEnumerable<Post>> GetPostsWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "PostId",
            bool isDescending = false)
        {
            Expression<Func<Post, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => c.UserId.ToString().Contains(searchTerm)
                           || (c.Status != null && c.Status.ToLower().Contains(searchTerm));
            }

            Func<IQueryable<Post>, IOrderedQueryable<Post>>? orderBy = sortBy.ToLower() switch
            {
                "postid" => q => isDescending ? q.OrderByDescending(c => c.PostId) : q.OrderBy(c => c.PostId),
                "userid" => q => isDescending ? q.OrderByDescending(c => c.UserId) : q.OrderBy(c => c.UserId),
                "title" => q => isDescending ? q.OrderByDescending(c => c.Title) : q.OrderBy(c => c.Title),
                "status" => q => isDescending ? q.OrderByDescending(c => c.Status) : q.OrderBy(c => c.Status),
                "createdat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                _ => q => q.OrderBy(c => c.PostId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<Post, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => c.Title.ToString().Contains(searchTerm)
                           || (c.Content != null && c.Content.ToLower().Contains(searchTerm));
            }
            return await CountWithFilter(filter);
        }
        /* Example
        public async Task<IEnumerable<Order>> GetOrdersWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "ProductId",
            bool isDescending = false)
        {
            IQueryable<Order> query = _context.Orders;

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();

                query = query.Where(c =>
                c.UserId > 0 && c.UserId.ToString().Contains(searchTerm) ||
                c.OrderDate != null && c.Status.ToLower().Contains(searchTerm) ||
                c.Status != null && c.Status.ToLower().Contains(searchTerm)
                );
            }

            // Apply sorting
            switch (sortBy.ToLowerInvariant())
            {
                case "userid":
                    query = isDescending ?
                        query.OrderByDescending(c => c.UserId) :
                        query.OrderBy(c => c.UserId);
                    break;
                case "orderdate":
                    query = isDescending ?
                        query.OrderByDescending(c => c.OrderDate) :
                        query.OrderBy(c => c.OrderDate);
                    break;
                case "status":
                    query = isDescending ?
                        query.OrderByDescending(c => c.Status) :
                        query.OrderBy(c => c.Status);
                    break;
                default:
                    query = query.OrderBy(c => c.OrderId);
                    break;
            }

            // Apply pagination
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            IQueryable<Order> query = _context.Orders;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();

                query = query.Where(c =>
                    c.UserId > 0 && c.UserId.ToString().Contains(searchTerm) ||
                    c.OrderDate != null && c.Status.ToLower().Contains(searchTerm) ||
                    c.Status != null && c.Status.ToLower().Contains(searchTerm)
                    );
            }

            return await query.CountAsync();
        }
        */
    }
}
