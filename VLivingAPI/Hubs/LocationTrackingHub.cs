using Microsoft.AspNetCore.SignalR;
using Services;
using Services.RequestsResponses.Location;

namespace VLivingAPI.Hubs
{
    public class LocationTrackingHub : Hub
    {
        private readonly ILocationService _locationService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<LocationTrackingHub> _logger;

        public LocationTrackingHub(ILocationService locationService, 
            IBookingService bookingService, ILogger<LocationTrackingHub> logger)
        {
            _locationService = locationService;
            _bookingService = bookingService;
            _logger = logger;
        }

        #region Connection Management

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].FirstOrDefault() ?? Context.ConnectionId;
            Context.Items["UserId"] = userId;

            _logger.LogInformation($"User {userId} connected to LocationTrackingHub with connection {Context.ConnectionId}");

            // Join user to their personal room
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.Items["UserId"]?.ToString() ?? Context.ConnectionId;
            _logger.LogInformation($"User {userId} disconnected from LocationTrackingHub");

            await base.OnDisconnectedAsync(exception);
        }

        #endregion

        #region Location Tracking

        /// <summary>
        /// Update user location and notify participants in booking
        /// </summary>
        public async Task UpdateLocation(int bookingId, decimal latitude, decimal longitude)
        {
            var userId = Context.Items["UserId"]?.ToString() ?? Context.ConnectionId;

            try
            {
                if (!int.TryParse(userId, out int userIdInt))
                {
                    throw new HubException("Invalid user ID");
                }

                // Track location in database
                var trackingRecord = await _locationService.TrackUserLocationAsync(
                    bookingId, userIdInt, latitude, longitude);

                if (trackingRecord == null)
                {
                    throw new HubException("Failed to track location");
                }

                // Get booking details to find other participants
                var booking = await _bookingService.GetDetailAsync(bookingId);
                if (booking == null)
                {
                    throw new HubException("Booking not found");
                }

                // Calculate distance info if meeting location is set
                LocationDistanceResponse distanceInfo = null;
                if (booking.MeetingLatitude.HasValue && booking.MeetingLongitude.HasValue)
                {
                    distanceInfo = await _locationService.GetDistanceAndDurationAsync(
                        latitude, longitude,
                        booking.MeetingLatitude.Value, booking.MeetingLongitude.Value);
                }

                // Notify all participants in the booking about location update
                var participants = new[] { booking.RenterId.ToString(), booking.LandlordId.ToString() };
                
                foreach (var participantId in participants)
                {
                    await Clients.Group($"User_{participantId}")
                        .SendAsync("LocationUpdated", new LocationUpdateNotification
                        {
                            BookingId = bookingId,
                            UserId = userIdInt,
                            UserName = booking.RenterId == userIdInt ? booking.RenterName : "Landlord",
                            Latitude = latitude,
                            Longitude = longitude,
                            DistanceToMeeting = distanceInfo?.DistanceKm,
                            EstimatedArrivalMinutes = distanceInfo?.DurationMinutes,
                            DistanceText = distanceInfo?.DistanceText,
                            DurationText = distanceInfo?.DurationText,
                            Timestamp = DateTime.UtcNow
                        });
                }

                _logger.LogInformation("Location updated for BookingId: {BookingId}, UserId: {UserId}", 
                    bookingId, userIdInt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating location for BookingId: {BookingId}", bookingId);
                await Clients.Caller.SendAsync("LocationError", "Failed to update location");
                throw;
            }
        }

        /// <summary>
        /// Start location tracking for a booking
        /// </summary>
        public async Task StartLocationTracking(int bookingId)
        {
            var userId = Context.Items["UserId"]?.ToString() ?? Context.ConnectionId;

            try
            {
                // Join location tracking group for this booking
                await Groups.AddToGroupAsync(Context.ConnectionId, $"LocationTracking_{bookingId}");

                // Notify other participants
                await Clients.GroupExcept($"LocationTracking_{bookingId}", Context.ConnectionId)
                    .SendAsync("LocationTrackingStarted", new
                    {
                        BookingId = bookingId,
                        UserId = userId,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("Location tracking started for BookingId: {BookingId}, UserId: {UserId}", 
                    bookingId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting location tracking for BookingId: {BookingId}", bookingId);
                throw;
            }
        }

        /// <summary>
        /// Stop location tracking for a booking
        /// </summary>
        public async Task StopLocationTracking(int bookingId)
        {
            var userId = Context.Items["UserId"]?.ToString() ?? Context.ConnectionId;

            try
            {
                // Leave location tracking group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"LocationTracking_{bookingId}");

                // Notify other participants
                await Clients.Group($"LocationTracking_{bookingId}")
                    .SendAsync("LocationTrackingStopped", new
                    {
                        BookingId = bookingId,
                        UserId = userId,
                        Timestamp = DateTime.UtcNow
                    });

                _logger.LogInformation("Location tracking stopped for BookingId: {BookingId}, UserId: {UserId}", 
                    bookingId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping location tracking for BookingId: {BookingId}", bookingId);
                throw;
            }
        }

        #endregion
    }
}