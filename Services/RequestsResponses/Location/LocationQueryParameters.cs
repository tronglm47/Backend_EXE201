namespace Services.RequestsResponses.Location
{
    public class LocationQueryParameters : QueryParametersBase
    {
        public override string DefaultSortBy => "LocationId";
        
        /// <summary>
        /// Filter locations where ParentLocationId is greater than this value
        /// </summary>
        public int? MinParentLocationId { get; set; }
    }
}
