using Repositories.Basic;
using System.Linq.Expressions;
using VLivingAPI.Repositories.Data.Models;

namespace Repositories
{
    public class PostReviewRepository : GenericRepository<PostReview>
    {
        public PostReviewRepository() : base()
        {
        }
        public PostReviewRepository(VLivingDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<PostReview>> GetLocationsWithAdvancedQuery(
            string? search = null,
            int page = 1,
            int pageSize = 5,
            string sortBy = "ReviewId",
            bool isDescending = false)
        {
            Expression<Func<PostReview, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c =>
                    c.ReviewText.ToLower().Contains(searchTerm) ||
                    (c.PostId != null && c.PostId.ToString().Contains(searchTerm)) ||
                    (c.ReviewerId != null && c.ReviewerId.ToString().Contains(searchTerm)) ||
                    (c.BookingId != null && c.BookingId.ToString().Contains(searchTerm)) ||
                    (c.Rating != null && c.Rating.ToString().Contains(searchTerm)) ||
                    (c.ReviewDate != null && c.ReviewDate.ToString().Contains(searchTerm))
                    ;
            }

            Func<IQueryable<PostReview>, IOrderedQueryable<PostReview>>? orderBy = sortBy.ToLower() switch
            {
                "ReviewId" => q => isDescending ? q.OrderByDescending(c => c.ReviewId) : q.OrderBy(c => c.ReviewId),
                "PostId" => q => isDescending ? q.OrderByDescending(c => c.PostId) : q.OrderBy(c => c.PostId),
                "ReviewerId" => q => isDescending ? q.OrderByDescending(c => c.ReviewerId) : q.OrderBy(c => c.ReviewerId),
                "BookingId" => q => isDescending ? q.OrderByDescending(c => c.BookingId) : q.OrderBy(c => c.BookingId),
                "Rating" => q => isDescending ? q.OrderByDescending(c => c.Rating) : q.OrderBy(c => c.Rating),
                "ReviewText" => q => isDescending ? q.OrderByDescending(c => c.ReviewText) : q.OrderBy(c => c.ReviewText),
                "ReviewDate" => q => isDescending ? q.OrderByDescending(c => c.ReviewDate) : q.OrderBy(c => c.ReviewDate),
                "IsVerified" => q => isDescending ? q.OrderByDescending(c => c.IsVerified) : q.OrderBy(c => c.IsVerified),
                _ => q => q.OrderBy(c => c.ReviewId) // Default case
            };

            return await base.GetWithAdvancedQuery(filter, page, pageSize, orderBy);
        }

        public async Task<int> CountWithSearch(string? search = null)
        {
            Expression<Func<PostReview, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                filter = c =>
                    c.ReviewText.ToLower().Contains(searchTerm) ||
                    (c.PostId != null && c.PostId.ToString().Contains(searchTerm)) ||
                    (c.ReviewerId != null && c.ReviewerId.ToString().Contains(searchTerm)) ||
                    (c.BookingId != null && c.BookingId.ToString().Contains(searchTerm)) ||
                    (c.Rating != null && c.Rating.ToString().Contains(searchTerm)) ||
                    (c.ReviewDate != null && c.ReviewDate.ToString().Contains(searchTerm))
                    ;
            }
            return await CountWithFilter(filter);
        }
    }
}
