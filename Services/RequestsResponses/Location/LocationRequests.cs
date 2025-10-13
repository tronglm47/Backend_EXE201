namespace Services.RequestsResponses.Location
{
    public class UpdateLocationRequest
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class CalculateDistanceRequest
    {
        public decimal StartLatitude { get; set; }
        public decimal StartLongitude { get; set; }
        public decimal EndLatitude { get; set; }
        public decimal EndLongitude { get; set; }
    }

    public class LocationDistanceResponse
    {
        public double DistanceKm { get; set; }
        public int DurationMinutes { get; set; }
        public string DistanceText { get; set; }
        public string DurationText { get; set; }
    }

    public class LocationUpdateNotification
    {
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double? DistanceToMeeting { get; set; }
        public int? EstimatedArrivalMinutes { get; set; }
        public string DistanceText { get; set; }
        public string DurationText { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class GoogleDirectionsResponse
    {
        public Route[] Routes { get; set; }
        public string Status { get; set; }
    }

    public class Route
    {
        public Leg[] Legs { get; set; }
    }

    public class Leg
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
    }

    public class Distance
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class Duration
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
}