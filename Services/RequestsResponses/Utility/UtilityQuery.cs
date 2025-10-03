namespace Services.RequestsResponses.Utility
{
    public class UtilityQuery : QueryParametersBase
    {
        public override string DefaultSearchField => "Name";
        public override string DefaultSortBy => "UtilityId";
    }
}
