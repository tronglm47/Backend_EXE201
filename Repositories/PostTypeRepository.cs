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
    public class PostTypeRepository : GenericRepository<PostType>
    {
        public PostTypeRepository() : base() { }
        public PostTypeRepository(VLivingDbContext context) : base(context) { }
        public async Task<IEnumerable<PostType>> GetPostTypesWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "PostTypeId",
            bool isDescending = false)
        {
            Expression<Func<PostType, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c =>
                    c.Name.ToLower().Contains(searchTerm) || 
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm));
            }

            Func<IQueryable<PostType>, IOrderedQueryable<PostType>>? orderBy = sortBy.ToLower() switch
            {
                "posttypeid" => q => isDescending ? q.OrderByDescending(c => c.PostTypeId) : q.OrderBy(c => c.PostTypeId),
                "name" => q => isDescending ? q.OrderByDescending(c => c.Name) : q.OrderBy(c => c.Name),
                "description" => q => isDescending ? q.OrderByDescending(c => c.Description) : q.OrderBy(c => c.Description),
                "createdat" => q => isDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt),
                _ => q => q.OrderBy(c => c.PostTypeId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<PostType, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c => 
                    c.Name.ToLower().Contains(searchTerm) ||
                    (c.Description != null && c.Description.ToLower().Contains(searchTerm));
            }
            return await CountWithFilter(filter);
        }
    }
}


