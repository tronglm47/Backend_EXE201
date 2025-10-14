namespace Services.RequestsResponses.Chat
{
    public class ChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool Success { get; set; } = true;
    }
}