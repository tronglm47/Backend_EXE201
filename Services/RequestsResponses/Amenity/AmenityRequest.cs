namespace Services.RequestsResponses.Amenity
{
    public class AmenityRequest
    {
        public class CreateAmenity
        {
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
        }
        public class UpdateAmenity
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }
    }
}
