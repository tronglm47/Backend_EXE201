using Services.RequestsResponses.PropertyForm;

namespace Services.RequestsResponses.Amenity
{
    public class AmenityFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "amenityid", "name", "description", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "amenityid":
                    result["amenityid"] = (item as AmenityResponse.GetALlResponse)?.AmenityId
                                        ?? (item as AmenityResponse.GetByIdResponse)?.AmenityId;
                    break;
                case "name":
                    result["name"] = (item as AmenityResponse.GetALlResponse)?.Name
                                        ?? (item as AmenityResponse.GetByIdResponse)?.Name;
                    break;
                case "description":
                    result["description"] = (item as AmenityResponse.GetALlResponse)?.Description
                                        ?? (item as AmenityResponse.GetByIdResponse)?.Description;
                    break;
                case "createdat":
                    result["createdat"] = (item as AmenityResponse.GetALlResponse)?.CreatedAt
                                        ?? (item as AmenityResponse.GetByIdResponse)?.CreatedAt;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for PropertyForm.");
            }
        }
    }
}
