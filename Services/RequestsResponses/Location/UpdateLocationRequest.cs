using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Location
{
    public class UpdateLocationRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string? Description { get; set; }

        public int? ParentLocationId { get; set; }
    }
}