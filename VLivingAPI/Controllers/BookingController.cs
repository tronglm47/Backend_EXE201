using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Booking;
using System.Security.Claims;
using VLivingAPI.Authorization;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all bookings with pagination, sorting, and search (Admin only)
        /// </summary>
        /// <param name="queryParams">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of bookings</returns>
        /// <response code="200">Returns the paginated list of bookings</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized (Admin only)</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<BookingResponse.BookingInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] BookingQuery queryParams)
        {
            try
            {
                var result = await _bookingService.GetAllAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all bookings");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get booking details by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details</returns>
        /// <response code="200">Returns the booking details</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to view this booking</response>
        /// <response code="404">Booking not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(BookingResponse.BookingDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var booking = await _bookingService.GetDetailAsync(id);
                if (booking == null)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                // Check authorization - user can only view their own bookings or bookings for their posts
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != "Admin" && booking.RenterId != userId && booking.LandlordId != userId)
                {
                    return Forbid();
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking {BookingId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get bookings for the current renter
        /// </summary>
        /// <param name="queryParams">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of renter's bookings</returns>
        /// <response code="200">Returns the paginated list of renter's bookings</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my-bookings")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<BookingResponse.BookingForRenter>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyBookings([FromQuery] BookingQuery queryParams)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _bookingService.GetBookingsForRenterAsync(userId, queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings for current user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get bookings for the current landlord's posts
        /// </summary>
        /// <param name="queryParams">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of landlord's bookings</returns>
        /// <response code="200">Returns the paginated list of landlord's bookings</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("landlord-bookings")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResponse<BookingResponse.BookingForLandlord>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLandlordBookings([FromQuery] BookingQuery queryParams)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _bookingService.GetBookingsForLandlordAsync(userId, queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting bookings for landlord");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        /// <param name="request">Booking creation data</param>
        /// <returns>Created booking ID</returns>
        /// <response code="201">Booking created successfully</response>
        /// <response code="400">Invalid request data or business rule violation</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequest.BookingCreate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var bookingId = await _bookingService.CreateBookingAsync(request, userId);
                return CreatedAtAction(nameof(GetById), new { id = bookingId }, new { bookingId, message = "Booking created successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update booking details
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Booking update data</param>
        /// <returns>Success status</returns>
        /// <response code="200">Booking updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to update this booking</response>
        /// <response code="404">Booking not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] BookingRequest.BookingUpdate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var success = await _bookingService.UpdateBookingAsync(id, request, userId);
                if (!success)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                return Ok(new { message = "Booking updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking {BookingId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update booking status (accept/reject/cancel)
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="request">Status update data</param>
        /// <returns>Success status</returns>
        /// <response code="200">Booking status updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to update this booking status</response>
        /// <response code="404">Booking not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] BookingRequest.BookingStatusUpdate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var success = await _bookingService.UpdateBookingStatusAsync(id, request, userId);
                if (!success)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                return Ok(new { message = "Booking status updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking status {BookingId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Success status</returns>
        /// <response code="200">Booking deleted successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to delete this booking</response>
        /// <response code="404">Booking not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var success = await _bookingService.DeleteBookingAsync(id, userId);
                if (!success)
                {
                    return NotFound(new { message = "Booking not found" });
                }

                return Ok(new { message = "Booking deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting booking {BookingId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if current user can create a booking for a specific post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>Boolean indicating if booking can be created</returns>
        /// <response code="200">Returns whether booking can be created</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("can-book/{postId}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CanCreateBooking(int postId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var canBook = await _bookingService.CanCreateBookingAsync(userId, postId);
                return Ok(new { canBook, postId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if user can create booking for post {PostId}", postId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}