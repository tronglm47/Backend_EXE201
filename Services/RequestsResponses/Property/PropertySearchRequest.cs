namespace Services.RequestsResponses.Property
{
    public class PropertySearchRequest
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Type { get; set; }
        public int? LocationId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}