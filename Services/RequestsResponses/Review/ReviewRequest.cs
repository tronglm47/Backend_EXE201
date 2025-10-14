using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Review
{
    public class ReviewRequest
    {
        public class ReviewCreate
        {
            [Required(ErrorMessage = "BookingID is required")]
            [Range(1, int.MaxValue, ErrorMessage = "BookingID must be greater than 0")]
            public int BookingId { get; set; }

            [Required(ErrorMessage = "Rating is required")]
            [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
            public int Rating { get; set; }

            [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters")]
            public string? Description { get; set; }
        }

        public class ReviewUpdate
        {
            [Required(ErrorMessage = "Rating is required")]
            [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
            public int Rating { get; set; }

            [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters")]
            public string? Description { get; set; }
        }
    }
}