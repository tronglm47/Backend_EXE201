namespace Services.RequestsResponses.Activity
{
    public class ActivitySearchRequest
    {
        public int? CreatorId { get; set; }
        public int? LocationId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Title { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}