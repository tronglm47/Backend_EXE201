namespace Services.RequestsResponses.PostUtility
{
    public class PostUtilityField : FieldResponseBase
    {
        protected override List<string> ValidFields => new List<string>
        {
            "postid", "utilityid", "note"
        };
        protected override void MapField<T>(string field, T item, Dictionary<string, object?> result)
        {
            switch (field)
            {
                case "postid":
                    result["postid"] = (item as PostUtilityResponse.PostUtilityGetAll)?.PostId
                                            ?? (item as PostUtilityResponse.PostUtilityDetail)?.PostId;
                    break;
                case "utilityid":
                    result["utilityid"] = (item as PostUtilityResponse.PostUtilityGetAll)?.UtilityId
                                            ?? (item as PostUtilityResponse.PostUtilityDetail)?.UtilityId;
                    break;
                case "note":
                    result["note"] = (item as PostUtilityResponse.PostUtilityGetAll)?.Note
                                            ?? (item as PostUtilityResponse.PostUtilityDetail)?.Note;
                    break;
                default:
                    result["postid"] = (item as PostUtilityResponse.PostUtilityGetAll)?.PostId
                                            ?? (item as PostUtilityResponse.PostUtilityDetail)?.PostId;
                    break;
            }
        }
    }
}
