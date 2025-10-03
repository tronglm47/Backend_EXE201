namespace Services.RequestsResponses.Building
{
    public class BuildingRequest
    {
        public class BuildingCreate
        {
            public int? SubdivisionId { get; set; }
            public string Name { get; set; }
            public string BlockCode { get; set; }
            public string Description { get; set; }
        }
        public class BuildingUpdate
        {
            public int? SubdivisionId { get; set; }
            public string Name { get; set; }
            public string BlockCode { get; set; }
            public string Description { get; set; }
        }
    }
}
