namespace Services.RequestsResponses.Amenity
{
    public class AmenityResponse
    {
        public class GetALlResponse
        {
            public int AmenityId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class GetByIdResponse
        {
            public int AmenityId { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
