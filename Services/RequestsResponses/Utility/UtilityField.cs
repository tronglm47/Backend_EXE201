namespace Services.RequestsResponses.Utility
{
    public class UtilityField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "utilityid", "name", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "utilityid":
                    result["utilityid"] = (item as UtilityResponse.UtilityGetAll)?.UtilityId
                                            ?? (item as UtilityResponse.UtilityDetail)?.UtilityId;
                    break;
                case "name":
                    result["name"] = (item as UtilityResponse.UtilityGetAll)?.Name
                                            ?? (item as UtilityResponse.UtilityDetail)?.Name;
                    break;
                case "createdat":
                    result["createdat"] = (item as UtilityResponse.UtilityGetAll)?.CreatedAt
                                            ?? (item as UtilityResponse.UtilityDetail)?.CreatedAt;
                    break;
                default:
                    result["utilityid"] = (item as UtilityResponse.UtilityGetAll)?.UtilityId
                                            ?? (item as UtilityResponse.UtilityDetail)?.UtilityId;
                    break;
            }
        }
    }
}
