using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.AdRequest
{
    public class CreateAdRequestRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string AdContent { get; set; } = string.Empty;

        [StringLength(255)]
        public string? TargetAudience { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value")]
        public decimal? Budget { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 day")]
        public int? DurationDays { get; set; }
    }
}