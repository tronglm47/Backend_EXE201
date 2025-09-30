namespace Services.RequestsResponses.PostType
{
    public class PostTypeResponse
    {
        public class PostTypeGetAll
        {
            public int PostTypeID {  get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime CreateAt { get; set; }
        }
        public class PostTypeGetById
        {
            public int PostTypeID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime CreateAt { get; set; }
        }
    }
}
