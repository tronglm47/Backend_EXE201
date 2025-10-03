namespace Services.RequestsResponses.User
{
    public class UserQueryParameters : QueryParametersBase
    {
        public override string DefaultSortBy => "UserID";
        public override string DefaultSearchField => "username";
    }
}

