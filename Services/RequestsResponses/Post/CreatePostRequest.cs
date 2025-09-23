using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Post
{
    public class CreatePostRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public int? PropertyId { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(20, ErrorMessage = "Type cannot exceed 20 characters")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        public string? Images { get; set; }
    }
}