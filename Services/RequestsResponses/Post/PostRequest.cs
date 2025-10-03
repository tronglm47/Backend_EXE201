using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Services.RequestsResponses.Post
{
    public class PostRequest
    {
        public class CreatePost
        {
            [Required(ErrorMessage = "UserId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
            public int UserId { get; set; }

            [Required(ErrorMessage = "PostTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PostTypeId must be greater than 0")]
            public int PostTypeId { get; set; }

            [Required(ErrorMessage = "PropertyTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyTypeId must be greater than 0")]
            public int PropertyTypeId { get; set; }

            [Required(ErrorMessage = "PropertyFormId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyFormId must be greater than 0")]
            public int PropertyFormId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "LocationId must be greater than 0 if provided")]
            public int? LocationId { get; set; } = null;

            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Content is required")]
            [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters")]
            public string Content { get; set; } = string.Empty;

            [StringLength(2000, ErrorMessage = "Images field cannot exceed 2000 characters")]
            public string? Images { get; set; }

            [Required(ErrorMessage = "Price is required")]
            [Range(0.01, 999999999.99, ErrorMessage = "Price must be between 0.01 and 999,999,999.99")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "At least one amenity must be selected")]
            [MinLength(1, ErrorMessage = "At least one amenity must be selected")]
            public List<int> AmenityId { get; set; } = new List<int>();
        }

        public class CreatePostWithFiles
        {
            [Required(ErrorMessage = "UserId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
            public int UserId { get; set; }

            [Required(ErrorMessage = "PostTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PostTypeId must be greater than 0")]
            public int PostTypeId { get; set; }

            [Required(ErrorMessage = "PropertyTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyTypeId must be greater than 0")]
            public int PropertyTypeId { get; set; }

            [Required(ErrorMessage = "PropertyFormId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyFormId must be greater than 0")]
            public int PropertyFormId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "LocationId must be greater than 0 if provided")]
            public int? LocationId { get; set; } = null;

            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Content is required")]
            [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters")]
            public string Content { get; set; } = string.Empty;

            [Required(ErrorMessage = "Price is required")]
            [Range(0.01, 999999999.99, ErrorMessage = "Price must be between 0.01 and 999,999,999.99")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "At least one amenity must be selected")]
            [MinLength(1, ErrorMessage = "At least one amenity must be selected")]
            public List<int> AmenityId { get; set; } = new List<int>();

            // Optional image files for upload
            public List<IFormFile>? ImageFiles { get; set; }
        }
        public class PostUpdateRequest
        {
            [Required(ErrorMessage = "UserId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
            public int UserId { get; set; }

            [Required(ErrorMessage = "PostTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PostTypeId must be greater than 0")]
            public int PostTypeId { get; set; }

            [Required(ErrorMessage = "PropertyTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyTypeId must be greater than 0")]
            public int PropertyTypeId { get; set; }

            [Required(ErrorMessage = "PropertyFormId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyFormId must be greater than 0")]
            public int PropertyFormId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "LocationId must be greater than 0 if provided")]
            public int? LocationId { get; set; } = null;

            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Content is required")]
            [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters")]
            public string Content { get; set; } = string.Empty;

            [StringLength(2000, ErrorMessage = "Images field cannot exceed 2000 characters")]
            public string? Images { get; set; }

            [Range(0.01, 999999999.99, ErrorMessage = "Price must be between 0.01 and 999,999,999.99")]
            public decimal? Price { get; set; }

            [Required(ErrorMessage = "Status is required")]
            [StringLength(50, MinimumLength = 1, ErrorMessage = "Status must be between 1 and 50 characters")]
            public string Status { get; set; } = string.Empty;

            // Optional: Add AmenityIds for updating amenities relationships
            public List<int>? AmenityId { get; set; }
        }

        public class UpdatePostWithFiles
        {
            [Required(ErrorMessage = "UserId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
            public int UserId { get; set; }

            [Required(ErrorMessage = "PostTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PostTypeId must be greater than 0")]
            public int PostTypeId { get; set; }

            [Required(ErrorMessage = "PropertyTypeId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyTypeId must be greater than 0")]
            public int PropertyTypeId { get; set; }

            [Required(ErrorMessage = "PropertyFormId is required")]
            [Range(1, int.MaxValue, ErrorMessage = "PropertyFormId must be greater than 0")]
            public int PropertyFormId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "LocationId must be greater than 0 if provided")]
            public int? LocationId { get; set; } = null;

            [Required(ErrorMessage = "Title is required")]
            [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Content is required")]
            [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters")]
            public string Content { get; set; } = string.Empty;

            [Range(0.01, 999999999.99, ErrorMessage = "Price must be between 0.01 and 999,999,999.99")]
            public decimal? Price { get; set; }

            [Required(ErrorMessage = "Status is required")]
            [StringLength(50, MinimumLength = 1, ErrorMessage = "Status must be between 1 and 50 characters")]
            public string Status { get; set; } = string.Empty;

            // Optional: Add AmenityIds for updating amenities relationships
            public List<int>? AmenityId { get; set; }

            // Optional image files for upload - these will replace existing images
            public List<IFormFile>? ImageFiles { get; set; }

            // Optional: Keep existing images (true) or replace all with new files (false)
            public bool KeepExistingImages { get; set; } = false;
        }

        public class UpdatePostImages
        {
            // Optional image files for upload - these will replace existing images
            public List<IFormFile>? ImageFiles { get; set; }

            // Optional: Keep existing images (true) or replace all with new files (false)
            public bool KeepExistingImages { get; set; } = false;
        }
    }
}
