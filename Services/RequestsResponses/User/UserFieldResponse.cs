namespace Services.RequestsResponses.User
{
    public class UserFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "userid", "username", "email", "fullname", "role", "isemailverified", "createdat"
        };

        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "userid":
                    result["userid"] = (item as UserResponse.UserGetAll)?.UserId
                                        ?? (item as UserResponse.UserDetail)?.UserId;
                    break;
                case "username":
                    result["username"] = (item as UserResponse.UserGetAll)?.Username
                                        ?? (item as UserResponse.UserDetail)?.Username;
                    break;
                case "email":
                    result["email"] = (item as UserResponse.UserGetAll)?.Email
                                        ?? (item as UserResponse.UserDetail)?.Email;
                    break;
                case "fullname":
                    result["fullname"] = (item as UserResponse.UserGetAll)?.FullName
                                        ?? (item as UserResponse.UserDetail)?.FullName;
                    break;
                case "role":
                    result["role"] = (item as UserResponse.UserGetAll)?.Role
                                        ?? (item as UserResponse.UserDetail)?.Role;
                    break;
                case "isemailverified":
                    result["isemailverified"] = (item as UserResponse.UserGetAll)?.IsEmailVerified
                                        ?? (item as UserResponse.UserDetail)?.IsEmailVerified;
                    break;
                case "createdat":
                    result["createdat"] = (item as UserResponse.UserGetAll)?.CreatedAt
                                        ?? (item as UserResponse.UserDetail)?.CreatedAt;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for User.");
            }
        }
    }
}

