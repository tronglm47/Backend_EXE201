namespace Services.RequestsResponses.PostAmenity
{
    public class PostAmenityRequest
    {
        public class CreatePostAmenity
        {
            public int PostID { get; set; }
            public int AmenityId { get; set; }
            public string? Notes { get; set; }
        }
        public class UpdatePostAmenity
        {
            public int PostID { get; set; }
            public int AmenityId { get; set; }
            public string? Notes { get; set; } = null;
        }
    }
}
