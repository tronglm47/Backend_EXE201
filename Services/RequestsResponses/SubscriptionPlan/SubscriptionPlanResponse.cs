namespace Services.RequestsResponses.SubscriptionPlan
{
    public class SubscriptionPlanResponse
    {
        public int PlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MonthlyPrice { get; set; }
        public string? Features { get; set; }
        public int? DurationMonths { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Statistical information
        public int ActiveSubscriptionsCount { get; set; }
        public int TotalSubscriptionsCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}