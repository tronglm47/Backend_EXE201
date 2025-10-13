using Services.RequestsResponses.Subdivision;

namespace Services.RequestsResponses.Building
{
    public class BuildingResponse
    {
        public class BuildingGetAll
        {
            public int BuildingId { get; set; }
            public int? SubdivisionId { get; set; }
            public string? SubdivisionName { get; set; }
            public string Name { get; set; } = string.Empty;
            public string BlockCode { get; set; } = string.Empty;
            public int? MaxFloor { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
        public class BuildingDetail
        {
            public int BuildingId { get; set; }
            public int? SubdivisionId { get; set; }
            public SubdivisionResponse.SubdivisionDetail? Subdivision { get; set; }
            public string Name { get; set; } = string.Empty;
            public string BlockCode { get; set; } = string.Empty;
            public int? MaxFloor { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}
