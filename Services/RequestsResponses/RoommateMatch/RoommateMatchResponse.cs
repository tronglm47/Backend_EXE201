namespace Services.RequestsResponses.RoommateMatch
{
    public class RoommateMatchResponse
    {
        public int MatchId { get; set; }
        public int UserId1 { get; set; }
        public int UserId2 { get; set; }
        public decimal? Score { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? MatchedAt { get; set; }

        // Optional: Include related data
        public string? User1Name { get; set; }
        public string? User2Name { get; set; }
    }
}