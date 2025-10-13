using Services.RequestsResponses.Subdivision;

namespace Services.RequestsResponses.Building
{
    public class BuildingField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "buildingid", "subdivisionid", "name", "blockcode", "maxfloor", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "buildingid":
                    result["buildingid"] = (item as BuildingResponse.BuildingGetAll)?.BuildingId
                                            ?? (item as BuildingResponse.BuildingDetail)?.BuildingId;
                    break;
                case "subdivisionid":
                    result["subdivisionid"] = (item as BuildingResponse.BuildingGetAll)?.SubdivisionId
                                            ?? (item as BuildingResponse.BuildingDetail)?.SubdivisionId;
                    break;
                case "name":
                    result["name"] = (item as BuildingResponse.BuildingGetAll)?.Name
                                            ?? (item as BuildingResponse.BuildingDetail)?.Name;
                    break;
                case "blockcode":
                    result["blockcode"] = (item as BuildingResponse.BuildingGetAll)?.BlockCode 
                                        ?? (item as BuildingResponse.BuildingDetail)?.BlockCode;
                    break;
                case "maxfloor":
                    result["maxfloor"] = (item as BuildingResponse.BuildingGetAll)?.MaxFloor 
                                        ?? (item as BuildingResponse.BuildingDetail)?.MaxFloor;
                    break;
                case "createdat":
                    result["createdat"] = (item as BuildingResponse.BuildingGetAll)?.CreatedAt
                                            ?? (item as BuildingResponse.BuildingDetail)?.CreatedAt;
                    break;
                default:
                    result["buildingid"] = (item as BuildingResponse.BuildingGetAll)?.BuildingId
                                            ?? (item as BuildingResponse.BuildingDetail)?.BuildingId;
                    break;
            }
        }
    }
}
