namespace Services.Models
{
    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 500;
        public double Temperature { get; set; } = 0.7;
    }

    public class ChatLimitingSettings
    {
        public int MaxRequestsPerHour { get; set; } = 10;
        public int MaxRequestsPerDay { get; set; } = 50;
        public bool EnableRateLimiting { get; set; } = true;
    }
}