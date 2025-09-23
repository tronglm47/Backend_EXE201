using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.AdRequest
{
    public class UpdateAdRequestRequest
    {
        [StringLength(100)]
        public string? CompanyName { get; set; }

        public string? AdContent { get; set; }

        [StringLength(255)]
        public string? TargetAudience { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be a positive value")]
        public decimal? Budget { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 day")]
        public int? DurationDays { get; set; }
    }
}