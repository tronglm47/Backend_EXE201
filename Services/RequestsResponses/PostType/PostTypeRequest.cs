namespace Services.RequestsResponses.PostType
{
    public class PostTypeRequest
    {
        public class PostTypeCreate
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public class PostTypeUpdate
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
