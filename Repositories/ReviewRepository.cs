using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;
using Repositories.Constants;

namespace Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetReviewsByPostIdAsync(
            int postId,
            int page = 1,
            int pageSize = 10,
            string sortBy = "createdAt",
            bool isDescending = true);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(
            int userId,
            int page = 1,
            int pageSize = 10,
            string sortBy = "createdAt",
            bool isDescending = true);
        Task<Review?> GetReviewByBookingIdAsync(int bookingId);
        Task<Review?> GetByIdWithDetailsAsync(int id);
        Task<bool> CheckUserCanReviewAsync(int userId, int bookingId);
        Task<decimal?> GetPostAverageRatingAsync(int postId);
        Task<int> GetPostTotalReviewsAsync(int postId);
        Task<int> CountReviewsByPostIdAsync(int postId);
        Task<int> CountReviewsByUserIdAsync(int userId);
        Task<bool> HasUserReviewedBookingAsync(int userId, int bookingId);
    }

    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository() : base() { }
        public ReviewRepository(VLivingDbContext context) : base() 
        { 
            _context = context;
        }

        /// <summary>
        /// Get reviews for a specific post with pagination
        /// </summary>
        public async Task<IEnumerable<Review>> GetReviewsByPostIdAsync(
            int postId,
            int page = 1,
            int pageSize = 10,
            string sortBy = "createdAt",
            bool isDescending = true)
        {
            try
            {
                var query = _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Booking)
                    .Where(r => r.PostId == postId && !r.IsDeleted);

                // Apply sorting
                query = sortBy.ToLower() switch
                {
                    "rating" => isDescending 
                        ? query.OrderByDescending(r => r.Rating)
                        : query.OrderBy(r => r.Rating),
                    "createdat" or "created" => isDescending 
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt),
                    "updatedat" or "updated" => isDescending 
                        ? query.OrderByDescending(r => r.UpdatedAt)
                        : query.OrderBy(r => r.UpdatedAt),
                    _ => isDescending 
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt)
                };

                return await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get reviews by a specific user with pagination
        /// </summary>
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(
            int userId,
            int page = 1,
            int pageSize = 10,
            string sortBy = "createdAt",
            bool isDescending = true)
        {
            try
            {
                var query = _context.Reviews
                    .Include(r => r.Post)
                    .Include(r => r.Booking)
                    .Where(r => r.UserId == userId && !r.IsDeleted);

                // Apply sorting
                query = sortBy.ToLower() switch
                {
                    "rating" => isDescending 
                        ? query.OrderByDescending(r => r.Rating)
                        : query.OrderBy(r => r.Rating),
                    "createdat" or "created" => isDescending 
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt),
                    "updatedat" or "updated" => isDescending 
                        ? query.OrderByDescending(r => r.UpdatedAt)
                        : query.OrderBy(r => r.UpdatedAt),
                    _ => isDescending 
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt)
                };

                return await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get review by booking ID
        /// </summary>
        public async Task<Review?> GetReviewByBookingIdAsync(int bookingId)
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Post)
                    .Include(r => r.Booking)
                    .FirstOrDefaultAsync(r => r.BookingId == bookingId && !r.IsDeleted);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get review by ID with all details
        /// </summary>
        public async Task<Review?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Post)
                        .ThenInclude(p => p.Apartment)
                            .ThenInclude(a => a.Building)
                    .Include(r => r.Booking)
                    .FirstOrDefaultAsync(r => r.ReviewId == id && !r.IsDeleted);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check if user can review a specific booking
        /// </summary>
        public async Task<bool> CheckUserCanReviewAsync(int userId, int bookingId)
        {
            try
            {
                // Get booking with details
                var booking = await _context.Bookings
                    .Include(b => b.Post)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                    return false;

                // Check if user is the renter of this booking
                if (booking.RenterId != userId)
                    return false;

                // Check if booking status is complete
                if (booking.Status?.ToLower() != "complete")
                    return false;

                // Check if user has role "user"
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user?.Role?.ToLower() != UserRoleConstants.UserRole)
                    return false;

                // Check if booking already has a review
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.BookingId == bookingId && !r.IsDeleted);
                
                return existingReview == null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get average rating for a post
        /// </summary>
        public async Task<decimal?> GetPostAverageRatingAsync(int postId)
        {
            try
            {
                var reviews = await _context.Reviews
                    .Where(r => r.PostId == postId && !r.IsDeleted)
                    .ToListAsync();

                if (!reviews.Any())
                    return null;

                return Math.Round((decimal)reviews.Average(r => r.Rating), 2);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get total number of reviews for a post
        /// </summary>
        public async Task<int> GetPostTotalReviewsAsync(int postId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.PostId == postId && !r.IsDeleted);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Count reviews for a specific post
        /// </summary>
        public async Task<int> CountReviewsByPostIdAsync(int postId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.PostId == postId && !r.IsDeleted);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Count reviews by a specific user
        /// </summary>
        public async Task<int> CountReviewsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Reviews
                    .CountAsync(r => r.UserId == userId && !r.IsDeleted);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Check if user has already reviewed a booking
        /// </summary>
        public async Task<bool> HasUserReviewedBookingAsync(int userId, int bookingId)
        {
            try
            {
                return await _context.Reviews
                    .AnyAsync(r => r.UserId == userId && r.BookingId == bookingId && !r.IsDeleted);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}