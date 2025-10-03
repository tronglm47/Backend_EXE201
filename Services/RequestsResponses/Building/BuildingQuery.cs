namespace Services.RequestsResponses.Building
{
    public class BuildingQuery : QueryParametersBase
    {
        public override string DefaultSortBy => "buildingid";
        public override string DefaultSearchField => "Name";
    }
}
