namespace Services.RequestsResponses.PostUtility
{
    public class PostUtilityRequest
    {
        public class PostUtilityCreate
        {
            public required int PostId { get; set; }
            public required int UtilityId { get; set; }
            public string Note { get; set; }
        }
        public class PostUtilityUpdate
        {
            public string Note { get; set; }
        }
    }
}
