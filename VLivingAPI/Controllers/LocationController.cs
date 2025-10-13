using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Location;
using System.Security.Claims;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ILocationService locationService, 
            IBookingService bookingService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Update user's current location
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var success = await _locationService.UpdateUserLocationAsync(
                    userId, request.Latitude, request.Longitude);

                if (!success)
                {
                    return BadRequest(new { message = "Failed to update location" });
                }

                return Ok(new { message = "Location updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Calculate distance between two points
        /// </summary>
        [HttpPost("calculate-distance")]
        public async Task<IActionResult> CalculateDistance([FromBody] CalculateDistanceRequest request)
        {
            try
            {
                var result = await _locationService.GetDistanceAndDurationAsync(
                    request.StartLatitude, request.StartLongitude,
                    request.EndLatitude, request.EndLongitude);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get location tracking history for a booking
        /// </summary>
        [HttpGet("history/{bookingId}")]
        public async Task<IActionResult> GetLocationHistory(int bookingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var history = await _locationService.GetLocationHistoryAsync(bookingId, userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location history for booking {BookingId}", bookingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Track location for a booking
        /// </summary>
        [HttpPost("track/{bookingId}")]
        public async Task<IActionResult> TrackLocation(int bookingId, [FromBody] UpdateLocationRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var trackingRecord = await _locationService.TrackUserLocationAsync(
                    bookingId, userId, request.Latitude, request.Longitude);

                if (trackingRecord == null)
                {
                    return BadRequest(new { message = "Failed to track location" });
                }

                return Ok(new { 
                    message = "Location tracked successfully",
                    trackingId = trackingRecord.Id,
                    distanceToMeeting = trackingRecord.DistanceToDestination
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking location for booking {BookingId}", bookingId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}