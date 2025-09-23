namespace Services.RequestsResponses.Ad
{
    public class AdSearchRequest
    {
        public int? UserId { get; set; }
        public int? RequestId { get; set; }
        public string? Status { get; set; }
        public string? Title { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? MinViews { get; set; }
        public int? MaxViews { get; set; }
        public int? MinClicks { get; set; }
        public int? MaxClicks { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}