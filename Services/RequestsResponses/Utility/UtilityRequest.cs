namespace Services.RequestsResponses.Utility
{
    public class UtilityRequest
    {
        public class UtilityCreate
        {
            public required string Name { get; set; }
        }
        public class UtilityUpdate
        {
            public required string Name { get; set; }
        }
    }
}
