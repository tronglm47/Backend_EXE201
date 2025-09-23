namespace Services.RequestsResponses.RoommateMatch
{
    public class RoommateMatchSearchRequest
    {
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}