using Repositories.Basic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class PostAmenityRepository : GenericRepository<PostAmenity>
    {
        public PostAmenityRepository() : base() { }
        public PostAmenityRepository(VLivingDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<PostAmenity>> GetPostAmenityWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "PostID",
            bool isDescending = false)
        {
            Expression<Func<PostAmenity, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c =>
                    c.Notes.ToLower().Contains(searchTerm) ||
                    c.PostId.ToString().Contains(searchTerm) ||
                    c.AmenityId.ToString().Contains(searchTerm);
            }

            Func<IQueryable<PostAmenity>, IOrderedQueryable<PostAmenity>>? orderBy = sortBy.ToLower() switch
            {
                "postid" => q => isDescending ? q.OrderByDescending(c => c.PostId) : q.OrderBy(c => c.PostId),
                "amenityid" => q => isDescending ? q.OrderByDescending(c => c.AmenityId) : q.OrderBy(c => c.AmenityId),
                "note" => q => isDescending ? q.OrderByDescending(c => c.Notes) : q.OrderBy(c => c.Notes),
                _ => q => q.OrderBy(c => c.PostId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<PostAmenity, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c =>
                c.Notes.ToLower().Contains(searchTerm) ||
                    c.PostId.ToString().Contains(searchTerm) ||
                    c.AmenityId.ToString().Contains(searchTerm);
            }
            return await base.CountWithFilter(filter);
        }
    }
}
