namespace Services.RequestsResponses.Apartment
{
    public class ApartmentQuery : QueryParametersBase
    {
        public override string DefaultSearchField => "ApartmentCode";
        public override string DefaultSortBy => "ApartmentId";
    }
}
