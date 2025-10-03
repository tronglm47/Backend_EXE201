using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Location
{
    public class LocationRequest
    {
        public class LocationCreate
        {
            [Required(ErrorMessage = "Location name is required")]
            [StringLength(100, ErrorMessage = "Location name cannot exceed 100 characters")]
            public string Name { get; set; } = string.Empty;
            
            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
            public string? Description { get; set; }
            
            [Required(ErrorMessage = "Location type is required")]
            [StringLength(50, ErrorMessage = "Location type cannot exceed 50 characters")]
            public string LocationType { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Location code is required")]
            [StringLength(20, ErrorMessage = "Location code cannot exceed 20 characters")]
            public string LocationCode { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Full address is required")]
            [StringLength(255, ErrorMessage = "Full address cannot exceed 255 characters")]
            public string FullAddress { get; set; } = string.Empty;
            
            [Range(1, int.MaxValue, ErrorMessage = "Parent location ID must be greater than 0")]
            public int? ParentLocationId { get; set; }
            
            [Range(0, 10, ErrorMessage = "Level must be between 0 and 10")]
            public int Level { get; set; }
        }
        
        public class LocationUpdate
        {
            [Required(ErrorMessage = "Location name is required")]
            [StringLength(100, ErrorMessage = "Location name cannot exceed 100 characters")]
            public string Name { get; set; } = string.Empty;
            
            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
            public string? Description { get; set; }
            
            [Required(ErrorMessage = "Location type is required")]
            [StringLength(50, ErrorMessage = "Location type cannot exceed 50 characters")]
            public string LocationType { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Location code is required")]
            [StringLength(20, ErrorMessage = "Location code cannot exceed 20 characters")]
            public string LocationCode { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Full address is required")]
            [StringLength(255, ErrorMessage = "Full address cannot exceed 255 characters")]
            public string FullAddress { get; set; } = string.Empty;
            
            [Range(1, int.MaxValue, ErrorMessage = "Parent location ID must be greater than 0")]
            public int? ParentLocationId { get; set; }
            
            [Range(0, 10, ErrorMessage = "Level must be between 0 and 10")]
            public int Level { get; set; }
            
            public bool IsActive { get; set; }
        }
    }
}
