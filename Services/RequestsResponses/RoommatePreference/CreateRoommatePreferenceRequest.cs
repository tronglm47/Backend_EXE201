using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.RoommatePreference
{
    public class CreateRoommatePreferenceRequest
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(20)]
        public string? PreferredGender { get; set; }

        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
        public int? AgeRangeMin { get; set; }

        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
        public int? AgeRangeMax { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be positive")]
        public decimal? BudgetMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budget must be positive")]
        public decimal? BudgetMax { get; set; }

        public string? Habits { get; set; }

        public string? Interests { get; set; }

        public int? LocationId { get; set; }
    }
}