namespace Services.RequestsResponses.RoommateMatch
{
    public class CreateRoommateMatchRequest
    {
        public int UserId1 { get; set; }
        public int UserId2 { get; set; }
        public decimal? Score { get; set; }
        public string Status { get; set; } = "pending";
    }
}