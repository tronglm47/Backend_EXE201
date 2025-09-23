using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.SubscriptionPlan
{
    public class SubscriptionPlanSearchRequest
    {
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Min price must be between 0 and 999,999.99")]
        public decimal? MinPrice { get; set; }

        [Range(0, 999999.99, ErrorMessage = "Max price must be between 0 and 999,999.99")]
        public decimal? MaxPrice { get; set; }

        [Range(1, 120, ErrorMessage = "Min duration must be between 1 and 120 months")]
        public int? MinDurationMonths { get; set; }

        [Range(1, 120, ErrorMessage = "Max duration must be between 1 and 120 months")]
        public int? MaxDurationMonths { get; set; }

        public bool? HasActiveSubscriptions { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; } = "Name"; // Name, MonthlyPrice, CreatedAt, DurationMonths
        public string? SortOrder { get; set; } = "asc"; // asc, desc
    }
}