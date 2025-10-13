using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repositories.Basic;
using Repositories.Models;
using Services.RequestsResponses.Location;
using System.Text.Json;

namespace Services
{
    public interface ILocationService
    {
        Task<double> CalculateDistanceAsync(decimal lat1, decimal lon1, decimal lat2, decimal lon2);
        Task<LocationDistanceResponse> GetDistanceAndDurationAsync(decimal startLat, decimal startLon, decimal endLat, decimal endLon);
        Task<bool> UpdateUserLocationAsync(int userId, decimal latitude, decimal longitude);
        Task<LocationTrackingHistory?> TrackUserLocationAsync(int bookingId, int userId, decimal latitude, decimal longitude);
        Task<List<LocationTrackingHistory>> GetLocationHistoryAsync(int bookingId, int userId);
    }

    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationService> _logger;

        public LocationService(IUnitOfWork unitOfWork, IConfiguration configuration, 
            HttpClient httpClient, ILogger<LocationService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Calculate distance using Haversine formula (straight line distance)
        /// </summary>
        public async Task<double> CalculateDistanceAsync(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            var R = 6371; // Earth radius in km
            var dLat = ToRadians((double)(lat2 - lat1));
            var dLon = ToRadians((double)(lon2 - lon1));
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return R * c; // Distance in km
        }

        /// <summary>
        /// Get distance and duration using Google Maps Directions API
        /// </summary>
        public async Task<LocationDistanceResponse> GetDistanceAndDurationAsync(
            decimal startLat, decimal startLon, decimal endLat, decimal endLon)
        {
            try
            {
                var apiKey = _configuration["GoogleMaps:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    // Fallback to straight line distance
                    var straightDistance = await CalculateDistanceAsync(startLat, startLon, endLat, endLon);
                    return new LocationDistanceResponse
                    {
                        DistanceKm = straightDistance,
                        DurationMinutes = (int)(straightDistance * 2), // Rough estimate
                        DistanceText = $"{straightDistance:F1} km",
                        DurationText = $"{straightDistance * 2:F0} phút"
                    };
                }

                var origin = $"{startLat},{startLon}";
                var destination = $"{endLat},{endLon}";
                
                var url = $"https://maps.googleapis.com/maps/api/directions/json" +
                         $"?origin={origin}&destination={destination}" +
                         $"&mode=driving&key={apiKey}";

                var response = await _httpClient.GetStringAsync(url);
                var result = JsonSerializer.Deserialize<GoogleDirectionsResponse>(response);

                if (result?.Routes?.Any() == true)
                {
                    var route = result.Routes.First();
                    var leg = route.Legs.First();
                    
                    return new LocationDistanceResponse
                    {
                        DistanceKm = leg.Distance.Value / 1000.0, // Convert to km
                        DurationMinutes = leg.Duration.Value / 60, // Convert to minutes
                        DistanceText = leg.Distance.Text,
                        DurationText = leg.Duration.Text
                    };
                }

                // Fallback to straight line distance
                var fallbackDistance = await CalculateDistanceAsync(startLat, startLon, endLat, endLon);
                return new LocationDistanceResponse
                {
                    DistanceKm = fallbackDistance,
                    DurationMinutes = (int)(fallbackDistance * 2), // Rough estimate
                    DistanceText = $"{fallbackDistance:F1} km",
                    DurationText = $"{fallbackDistance * 2:F0} phút"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting directions from Google Maps API");
                
                // Fallback to straight line distance
                var distance = await CalculateDistanceAsync(startLat, startLon, endLat, endLon);
                return new LocationDistanceResponse
                {
                    DistanceKm = distance,
                    DurationMinutes = (int)(distance * 2),
                    DistanceText = $"{distance:F1} km (ước tính)",
                    DurationText = $"{distance * 2:F0} phút (ước tính)"
                };
            }
        }

        public async Task<bool> UpdateUserLocationAsync(int userId, decimal latitude, decimal longitude)
        {
            try
            {
                // For now, we'll use the generic repository pattern since we don't have specific user repository
                // This can be updated later when user repository is properly integrated
                _logger.LogInformation("Updating location for UserId: {UserId} to Lat: {Lat}, Lon: {Lon}", 
                    userId, latitude, longitude);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user location for UserId: {UserId}", userId);
                return false;
            }
        }

        public async Task<LocationTrackingHistory?> TrackUserLocationAsync(
            int bookingId, int userId, decimal latitude, decimal longitude)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null) return null;

                // Calculate distance to meeting location
                decimal? distanceToMeeting = null;
                if (booking.MeetingLatitude.HasValue && booking.MeetingLongitude.HasValue)
                {
                    var distance = await CalculateDistanceAsync(
                        latitude, longitude, 
                        booking.MeetingLatitude.Value, booking.MeetingLongitude.Value);
                    distanceToMeeting = (decimal)distance;
                }

                var trackingRecord = new LocationTrackingHistory
                {
                    BookingId = bookingId,
                    UserId = userId,
                    Latitude = latitude,
                    Longitude = longitude,
                    DistanceToDestination = distanceToMeeting,
                    Timestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // For now, just log the tracking record since we don't have LocationTrackingHistory repository yet
                _logger.LogInformation("Tracking location for BookingId: {BookingId}, UserId: {UserId}, Distance: {Distance}km", 
                    bookingId, userId, distanceToMeeting);

                // Update user's current location
                await UpdateUserLocationAsync(userId, latitude, longitude);

                return trackingRecord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking location for BookingId: {BookingId}, UserId: {UserId}", 
                    bookingId, userId);
                return null;
            }
        }

        public async Task<List<LocationTrackingHistory>> GetLocationHistoryAsync(int bookingId, int userId)
        {
            try
            {
                // For now, return empty list since we don't have LocationTrackingHistory repository yet
                // This can be implemented later when the repository is properly set up
                _logger.LogInformation("Getting location history for BookingId: {BookingId}, UserId: {UserId}", 
                    bookingId, userId);
                
                return new List<LocationTrackingHistory>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting location history for BookingId: {BookingId}, UserId: {UserId}", 
                    bookingId, userId);
                return new List<LocationTrackingHistory>();
            }
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }
    }
}