namespace Services.RequestsResponses.PostUtility
{
    public class PostUtilityQuery : QueryParametersBase
    {
        public override string DefaultSearchField => "postId";
        public override string DefaultSortBy => "PostId";
    }
}
