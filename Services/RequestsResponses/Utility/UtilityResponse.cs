namespace Services.RequestsResponses.Utility
{
    public class UtilityResponse
    {
        public class UtilityGetAll
        {
            public int UtilityId { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class UtilityDetail
        {
            public int UtilityId { get; set; }
            public string Name { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
