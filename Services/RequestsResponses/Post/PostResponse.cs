namespace Services.RequestsResponses.Post
{
    public class PostResponse
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public int? PropertyId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Images { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Views { get; set; }

        // Navigation properties for response
        public string? UserName { get; set; }
        public string? PropertyTitle { get; set; }
        public string? PropertyType { get; set; }
        public int BookingsCount { get; set; }
    }
}