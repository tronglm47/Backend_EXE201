namespace Services.RequestsResponses.Notification
{
    public class NotificationResponse
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsRead { get; set; }

        // Optional: Include related data
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
    }
}