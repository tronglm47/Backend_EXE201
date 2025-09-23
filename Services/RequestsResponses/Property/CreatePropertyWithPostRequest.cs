namespace Services.RequestsResponses.Property
{
    public class CreatePropertyWithPostRequest
    {
        public int OwnerId { get; set; }
        public int LocationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? Area { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public string? Description { get; set; }
        public string? Images { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public string PostContent { get; set; } = string.Empty;
    }
}