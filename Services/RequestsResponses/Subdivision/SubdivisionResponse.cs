namespace Services.RequestsResponses.Subdivision
{
    public class SubdivisionResponse
    {
        public class SubdivisionGetAll()
        {
            public int SubdivisionId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
        public class SubdivisionDetail()
        {
            public int SubdivisionId { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}
