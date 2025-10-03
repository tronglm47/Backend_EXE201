namespace Services.RequestsResponses.Apartment
{
    public class ApartmentField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "apartmentid", "buildingid", "apartmentcode", "floor", "area", "apartmenttype", "status", "createdat"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "apartmentid":
                    result["apartmentid"] = (item as ApartmentResponse.ApartmentGetAll)?.ApartmentId
                                            ?? (item as ApartmentResponse.ApartmentDetail)?.ApartmentId;
                    break;
                case "buildingid":
                    result["buildingid"] = (item as ApartmentResponse.ApartmentGetAll)?.BuildingId
                                            ?? (item as ApartmentResponse.ApartmentDetail)?.BuildingId;
                    break;
                case "apartmentcode":
                    result["apartmentcode"] = (item as ApartmentResponse.ApartmentGetAll)?.ApartmentCode
                                            ?? (item as ApartmentResponse.ApartmentDetail)?.ApartmentCode;
                    break;
                case "floor":
                    result["floor"] = (item as ApartmentResponse.ApartmentGetAll)?.Floor
                                            ?? (item as ApartmentResponse.ApartmentDetail)?.Floor;
                    break;
                case "area":
                    // Type field - add to response classes if needed
                    result["area"] = (item as ApartmentResponse.ApartmentGetAll)?.Area
                                        ?? (item as ApartmentResponse.ApartmentDetail)?.Area;
                    break;
                case "apartmenttype":
                    // Type field - add to response classes if needed
                    result["apartmenttype"] = (item as ApartmentResponse.ApartmentGetAll)?.ApartmentType
                                        ?? (item as ApartmentResponse.ApartmentDetail)?.ApartmentType;
                    break;
                case "status":
                    // Type field - add to response classes if needed
                    result["status"] = (item as ApartmentResponse.ApartmentGetAll)?.Status
                                        ?? (item as ApartmentResponse.ApartmentDetail)?.Status;
                    break;
                case "createdat":
                    result["createdat"] = (item as ApartmentResponse.ApartmentGetAll)?.CreatedAt
                                            ?? (item as ApartmentResponse.ApartmentDetail)?.CreatedAt;
                    break;
                default:
                    result["apartmentid"] = (item as ApartmentResponse.ApartmentGetAll)?.ApartmentId
                                            ?? (item as ApartmentResponse.ApartmentDetail)?.ApartmentId;
                    break;
            }
        }
    }
}
