using Microsoft.AspNetCore.Mvc;
using Services;
using Services.RequestsResponses.Location;

namespace VLivingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestLocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<TestLocationController> _logger;

        public TestLocationController(ILocationService locationService, ILogger<TestLocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        /// <summary>
        /// Test distance calculation between two points
        /// </summary>
        [HttpGet("test-distance")]
        public async Task<IActionResult> TestDistance(
            decimal lat1 = 10.7769m, decimal lon1 = 106.7009m,  // Ho Chi Minh City
            decimal lat2 = 21.0285m, decimal lon2 = 105.8542m)  // Hanoi
        {
            try
            {
                var distance = await _locationService.CalculateDistanceAsync(lat1, lon1, lat2, lon2);
                var distanceInfo = await _locationService.GetDistanceAndDurationAsync(lat1, lon1, lat2, lon2);
                
                return Ok(new 
                { 
                    straightLineDistance = $"{distance:F2} km",
                    googleMapsInfo = distanceInfo,
                    testPoints = new {
                        from = $"Lat: {lat1}, Lon: {lon1}",
                        to = $"Lat: {lat2}, Lon: {lon2}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing distance calculation");
                return StatusCode(500, new { message = "Error testing distance", error = ex.Message });
            }
        }

        /// <summary>
        /// Test location tracking simulation
        /// </summary>
        [HttpPost("simulate-tracking/{bookingId}")]
        public async Task<IActionResult> SimulateTracking(int bookingId, [FromBody] UpdateLocationRequest request)
        {
            try
            {
                // Simulate user ID (in real app this would come from JWT token)
                int simulatedUserId = 1;
                
                var trackingResult = await _locationService.TrackUserLocationAsync(
                    bookingId, simulatedUserId, request.Latitude, request.Longitude);
                
                if (trackingResult == null)
                {
                    return BadRequest(new { message = "Failed to track location" });
                }
                
                return Ok(new 
                { 
                    message = "Location tracked successfully",
                    trackingId = trackingResult.Id,
                    bookingId = trackingResult.BookingId,
                    userId = trackingResult.UserId,
                    latitude = trackingResult.Latitude,
                    longitude = trackingResult.Longitude,
                    distanceToDestination = trackingResult.DistanceToDestination,
                    timestamp = trackingResult.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error simulating location tracking");
                return StatusCode(500, new { message = "Error simulating tracking", error = ex.Message });
            }
        }

        /// <summary>
        /// Test location update
        /// </summary>
        [HttpPost("test-update")]
        public async Task<IActionResult> TestLocationUpdate([FromBody] UpdateLocationRequest request)
        {
            try
            {
                // Simulate user ID
                int simulatedUserId = 1;
                
                var success = await _locationService.UpdateUserLocationAsync(
                    simulatedUserId, request.Latitude, request.Longitude);
                
                return Ok(new 
                { 
                    message = "Location update test completed",
                    success = success,
                    latitude = request.Latitude,
                    longitude = request.Longitude,
                    userId = simulatedUserId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing location update");
                return StatusCode(500, new { message = "Error testing location update", error = ex.Message });
            }
        }

        /// <summary>
        /// Health check for location service
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "Location service is running",
                timestamp = DateTime.UtcNow,
                endpoints = new[]
                {
                    "/api/testlocation/test-distance - Test distance calculation",
                    "/api/testlocation/simulate-tracking/{bookingId} - Simulate location tracking",
                    "/api/testlocation/test-update - Test location update",
                    "/api/location/update - Update user location (Auth required)",
                    "/api/location/calculate-distance - Calculate distance (Auth required)",
                    "/api/location/track/{bookingId} - Track location for booking (Auth required)",
                    "/locationHub - SignalR hub for real-time location updates"
                }
            });
        }
    }
}