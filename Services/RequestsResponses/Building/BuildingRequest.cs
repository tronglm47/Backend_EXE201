namespace Services.RequestsResponses.Building
{
    public class BuildingRequest
    {
        public class BuildingCreate
        {
            public int? SubdivisionId { get; set; }
            public required string Name { get; set; }
            public required string BlockCode { get; set; }
        }
        public class BuildingUpdate
        {
            public int? SubdivisionId { get; set; }
            public required string Name { get; set; }
            public required string BlockCode { get; set; }
        }
    }
}
