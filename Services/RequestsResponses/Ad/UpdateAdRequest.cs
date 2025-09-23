using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Ad
{
    public class UpdateAdRequest
    {
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [StringLength(255, ErrorMessage = "Image URL cannot exceed 255 characters")]
        public string? ImageUrl { get; set; }

        [StringLength(255, ErrorMessage = "Link URL cannot exceed 255 characters")]
        public string? LinkUrl { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string? Status { get; set; }
    }
}