namespace Services.RequestsResponses.Post
{
    public class PostField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "postid", "userid", "title", "description", "createdat", "price", "status", "posttype", "apartmentid",
            "apartmentcode", "floor", "area", "numberbathroom", "buildingid", "buildingname", "blockcode", 
            "subdivisionid", "subdivisionname"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "postid":
                    result["postid"] = (item as PostResponse.PostGetAll)?.PostId 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.PostId;
                    break;
                case "userid":
                    result["userid"] = (item as PostResponse.PostGetAll)?.UserId 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.UserId;
                    break;
                case "title":
                    result["title"] = (item as PostResponse.PostGetAll)?.Title 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.Title;
                    break;
                case "description":
                    result["description"] = (item as PostResponse.PostGetAll)?.Description 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.Description;
                    break;
                case "price":
                    result["price"] = (item as PostResponse.PostGetAll)?.Price 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.Price;
                    break;
                case "status":
                    result["status"] = (item as PostResponse.PostGetAll)?.Status 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.Status;
                    break;
                case "posttype":
                    result["posttype"] = (item as PostResponse.PostGetAll)?.PostType 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.PostType;
                    break;
                case "apartmentid":
                    result["apartmentid"] = (item as PostResponse.PostGetAll)?.ApartmentId 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.ApartmentId;
                    break;
                case "createdat":
                    result["createdat"] = (item as PostResponse.PostGetAll)?.CreatedAt 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.CreatedAt;
                    break;
                // LandLord-specific fields
                case "apartmentcode":
                    result["apartmentcode"] = (item as PostResponse.PostGetAllForLandLord)?.ApartmentCode;
                    break;
                case "floor":
                    result["floor"] = (item as PostResponse.PostGetAllForLandLord)?.Floor;
                    break;
                case "area":
                    result["area"] = (item as PostResponse.PostGetAllForLandLord)?.Area;
                    break;
                case "numberbathroom":
                    result["numberbathroom"] = (item as PostResponse.PostGetAllForLandLord)?.NumberBathroom;
                    break;
                case "buildingid":
                    result["buildingid"] = (item as PostResponse.PostGetAllForLandLord)?.BuildingId;
                    break;
                case "buildingname":
                    result["buildingname"] = (item as PostResponse.PostGetAllForLandLord)?.BuildingName;
                    break;
                case "blockcode":
                    result["blockcode"] = (item as PostResponse.PostGetAllForLandLord)?.BlockCode;
                    break;
                case "subdivisionid":
                    result["subdivisionid"] = (item as PostResponse.PostGetAllForLandLord)?.SubdivisionId;
                    break;
                case "subdivisionname":
                    result["subdivisionname"] = (item as PostResponse.PostGetAllForLandLord)?.SubdivisionName;
                    break;
                default:
                    result["postid"] = (item as PostResponse.PostGetAll)?.PostId 
                                        ?? (item as PostResponse.PostGetAllForLandLord)?.PostId;
                    break;
            }
        }
    }
}
