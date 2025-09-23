using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.Ad
{
    public class UpdateAdStatusRequest
    {
        [Required]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        public string Status { get; set; } = string.Empty;
    }
}