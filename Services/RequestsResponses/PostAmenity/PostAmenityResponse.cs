namespace Services.RequestsResponses.PostAmenity
{
    public class PostAmenityResponse
    {
        public class GetAll
        {
            public int PostId { get; set; }
            public int AmenityId { get; set; }
            public string? Notes { get; set; }
        }
        public class GetById
        {
            public int PostId { get; set; }
            public int AmenityId { get; }
            public string? Notes { get; set; }
        }
    }
}
