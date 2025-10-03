using Services.RequestsResponses.Utility;

namespace Services.RequestsResponses.PostUtility
{
    public class PostUtilityResponse
    {
        public class PostUtilityGetAll
        {
            public int PostId { get; set; }
            public int UtilityId { get; set; }
            public string Note { get; set; }
        }
        public class PostUtilityDetail
        {
            public int PostId { get; set; }
            public int UtilityId { get; set; }
            public UtilityResponse.UtilityDetail Utility { get; set; }
            public string Note { get; set; }
        }
    }
}
