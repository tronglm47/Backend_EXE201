
namespace Services.RequestsResponses.Subdivision
{
    public class SubdivisionField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "subdivisionid", "name", "description", "type", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "subdivisionid":
                    result["subdivisionid"] = (item as SubdivisionResponse.SubdivisionGetAll)?.SubdivisionId
                                            ?? (item as SubdivisionResponse.SubdivisionDetail)?.SubdivisionId;
                    break;
                case "name":
                    result["name"] = (item as SubdivisionResponse.SubdivisionGetAll)?.Name
                                            ?? (item as SubdivisionResponse.SubdivisionDetail)?.Name;
                    break;
                case "description":
                    result["description"] = (item as SubdivisionResponse.SubdivisionGetAll)?.Description
                                            ?? (item as SubdivisionResponse.SubdivisionDetail)?.Description;
                    break;
                case "type":
                    // Type field - add to response classes if needed
                    result["type"] = null; // Placeholder
                    break;
                case "createdat":
                    result["createdat"] = (item as SubdivisionResponse.SubdivisionGetAll)?.CreatedAt
                                            ?? (item as SubdivisionResponse.SubdivisionDetail)?.CreatedAt;
                    break;
                default:
                    result["subdivisionid"] = (item as SubdivisionResponse.SubdivisionGetAll)?.SubdivisionId
                                            ?? (item as SubdivisionResponse.SubdivisionDetail)?.SubdivisionId;
                    break;
            }
        }
    }
}
