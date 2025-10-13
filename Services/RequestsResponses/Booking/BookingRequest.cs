using System.ComponentModel.DataAnnotations;
using Services.RequestsResponses;

namespace Services.RequestsResponses.Booking
{
    public class BookingRequest
    {
        public class BookingCreate
        {
            [Required(ErrorMessage = "PostId is required")]
            public int PostId { get; set; }

            [Required(ErrorMessage = "MeetingTime is required")]
            public DateTime MeetingTime { get; set; }

            [Required(ErrorMessage = "PlaceMeet is required")]
            [StringLength(255, ErrorMessage = "PlaceMeet cannot exceed 255 characters")]
            public string PlaceMeet { get; set; } = null!;

            [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
            public string? Note { get; set; }
        }

        public class BookingUpdate
        {
            public DateTime? MeetingTime { get; set; }

            [StringLength(255, ErrorMessage = "PlaceMeet cannot exceed 255 characters")]
            public string? PlaceMeet { get; set; }

            [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
            public string? Status { get; set; }

            [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
            public string? Note { get; set; }
        }

        public class BookingStatusUpdate
        {
            [Required(ErrorMessage = "Status is required")]
            [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
            public string Status { get; set; } = null!;

            [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
            public string? Note { get; set; }
        }
    }

    public class BookingQuery : QueryParametersBase
    {
        public override string DefaultSortBy => "createdat";
        public override string DefaultSearchField => "status";
        
        public string? Status { get; set; }
        public int? RenterId { get; set; }
        public int? PostId { get; set; }
        public int? LandlordId { get; set; }
    }
}