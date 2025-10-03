namespace Services.RequestsResponses.Post
{
    public class PostQuery : QueryParametersBase
    {
        public override string DefaultSearchField => "Title";
        public override string DefaultSortBy => "PostId";
    }
}
