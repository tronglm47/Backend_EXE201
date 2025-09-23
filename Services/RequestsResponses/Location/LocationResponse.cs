namespace Services.RequestsResponses.Location
{
    public class LocationResponse
    {
        public int LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentLocationId { get; set; }
        
        // Navigation properties for response
        public string? ParentLocationName { get; set; }
        public List<LocationResponse>? ChildLocations { get; set; }
        public int PropertiesCount { get; set; }
        public int ActivitiesCount { get; set; }
    }
}