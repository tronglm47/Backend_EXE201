namespace Services.RequestsResponses.Booking
{
    public class BookingResponse
    {
        public class BookingInfo
        {
            public int BookingId { get; set; }
            public int RenterId { get; set; }
            public string RenterName { get; set; } = null!;
            public string RenterEmail { get; set; } = null!;
            public string? RenterPhone { get; set; }
            public int PostId { get; set; }
            public string PostTitle { get; set; } = null!;
            public DateTime? MeetingTime { get; set; }
            public string? PlaceMeet { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class BookingDetail
        {
            public int BookingId { get; set; }
            public int RenterId { get; set; }
            public string RenterName { get; set; } = null!;
            public string RenterEmail { get; set; } = null!;
            public string? RenterPhone { get; set; }
            public int PostId { get; set; }
            public string PostTitle { get; set; } = null!;
            public string PostDescription { get; set; } = null!;
            public decimal? PostPrice { get; set; }
            public int LandlordId { get; set; }
            public string LandlordName { get; set; } = null!;
            public string LandlordEmail { get; set; } = null!;
            public string? LandlordPhone { get; set; }
            public DateTime? MeetingTime { get; set; }
            public string? PlaceMeet { get; set; }
            public decimal? MeetingLatitude { get; set; }
            public decimal? MeetingLongitude { get; set; }
            public string? MeetingAddress { get; set; }
            public decimal? DistanceToMeeting { get; set; }
            public DateTime? EstimatedArrivalTime { get; set; }
            public bool? IsLocationTrackingEnabled { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class BookingForRenter
        {
            public int BookingId { get; set; }
            public int PostId { get; set; }
            public string PostTitle { get; set; } = null!;
            public string PostDescription { get; set; } = null!;
            public decimal? PostPrice { get; set; }
            public string PostType { get; set; } = null!;
            public int LandlordId { get; set; }
            public string LandlordName { get; set; } = null!;
            public string LandlordEmail { get; set; } = null!;
            public string? LandlordPhone { get; set; }
            public ApartmentInfo? Apartment { get; set; }
            public DateTime? MeetingTime { get; set; }
            public string? PlaceMeet { get; set; }
            public decimal? MeetingLatitude { get; set; }
            public decimal? MeetingLongitude { get; set; }
            public string? MeetingAddress { get; set; }
            public decimal? DistanceToMeeting { get; set; }
            public DateTime? EstimatedArrivalTime { get; set; }
            public bool? IsLocationTrackingEnabled { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class BookingForLandlord
        {
            public int BookingId { get; set; }
            public int RenterId { get; set; }
            public string RenterName { get; set; } = null!;
            public string RenterEmail { get; set; } = null!;
            public string? RenterPhone { get; set; }
            public int PostId { get; set; }
            public string PostTitle { get; set; } = null!;
            public ApartmentInfo? Apartment { get; set; }
            public DateTime? MeetingTime { get; set; }
            public string? PlaceMeet { get; set; }
            public decimal? MeetingLatitude { get; set; }
            public decimal? MeetingLongitude { get; set; }
            public string? MeetingAddress { get; set; }
            public decimal? DistanceToMeeting { get; set; }
            public DateTime? EstimatedArrivalTime { get; set; }
            public bool? IsLocationTrackingEnabled { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class ApartmentInfo
        {
            public int ApartmentId { get; set; }
            public string ApartmentCode { get; set; } = null!;
            public int Floor { get; set; }
            public double Area { get; set; }
            public int NumberBathroom { get; set; }
            public BuildingInfo Building { get; set; } = null!;
        }

        public class BuildingInfo
        {
            public int BuildingId { get; set; }
            public string BuildingName { get; set; } = null!;
            public string BlockCode { get; set; } = null!;
            public SubdivisionInfo Subdivision { get; set; } = null!;
        }

        public class SubdivisionInfo
        {
            public string SubdivisionId { get; set; } = null!;
            public string SubdivisionName { get; set; } = null!;
            public string? Type { get; set; }
            public string? Description { get; set; }
        }
    }
}