namespace Services.RequestsResponses.AdRequest
{
    public class AdRequestSearchRequest
    {
        public int? UserId { get; set; }
        public string? CompanyName { get; set; }
        public string? Status { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}