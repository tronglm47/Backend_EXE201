namespace Services.RequestsResponses.Post
{
    public class PostSearchRequest
    {
        public int? UserId { get; set; }
        public int? PropertyId { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int? MinViews { get; set; }
        public int? MaxViews { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}