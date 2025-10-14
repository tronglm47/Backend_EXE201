using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses;
using Services.RequestsResponses.Review;
using System.Security.Claims;
using VLivingAPI.Authorization;
using Repositories.Constants;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Get all reviews for a specific post
        /// </summary>
        /// <param name="postId">Post ID to get reviews for</param>
        /// <param name="queryParams">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of reviews for the post</returns>
        /// <response code="200">Returns the paginated list of reviews</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("post/{postId}")]
        [ProducesResponseType(typeof(PagedResponse<ReviewResponse.ReviewInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReviewsByPost(int postId, [FromQuery] ReviewQuery queryParams)
        {
            try
            {
                if (postId <= 0)
                {
                    return BadRequest(new { message = "Invalid post ID" });
                }

                var result = await _reviewService.GetReviewsByPostAsync(postId, queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting reviews for post {PostId}", postId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user's reviews
        /// </summary>
        /// <param name="queryParams">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of user's reviews</returns>
        /// <response code="200">Returns the paginated list of user's reviews</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("my")]
        [ProducesResponseType(typeof(PagedResponse<ReviewResponse.ReviewInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMyReviews([FromQuery] ReviewQuery queryParams)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var result = await _reviewService.GetMyReviewsAsync(userId, queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting user reviews");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="select">Comma-separated list of fields to select</param>
        /// <returns>Review details</returns>
        /// <response code="200">Returns the review details</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="404">Review not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReviewResponse.ReviewDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id, [FromQuery] string? select = null)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid review ID" });
                }

                var selectedFields = !string.IsNullOrWhiteSpace(select)
                    ? select.Split(',').Select(f => f.Trim()).ToList()
                    : null;

                var result = await _reviewService.GetByIdAsync(id, selectedFields);
                
                if (result == null)
                {
                    return NotFound(new { message = "Review not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting review {ReviewId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new review (User role only)
        /// </summary>
        /// <param name="request">Review creation data</param>
        /// <returns>Created review ID</returns>
        /// <response code="201">Review created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized or not eligible to review</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [Authorize(Roles = UserRoleConstants.UserRole)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ReviewRequest.ReviewCreate request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var reviewId = await _reviewService.CreateReviewAsync(request, userId);
                
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = reviewId },
                    new { reviewId, message = "Review created successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to create review for booking {BookingId}", request.BookingId);
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while creating review for booking {BookingId}", request.BookingId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating review for booking {BookingId}", request.BookingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing review (Owner only)
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="request">Review update data</param>
        /// <returns>Success message</returns>
        /// <response code="200">Review updated successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to update this review</response>
        /// <response code="404">Review not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleConstants.UserRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] ReviewRequest.ReviewUpdate request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid review ID" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var success = await _reviewService.UpdateReviewAsync(id, request, userId);
                
                if (!success)
                {
                    return NotFound(new { message = "Review not found" });
                }

                return Ok(new { message = "Review updated successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update review {ReviewId}", id);
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while updating review {ReviewId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating review {ReviewId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a review (Owner only)
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>Success message</returns>
        /// <response code="200">Review deleted successfully</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="403">User not authorized to delete this review</response>
        /// <response code="404">Review not found</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleConstants.UserRole)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid review ID" });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var success = await _reviewService.DeleteReviewAsync(id, userId);
                
                if (!success)
                {
                    return NotFound(new { message = "Review not found" });
                }

                return Ok(new { message = "Review deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to delete review {ReviewId}", id);
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument while deleting review {ReviewId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting review {ReviewId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if user can review a specific booking
        /// </summary>
        /// <param name="bookingId">Booking ID to check</param>
        /// <returns>Boolean indicating if user can review</returns>
        /// <response code="200">Returns whether user can review the booking</response>
        /// <response code="401">User not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("can-review/{bookingId}")]
        [Authorize(Roles = UserRoleConstants.UserRole)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CanReview(int bookingId)
        {
            try
            {
                if (bookingId <= 0)
                {
                    return BadRequest(new { message = "Invalid booking ID" });
                }

                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                var canReview = await _reviewService.CanUserReviewAsync(userId, bookingId);
                
                return Ok(new { canReview, bookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if user can review booking {BookingId}", bookingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}