using Services.RequestsResponses.PropertyType;

namespace Services.RequestsResponses.Location
{
    public class LocationFieldResponse : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "locationid", "name", "description", "locationtype", "locationcode", "fulladdress",
            "parentlocationid", "level", "isactive", "activities", "inverseparentlocation", "parentlocation",
            "posts", "roommateprefernces", "createdat", "hierarchy"
        };

        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "locationid":
                    result["locationId"] = (item as LocationResponse.LocationGetAll)?.LocationId
                                        ?? (item as LocationResponse.LocationGetDetail)?.LocationId
                                        ?? (item as LocationResponse.LocationInfo)?.LocationId;
                    break;
                case "name":
                    result["name"] = (item as LocationResponse.LocationGetAll)?.Name
                                        ?? (item as LocationResponse.LocationGetDetail)?.Name
                                        ?? (item as LocationResponse.LocationInfo)?.Name;
                    break;
                case "description":
                    result["description"] = (item as LocationResponse.LocationGetAll)?.Description
                                        ?? (item as LocationResponse.LocationGetDetail)?.Description
                                        ?? (item as LocationResponse.LocationInfo)?.Description;
                    break;
                case "locationtype":
                    result["locationType"] = (item as LocationResponse.LocationGetAll)?.LocationType
                                        ?? (item as LocationResponse.LocationGetDetail)?.LocationType
                                        ?? (item as LocationResponse.LocationInfo)?.LocationType;
                    break;
                case "locationcode":
                    result["locationCode"] = (item as LocationResponse.LocationGetAll)?.LocationCode
                                        ?? (item as LocationResponse.LocationGetDetail)?.LocationCode
                                        ?? (item as LocationResponse.LocationInfo)?.LocationCode;
                    break;
                case "fulladdress":
                    result["fullAddress"] = (item as LocationResponse.LocationGetAll)?.FullAddress
                                        ?? (item as LocationResponse.LocationGetDetail)?.FullAddress
                                        ?? (item as LocationResponse.LocationInfo)?.FullAddress;
                    break;
                case "parentlocationid":
                    result["parentLocationId"] = (item as LocationResponse.LocationGetAll)?.ParentLocationId
                                        ?? (item as LocationResponse.LocationGetDetail)?.ParentLocationId
                                        ?? (item as LocationResponse.LocationInfo)?.ParentLocationId;
                    break;
                case "level":
                    result["level"] = (item as LocationResponse.LocationGetAll)?.Level
                                        ?? (item as LocationResponse.LocationGetDetail)?.Level
                                        ?? (item as LocationResponse.LocationInfo)?.Level;
                    break;
                case "isactive":
                    result["isActive"] = (item as LocationResponse.LocationGetAll)?.IsActive
                                        ?? (item as LocationResponse.LocationGetDetail)?.IsActive
                                        ?? (item as LocationResponse.LocationInfo)?.IsActive;
                    break;
                case "createdat":
                    result["createdAt"] = (item as LocationResponse.LocationGetAll)?.CreatedAt
                                        ?? (item as LocationResponse.LocationGetDetail)?.CreatedAt
                                        ?? (item as LocationResponse.LocationInfo)?.CreatedAt;
                    break;
                case "hierarchy":
                    result["hierarchy"] = (item as LocationResponse.LocationHierarchy)?.Hierarchy;
                    break;
                default:
                    throw new ArgumentException($"Field '{field}' is not a valid field for Location.");
            }
        }
    }
}
