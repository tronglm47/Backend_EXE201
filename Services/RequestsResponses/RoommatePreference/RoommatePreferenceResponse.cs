namespace Services.RequestsResponses.RoommatePreference
{
    public class RoommatePreferenceResponse
    {
        public int PreferenceId { get; set; }
        public int UserId { get; set; }
        public string? PreferredGender { get; set; }
        public int? AgeRangeMin { get; set; }
        public int? AgeRangeMax { get; set; }
        public decimal? BudgetMin { get; set; }
        public decimal? BudgetMax { get; set; }
        public string? Habits { get; set; }
        public string? Interests { get; set; }
        public int? LocationId { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Optional: Include related data
        public string? UserName { get; set; }
        public string? LocationName { get; set; }
    }
}