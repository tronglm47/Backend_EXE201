namespace Services.RequestsResponses.AdRequest
{
    public class AdRequestResponse
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string AdContent { get; set; } = string.Empty;
        public string? TargetAudience { get; set; }
        public decimal? Budget { get; set; }
        public int? DurationDays { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Optional: Include related data
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }
}