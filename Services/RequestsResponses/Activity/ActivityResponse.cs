namespace Services.RequestsResponses.Activity
{
    public class ActivityResponse
    {
        public int ActivityId { get; set; }
        public int CreatorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Optional: Include related data
        public string? CreatorName { get; set; }
        public string? LocationName { get; set; }
    }
}