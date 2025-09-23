using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Ad
{
    public class CreateAdRequest
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
        public string? ImageUrl { get; set; }

        [StringLength(255, ErrorMessage = "Link URL cannot exceed 255 characters")]
        public string? LinkUrl { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string Status { get; set; } = "pending";
    }
}