using Services.RequestsResponses.PropertyForm;

namespace Services.RequestsResponses.PostType
{
    public class PostTypeFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "posttypeid", "name", "description", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "posttypeid":
                    result["posttypeid"] = (item as PostTypeResponse.PostTypeGetAll)?.PostTypeID
                                        ?? (item as PostTypeResponse.PostTypeGetById)?.PostTypeID;
                    break;
                case "name":
                    result["name"] = (item as PostTypeResponse.PostTypeGetAll)?.Name
                                        ?? (item as PostTypeResponse.PostTypeGetById)?.Name;
                    break;
                case "description":
                    result["description"] = (item as PostTypeResponse.PostTypeGetAll)?.Description
                                        ?? (item as PostTypeResponse.PostTypeGetById)?.Description;
                    break;
                case "createdat":
                    result["createdat"] = (item as PostTypeResponse.PostTypeGetAll)?.CreateAt
                                        ?? (item as PostTypeResponse.PostTypeGetById)?.CreateAt;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for PropertyForm.");
            }
        }
    }
}
