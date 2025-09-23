using System.ComponentModel.DataAnnotations;

namespace Services.RequestsResponses.SubscriptionPlan
{
    public class CreateSubscriptionPlanRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Monthly price is required")]
        [Range(0, 999999.99, ErrorMessage = "Monthly price must be between 0 and 999,999.99")]
        public decimal MonthlyPrice { get; set; }

        [StringLength(2000, ErrorMessage = "Features cannot exceed 2000 characters")]
        public string? Features { get; set; }

        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months")]
        public int? DurationMonths { get; set; }
    }
}