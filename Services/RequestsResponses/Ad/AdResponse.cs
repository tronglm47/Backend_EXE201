namespace Services.RequestsResponses.Ad
{
    public class AdResponse
    {
        public int AdId { get; set; }
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? Views { get; set; }
        public int? Clicks { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }

        // Additional related data (optional)
        public string? UserName { get; set; }
        public string? CompanyName { get; set; }
    }
}