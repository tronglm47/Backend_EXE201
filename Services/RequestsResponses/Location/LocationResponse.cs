namespace Services.RequestsResponses.Location
{
    public class LocationResponse
    {
        public class LocationGetAll
        {
            public int LocationId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string LocationType { get; set; } = string.Empty;
            public string LocationCode { get; set; } = string.Empty;
            public string FullAddress { get; set; } = string.Empty;
            public int? ParentLocationId { get; set; }
            public int Level { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class LocationGetDetail
        {
            public int LocationId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string LocationType { get; set; } = string.Empty;
            public string LocationCode { get; set; } = string.Empty;
            public string FullAddress { get; set; } = string.Empty;
            public int? ParentLocationId { get; set; }
            public int Level { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class LocationHierarchy
        {
            public List<LocationInfo> Hierarchy { get; set; } = new List<LocationInfo>();
        }

        public class LocationInfo
        {
            public int LocationId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string LocationType { get; set; } = string.Empty;
            public string LocationCode { get; set; } = string.Empty;
            public string FullAddress { get; set; } = string.Empty;
            public int? ParentLocationId { get; set; }
            public int Level { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
