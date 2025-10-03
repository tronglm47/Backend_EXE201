namespace Services.RequestsResponses.Subdivision
{
    public class SubdivisionQuery : QueryParametersBase
    {
        public override string DefaultSortBy => "SubdivisionId";
        public override string DefaultSearchField => "name";
    }
}
