namespace Services.RequestsResponses.Property
{
    public class PropertyResponse
    {
        public int PropertyId { get; set; }
        public int OwnerId { get; set; }
        public int LocationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? Area { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public string? Description { get; set; }
        public string? Images { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Optional: Include related data
        public string? OwnerName { get; set; }
        public string? LocationName { get; set; }
    }
}