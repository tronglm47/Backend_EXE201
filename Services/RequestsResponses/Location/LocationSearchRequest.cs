namespace Services.RequestsResponses.Location
{
    public class LocationSearchRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ParentLocationId { get; set; }
        public bool? HasChildren { get; set; }
        public bool? HasProperties { get; set; }
        public bool? HasActivities { get; set; }
        
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Sorting
        public string? SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
    }
}