using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Post
{
    public class UpdatePostRequest
    {
        public int? PropertyId { get; set; }

        [StringLength(20, ErrorMessage = "Type cannot exceed 20 characters")]
        public string? Type { get; set; }

        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        public string? Content { get; set; }

        public string? Images { get; set; }
    }
}