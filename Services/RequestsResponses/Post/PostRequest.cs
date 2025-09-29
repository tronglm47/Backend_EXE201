namespace Services.RequestsResponses.Post
{
    public class PostRequest
    {
        public class PostCreateRequest
        {
            public int UserId { get; set; }
            public int PostTypeId { get; set; }
            public int PropertyTypeId { get; set; }
            public int PropertyFormId { get; set; }
            public int LocationId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? Images { get; set; }
            public decimal? Price { get; set; }
        }
        public class PostUpdateRequest
        {
            public int PostId { get; set; }
            public int UserId { get; set; }
            public int PostTypeId { get; set; }
            public int PropertyTypeId { get; set; }
            public int PropertyFormId { get; set; }
            public int LocationId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public string? Images { get; set; }
            public decimal? Price { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}
