namespace Services.RequestsResponses.PropertyForm
{
    public class PropertyFormFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "propertyformid", "name", "description", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "propertyformid":
                    result["propertyformid"] = (item as PropertyFormResponse.PropertyFormGetAllResponse)?.PropertyFormId
                                        ?? (item as PropertyFormResponse.PropertyFormGetByIdResponse)?.PropertyFormId;
                    break;
                case "name":
                    result["name"] = (item as PropertyFormResponse.PropertyFormGetAllResponse)?.Name
                                        ?? (item as PropertyFormResponse.PropertyFormGetByIdResponse)?.Name;
                    break;
                case "description":
                    result["description"] = (item as PropertyFormResponse.PropertyFormGetAllResponse)?.Description
                                        ?? (item as PropertyFormResponse.PropertyFormGetByIdResponse)?.Description;
                    break;
                case "createdat":
                    result["createdat"] = (item as PropertyFormResponse.PropertyFormGetAllResponse)?.CreatedAt
                                        ?? (item as PropertyFormResponse.PropertyFormGetByIdResponse)?.CreatedAt;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for PropertyForm.");
            }
        }
    }
}
