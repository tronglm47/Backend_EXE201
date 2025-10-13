using Microsoft.EntityFrameworkCore;
using Repositories.Basic;
using Repositories.Data;
using Repositories.Models;

namespace Repositories
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "bookingId",
            bool isDescending = false);
        Task<IEnumerable<Booking>> GetBookingsByRenterIdAsync(
            int renterId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string sortBy = "createdAt",
            bool isDescending = true);
        Task<IEnumerable<Booking>> GetBookingsByPostIdAsync(
            int postId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string sortBy = "createdAt",
            bool isDescending = true);
        Task<IEnumerable<Booking>> GetBookingsByLandlordAsync(
            int landlordId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string sortBy = "createdAt",
            bool isDescending = true);
        Task<Booking?> GetByIdWithDetailsAsync(int id);
        Task<int> CountBookingsWithSearchAsync(
            string? searchField = null,
            string? search = null);
        Task<int> CountBookingsByRenterIdAsync(int renterId, string? status = null);
        Task<int> CountBookingsByPostIdAsync(int postId, string? status = null);
        Task<int> CountBookingsByLandlordAsync(int landlordId, string? status = null);
        Task<bool> CanCreateBookingAsync(int renterId, int postId);
    }

    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository() : base() { }
        public BookingRepository(VLivingDbContext context) : base(context) { }

        /// <summary>
        /// Get bookings with details including Post, Renter and Post.User navigation properties
        /// </summary>
        public async Task<IEnumerable<Booking>> GetBookingsWithDetailsAsync(
            int page = 1,
            int pageSize = 10,
            string? searchField = null,
            string? search = null,
            string sortBy = "BookingId",
            bool isDescending = false)
        {
            var query = _context.Set<Booking>()
                .Include(b => b.Post)
                    .ThenInclude(p => p.User)
                .Include(b => b.Renter)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchField) && !string.IsNullOrWhiteSpace(search))
            {
                var searchExpression = BuildSearchExpression(searchField, search);
                if (searchExpression != null)
                {
                    query = query.Where(searchExpression);
                }
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "bookingid" => isDescending ? query.OrderByDescending(b => b.BookingId) : query.OrderBy(b => b.BookingId),
                "renterid" => isDescending ? query.OrderByDescending(b => b.RenterId) : query.OrderBy(b => b.RenterId),
                "postid" => isDescending ? query.OrderByDescending(b => b.PostId) : query.OrderBy(b => b.PostId),
                "meetingtime" => isDescending ? query.OrderByDescending(b => b.MeetingTime) : query.OrderBy(b => b.MeetingTime),
                "status" => isDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                "createdat" => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => isDescending ? query.OrderByDescending(b => b.BookingId) : query.OrderBy(b => b.BookingId)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get bookings by renter ID with optional status filter
        /// </summary>
        public async Task<IEnumerable<Booking>> GetBookingsByRenterIdAsync(
            int renterId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string sortBy = "createdAt",
            bool isDescending = true)
        {
            var query = _context.Set<Booking>()
                .Include(b => b.Post)
                    .ThenInclude(p => p.User)
                .Include(b => b.Post.Apartment)
                    .ThenInclude(a => a.Building)
                        .ThenInclude(bu => bu.Subdivision)
                .Where(b => b.RenterId == renterId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "bookingid" => isDescending ? query.OrderByDescending(b => b.BookingId) : query.OrderBy(b => b.BookingId),
                "meetingtime" => isDescending ? query.OrderByDescending(b => b.MeetingTime) : query.OrderBy(b => b.MeetingTime),
                "status" => isDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                "createdat" => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get bookings by post ID with optional status filter
        /// </summary>
        public async Task<IEnumerable<Booking>> GetBookingsByPostIdAsync(
            int postId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string sortBy = "createdAt",
            bool isDescending = true)
        {
            var query = _context.Set<Booking>()
                .Include(b => b.Renter)
                .Include(b => b.Post)
                .Where(b => b.PostId == postId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "bookingid" => isDescending ? query.OrderByDescending(b => b.BookingId) : query.OrderBy(b => b.BookingId),
                "meetingtime" => isDescending ? query.OrderByDescending(b => b.MeetingTime) : query.OrderBy(b => b.MeetingTime),
                "status" => isDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                "createdat" => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get bookings for landlord (posts belong to landlord)
        /// </summary>
        public async Task<IEnumerable<Booking>> GetBookingsByLandlordAsync(
            int landlordId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string sortBy = "createdAt",
            bool isDescending = true)
        {
            var query = _context.Set<Booking>()
                .Include(b => b.Renter)
                .Include(b => b.Post)
                    .ThenInclude(p => p.Apartment)
                        .ThenInclude(a => a.Building)
                            .ThenInclude(bu => bu.Subdivision)
                .Where(b => b.Post.UserId == landlordId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "bookingid" => isDescending ? query.OrderByDescending(b => b.BookingId) : query.OrderBy(b => b.BookingId),
                "meetingtime" => isDescending ? query.OrderByDescending(b => b.MeetingTime) : query.OrderBy(b => b.MeetingTime),
                "status" => isDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                "createdat" => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
                _ => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt)
            };

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get booking by ID with full details
        /// </summary>
        public async Task<Booking?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Set<Booking>()
                .Include(b => b.Post)
                    .ThenInclude(p => p.User)
                .Include(b => b.Post.Apartment)
                    .ThenInclude(a => a.Building)
                        .ThenInclude(bu => bu.Subdivision)
                .Include(b => b.Renter)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        /// <summary>
        /// Count bookings with search filter
        /// </summary>
        public async Task<int> CountBookingsWithSearchAsync(
            string? searchField = null,
            string? search = null)
        {
            var query = _context.Set<Booking>().AsQueryable();

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
        /// Count bookings by renter ID
        /// </summary>
        public async Task<int> CountBookingsByRenterIdAsync(int renterId, string? status = null)
        {
            var query = _context.Set<Booking>().Where(b => b.RenterId == renterId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Count bookings by post ID
        /// </summary>
        public async Task<int> CountBookingsByPostIdAsync(int postId, string? status = null)
        {
            var query = _context.Set<Booking>().Where(b => b.PostId == postId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Count bookings by landlord ID
        /// </summary>
        public async Task<int> CountBookingsByLandlordAsync(int landlordId, string? status = null)
        {
            var query = _context.Set<Booking>()
                .Where(b => b.Post.UserId == landlordId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.Status == status);
            }

            return await query.CountAsync();
        }

        /// <summary>
        /// Check if a renter can create a booking for a specific post
        /// (no pending or confirmed booking exists)
        /// </summary>
        public async Task<bool> CanCreateBookingAsync(int renterId, int postId)
        {
            var existingBooking = await _context.Set<Booking>()
                .FirstOrDefaultAsync(b => b.RenterId == renterId 
                    && b.PostId == postId 
                    && (b.Status == "Pending" || b.Status == "Confirmed"));

            return existingBooking == null;
        }
    }
}