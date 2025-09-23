namespace Services.RequestsResponses.RoommateMatch
{
    public class UpdateRoommateMatchRequest
    {
        public decimal? Score { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}