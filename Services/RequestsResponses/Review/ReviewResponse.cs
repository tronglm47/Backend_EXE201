namespace Services.RequestsResponses.Review
{
    public class ReviewResponse
    {
        public class ReviewInfo
        {
            public int ReviewId { get; set; }
            public int BookingId { get; set; }
            public int UserId { get; set; }
            public int PostId { get; set; }
            public int Rating { get; set; }
            public string? Description { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            
            // User info
            public ReviewUserInfo? User { get; set; }
        }

        public class ReviewDetail
        {
            public int ReviewId { get; set; }
            public int BookingId { get; set; }
            public int UserId { get; set; }
            public int PostId { get; set; }
            public int Rating { get; set; }
            public string? Description { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            
            // Related entities
            public ReviewUserInfo? User { get; set; }
            public PostInfo? Post { get; set; }
            public ReviewBookingInfo? Booking { get; set; }
        }

        public class ReviewUserInfo
        {
            public int UserId { get; set; }
            public string? Username { get; set; }
            public string? FullName { get; set; }
            public string? ProfilePictureUrl { get; set; }
        }

        public class PostInfo
        {
            public int PostId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public decimal? Price { get; set; }
            public decimal? AverageRating { get; set; }
            public int TotalReviews { get; set; }
            public ReviewApartmentInfo? Apartment { get; set; }
        }

        public class ReviewApartmentInfo
        {
            public int ApartmentId { get; set; }
            public string? ApartmentCode { get; set; }
            public int? Floor { get; set; }
            public decimal? Area { get; set; }
            public ReviewBuildingInfo? Building { get; set; }
        }

        public class ReviewBuildingInfo
        {
            public int BuildingId { get; set; }
            public string? Name { get; set; }
            public string? BlockCode { get; set; }
        }

        public class ReviewBookingInfo
        {
            public int BookingId { get; set; }
            public DateTime? MeetingTime { get; set; }
            public string? PlaceMeet { get; set; }
            public string? Status { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}